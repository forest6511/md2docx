# Documentation Maintenance Quick Start

Simple guide for maintaining md2docx documentation.

## 🎯 Most Common Tasks

### Before Committing Documentation Changes

```bash
make docs-check
```

If there are warnings, fix them before committing.

---

### Preparing a New Release

```bash
make release-prepare
```

This will interactively:
1. Ask for version number
2. Update all version numbers
3. Generate release notes

Then review and commit:
```bash
git diff                    # Review changes
make docs-full              # Run comprehensive check
git commit -m "chore: prepare release v0.2.0"
git tag v0.2.0
git push && git push --tags
```

---

### Building and Testing

```bash
make build          # Build C# project
make test           # Run unit tests
make docker-build   # Build Docker image
```

---

## 📖 Complete Task List

Run `make help` to see all available tasks:

```bash
make help
```

Output:
```
📚 Documentation Tasks:
  make docs-check          Quick documentation check
  make docs-sync           Check English/Japanese sync
  make docs-source         Check source consistency
  make docs-all            Run all documentation checks

🚀 Release Tasks:
  make release-prepare     Interactive release preparation
  make release-version VERSION=x.y.z
  make release-notes VERSION=x.y.z
  make release-commit VERSION=x.y.z
  make release-tag VERSION=x.y.z
  make release-publish VERSION=x.y.z

🔧 Development Tasks:
  make build               Build C# project
  make test                Run unit tests
  make test-coverage       Run tests with coverage
  make clean               Clean build artifacts

🐳 Docker Tasks:
  make docker-build        Build Docker image
  make docker-test         Test Docker image

⚙️  Utility Tasks:
  make hooks-install       Install git hooks
  make status              Show project status
  make version-show        Show current versions
  make help                Show this help message
```

---

## 🚨 Troubleshooting

### Documentation check fails on push

The pre-push hook automatically runs documentation checks.

**To see detailed errors**:
```bash
make docs-all
```

**Common issues**:
1. **Missing Japanese translation**
   - Add corresponding file in `docs/ja/`

2. **Undocumented configuration key**
   - Add explanation in `docs/en/configuration.md`
   - Add Japanese translation in `docs/ja/configuration.md`

3. **New preset not documented**
   - Add section in `docs/en/presets.md`
   - Add Japanese translation in `docs/ja/presets.md`

---

### Version numbers out of sync

**Fix**:
```bash
make release-version VERSION=0.2.0
```

This updates all version numbers across the project.

---

### Need to skip checks temporarily

**Not recommended**, but if absolutely necessary:

```bash
git push --no-verify
```

⚠️ **Warning**: This skips all quality checks. Use only in emergencies.

---

## 🔄 Typical Development Workflow

```bash
# 1. Start working
make status                    # Check current state

# 2. Make changes to code and documentation

# 3. Before committing
make docs-check                # Quick check
make build test                # Build and test

# 4. Commit
git add .
git commit -m "feat: add new feature"

# 5. Push
git push                       # Pre-push hook runs automatically
```

---

## 📅 Release Workflow

```bash
# 1. Prepare release
make release-prepare           # Interactive: asks for version

# 2. Review changes
git diff
vim docs/RELEASE_NOTES_v0.2.0.md    # Edit release notes

# 3. Final check
make docs-full                 # Comprehensive documentation check
make test                      # Ensure all tests pass

# 4. Commit and tag
git add -A
git commit -m "chore: prepare release v0.2.0"
git tag v0.2.0

# 5. Publish
git push origin main
git push origin v0.2.0

# 6. Create GitHub Release
gh release create v0.2.0 --notes-file docs/RELEASE_NOTES_v0.2.0.md

# 7. Build and publish Docker
make docker-build
docker tag md2docx:latest forest6511/md2docx:0.2.0
docker push forest6511/md2docx:0.2.0
docker tag md2docx:latest forest6511/md2docx:latest
docker push forest6511/md2docx:latest
```

---

## 💡 Tips

### Check what version you're on

```bash
make version-show
```

### See project status

```bash
make status
```

### Re-install git hooks

```bash
make hooks-install
```

---

## 📚 More Information

- **Detailed documentation**: `internal-docs/ja/DOCUMENTATION_ARCHITECTURE.md`
- **Consistency checks**: `internal-docs/ja/CONSISTENCY_CHECKS.md`
- **Scripts README**: `scripts/README.md`
- **Makefile**: `Makefile` (run `make help`)

---

**Last Updated**: 2026-02-16
