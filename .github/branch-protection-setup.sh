#!/bin/bash
# GitHub Branch Protection Rules Setup
# Protects main and dev branches to require PR reviews
# Allows admins to bypass review requirements

set -e

REPO="forest6511/md2docx"
BRANCHES=("main" "dev")

echo "🔒 Setting up branch protection rules for ${REPO}..."
echo ""

for BRANCH in "${BRANCHES[@]}"; do
  echo "Protecting branch: ${BRANCH}"

  # Create protection configuration JSON
  cat > /tmp/branch-protection.json <<EOF
{
  "required_status_checks": null,
  "enforce_admins": false,
  "required_pull_request_reviews": {
    "required_approving_review_count": 1,
    "dismiss_stale_reviews": true,
    "require_code_owner_reviews": false
  },
  "restrictions": null,
  "required_linear_history": false,
  "allow_force_pushes": false,
  "allow_deletions": false,
  "block_creations": false,
  "required_conversation_resolution": true,
  "lock_branch": false,
  "allow_fork_syncing": false
}
EOF

  # Apply branch protection
  gh api \
    --method PUT \
    -H "Accept: application/vnd.github+json" \
    -H "X-GitHub-Api-Version: 2022-11-28" \
    "/repos/${REPO}/branches/${BRANCH}/protection" \
    --input /tmp/branch-protection.json || {
      echo "⚠️  Failed to protect ${BRANCH}"
      continue
    }

  echo "✅ ${BRANCH} branch protected"
  echo ""
done

# Cleanup
rm -f /tmp/branch-protection.json

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "✅ Branch protection rules configured"
echo ""
echo "📋 Protection Details:"
echo "   - Require 1 approving review before merging (for non-admins)"
echo "   - Admins can merge without approval"
echo "   - Dismiss stale reviews when new commits pushed"
echo "   - Require conversation resolution before merging"
echo "   - Prevent force pushes"
echo "   - Prevent branch deletion"
echo ""
echo "🔍 Verify settings:"
echo "   https://github.com/${REPO}/settings/branches"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
