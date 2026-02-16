#!/usr/bin/env python3
"""
ADR-Source Consistency Validator

Validates that accepted ADRs are actually implemented in the codebase.

Architecture:
- Modular validators (easy to test and extend)
- Plugin-based validation types
- Detailed error reporting
- Dry-run support
"""

import re
import os
import sys
import glob
import argparse
from pathlib import Path
from typing import List, Dict, Optional, Tuple
from dataclasses import dataclass, field
from enum import Enum

# ============================================================
# Data Models
# ============================================================

class ADRStatus(Enum):
    PROPOSED = "Proposed"
    ACCEPTED = "Accepted"
    DEPRECATED = "Deprecated"
    SUPERSEDED = "Superseded"
    REJECTED = "Rejected"
    UNKNOWN = "Unknown"

@dataclass
class ValidationResult:
    """Result of a single validation check"""
    rule_id: str
    passed: bool
    message: str
    severity: str = "error"  # error, warning, info
    details: Optional[str] = None

@dataclass
class ADR:
    """Architecture Decision Record metadata"""
    number: int
    title: str
    status: ADRStatus
    path: str
    content: str
    results: List[ValidationResult] = field(default_factory=list)

    def add_result(self, result: ValidationResult):
        self.results.append(result)

    @property
    def passed(self) -> bool:
        return all(r.passed for r in self.results if r.severity == "error")

    @property
    def has_errors(self) -> bool:
        return any(not r.passed and r.severity == "error" for r in self.results)

    @property
    def has_warnings(self) -> bool:
        return any(not r.passed and r.severity == "warning" for r in self.results)

# ============================================================
# Base Validator
# ============================================================

class BaseValidator:
    """Base class for all validators"""

    def __init__(self, verbose: bool = False):
        self.verbose = verbose

    def can_validate(self, adr: ADR) -> bool:
        """Check if this validator applies to the given ADR"""
        raise NotImplementedError

    def validate(self, adr: ADR) -> List[ValidationResult]:
        """Perform validation and return results"""
        raise NotImplementedError

    def log(self, message: str):
        """Debug logging"""
        if self.verbose:
            print(f"[DEBUG] {message}", file=sys.stderr)

# ============================================================
# Concrete Validators
# ============================================================

class FontLicenseValidator(BaseValidator):
    """Validates font licensing decisions"""

    COMMERCIAL_FONTS = {
        '游明朝': 'Yu.*Mincho|游明朝',
        '游ゴシック': 'Yu.*Gothic|游ゴシック',
        'Hiragino': 'Hiragino|ヒラギノ',
        'MS Fonts': r'MS.*(明朝|ゴシック|Mincho|Gothic)'
    }

    def can_validate(self, adr: ADR) -> bool:
        content_lower = adr.content.lower()
        return 'font' in content_lower or 'noto' in content_lower

    def validate(self, adr: ADR) -> List[ValidationResult]:
        results = []

        # Check Dockerfile exists
        if not os.path.exists('Dockerfile'):
            results.append(ValidationResult(
                rule_id='FONT-001',
                passed=False,
                message='Dockerfile not found',
                severity='warning'
            ))
            return results

        with open('Dockerfile', 'r', encoding='utf-8') as f:
            dockerfile = f.read()

        # Check for Noto fonts if ADR specifies them
        if 'Noto' in adr.content:
            if 'noto' in dockerfile.lower():
                results.append(ValidationResult(
                    rule_id='FONT-002',
                    passed=True,
                    message='Noto fonts present in Dockerfile'
                ))
            else:
                results.append(ValidationResult(
                    rule_id='FONT-002',
                    passed=False,
                    message='ADR requires Noto fonts but not found in Dockerfile',
                    details='Add Noto Serif/Sans CJK JP to Dockerfile'
                ))

        # Check for commercial fonts (should not exist)
        commercial_found = []
        for font_name, pattern in self.COMMERCIAL_FONTS.items():
            if re.search(pattern, dockerfile, re.IGNORECASE):
                commercial_found.append(font_name)

        if commercial_found:
            results.append(ValidationResult(
                rule_id='FONT-003',
                passed=False,
                message=f'Commercial fonts found: {", ".join(commercial_found)}',
                details='Replace with SIL OFL licensed fonts (Noto family)'
            ))
        else:
            results.append(ValidationResult(
                rule_id='FONT-003',
                passed=True,
                message='No commercial fonts detected'
            ))

        return results

