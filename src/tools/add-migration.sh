#!/usr/bin/env bash
# Adds an EF Core migration for every supported database provider, so the
# SqlServer and Postgres migration sets never drift apart.
# Usage: src/tools/add-migration.sh <MigrationName> <ApplicationDbContext|CacheDbContext>
set -euo pipefail

name="${1:?Usage: $0 <MigrationName> <ApplicationDbContext|CacheDbContext>}"
context="${2:?Usage: $0 <MigrationName> <ApplicationDbContext|CacheDbContext>}"

case "$context" in
  ApplicationDbContext) outdir="Migrations" ;;
  CacheDbContext)       outdir="Migrations/Cache" ;;
  *) echo "Unknown context '$context'" >&2; exit 1 ;;
esac

src="$(cd "$(dirname "$0")/.." && pwd)"
dotnet build "$src/MH.Capstone.WebApp" -v q

for provider in SqlServer Postgres; do
  echo "==> $provider: adding '$name' for $context"
  Database__Provider="$provider" dotnet ef migrations add "$name" --no-build \
    -p "$src/MH.Capstone.Migrations.$provider" -s "$src/MH.Capstone.WebApp" \
    -c "$context" -o "$outdir"
done
