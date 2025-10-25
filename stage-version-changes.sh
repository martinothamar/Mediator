#!/bin/sh

# Stage files where the only changes are GeneratedCode attribute version updates

git diff --name-only | while read -r file; do
    # Count total lines changed (excluding diff metadata +++ and ---)
    total_changes=$(git diff "$file" | grep -E '^[+\-]' | grep -v -E '^[+\-]{3}' | wc -l)

    # Count GeneratedCode attribute lines changed (with version numbers)
    generated_changes=$(git diff "$file" | grep -E '^[+\-].*GeneratedCode.*"[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+"' | wc -l)

    # If all changes are GeneratedCode version changes, stage the file
    if [ "$total_changes" -gt 0 ] && [ "$total_changes" -eq "$generated_changes" ]; then
        echo "Staging $file"
        git add "$file"
    fi
done

echo "Done!"