class LibraryDependencyValidator(BaseValidator):
    """Validates library/dependency decisions"""

    def can_validate(self, adr: ADR) -> bool:
        return 'library' in adr.content.lower() or 'DocumentFormat.OpenXml' in adr.content

    def validate(self, adr: ADR) -> List[ValidationResult]:
        results = []

        # Find all .csproj files
        csproj_files = glob.glob('csharp-version/**/*.csproj', recursive=True)

        if not csproj_files:
            results.append(ValidationResult(
                rule_id='LIB-001',
                passed=False,
                message='No .csproj files found',
                severity='warning'
            ))
            return results

        # Check for required libraries
        if 'DocumentFormat.OpenXml' in adr.content:
            found_in = []

            for csproj in csproj_files:
                with open(csproj, 'r', encoding='utf-8') as f:
                    if 'DocumentFormat.OpenXml' in f.read():
                        found_in.append(Path(csproj).name)

            if found_in:
                results.append(ValidationResult(
                    rule_id='LIB-002',
                    passed=True,
                    message=f'DocumentFormat.OpenXml in dependencies',
                    details=f'Found in: {", ".join(found_in)}'
                ))
            else:
                results.append(ValidationResult(
                    rule_id='LIB-002',
                    passed=False,
                    message='DocumentFormat.OpenXml not in any .csproj',
                    details='Add NuGet package: DocumentFormat.OpenXml'
                ))

        # Check rejected alternatives (should not exist)
        rejected_libs = self._extract_rejected_libraries(adr.content)

        if rejected_libs:
            found_rejected = []

            for lib in rejected_libs:
                for csproj in csproj_files:
                    with open(csproj, 'r', encoding='utf-8') as f:
                        if lib in f.read():
                            found_rejected.append((lib, Path(csproj).name))

            if found_rejected:
                violations = [f"{lib} in {proj}" for lib, proj in found_rejected]
                results.append(ValidationResult(
                    rule_id='LIB-003',
                    passed=False,
                    message=f'Rejected libraries found: {len(found_rejected)}',
                    details='; '.join(violations)
                ))
            else:
                results.append(ValidationResult(
                    rule_id='LIB-003',
                    passed=True,
                    message='No rejected libraries detected'
                ))

        return results

    def _extract_rejected_libraries(self, content: str) -> List[str]:
        """Extract library names from rejected alternatives section"""
        # Match patterns like "- ❌ NPOI" or "- NPOI (rejected)"
        pattern = r'[-❌]\s+(\w+(?:\.\w+)*)'
        matches = re.findall(pattern, content)
        return [m for m in matches if m and len(m) > 2]

class YAMLSchemaValidator(BaseValidator):
    """Validates YAML schema version decisions"""

    def can_validate(self, adr: ADR) -> bool:
        return 'yaml' in adr.content.lower() and 'schema' in adr.content.lower()

    def validate(self, adr: ADR) -> List[ValidationResult]:
        results = []

        # Extract required schema version
        version_match = re.search(r'schema_version.*["\'](\d+\.\d+)["\']', adr.content)
        if not version_match:
            results.append(ValidationResult(
                rule_id='YAML-001',
                passed=True,
                message='No specific schema version requirement found',
                severity='info'
            ))
            return results

        required_version = version_match.group(1)

        # Find all YAML preset files
        preset_patterns = [
            'config/presets/**/*.yaml',
            'config/publishing/**/*.yaml',
            'config/vertical/**/*.yaml'
        ]

        preset_files = []
        for pattern in preset_patterns:
            preset_files.extend(glob.glob(pattern, recursive=True))

        if not preset_files:
            results.append(ValidationResult(
                rule_id='YAML-002',
                passed=False,
                message='No YAML preset files found',
                severity='warning'
            ))
            return results

        # Validate schema versions
        try:
            import yaml
        except ImportError:
            results.append(ValidationResult(
                rule_id='YAML-003',
                passed=False,
                message='PyYAML not installed, cannot validate',
                severity='warning',
                details='Install: pip install pyyaml'
            ))
            return results

        wrong_version = []

        for preset_file in preset_files:
            try:
                with open(preset_file, 'r', encoding='utf-8') as f:
                    config = yaml.safe_load(f)

                if isinstance(config, dict):
                    actual_version = config.get('schema_version')

                    if actual_version != required_version:
                        wrong_version.append({
                            'file': Path(preset_file).name,
                            'actual': actual_version,
                            'expected': required_version
                        })

            except yaml.YAMLError as e:
                # YAML syntax error (caught by R001)
                pass

        if wrong_version:
            details = '\n'.join([
                f"  - {item['file']}: has v{item['actual']}, needs v{item['expected']}"
                for item in wrong_version
            ])

            results.append(ValidationResult(
                rule_id='YAML-004',
                passed=False,
                message=f'{len(wrong_version)} preset(s) with wrong schema version',
                details=details
            ))
        else:
            results.append(ValidationResult(
                rule_id='YAML-004',
                passed=True,
                message=f'All presets use schema v{required_version}'
            ))

        return results

