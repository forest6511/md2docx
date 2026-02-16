.PHONY: help
.DEFAULT_GOAL := help

# ============================================================
# Documentation Maintenance Tasks
# ============================================================

## docs-check: Quick documentation consistency check
docs-check:
	@echo "📚 Running quick documentation check..."
	@./scripts/check-docs-sync.sh

## docs-full: Comprehensive documentation validation (uses SKILL)
docs-full:
	@echo "🔍 Running comprehensive documentation check..."
	@echo "💡 This uses /check-docs-consistency SKILL (requires Claude Code)"
	@echo ""
	@echo "Please run manually: /check-docs-consistency"

## docs-sync: Check English/Japanese documentation sync
docs-sync:
	@./scripts/check-docs-sync.sh

## docs-source: Check documentation-source consistency
docs-source:
	@./scripts/check-docs-source-consistency.sh

## docs-all: Run all documentation checks
docs-all: docs-sync docs-source
	@echo ""
	@echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
	@echo "✅ All documentation checks completed"
	@echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

# ============================================================
# Release Management Tasks
# ============================================================

## release-prepare: Prepare for release (interactive: asks for version)
release-prepare:
	@echo "🚀 Release Preparation"
	@echo ""
	@read -p "Enter version number (e.g., 0.2.0): " VERSION; \
	$(MAKE) release-version VERSION=$$VERSION && \
	$(MAKE) release-notes VERSION=$$VERSION && \
	echo "" && \
	echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" && \
	echo "✅ Release preparation complete for v$$VERSION" && \
	echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" && \
	echo "" && \
	echo "📋 Next steps:" && \
	echo "1. Review changes: git diff" && \
	echo "2. Edit release notes: docs/RELEASE_NOTES_v$$VERSION.md" && \
	echo "3. Run comprehensive check: make docs-full" && \
	echo "4. Commit: make release-commit VERSION=$$VERSION" && \
	echo "5. Tag and push: make release-publish VERSION=$$VERSION"

## release-version: Update version number (requires VERSION=x.y.z)
release-version:
ifndef VERSION
	@echo "❌ Error: VERSION not specified"
	@echo "Usage: make release-version VERSION=0.2.0"
	@exit 1
endif
	@echo "🔢 Updating version to $(VERSION)..."
	@./scripts/update-version.sh $(VERSION)

## release-notes: Generate release notes (requires VERSION=x.y.z)
release-notes:
ifndef VERSION
	@echo "❌ Error: VERSION not specified"
	@echo "Usage: make release-notes VERSION=0.2.0"
	@exit 1
endif
	@echo "📝 Generating release notes for v$(VERSION)..."
	@./scripts/generate-release-notes.sh $(VERSION)

## release-commit: Commit release changes (requires VERSION=x.y.z)
release-commit:
ifndef VERSION
	@echo "❌ Error: VERSION not specified"
	@echo "Usage: make release-commit VERSION=0.2.0"
	@exit 1
endif
	@echo "💾 Committing release v$(VERSION)..."
	@git add -A
	@git commit -m "chore: prepare release v$(VERSION)"
	@echo "✅ Release committed"

## release-tag: Create git tag (requires VERSION=x.y.z)
release-tag:
ifndef VERSION
	@echo "❌ Error: VERSION not specified"
	@echo "Usage: make release-tag VERSION=0.2.0"
	@exit 1
endif
	@echo "🏷️  Creating tag v$(VERSION)..."
	@git tag -a "v$(VERSION)" -m "Release v$(VERSION)"
	@echo "✅ Tag created: v$(VERSION)"

## release-publish: Push commits and tags to remote (requires VERSION=x.y.z)
release-publish:
ifndef VERSION
	@echo "❌ Error: VERSION not specified"
	@echo "Usage: make release-publish VERSION=0.2.0"
	@exit 1
endif
	@echo "📤 Publishing release v$(VERSION)..."
	@git push origin main
	@git push origin v$(VERSION)
	@echo ""
	@echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
	@echo "✅ Release v$(VERSION) published!"
	@echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
	@echo ""
	@echo "📋 Next steps:"
	@echo "1. Create GitHub Release: gh release create v$(VERSION) --notes-file docs/RELEASE_NOTES_v$(VERSION).md"
	@echo "2. Build and push Docker image"
	@echo "3. Publish to NuGet (if applicable)"

# ============================================================
# Development Tasks
# ============================================================

