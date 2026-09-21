[CmdletBinding()]
param([Parameter(Mandatory)][string]$ReleaseDirectory,[string]$ExpectedVersion,[ValidatePattern('^[0-9a-fA-F]{40}$')][string]$ExpectedSourceCommit)
$ErrorActionPreference='Stop';$checker=Join-Path $PSScriptRoot 'Test-Package.ps1';$source=[IO.Path]::GetFullPath($ReleaseDirectory);$projectRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
. (Join-Path $PSScriptRoot 'Release-Common.ps1')
if(!$ExpectedVersion){$ExpectedVersion=(Get-ReleaseMetadata).Version}
if(!$ExpectedSourceCommit){$ExpectedSourceCommit=(& git -C $projectRoot rev-parse HEAD 2>$null|Out-String).Trim();if($LASTEXITCODE-ne0 -or $ExpectedSourceCommit-notmatch'^[0-9a-fA-F]{40}$'){throw 'Unable to resolve the expected source commit from git HEAD.'}}
function Check($dir,$version=$ExpectedVersion,$commit=$ExpectedSourceCommit){& $checker -ReleaseDirectory $dir -ExpectedVersion $version -ExpectedSourceCommit $commit|Out-Null}
function Reject($name,[scriptblock]$change,$version=$ExpectedVersion,$commit=$ExpectedSourceCommit){$case=Join-Path $root $name;Copy-Item -LiteralPath $source -Destination $case -Recurse;& $change $case;try{Check $case $version $commit}catch{Write-Output "PASS rejected $name";return};throw "Accepted invalid release case: $name"}
function Refresh-ManifestChecksum($case){$manifest=Join-Path $case 'release-manifest.json';$hash=(Get-FileHash -LiteralPath $manifest -Algorithm SHA256).Hash.ToLowerInvariant();$sum=Join-Path $case 'SHA256SUMS.txt';$lines=@(Get-Content -LiteralPath $sum|Where-Object{$_-notmatch'  release-manifest\.json$'});$lines+=($hash+'  release-manifest.json');$lines|Set-Content -LiteralPath $sum -Encoding ascii}
Check $source
$root=Join-Path $projectRoot ('artifacts\release-guard-tests\'+(Get-Date -Format yyyyMMdd-HHmmss)+'-'+[guid]::NewGuid().ToString('N').Substring(0,8));New-Item -ItemType Directory -Path $root -Force|Out-Null
Reject 'tampered-zip' {param($case)$m=Get-Content -Raw (Join-Path $case release-manifest.json)|ConvertFrom-Json;$p=Join-Path $case (@($m.Files|Where-Object Name -Like '*.zip')[0].Name);[IO.File]::AppendAllText($p,'tamper')}
Reject 'wrong-expected-version' {param($case)} '9.9.9' $ExpectedSourceCommit
Reject 'wrong-expected-source' {param($case)} $ExpectedVersion ('0'*40)
Reject 'manifest-version-mismatch' {param($case)$p=Join-Path $case release-manifest.json;$m=Get-Content -Raw $p|ConvertFrom-Json;$m.Version='9.9.9';$m|ConvertTo-Json -Depth 6|Set-Content -LiteralPath $p -Encoding utf8;Refresh-ManifestChecksum $case}
Write-Output 'Release guard negative tests passed: tampering, version mismatch and source mismatch were rejected.'
Write-Output "Evidence retained: $root"
