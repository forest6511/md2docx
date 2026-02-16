# Contributing to Markdown to Word Converter

Thank you for your interest in contributing to md2docx! This document provides guidelines for contributing to the project.

---

## 📋 Table of Contents

- [Development Status](#development-status)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Code Standards](#code-standards)
- [Testing Requirements](#testing-requirements)
- [Submitting Changes](#submitting-changes)

---

## 🚧 Development Status

**Current Phase**: Phase 1 - MVP Implementation (In Progress)

We are currently building the foundational architecture and basic conversion capabilities. See [STATUS.md](STATUS.md) for detailed progress.

**Priority Areas**:
1. Core layer implementation (OpenXmlDocumentBuilder)
2. Styling layer (YAML configuration, text transformers)
3. Test coverage expansion (target: 80%)

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (optional)
- [Git](https://git-scm.com/)
- Code editor ([Visual Studio Code](https://code.visualstudio.com/) recommended)

### Initial Setup

```bash
# Clone the repository
git clone https://github.com/forest6511/md2docx.git
cd md2docx

# Install Git hooks (quality gates)
./.claude/hooks/install.sh

# Restore dependencies
cd csharp-version
dotnet restore src/MarkdownToDocx.sln

# Build the solution
dotnet build src/MarkdownToDocx.sln

# Run tests
dotnet test src/MarkdownToDocx.sln
```

---

## 🔄 Development Workflow

### 1. Create a Feature Branch

```bash
git checkout -b feature/your-feature-name
```

**Branch naming convention**:
- `feature/` - New features
- `fix/` - Bug fixes
- `refactor/` - Code refactoring
- `docs/` - Documentation updates
- `test/` - Test additions

### 2. Make Changes

- Follow [Code Standards](#code-standards)
- Write tests for new functionality
- Update documentation as needed

### 3. Run Quality Checks

```bash
# Run all tests
dotnet test src/MarkdownToDocx.sln

# Check code coverage (target: 80%)
dotnet test src/MarkdownToDocx.sln /p:CollectCoverage=true

# Build documentation (if applicable)
dotnet build -c Release
```

### 4. Commit Changes

```bash
# Stage your changes
git add .

# Commit with conventional commit message
git commit -m "feat: add vertical text provider implementation"
```

**Commit message format**:
- `feat:` - New features
- `fix:` - Bug fixes
- `docs:` - Documentation changes
- `test:` - Test additions
- `refactor:` - Code refactoring
- `chore:` - Build, CI/CD, dependencies

### 5. Pre-Push Review

**IMPORTANT**: All changes MUST be reviewed by Codex before pushing.

The pre-push Git hook automatically triggers Codex review. If blocked, address the issues before pushing.

```bash
# Push your branch
git push origin feature/your-feature-name

# Create pull request on GitHub
```

---

## 📐 Code Standards

### C# Style Guidelines

- **Language Version**: C# 12 with .NET 8.0
- **Nullable Reference Types**: Enabled
- **File-Scoped Namespaces**: Preferred
- **Record Types**: Use for immutable models

**Example**:
```csharp
namespace MarkdownToDocx.Core.Models;

/// <summary>
/// Represents page configuration for a Word document
/// </summary>
public sealed record PageConfiguration
{
    public required UInt32Value Width { get; init; }
    public required UInt32Value Height { get; init; }
}
```

### SOLID Principles

- **Single Responsibility**: One class, one purpose
- **Open/Closed**: Open for extension, closed for modification
- **Liskov Substitution**: Derived classes substitutable for base
- **Interface Segregation**: Small, focused interfaces
- **Dependency Inversion**: Depend on abstractions, not concretions

---

## 🧪 Testing Requirements

### Unit Tests

- **Coverage Target**: ≥80%
- **Framework**: xUnit + FluentAssertions
- **Naming**: `{MethodName}_{Scenario}_Should{ExpectedBehavior}`

**Example**:
```csharp
[Fact]
public void Parse_WithNullInput_ShouldThrowArgumentNullException()
{
    // Arrange
    string? markdown = null;

    // Act
    Action act = () => _parser.Parse(markdown!);

    // Assert
    act.Should().Throw<ArgumentNullException>()
        .WithParameterName("markdown");
}
```

### Running Tests

```bash
# Run all tests
dotnet test src/MarkdownToDocx.sln

# Run with coverage
dotnet test src/MarkdownToDocx.sln /p:CollectCoverage=true
```

---

## 📤 Submitting Changes

### Pull Request Checklist

- [ ] Code follows style guidelines
- [ ] All tests passing
- [ ] Test coverage ≥80% for new code
- [ ] Documentation updated
- [ ] Codex review passed
- [ ] No breaking changes

---

## 📄 License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing to md2docx! 🎉
