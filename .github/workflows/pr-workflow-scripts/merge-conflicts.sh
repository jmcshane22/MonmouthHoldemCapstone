#!/usr/bin/env bash
set -euo pipefail

# Extract required fields from the PR payload
PR_NUMBER=$(echo "$PR" | jq -r '.number')
REPO_FULL_NAME=$(echo "$PR" | jq -r '.base.repo.full_name')

# mergeable can be: true, false, or null
MERGEABLE=$(echo "$PR" | jq -r '.mergeable')
MERGEABLE_STATE=$(echo "$PR" | jq -r '.mergeable_state')

# If GitHub hasn't computed mergeability yet, exit gracefully
if [[ "$MERGEABLE" == "null" || "$MERGEABLE" == "true" ]]; then
    # Add labels for visibility
  gh pr edit "$PR_NUMBER" \
    --repo "$REPO_FULL_NAME" \
    --remove-label "conflicts detected"
  exit 0
fi

# Only act when merge conflicts are present
if [[ "$MERGEABLE" == "false" && "$MERGEABLE_STATE" == "dirty" ]]; then
  COMMENT_BODY=$(
    cat <<'EOF'
Hey there! 👋

This pull request currently has **merge conflicts**, which means it can’t be merged in its current state.

I’ve marked the PR as **Draft** for now. Please resolve the conflicts locally, push the fixes, and then mark the PR **Ready for Review** once everything is clean.

If you run into trouble or want a second set of eyes, feel free to ping **JD** — happy to help 👍  
That said, the PR can’t be marked *Ready for Review* until all conflicts are resolved.

Thanks, and looking forward to the update!
EOF
  )

  # Add labels for visibility
  gh pr edit "$PR_NUMBER" \
    --repo "$REPO_FULL_NAME" \
    --add-label "needs attention,conflicts detected,compliance check failed"

  # Mark PR as draft
  gh pr ready "$PR_NUMBER" \
    --repo "$REPO_FULL_NAME" \
    --undo

  # Leave a comment explaining next steps
  gh pr comment "$PR_NUMBER" \
    --repo "$REPO_FULL_NAME" \
    --body "$COMMENT_BODY"
fi