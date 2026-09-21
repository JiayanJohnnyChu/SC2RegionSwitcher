[CmdletBinding()]
param(
    [ValidateSet('Debug','Release')][string]$Configuration='Release',
    [string]$DotNet
)
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'Common.ps1')
$projectRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$testProject=Join-Path $projectRoot 'tests\SC2Switcher.Tests\SC2Switcher.Tests.csproj'
$testDll=Join-Path $projectRoot "tests\SC2Switcher.Tests\bin\$Configuration\net10.0-windows\SC2Switcher.Tests.dll"
$runName=(Get-Date -Format 'yyyyMMdd-HHmmss')+'-'+[guid]::NewGuid().ToString('N').Substring(0,8)
$runDirectory=Join-Path $projectRoot "artifacts\tests\$runName"
Push-Location $projectRoot
try {
    Invoke-ProjectDotNet -DotNet $DotNet -Arguments @('build',$testProject,'--configuration',$Configuration,'--nologo')
    New-Item -ItemType Directory -Path $runDirectory | Out-Null
    Invoke-ProjectDotNet -DotNet $DotNet -Arguments @($testDll,(Join-Path $runDirectory 'fixtures')) 2>&1 | Tee-Object -FilePath (Join-Path $runDirectory 'results.log')
    Write-Output "Isolated test results: $runDirectory"
} finally {Pop-Location}
