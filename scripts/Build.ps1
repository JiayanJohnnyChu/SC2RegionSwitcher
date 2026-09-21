[CmdletBinding()]
param(
    [ValidateSet('Debug','Release')][string]$Configuration='Release',
    [string]$DotNet
)
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'Common.ps1')
$projectRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
Push-Location $projectRoot
try {
    Invoke-ProjectDotNet -DotNet $DotNet -Arguments @('build','SC2RegionSwitcher.slnx','--configuration',$Configuration,'--nologo')
} finally {Pop-Location}
