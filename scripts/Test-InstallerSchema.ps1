[CmdletBinding()]
param([Parameter(Mandatory)][string]$ReleaseDirectory)
$ErrorActionPreference='Stop'
$projectRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$directory=[IO.Path]::GetFullPath($ReleaseDirectory)
$release=Get-Content -LiteralPath (Join-Path $directory 'release-manifest.json') -Raw | ConvertFrom-Json
if([string]$release.Version -notmatch '^\d+\.\d+\.\d+$'){throw 'Invalid release version.'}
$msi=Join-Path $directory (@($release.Files | Where-Object Name -like '*.msi')[0].Name)
$before=(Get-FileHash -LiteralPath $msi -Algorithm SHA256).Hash
& (Join-Path $PSScriptRoot 'Setup-InstallerTools.ps1')
$validator=Join-Path $projectRoot '.tools\wix-validation'
$output=Join-Path $projectRoot ('artifacts\installer-schema\'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $output -Force | Out-Null
$log=@(& (Join-Path $validator 'smoke.exe') -nologo -cub (Join-Path $validator 'darice.cub') $msi 2>&1 | ForEach-Object { $_.ToString() })
$exitCode=$LASTEXITCODE
$log | Set-Content -LiteralPath (Join-Path $output 'smoke.log') -Encoding utf8
$warnings=@($log | Where-Object {$_ -match '\bwarning SMOK\d+\s*:'})
$result=[ordered]@{
    MsiSHA256=$before; ValidatorVersion='3.14.1.8722'; ExitCode=$exitCode
    IceErrors=@($log | Where-Object {$_ -match '\berror SMOK\d+\s*:'})
    Warnings=$warnings; SuppressedICE=@()
    OriginalUnchanged=((Get-FileHash -LiteralPath $msi -Algorithm SHA256).Hash -eq $before)
}
$result | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $output 'result.json') -Encoding utf8
if(!$result.OriginalUnchanged){throw 'Schema validation altered the candidate MSI.'}
if($exitCode -ne 0 -or $result.IceErrors.Count){throw "Windows Installer schema validation failed. See $output"}
if($warnings.Count){throw "Unexpected installer validation warning. See $output"}
Write-Output "Windows Installer ICE validation passed with no errors, warnings or suppressions. Evidence: $output"
