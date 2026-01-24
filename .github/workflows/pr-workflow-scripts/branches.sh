#!/usr/bin/env bash
set -euo pipefail

# Extract commonly used fields from the PR payload
PR_AUTHOR=$(echo "$PR" | jq -r '.user.login')
BASE_OWNER=$(echo "$PR" | jq -r '.base.repo.owner.login')
BASE_REF=$(echo "$PR" | jq -r '.base.ref')
HEAD_REF=$(echo "$PR" | jq -r '.head.ref')

PR_NUMBER=$(echo "$PR" | jq -r '.number')
REPO_FULL_NAME=$(echo "$PR" | jq -r '.base.repo.full_name')

# If the PR author is jmcshane22, always pass
if [[ "$PR_AUTHOR" == "jmcshane22" ]]; then
  echo "branch_checks=true" >> "$GITHUB_OUTPUT"
  exit 0
fi

# Check required conditions
if [[ "$BASE_OWNER" == "jmcshane22" ]] \
   && [[ "$BASE_REF" == "dev" ]] \
   && [[ "$HEAD_REF" != "main" ]]; then
  echo "branch_checks=true" >> "$GITHUB_OUTPUT"
  exit 0
fi

# ---- Failed checks path ----
echo "branch_checks=false" >> "$GITHUB_OUTPUT"

COMMENT_BODY=$(
  cat <<'EOF'
Thanks for opening this pull request!

To keep our branching strategy consistent and protect release stability, the following rules are enforced on the repo:

• The base branch **must be `dev`**  
• The source branch **must not be `main`**  
• Contributions should come from a fork or a non-`main` branch in this repository  

This PR doesn’t currently meet those requirements and is being closed automatically due to failing this complience check.

Please update the branch configuration and reopen this PR, or create a new PR that follows these guidelines. If you believe this was closed in error, feel free to reach out to JD via ping on this PR.

Thanks for your contribution!
EOF
)

# Add compliance label
gh pr edit "$PR_NUMBER" \
  --repo "$REPO_FULL_NAME" \
  --add-label "compliance check failed"

# Post explanatory comment
gh pr comment "$PR_NUMBER" \
  --repo "$REPO_FULL_NAME" \
  --body "$COMMENT_BODY"

# Close the PR
gh pr close "$PR_NUMBER" \
  --repo "$REPO_FULL_NAME"

# Exit with 1 to signal failed check to the GitHub Action
echo "::error::Branch checks failed. Failing the job."
exit 1