Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionPath = Join-Path $projectRoot "CDriveCleanupMaster.sln"
$appProject = Join-Path $projectRoot "src\CDriveCleanupMaster.App\CDriveCleanupMaster.App.csproj"

dotnet restore $solutionPath
dotnet run --project $appProject --no-restore
