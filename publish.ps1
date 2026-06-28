Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$appProject = Join-Path $projectRoot "src\CDriveCleanupMaster.App\CDriveCleanupMaster.App.csproj"
$distDir = Join-Path $projectRoot "dist"
$version = "1.0.0"
$rid = "win-x64"
$publishDir = Join-Path $projectRoot "src\CDriveCleanupMaster.App\bin\Release\net8.0-windows\$rid\publish"
$zipName = "CCareFree-v$version-$rid.zip"
$zipPath = Join-Path $distDir $zipName

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    $fallback = Join-Path (Split-Path -Parent (Split-Path -Parent $projectRoot)) "work\.dotnet\dotnet.exe"
    if (Test-Path $fallback) {
        $dotnet = Get-Item $fallback
    }
    else {
        throw "dotnet not found. Install .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0"
    }
}
$dotnetExe = if ($dotnet -is [System.Management.Automation.ApplicationInfo] -or $dotnet.GetType().Name -eq 'CommandInfo') {
    $dotnet.Source
} else {
    $dotnet.FullName
}

if (Test-Path $distDir) {
    Remove-Item $distDir -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $distDir | Out-Null

Write-Host "Publishing CCareFree $version ($rid)..."
& $dotnetExe publish $appProject `
    -c Release `
    -r $rid `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:DebugType=none `
    -p:DebugSymbols=false

if (-not (Test-Path (Join-Path $publishDir "CCareFree.exe"))) {
    throw "Publish failed: CCareFree.exe not found in $publishDir"
}

Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath -Force

Write-Host ""
Write-Host "Published:"
Write-Host "  EXE: $(Join-Path $publishDir 'CCareFree.exe')"
Write-Host "  ZIP: $zipPath"
