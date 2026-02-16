# Docker Strategy - Docker Environment Strategy

## 🐳 Why Docker is Optimal

### 1. Complete Font Problem Resolution ⭐️

**Greatest Advantage**: Japanese fonts can be bundled in Docker containers

#### Problems (Without Docker)

```text
User A (Windows): No Noto Serif JP → Conversion fails
User B (Mac):     No Noto Serif JP → Substituted font → Different appearance
User C (Linux):   No Japanese fonts → Garbled characters
```text

#### Solution (With Docker)

```dockerfile
# Install fonts in Dockerfile
RUN apt-get install -y fonts-noto-cjk fonts-ipafont
# or
COPY fonts/ /usr/share/fonts/
```text

**Result**:

- ✅ Same fonts in all environments
- ✅ Reliable font embedding
- ✅ Users don't need to install fonts

---

### 2. Cross-Platform Support

| Environment | Without Docker | With Docker |
| ----------- | -------------------------------- | ------------------- |
| Windows | .NET required, separate fonts | Only `docker run` |
| Mac | .NET required, separate fonts | Only `docker run` |
| Linux | .NET required, separate fonts | Only `docker run` |

---

### 3. Complete Dependency Management

**Without Docker**:

```text
User: Install .NET 8.0...
User: Install NuGet packages...
User: Install Japanese fonts...
User: Wait, which ones? How?
```text

**With Docker**:

```bash
docker run -v $(pwd):/workspace md2word input.md -o output.docx
```text

---

### 4. Reproducibility Guarantee

- Same results in CI/CD environments
- Same behavior in development and production
- Almost zero "doesn't work" problems

---

## 📦 Docker Image Strategy

### Image Variants (3 Types)

#### 1. **Slim Version** (Minimal) - Recommended

```dockerfile
# Base: .NET Runtime
FROM mcr.microsoft.com/dotnet/runtime:8.0-slim

# Essential fonts only
RUN apt-get update && apt-get install -y \
    fonts-noto-cjk-jp \
    && rm -rf /var/lib/apt/lists/*

# Application
COPY bin/Release/net8.0/publish/ /app/
WORKDIR /app

ENTRYPOINT ["dotnet", "MarkdownToDocx.CLI.dll"]
```text

**Size**: ~300MB
**Use Case**: General usage
**Fonts**: Noto Sans/Serif JP (free, high quality)

---

#### 2. **Full Version** (All Fonts)

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:8.0

