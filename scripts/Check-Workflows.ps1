[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$projectRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$executable=Join-Path $projectRoot '.tools\actionlint\actionlint.exe'
if(!(Test-Path -LiteralPath $executable)){throw 'Run scripts/Setup-WorkflowTools.ps1 first.'}
$workflows=@(Get-ChildItem -LiteralPath (Join-Path $projectRoot '.github\workflows') -File -Filter '*.yml' | ForEach-Object FullName)
if(!$workflows.Count){throw 'No workflows found.'}
Push-Location $projectRoot
try {
    & $executable '-shellcheck=' '-pyflakes=' @workflows
    if($LASTEXITCODE -ne 0){throw 'GitHub workflow validation failed.'}
    Write-Output "Workflow syntax and expression checks passed ($($workflows.Count) workflows)."
} finally {Pop-Location}
