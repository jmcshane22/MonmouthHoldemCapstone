#!/usr/bin/env bash
set -euo pipefail

PR_NUMBER=$(jq -r '.number' <<< "$PR")
REPO_FULL_NAME=$(jq -r '.base.repo.full_name' <<< "$PR")

# Get existing labels
EXISTING_LABELS=$(gh pr view "$PR_NUMBER" --repo "$REPO_FULL_NAME" --json labels -q '.labels[].name')

LABELS_TO_REMOVE=()
for label in "needs attention" "compliance check failed"; do
if grep -qFx "$label" <<< "$EXISTING_LABELS"; then
LABELS_TO_REMOVE+=("$label")
fi
done

if [ ${#LABELS_TO_REMOVE[@]} -gt 0 ]; then
echo "Removing labels: ${LABELS_TO_REMOVE[*]}"
gh pr edit "$PR_NUMBER" --repo "$REPO_FULL_NAME" --remove-label "$(IFS=,; echo "${LABELS_TO_REMOVE[*]}")"
else
echo "No labels to remove"
fi