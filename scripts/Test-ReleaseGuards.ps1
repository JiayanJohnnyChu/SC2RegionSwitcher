[CmdletBinding()]
param([Parameter(Mandatory)][string]$ReleaseDirectory,[string]$ExpectedVersion,[ValidatePattern('^[0-9a-fA-F]{40}$')][string]$ExpectedSourceCommit)
$ErrorActionPreference='Stop';$checker=Join-Path $PSScriptRoot 'Test-Package.ps1';$source=[IO.Path]::GetFullPath($ReleaseDirectory);$projectRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
. (Join-Path $PSScriptRoot 'Release-Common.ps1')
if(!$ExpectedVersion){$ExpectedVersion=(Get-ReleaseMetadata).Version}
if(!$ExpectedSourceCommit){$ExpectedSourceCommit=(& git -C $projectRoot rev-parse HEAD 2>$null|Out-String).Trim();if($LASTEXITCODE-ne0 -or $ExpectedSourceCommit-notmatch'^[0-9a-fA-F]{40}$'){throw 'Unable to resolve the expected source commit from git HEAD.'}}
function Check($dir,$version=$ExpectedVersion,$commit=$ExpectedSourceCommit){& $checker -ReleaseDirectory $dir -ExpectedVersion $version -ExpectedSourceCommit $commit|Out-Null}
function Reject($name,[scriptblock]$change,$version=$ExpectedVersion,$commit=$ExpectedSourceCommit){$case=Join-Path $root $name;Copy-Item -LiteralPath $source -Destination $case -Recurse;& $change $case;try{Check $case $version $commit}catch{Write-Output "PASS rejected $name";return};throw "Accepted invalid release case: $name"}
function Refresh-ManifestChecksum($case){$manifest=Join-Path $case 'release-manifest.json';$hash=(Get-FileHash -LiteralPath $manifest -Algorithm SHA256).Hash.ToLowerInvariant();$sum=Join-Path $case 'SHA256SUMS.txt';$lines=@(Get-Content -LiteralPath $sum|Where-Object{$_-notmatch'  release-manifest\.json$'});$lines+=($hash+'  release-manifest.json');$lines|Set-Content -LiteralPath $sum -Encoding ascii}
function Refresh-PackageMetadata($case,[string]$packageName){
 $manifestPath=Join-Path $case 'release-manifest.json';$manifest=Get-Content -Raw -LiteralPath $manifestPath|ConvertFrom-Json;$package=Join-Path $case $packageName;$entry=@($manifest.Files|Where-Object Name -eq $packageName)
 if($entry.Count-ne1){throw "Cannot refresh missing package metadata: $packageName"}
 $item=Get-Item -LiteralPath $package;$entry[0].Bytes=$item.Length;$entry[0].SHA256=(Get-FileHash -LiteralPath $package -Algorithm SHA256).Hash
 $manifest|ConvertTo-Json -Depth 6|Set-Content -LiteralPath $manifestPath -Encoding utf8
 $sum=Join-Path $case 'SHA256SUMS.txt';$lines=@(Get-Content -LiteralPath $sum|Where-Object{$_-notmatch('  '+[regex]::Escape($packageName)+'$')-and$_-notmatch'  release-manifest\.json$'})
 $lines+=(($entry[0].SHA256.ToLowerInvariant())+'  '+$packageName);$lines+=((Get-FileHash -LiteralPath $manifestPath -Algorithm SHA256).Hash.ToLowerInvariant()+'  release-manifest.json');$lines|Set-Content -LiteralPath $sum -Encoding ascii
}
function Reject-ApplicationRegistryKeyPath($case){
 $manifest=Get-Content -Raw -LiteralPath (Join-Path $case 'release-manifest.json')|ConvertFrom-Json;$msiName=@($manifest.Files|Where-Object Name -Like '*.msi')[0].Name;$msi=Join-Path $case $msiName
 $installer=New-Object -ComObject WindowsInstaller.Installer;$database=$installer.OpenDatabase($msi,1)
 try{$view=$database.OpenView("UPDATE ``Component`` SET ``Attributes``=260, ``KeyPath``='AppPath' WHERE ``Component``='Application'");try{[void]$view.Execute()}finally{[void]$view.Close();[Runtime.InteropServices.Marshal]::FinalReleaseComObject($view)|Out-Null};[void]$database.Commit()}finally{[Runtime.InteropServices.Marshal]::FinalReleaseComObject($database)|Out-Null;[Runtime.InteropServices.Marshal]::FinalReleaseComObject($installer)|Out-Null}
 Refresh-PackageMetadata $case $msiName
 try{Check $case}catch{if($_.Exception.Message-ne'Component key path or attributes are incorrect.'){throw "Application registry-key-path case was rejected for the wrong reason: $($_.Exception.Message)"};Write-Output 'PASS rejected application-registry-key-path';return}
 throw 'Accepted invalid release case: application-registry-key-path'
}
function Reject-InstallerScope([string]$name,[string]$sql,[string]$expectedError){
 $case=Join-Path $root $name;Copy-Item -LiteralPath $source -Destination $case -Recurse
 $manifest=Get-Content -Raw -LiteralPath (Join-Path $case 'release-manifest.json')|ConvertFrom-Json;$msiName=@($manifest.Files|Where-Object Name -Like '*.msi')[0].Name
 $installer=New-Object -ComObject WindowsInstaller.Installer;$db=$installer.OpenDatabase((Join-Path $case $msiName),1)
 try{$view=$db.OpenView($sql);try{[void]$view.Execute()}finally{[void]$view.Close();[Runtime.InteropServices.Marshal]::FinalReleaseComObject($view)|Out-Null};[void]$db.Commit()}finally{[Runtime.InteropServices.Marshal]::FinalReleaseComObject($db)|Out-Null;[Runtime.InteropServices.Marshal]::FinalReleaseComObject($installer)|Out-Null}
 Refresh-PackageMetadata $case $msiName
 try{Check $case}catch{if($_.Exception.Message-ne$expectedError){throw "$name rejected for the wrong reason: $($_.Exception.Message)"};Write-Output "PASS rejected $name";return}
 throw "Accepted invalid release case: $name"
}
Check $source
$root=Join-Path $projectRoot ('artifacts\release-guard-tests\'+(Get-Date -Format yyyyMMdd-HHmmss)+'-'+[guid]::NewGuid().ToString('N').Substring(0,8));New-Item -ItemType Directory -Path $root -Force|Out-Null
Reject 'tampered-zip' {param($case)$m=Get-Content -Raw (Join-Path $case release-manifest.json)|ConvertFrom-Json;$p=Join-Path $case (@($m.Files|Where-Object Name -Like '*.zip')[0].Name);[IO.File]::AppendAllText($p,'tamper')}
Reject 'wrong-expected-version' {param($case)} '9.9.9' $ExpectedSourceCommit
Reject 'wrong-expected-source' {param($case)} $ExpectedVersion ('0'*40)
Reject 'manifest-version-mismatch' {param($case)$p=Join-Path $case release-manifest.json;$m=Get-Content -Raw $p|ConvertFrom-Json;$m.Version='9.9.9';$m|ConvertTo-Json -Depth 6|Set-Content -LiteralPath $p -Encoding utf8;Refresh-ManifestChecksum $case}
if([version]$ExpectedVersion-ge[version]'3.4.1'){$case=Join-Path $root 'application-registry-key-path';Copy-Item -LiteralPath $source -Destination $case -Recurse;Reject-ApplicationRegistryKeyPath $case}
if([version]$ExpectedVersion-ge[version]'3.4.1'){
 Reject-InstallerScope 'missing-machine-scope' "DELETE FROM ``Property`` WHERE ``Property``='ALLUSERS'" 'Machine installation properties are incorrect.'
 Reject-InstallerScope 'missing-preview-migration-guard' "DELETE FROM ``LaunchCondition`` WHERE ``Condition``='Installed OR NOT LEGACYUSERINSTALL'" 'Machine scope or preview migration condition is missing.'
 Reject-InstallerScope 'missing-appsearch-signature-table' 'DROP TABLE `Signature`' 'AppSearch Signature table is missing.'
}
Write-Output 'Release guard negative tests passed: tampering, version mismatch, source mismatch and current component rules were enforced.'
Write-Output "Evidence retained: $root"
