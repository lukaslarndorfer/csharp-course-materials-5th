#!/usr/bin/env bash

# Capture the name of this script
script_name="$(basename "$0")"

# Step 1: Initialize repo, add everything except the script, commit "initial"
git init
git add .
# Remove this script from staging if it exists in the current folder
if [ -f "$script_name" ]; then
  git rm --cached "$script_name"
fi

git commit -m "initial"

# Step 2: Fetch organizations and display selection list
echo "Fetching your GitHub organizations..."
ORGS=$(gh api user/orgs --jq '.[].login' 2>/dev/null)

# Convert the multiline string into an array
IFS=$'\n' read -r -d '' -a orgList <<< "$ORGS"$'\n'

if [ ${#orgList[@]} -eq 0 ]; then
  echo "No organizations found, or you are not authenticated."
  exit 1
fi

echo "Available organizations:"
PS3="Please select an organization: "

select ORG in "${orgList[@]}"; do
  if [ -n "$ORG" ]; then
    echo "You selected '$ORG'"
    break
  else
    echo "Invalid choice. Please select a valid organization number."
  fi
done

# Step 3: Prompt for new repository name
read -p "Enter the new repository name: " REPO

# Step 4: Create a new GitHub repo in that org
# Pass any extra argument (e.g., -y) to skip the "confirm" prompt
gh repo create "$ORG/$REPO" --private -y

# Add as remote and push (assuming default branch is 'master')
git remote add origin "git@github.com:$ORG/$REPO.git"
git push -u origin master

# Step 5: Mark the new repo as a template
gh repo edit "$ORG/$REPO" --template=true

# Retrieve the new repository URL (browser link)
# and display it so you can click or open it easily
REPO_URL=$(gh repo view "$ORG/$REPO" --json url -q .url)

echo "Repository '$REPO' created under organization '$ORG' and marked as a template."
echo "You can view it here: $REPO_URL"