Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionPath = Join-Path $projectRoot "CDriveCleanupMaster.sln"

dotnet restore $solutionPath
dotnet build $solutionPath --no-restore -v minimal
dotnet test $solutionPath --no-build -v minimal
