```bash
git switch -c feature/<x>-<xx>
add .
git commit -m "<xxx>"
gh pr create --base main --title "<xxx>" --body "Closes #<x>"
gh pr merge --merge --delete-branch --body "Closes #<x>"
```