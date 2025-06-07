# ----------- CONFIG -----------
$repoName  = "LastTravel"
$repoOwner = "mohsen-porrangi"
$repoUrl   = "https://github.com/$repoOwner/$repoName.git"
$currentDir = Get-Location
# ------------------------------

# Step 1: Clean up .git and .github
Write-Host "[INFO] Cleaning .git and .github folders..." -ForegroundColor Yellow
Remove-Item "$currentDir\.git" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "$currentDir\.github" -Recurse -Force -ErrorAction SilentlyContinue

# Step 2: Create README.md if folder is empty
if (-not (Get-ChildItem -File -Recurse | Where-Object { $_.Name -ne "reset-repo.ps1" })) {
    "## $repoName initialized." | Out-File "README.md" -Encoding utf8
    Write-Host "[INFO] Created README.md (folder was empty)" -ForegroundColor DarkGray
}

# Step 3: Initialize Git
git init
git add .
git commit -m "Initial commit"
git branch -M master

# Step 4: Try deleting repo (optional)
Write-Host "[INFO] Deleting GitHub repo (if it exists)..." -ForegroundColor Red
gh repo delete "$repoOwner/$repoName" --yes --confirm 2>$null

# Step 5: Create or reconnect to GitHub repo
$repoExists = gh repo view "$repoOwner/$repoName" 2>$null
if (-not $repoExists) {
    Write-Host "[INFO] Creating private GitHub repo '$repoName'..." -ForegroundColor Cyan
    gh repo create "$repoOwner/$repoName" --private
} else {
    Write-Host "[INFO] GitHub repo already exists. Skipping creation." -ForegroundColor Yellow
}

# Step 6: Set remote origin (replace if needed)
$remote = git remote get-url origin 2>$null
if ($remote) {
    git remote set-url origin $repoUrl
} else {
    git remote add origin $repoUrl
}

# Step 7: Push forcefully
Write-Host "[INFO] Pushing to GitHub..." -ForegroundColor Cyan
git push -u origin master --force

Write-Host "[✅ DONE] Repo '$repoName' pushed from root folder." -ForegroundColor Green
