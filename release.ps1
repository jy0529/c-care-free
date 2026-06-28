Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = "jy0529/c-care-free"
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$version = "1.0.0"
$tag = "v$version"
$zipPath = Join-Path $projectRoot "dist\CCareFree-v$version-win-x64.zip"
$notesPath = Join-Path $projectRoot "docs\release-notes-v$version.md"

if (-not (Test-Path $zipPath)) {
    & (Join-Path $projectRoot "publish.ps1")
}

$gh = Get-Command gh -ErrorAction SilentlyContinue
if (-not $gh) {
    throw "GitHub CLI (gh) not found. Run: winget install GitHub.cli"
}

gh auth status | Out-Null

$releaseExists = gh release view $tag --repo $repo 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "Uploading asset to existing release $tag..."
    gh release upload $tag $zipPath --repo $repo --clobber
}
else {
    Write-Host "Creating release $tag with asset..."
    gh release create $tag $zipPath `
        --repo $repo `
        --title "C盘无忧 $tag" `
        --notes-file $notesPath
}

Write-Host "Done: https://github.com/$repo/releases/tag/$tag"
