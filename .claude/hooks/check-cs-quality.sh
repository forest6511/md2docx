#!/bin/bash
# PostToolUse hook: Lightweight C# quality check after Edit/Write
# Runs dotnet format --verify-no-changes on edited .cs files
# and provides feedback to Claude if issues are detected.

set -euo pipefail

# Read JSON input from stdin
INPUT=$(cat)

# Extract file_path from tool_input (Edit and Write both have file_path)
FILE_PATH=$(echo "$INPUT" | jq -r '.tool_input.file_path // empty')

# Only process .cs files
if [[ -z "$FILE_PATH" || "$FILE_PATH" != *.cs ]]; then
  exit 0
fi

# Skip auto-generated files
if [[ "$FILE_PATH" == *obj/* || "$FILE_PATH" == *bin/* ]]; then
  exit 0
fi

# Determine project root and solution path
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
SLN_PATH="$PROJECT_ROOT/csharp-version/src/MarkdownToDocx.sln"

if [ ! -f "$SLN_PATH" ]; then
  exit 0
fi

# Run dotnet format check on the specific file (fast, <5s)
FORMAT_OUTPUT=$(dotnet format "$SLN_PATH" --include "$FILE_PATH" --verify-no-changes 2>&1) || {
  # Format issues detected - provide feedback to Claude
  BASENAME=$(basename "$FILE_PATH")
  jq -n --arg file "$BASENAME" --arg output "$FORMAT_OUTPUT" '{
    hookSpecificOutput: {
      hookEventName: "PostToolUse",
      additionalContext: ("C# formatting issues in " + $file + ". Please fix: " + $output)
    }
  }'
  exit 0
}

# No issues found
exit 0
