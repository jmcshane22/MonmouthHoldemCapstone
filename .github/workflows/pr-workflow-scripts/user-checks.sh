#!/usr/bin/env bash
set -euo pipefail

PR_NUMBER=$(echo "$PR" | jq -r '.number')
REPO_FULL_NAME=$(echo "$PR" | jq -r '.base.repo.full_name')
PR_AUTHOR=$(echo "$PR" | jq -r '.user.login')

echo "Starting assignee/reviewer compliance check for PR #${PR_NUMBER}"

# -------------------------
# Helper: bot detection
# -------------------------
is_bot() {
  local user="$1"
  [[ -z "$user" || "$user" =~ \[bot\]$ ]]
}

# -------------------------
# Collect existing assignees
# -------------------------
mapfile -t CURRENT_ASSIGNEES < <(
  gh pr view "$PR_NUMBER" \
    --repo "$REPO_FULL_NAME" \
    --json assignees \
    -q '.assignees[].login'
)

# -------------------------
# Collect commit authors
# -------------------------
mapfile -t COMMIT_AUTHORS < <(
  gh pr view "$PR_NUMBER" \
    --repo "$REPO_FULL_NAME" \
    --json commits \
    -q '.commits[].authors[].login' \
  | sort -u
)

# Always consider PR author for assignment (unless bot)
DESIRED_ASSIGNEES=()

if ! is_bot "$PR_AUTHOR"; then
  DESIRED_ASSIGNEES+=("$PR_AUTHOR")
else
  echo "Skipping PR author (bot): $PR_AUTHOR"
fi

for author in "${COMMIT_AUTHORS[@]}"; do
  if is_bot "$author"; then
    echo "Skipping commit author (bot or unresolved): $author"
    continue
  fi
  DESIRED_ASSIGNEES+=("$author")
done

# Deduplicate
DESIRED_ASSIGNEES=($(printf "%s\n" "${DESIRED_ASSIGNEES[@]}" | sort -u))

ASSIGNEES_ADDED=()

for user in "${DESIRED_ASSIGNEES[@]}"; do
  if [[ ! " ${CURRENT_ASSIGNEES[*]} " =~ " ${user} " ]]; then
    echo "Attempting to add assignee: $user"
    if gh pr edit "$PR_NUMBER" --repo "$REPO_FULL_NAME" --add-assignee "$user" 2>/dev/null; then
      ASSIGNEES_ADDED+=("$user")
      echo "✔ Added assignee: $user"
    else
      echo "⚠ Could not add assignee: $user (skipping)"
    fi
  fi
done

# -------------------------
# Reviewer enforcement
# -------------------------
mapfile -t CURRENT_REVIEWERS < <(
  gh pr view "$PR_NUMBER" \
    --repo "$REPO_FULL_NAME" \
    --json reviewRequests \
    -q '.reviewRequests[].login'
)

REQUIRED_REVIEWERS=("Copilot")

if [[ "$PR_AUTHOR" == "jmcshane22" ]]; then
  REQUIRED_REVIEWERS+=("beastmode24jd")
else
  REQUIRED_REVIEWERS+=("jmcshane22")
fi

REVIEWERS_ADDED=()

for reviewer in "${REQUIRED_REVIEWERS[@]}"; do
  if [[ ! " ${CURRENT_REVIEWERS[*]} " =~ " ${reviewer} " ]]; then
    echo "Adding required reviewer: $reviewer"
    gh pr edit "$PR_NUMBER" \
      --repo "$REPO_FULL_NAME" \
      --add-reviewer "$reviewer"
    REVIEWERS_ADDED+=("$reviewer")
  fi
done

# -------------------------
# Comment summary
# -------------------------
COMMENT_BODY=$(
  cat <<EOF
👋 **PR Assignment & Review Automation Update**

This pull request was automatically checked for compliance with the repository’s PR standards.

**What was done:**
- Ensured assignees include the PR author and applicable commit authors  
  - Bot accounts and unresolved commit authors were intentionally skipped
$(if [[ ${#ASSIGNEES_ADDED[@]} -gt 0 ]]; then
    printf "  - Added assignees: %s\n" "$(IFS=", "; echo "${ASSIGNEES_ADDED[*]}")"
  else
    echo "  - No new assignees needed"
  fi)
- Ensured required reviewers are assigned
$(if [[ ${#REVIEWERS_ADDED[@]} -gt 0 ]]; then
    printf "  - Added reviewers: %s\n" "$(IFS=", "; echo "${REVIEWERS_ADDED[*]}")"
  else
    echo "  - All required reviewers were already assigned"
  fi)

This automation helps ensure accountability and consistent review coverage across the repository.  
If you believe someone was added in error or another reviewer should be included, feel free to update the PR.

Thanks!
EOF
)

echo $COMMENT_BODY
echo "\nPosting automation summary comment"
gh pr comment "$PR_NUMBER" \
  --repo "$REPO_FULL_NAME" \
  --body "$COMMENT_BODY"

echo "Assignee/reviewer compliance check completed."