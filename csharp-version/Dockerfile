# Markdown to Word Converter - Docker Image
# Base image: .NET 8.0 Runtime (Slim)

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy project files and restore dependencies
COPY src/*.sln .
COPY src/MarkdownToDocx.Core/*.csproj ./MarkdownToDocx.Core/
COPY src/MarkdownToDocx.Styling/*.csproj ./MarkdownToDocx.Styling/
COPY src/MarkdownToDocx.CLI/*.csproj ./MarkdownToDocx.CLI/
RUN dotnet restore

# Copy source code and build
COPY src/ .
RUN dotnet publish MarkdownToDocx.CLI/MarkdownToDocx.CLI.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0-slim
WORKDIR /app

# Install Japanese fonts (Noto CJK)
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    # Noto Sans/Serif CJK JP (High quality, SIL OFL License)
    fonts-noto-cjk-jp \
    # IPA fonts (Backup)
    fonts-ipafont-mincho \
    fonts-ipafont-gothic \
    # Font cache utilities
    fontconfig && \
    # Update font cache
    fc-cache -fv && \
    # Cleanup (Reduce image size)
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Verify installed fonts (viewable in build logs)
RUN echo "=== Installed Fonts ===" && \
    fc-list | grep -E "(Noto|IPA)" | sort

# Copy application files
COPY --from=build /app/publish .

# Copy configuration files
COPY config/ /app/config/

# Set working directory to mount point
WORKDIR /workspace

# Entrypoint
ENTRYPOINT ["dotnet", "/app/MarkdownToDocx.CLI.dll"]

# Default command (show help)
CMD ["--help"]

# Labels (metadata)
LABEL maintainer="your-email@example.com"
LABEL version="1.0.0"
LABEL description="Markdown to Word Converter with Japanese font support"
LABEL license="MIT"

# Health check (optional)
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD dotnet /app/MarkdownToDocx.CLI.dll --version || exit 1
