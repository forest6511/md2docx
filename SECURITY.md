# Security Policy

## 🔒 Security for Open Source Project

This project is **open source** and publicly accessible. Security is paramount.

---

## 🚨 Reporting a Vulnerability

**DO NOT** open a public GitHub issue for security vulnerabilities.

Instead:

1. **Email**: security@your-domain.com
2. **Subject**: [SECURITY] Brief description
3. **Include**:
   - Description of the vulnerability
   - Steps to reproduce
   - Potential impact
   - Suggested fix (if any)

**Response Time**:

- Initial response: Within 48 hours
- Fix timeline: Depends on severity
  - Critical: 24-48 hours
  - High: 1 week
  - Medium: 2 weeks
  - Low: Best effort

---

## 🛡️ Security Measures

### Code Review

- **Mandatory Codex Review**: All code changes reviewed by Codex before merge
- **Pre-Push Hook**: Automated security checks in `.claude/hooks/pre-push.sh`
- **Dependency Scanning**: Regular dependency vulnerability scans

### Input Validation

- **File Paths**: All file paths sanitized and validated
- **YAML Parsing**: Safe YAML loading (no code execution)
- **Markdown Input**: Sanitized to prevent injection attacks
- **User Input**: All CLI inputs validated

### Dependencies

- **Minimal Dependencies**: Only essential, well-maintained packages
- **License Compliance**: All dependencies MIT-compatible
- **Regular Updates**: Monthly dependency security updates

### Docker Security

- **Base Images**: Official Microsoft .NET images only
- **No Secrets**: Never embed secrets in images
- **Minimal Layers**: Reduce attack surface
- **User Permissions**: Run as non-root user where possible

---

## ⚠️ Known Limitations

### File System Access

- Converter requires read access to input files
- Converter requires write access to output directory
- **Mitigation**: Always run with minimal necessary permissions

### YAML Configuration

- Custom YAML files are executed
- **Mitigation**: Validate YAML schema, no code execution allowed

### Font Files

- Docker images include open-source fonts only
- User-provided fonts: **Use at your own risk**
- **Mitigation**: Document supported fonts, warn about commercial fonts

---

## 🔐 Security Best Practices for Users

### Running the Converter

```bash
# ✅ Good: Minimal permissions
docker run --rm -v $(pwd)/input:/workspace/input:ro \
                -v $(pwd)/output:/workspace/output \
                md2word:latest input.md -o output.docx

# ❌ Bad: Full filesystem access
docker run --rm -v /:/host md2word:latest ...
```text

### Custom YAML Configurations

```yaml
# ✅ Safe: Style configuration only
styles:
  h1:
    size: 24pt
    color: "FF0000"

# ❌ Unsafe: Never include secrets
secret_key: "abc123"  # DON'T DO THIS
api_token: "xyz789"   # DON'T DO THIS
```text

### Commercial Fonts

```bash
# ✅ Safe: Use open-source fonts (included)
docker run md2word:latest input.md -o output.docx

# ⚠️ Caution: User-provided fonts (license compliance required)
docker run -v /path/to/fonts:/usr/share/fonts/custom md2word:premium ...
```text

---

## 🔍 Security Checklist for Contributors

Before submitting code:

- [ ] No hardcoded secrets or credentials
- [ ] All file paths validated and sanitized
- [ ] No SQL, code, or command injection vulnerabilities
- [ ] Dependencies up to date with no known CVEs
- [ ] Codex security review passed
- [ ] Unit tests include security test cases
- [ ] Documentation updated if security-relevant changes

---

## 📋 Security Audit Log

### Planned Security Audits

| Version | Date | Type | Status |
| --------- | ------ |------|--------|
| v0.1.0 | March 2026 | Internal | Planned |
| v0.5.0 | May 2026 | Internal | Planned |
| v1.0.0 | July 2026 | External | Planned |

### Security Fixes

None yet (pre-release).

---

## 🛠️ Automated Security Tools

### Pre-Push Hook Security Checks

```bash
# .claude/hooks/pre-push.sh includes:

1. Codex security review
2. Sensitive data detection
3. Dependency vulnerability scan
4. YAML validation
5. Dockerfile security check
```text

### Dependency Scanning

```bash
# Run manually or in CI/CD
dotnet list package --vulnerable
dotnet list package --outdated
```text

### Docker Image Scanning

```bash
# Trivy scan
trivy image md2word:latest

# Docker scan
docker scan md2word:latest
```text

---

## 🚫 Out of Scope

### Not Security Issues

- Feature requests
- Performance issues (unless DoS-related)
- UI/UX bugs
- Documentation typos

### Third-Party Issues

- Vulnerabilities in Word/LibreOffice
- Docker engine vulnerabilities
- .NET runtime vulnerabilities

**Action**: Report to respective projects, not here.

---

## 📚 Security Resources

### Secure Coding Guidelines

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [.NET Security Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/security/)
- [Docker Security Best Practices](https://docs.docker.com/engine/security/)

### Security Tools

- [Codex](https://code.claude.com/docs/mcp/codex) - Automated code review
- [Trivy](https://github.com/aquasecurity/trivy) - Container vulnerability scanner
- [OWASP Dependency-Check](https://owasp.org/www-project-dependency-check/)

---

## 🏆 Security Hall of Fame

Thank you to security researchers who responsibly disclose vulnerabilities:

(None yet - project is pre-release)

---

## 📞 Contact

- **Security Email**: security@your-domain.com
- **PGP Key**: [Link to public key]
- **Response Time**: 48 hours maximum

---

**Last Updated**: 2026-02-14
**Version**: 1.0
**Next Review**: Before v1.0 release