# Comprehensive Japanese fonts
RUN apt-get update && apt-get install -y \
    fonts-noto-cjk \
    fonts-ipafont \
    fonts-ipaexfont \
    fonts-takao \
    && rm -rf /var/lib/apt/lists/*

COPY bin/Release/net8.0/publish/ /app/
WORKDIR /app

ENTRYPOINT ["dotnet", "MarkdownToDocx.CLI.dll"]
```text

**Size**: ~500MB
**Use Case**: When diverse font requirements exist
**Fonts**: Noto, IPA, Takao, etc.

---

#### 3. **Premium Version** (Commercial Fonts Support)

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:8.0

# For user-provided commercial fonts
RUN mkdir -p /usr/share/fonts/custom
VOLUME /usr/share/fonts/custom

# Base fonts
RUN apt-get update && apt-get install -y fonts-noto-cjk

COPY bin/Release/net8.0/publish/ /app/
WORKDIR /app

ENTRYPOINT ["dotnet", "MarkdownToDocx.CLI.dll"]
```text

**Size**: ~300MB + user fonts
**Use Case**: Commercial font usage (Yu Mincho Pro, Hiragino, etc.)
**Usage**:

```bash
docker run -v /path/to/fonts:/usr/share/fonts/custom \
           -v $(pwd):/workspace \
           md2word:premium input.md -o output.docx
```text

---

## 🚀 Usage

### Basic Usage

```bash
# Pull image (first time only)
docker pull md2word/converter:latest

# Markdown → Word conversion
docker run --rm \
  -v $(pwd):/workspace \
  md2word/converter:latest \
  input.md -o output.docx
```text

### With Preset

```bash
docker run --rm \
  -v $(pwd):/workspace \
  md2word/converter:latest \
  input.md -o output.docx --preset technical
```text

### Custom Configuration

```bash
# Use custom YAML
docker run --rm \
  -v $(pwd):/workspace \
  -v $(pwd)/my-config.yaml:/app/config/custom/my-config.yaml \
  md2word/converter:latest \
  input.md -o output.docx --config custom/my-config.yaml
```text

### Batch Conversion

```bash
docker run --rm \
  -v $(pwd):/workspace \
  md2word/converter:latest \
  chapters/*.md -o book.docx --preset kdp-6x9
```text

---

## 📋 Fonts to Include in Docker Image

### Free Fonts (Recommended)

#### 1. **Noto Sans/Serif CJK JP** ⭐️ Top Priority

```dockerfile
RUN apt-get install -y fonts-noto-cjk-jp
```text

- Google-developed, high quality
- Both horizontal and vertical text support
- Commercial use allowed (SIL Open Font License)
- File size: ~40MB

#### 2. **IPA Fonts**

```dockerfile
RUN apt-get install -y fonts-ipafont fonts-ipaexfont
```text

- IPA Gothic, IPA Mincho
- Developed by Japanese government
- Commercial use allowed (IPA Font License)

#### 3. **Takao Fonts**

```dockerfile
RUN apt-get install -y fonts-takao
```text

- Derivative of IPA fonts
- Slightly bolder, more readable

---

### Font Embedding Configuration

**Important**: Font embedding is mandatory for KDP

```yaml
# config/publishing/kdp-*.yaml
font:
  family: "Noto Serif JP"  # Included in Docker image
  size: 11pt
  embed: true  # Must be true
```text

**DocumentFormat.OpenXml Implementation**:

```csharp
// Font embedding configuration
var fontTable = new Fonts();
var font = new Font()
{
    Name = "Noto Serif JP",
    EmbedRegular = new EmbedRegular()
    {
        FontKey = "{GUID}",
        Id = "rId1"
    }
};
fontTable.Append(font);
```text

---

## 🏗️ Docker Compose for Development

### docker-compose.yml

```yaml
version: '3.8'

services:
  converter:
    build:
      context: .
      dockerfile: Dockerfile
    volumes:
      - ./input:/workspace/input
      - ./output:/workspace/output
      - ./config:/app/config
    environment:
      - DOTNET_ENVIRONMENT=Production
    command: ["input/test.md", "-o", "output/test.docx", "--preset", "default"]

  # For development
  dev:
    build:
      context: .
      dockerfile: Dockerfile.dev
    volumes:
      - .:/app
      - /app/bin
      - /app/obj
    environment:
      - DOTNET_ENVIRONMENT=Development
    command: ["dotnet", "watch", "run"]

  # For testing
  test:
    build:
      context: .
      dockerfile: Dockerfile
    volumes:
      - ./tests:/workspace/tests
      - ./output:/workspace/output
    command: ["tests/*.md", "-o", "output/", "--preset", "default"]
```text

### Usage

```bash
# Run conversion
docker-compose up converter

# Development mode
docker-compose up dev

# Run tests
docker-compose up test
```text

---

## 🎯 Distribution Strategy

### 1. Docker Hub Distribution

```bash
# Tag
docker tag md2word:latest md2word/converter:latest
docker tag md2word:latest md2word/converter:1.0.0

# Push
docker push md2word/converter:latest
docker push md2word/converter:1.0.0

# User pulls
docker pull md2word/converter:latest
```text

---

### 2. GitHub Container Registry

```bash
# Tag
docker tag md2word:latest ghcr.io/your-org/md2word:latest

# Push
docker push ghcr.io/your-org/md2word:latest
```text

---

### 3. Multi-Architecture Support

```bash
# Support both AMD64 and ARM64
docker buildx build --platform linux/amd64,linux/arm64 \
  -t md2word/converter:latest \
  --push .
```text

**Benefits**:

- Intel Mac: amd64
- M1/M2 Mac: arm64
- Raspberry Pi: arm64
- x86 Linux: amd64

---

## 📊 Font Comparison Table

| Font | License | Size | Vertical | KDP | Docker Recommendation |
| ----------------- | ------------ | ---- | -------- | --- | ------------------------ |
| Noto Serif JP | SIL OFL | 40MB | ✅ | ✅ | ⭐️⭐️⭐️⭐️⭐️ |
| Noto Sans JP | SIL OFL | 35MB | ✅ | ✅ | ⭐️⭐️⭐️⭐️⭐️ |
| IPA Mincho | IPA License | 8MB | ✅ | ✅ | ⭐️⭐️⭐️⭐️ |
| IPA Gothic | IPA License | 8MB | ✅ | ✅ | ⭐️⭐️⭐️⭐️ |
| Takao Mincho | IPA License | 10MB | ✅ | ✅ | ⭐️⭐️⭐️ |
| Yu Mincho | Commercial | - | ✅ | ✅ | ❌ (Cannot include) |
| Hiragino Mincho | Commercial | - | ✅ | ✅ | ❌ (Cannot include) |

---

## 💡 Recommendation: Noto Serif JP as Standard

### Reasons

1. **Highest Quality**: Google-developed, professional quality
2. **Completely Free**: Commercial use, redistribution, embedding all OK
3. **Vertical Text Support**: Optimized for Japanese vertical writing
4. **KDP Compatible**: No font embedding issues
5. **Wide Support**: Adopted by many OSes

### Default Configuration

```yaml
# config/presets/default.yaml
font:
  family: "Noto Serif JP"  # Default
  fallback: "IPAMincho"    # Fallback
```text

### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:8.0-slim

# Install Noto CJK fonts
RUN apt-get update && \
    apt-get install -y \
    fonts-noto-cjk-jp \
    fontconfig && \
    fc-cache -fv && \
    rm -rf /var/lib/apt/lists/*

# Font list check (for debugging)
RUN fc-list | grep -i noto

COPY bin/Release/net8.0/publish/ /app/
WORKDIR /app

ENTRYPOINT ["dotnet", "MarkdownToDocx.CLI.dll"]
```text

---

## 🧪 Testing Strategy

### Font Embedding Verification

```bash
# Convert with Docker
docker run --rm \
  -v $(pwd):/workspace \
  md2word/converter:latest \
  test.md -o test.docx --preset kdp

# Verify font embedding (on Linux)
unzip test.docx -d test_docx/
cat test_docx/word/fontTable.xml
# → If <w:embedRegular> tag exists, it's OK
```text

---

## 🎁 User Experience

### Native Installation (Traditional)

```bash
# 1. Install .NET SDK
# 2. Install fonts
# 3. Install tool
dotnet tool install -g markdown-to-word

# 4. Usage
md2word input.md -o output.docx
```text

**Problems**:

- Complex environment setup
- Doesn't work without fonts
- Errors on Windows, garbled text on Mac, etc.

---

### Docker (Proposed)

```bash
# One-time: Pull image
docker pull md2word/converter:latest

# Ready to use
docker run --rm -v $(pwd):/workspace \
  md2word/converter:latest \
  input.md -o output.docx
```text

**Benefits**:

- ✅ Works immediately
- ✅ Same results everywhere
- ✅ Zero font problems

---

## 📦 Simplify with Aliases

### Bash Alias

```bash
# ~/.bashrc or ~/.zshrc
alias md2word='docker run --rm -v $(pwd):/workspace md2word/converter:latest'

# Usage
md2word input.md -o output.docx
md2word input.md -o output.docx --preset kdp
```text

### PowerShell Function (Windows)

```powershell
# $PROFILE
function md2word {
    docker run --rm -v ${PWD}:/workspace md2word/converter:latest @args
}

# Usage
md2word input.md -o output.docx
```text

---

## 🏆 Conclusion

### Docker Environment is **Strongly Recommended** ✅

**Reasons**:

1. **Complete Font Problem Resolution** (Greatest benefit)
   - Bundles Noto Serif JP
   - Reliable KDP font embedding
   - Zero environment differences

2. **Improved User Experience**
   - Easy installation
   - Works immediately
   - Fewer errors

3. **Maintainability**
   - Clear dependencies
   - High reproducibility
   - Excellent CI/CD compatibility

4. **Optimal for OSS Distribution**
   - Cross-platform
   - Same environment for everyone
   - Easy community contribution

### Implementation Priority

1. **Phase 1**: Docker image (Slim version, Noto Serif JP)
2. **Phase 2**: Docker Hub distribution
3. **Phase 3**: Multi-architecture support
4. **Phase 4**: Commercial font support (Premium)

---

## 📝 Next Steps

1. Create Dockerfile
2. Test font embedding
3. Prepare Docker Hub account
4. Set up CI/CD for automatic image builds