## build: Build the C# project
build:
	@echo "🔨 Building project..."
	@cd csharp-version && dotnet build --configuration Release

## test: Run unit tests
test:
	@echo "🧪 Running tests..."
	@cd csharp-version && dotnet test

## test-coverage: Run tests with coverage report
test-coverage:
	@echo "📊 Running tests with coverage..."
	@cd csharp-version && dotnet test --collect:"XPlat Code Coverage"

## clean: Clean build artifacts
clean:
	@echo "🧹 Cleaning build artifacts..."
	@cd csharp-version && dotnet clean
	@find . -type d -name "bin" -o -name "obj" | xargs rm -rf
	@echo "✅ Clean complete"

# ============================================================
# Docker Tasks
# ============================================================

## docker-build: Build Docker image
docker-build:
	@echo "🐳 Building Docker image..."
	@docker build -t md2docx:latest -f Dockerfile .

## docker-test: Test Docker image
docker-test:
	@echo "🧪 Testing Docker image..."
	@docker run --rm md2docx:latest --help

# ============================================================
# Git Hooks
# ============================================================

## hooks-install: Install git hooks
hooks-install:
	@echo "🔧 Installing git hooks..."
	@./.claude/hooks/install.sh
	@echo "✅ Git hooks installed"

# ============================================================
# Utility Tasks
# ============================================================

## status: Show project status
status:
	@echo "📊 Project Status"
	@echo ""
	@echo "Git Branch:"
	@git branch --show-current
	@echo ""
	@echo "Git Status:"
	@git status --short
	@echo ""
	@echo "Recent Commits:"
	@git log --oneline -5
	@echo ""
	@echo "Documentation Files:"
	@find docs -name "*.md" -type f | wc -l | xargs echo "  Markdown files:"
	@echo ""
	@echo "C# Source Files:"
	@find csharp-version/src -name "*.cs" -type f 2>/dev/null | wc -l | xargs echo "  C# files:"

## version-show: Show current version
version-show:
	@echo "📌 Current Versions:"
	@echo ""
	@echo "README.md:"
	@grep -oE 'version-[0-9]+\.[0-9]+\.[0-9]+' README.md | head -1 | sed 's/version-/  /'
	@echo ""
	@echo "docs/_config.yml:"
	@grep '^version:' docs/_config.yml | sed 's/version: */ /' || echo "  (not set)"
	@echo ""
	@echo "*.csproj:"
	@grep -h '<Version>' csharp-version/src/*/*.csproj 2>/dev/null | head -1 | sed 's/.*<Version>\(.*\)<\/Version>/  \1/' || echo "  (not set)"

# ============================================================
# Help
# ============================================================

## help: Show this help message
help:
	@echo ""
	@echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
	@echo "  md2docx Makefile - Task Runner"
	@echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
	@echo ""
	@echo "📚 Documentation Tasks:"
	@echo "  make docs-check          Quick documentation check"
	@echo "  make docs-sync           Check English/Japanese sync"
	@echo "  make docs-source         Check source consistency"
	@echo "  make docs-all            Run all documentation checks"
	@echo ""
	@echo "🚀 Release Tasks:"
	@echo "  make release-prepare     Interactive release preparation"
	@echo "  make release-version VERSION=x.y.z"
	@echo "  make release-notes VERSION=x.y.z"
	@echo "  make release-commit VERSION=x.y.z"
	@echo "  make release-tag VERSION=x.y.z"
	@echo "  make release-publish VERSION=x.y.z"
	@echo ""
	@echo "🔧 Development Tasks:"
	@echo "  make build               Build C# project"
	@echo "  make test                Run unit tests"
	@echo "  make test-coverage       Run tests with coverage"
	@echo "  make clean               Clean build artifacts"
	@echo ""
	@echo "🐳 Docker Tasks:"
	@echo "  make docker-build        Build Docker image"
	@echo "  make docker-test         Test Docker image"
	@echo ""
	@echo "⚙️  Utility Tasks:"
	@echo "  make hooks-install       Install git hooks"
	@echo "  make status              Show project status"
	@echo "  make version-show        Show current versions"
	@echo "  make help                Show this help message"
	@echo ""
	@echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
	@echo "💡 Quick Start:"
	@echo "  make docs-check          # Before commit"
	@echo "  make release-prepare     # Before release"
	@echo "  make build test          # Development"
	@echo ""
	@echo "📖 For more details: see scripts/README.md"
	@echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
	@echo ""