class DesignPatternValidator(BaseValidator):
    """Validates design pattern implementation"""

    def can_validate(self, adr: ADR) -> bool:
        content_lower = adr.content.lower()
        return 'factory' in content_lower or 'pattern' in content_lower

    def validate(self, adr: ADR) -> List[ValidationResult]:
        results = []

        # Extract implementation location
        location_match = re.search(
            r'Implementation.*[:`]\s*`?([^`\n]+\.cs)`?',
            adr.content,
            re.IGNORECASE
        )

        if not location_match:
            results.append(ValidationResult(
                rule_id='PATTERN-001',
                passed=True,
                message='No specific implementation location specified',
                severity='info'
            ))
            return results

        expected_file = location_match.group(1).strip('`')

        # Check file exists
        if os.path.exists(expected_file):
            results.append(ValidationResult(
                rule_id='PATTERN-002',
                passed=True,
                message=f'Implementation found at {expected_file}'
            ))
        else:
            results.append(ValidationResult(
                rule_id='PATTERN-002',
                passed=False,
                message=f'Implementation missing: {expected_file}',
                details='Create the specified implementation file'
            ))

        return results

# ============================================================
# ADR Validator Orchestrator
# ============================================================

class ADRValidator:
    """Main validator orchestrator"""

    def __init__(self, verbose: bool = False, dry_run: bool = False):
        self.verbose = verbose
        self.dry_run = dry_run
        self.validators: List[BaseValidator] = [
            FontLicenseValidator(verbose),
            LibraryDependencyValidator(verbose),
            YAMLSchemaValidator(verbose),
            DesignPatternValidator(verbose)
        ]
        self.adrs: List[ADR] = []

    def validate_all(self) -> int:
        """Run all validation checks"""
        print("🔍 Validating ADR-Source Consistency...\n")

        # Find all ADR files
        adr_files = sorted(glob.glob('docs/decisions/ADR-*.md'))

        if not adr_files:
            print("⚠️  No ADR files found in docs/decisions/\n")
            return 0

        # Load and validate each ADR
        for adr_path in adr_files:
            adr = self._load_adr(adr_path)
            self.adrs.append(adr)

            # Only validate Accepted ADRs
            if adr.status == ADRStatus.ACCEPTED:
                self._validate_adr(adr)
            else:
                self._skip_adr(adr)

        # Print report
        self._print_report()

        # Return exit code
        has_errors = any(adr.has_errors for adr in self.adrs)
        return 1 if has_errors else 0

    def _load_adr(self, adr_path: str) -> ADR:
        """Load ADR from file"""
        with open(adr_path, 'r', encoding='utf-8') as f:
            content = f.read()

        number = self._extract_number(adr_path)
        title = self._extract_title(content)
        status = self._extract_status(content)

        return ADR(
            number=number,
            title=title,
            status=status,
            path=adr_path,
            content=content
        )

    def _validate_adr(self, adr: ADR):
        """Validate a single ADR"""
        print(f"📋 ADR-{adr.number:04d}: {adr.title}")
        print(f"   Status: {adr.status.value}")

        # Run applicable validators
        applicable_validators = [v for v in self.validators if v.can_validate(adr)]

        if not applicable_validators:
            print("   ⏭️  No applicable validators\n")
            return

        for validator in applicable_validators:
            results = validator.validate(adr)
            for result in results:
                adr.add_result(result)
                self._print_result(result)

        print()

    def _skip_adr(self, adr: ADR):
        """Skip non-Accepted ADRs"""
        print(f"📋 ADR-{adr.number:04d}: {adr.title}")
        print(f"   Status: {adr.status.value}")
        print(f"   ⏭️  Skipped (not Accepted)\n")

    def _print_result(self, result: ValidationResult):
        """Print a single validation result"""
        if result.passed:
            icon = "✅"
        elif result.severity == "warning":
            icon = "⚠️ "
        else:
            icon = "❌"

        print(f"   {icon} {result.message}")

        if result.details and self.verbose:
            # Indent details
            for line in result.details.split('\n'):
                print(f"      {line}")

    def _print_report(self):
        """Print summary report"""
        print("━" * 60)
        print("📊 ADR-Source Consistency Report")
        print("━" * 60)
        print()

        # Collect all results
        all_errors = []
        all_warnings = []
        all_passed = []

        for adr in self.adrs:
            for result in adr.results:
                if not result.passed:
                    if result.severity == "error":
                        all_errors.append((adr, result))
                    elif result.severity == "warning":
                        all_warnings.append((adr, result))
                else:
                    all_passed.append(result)

        # Print violations
        if all_errors:
            print("❌ VIOLATIONS:")
            for adr, result in all_errors:
                print(f"   • ADR-{adr.number:04d}: {result.message}")
                if result.details:
                    print(f"     {result.details}")
            print()

        # Print warnings
        if all_warnings:
            print("⚠️  WARNINGS:")
            for adr, result in all_warnings:
                print(f"   • ADR-{adr.number:04d}: {result.message}")
            print()

        # Print passed
        if all_passed:
            print(f"✅ PASSED: {len(all_passed)} check(s)")

        print()
        print("━" * 60)

        # Compliance percentage
        total = len(all_errors) + len(all_warnings) + len(all_passed)
        compliance = (len(all_passed) / total * 100) if total > 0 else 100

        print(f"Compliance: {compliance:.1f}% ({len(all_passed)}/{total})")
        print("━" * 60)

        # Dry-run note
        if self.dry_run:
            print("\n⚠️  DRY-RUN MODE: No enforcement, validation only")

    # ============================================================
    # Utility Methods
    # ============================================================

    def _extract_number(self, adr_path: str) -> int:
        """Extract ADR number from filename"""
        match = re.search(r'ADR-(\d+)', adr_path)
        return int(match.group(1)) if match else 0

    def _extract_status(self, content: str) -> ADRStatus:
        """Extract status from ADR content"""
        patterns = [
            r'\*\*Status\*\*:\s*(\w+)',
            r'Status:\s*\*\*(\w+)\*\*',
            r'##\s+Status\s+(\w+)',
        ]

        for pattern in patterns:
            match = re.search(pattern, content)
            if match:
                status_str = match.group(1)
                try:
                    return ADRStatus(status_str)
                except ValueError:
                    return ADRStatus.UNKNOWN

        return ADRStatus.UNKNOWN

    def _extract_title(self, content: str) -> str:
        """Extract title from ADR content"""
        lines = content.split('\n')
        for line in lines:
            if line.startswith('# ADR-'):
                if ':' in line:
                    return line.split(':', 1)[1].strip()
                else:
                    # Remove "# ADR-XXXX: " prefix
                    return re.sub(r'^#\s*ADR-\d+:\s*', '', line).strip()
        return 'Unknown'

# ============================================================
# CLI Entry Point
# ============================================================

def main():
    parser = argparse.ArgumentParser(
        description='Validate ADR-Source consistency'
    )
    parser.add_argument(
        '--verbose', '-v',
        action='store_true',
        help='Enable verbose output with details'
    )
    parser.add_argument(
        '--dry-run',
        action='store_true',
        help='Validation only, no enforcement'
    )

    args = parser.parse_args()

    validator = ADRValidator(verbose=args.verbose, dry_run=args.dry_run)
    exit_code = validator.validate_all()

    sys.exit(exit_code)

if __name__ == '__main__':
    main()
