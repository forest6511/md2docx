# Multi-stage build for md2docx
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY csharp-version/src/MarkdownToDocx.Core/*.csproj ./MarkdownToDocx.Core/
COPY csharp-version/src/MarkdownToDocx.Styling/*.csproj ./MarkdownToDocx.Styling/
COPY csharp-version/src/MarkdownToDocx.CLI/*.csproj ./MarkdownToDocx.CLI/

# Restore dependencies
RUN dotnet restore MarkdownToDocx.CLI/MarkdownToDocx.CLI.csproj

# Copy source code
COPY csharp-version/src/MarkdownToDocx.Core/ ./MarkdownToDocx.Core/
COPY csharp-version/src/MarkdownToDocx.Styling/ ./MarkdownToDocx.Styling/
COPY csharp-version/src/MarkdownToDocx.CLI/ ./MarkdownToDocx.CLI/

# Build and publish
WORKDIR /src/MarkdownToDocx.CLI
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app

# Install fonts
RUN apt-get update && apt-get install -y --no-install-recommends \
    fonts-noto-core \
    fonts-noto-cjk \
    fonts-noto-mono \
    && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Copy presets
COPY config/presets/ /app/config/presets/
COPY config/vertical/ /app/config/vertical/

# Create workspace directory
WORKDIR /workspace
VOLUME /workspace

# Set entrypoint
ENTRYPOINT ["dotnet", "/app/md2docx.dll"]
CMD ["--help"]
