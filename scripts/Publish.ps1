[CmdletBinding()]
param([string]$DotNet)
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'Common.ps1')
$projectRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$publishDirectory=Join-Path $projectRoot 'artifacts\publish\win-x64'
Push-Location $projectRoot
try {
    Invoke-ProjectDotNet -DotNet $DotNet -Arguments @('publish','src\SC2Switcher.Wpf\SC2Switcher.Wpf.csproj','--configuration','Release','--self-contained','false','--output',$publishDirectory,'--nologo')
    Write-Output "Published application: $publishDirectory"
} finally {Pop-Location}
