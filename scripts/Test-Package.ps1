[CmdletBinding()]
param([Parameter(Mandatory)][string]$ReleaseDirectory,[string]$ExpectedVersion,[string]$ExpectedSourceCommit,[string]$ExpectedMsiSHA256,[string]$ExpectedZipSHA256)
$ErrorActionPreference='Stop'
function Require([bool]$ok,[string]$message){if(!$ok){throw $message}}
$directory=[IO.Path]::GetFullPath($ReleaseDirectory)
$manifestPath=Join-Path $directory 'release-manifest.json'
$manifest=Get-Content -Raw -LiteralPath $manifestPath|ConvertFrom-Json
Require ($manifest.SchemaVersion -eq 2) 'Unsupported release manifest schema.'
$version=[string]$manifest.Version
Require ($version -match '^(\d+)\.(\d+)\.(\d+)$') 'Version must be a three-part numeric MSI version.'
Require ([int]$Matches[1]-le 255 -and [int]$Matches[2]-le 255 -and [int]$Matches[3]-le 65535) 'Version exceeds MSI limits.'
if($ExpectedVersion){Require ($version-eq$ExpectedVersion) 'Release version does not match the expected version.'}
Require ([string]$manifest.SourceCommit -match '^[0-9a-fA-F]{40}$') 'SourceCommit must be a full commit hash.'
if($ExpectedSourceCommit){Require ([string]$manifest.SourceCommit-ieq$ExpectedSourceCommit) 'SourceCommit does not match the expected commit.'}
Require ($manifest.WorkingTreeDirty-is[bool]) 'WorkingTreeDirty must be Boolean.'
foreach($field in 'CiRunId','CiRunAttempt'){Require ($null-eq$manifest.$field -or [string]$manifest.$field-match '^\d+$') "$field must be null or a numeric string."}
$msiName="SC2Switcher-$version-current-user.msi";$zipName="SC2Switcher-$version-win-x64-preview.zip"
$expected=@($msiName,$zipName,'SHA256SUMS.txt','release-manifest.json')
$actual=@(Get-ChildItem -LiteralPath $directory -File|ForEach-Object Name)
Require (@(Get-ChildItem -LiteralPath $directory -Directory -Force).Count-eq0) 'Release payload must not contain subdirectories.'
Require ((@($actual|Sort-Object)-join'|')-eq(@($expected|Sort-Object)-join'|')) 'Unexpected release payload.'
Require (@($manifest.Files).Count-eq2) 'Manifest must describe exactly two packages.'
$hashes=@{}
foreach($entry in $manifest.Files){
 Require ($entry.Name-in@($msiName,$zipName) -and !$hashes.ContainsKey([string]$entry.Name)) 'Unexpected or duplicate manifest package.'
 $item=Get-Item -LiteralPath (Join-Path $directory $entry.Name);$hash=(Get-FileHash $item.FullName -Algorithm SHA256).Hash
 Require ([int64]$entry.Bytes-eq$item.Length) "Package size mismatch: $($entry.Name)"
 Require ($entry.SHA256-eq$hash) "Package hash mismatch: $($entry.Name)";$hashes[[string]$entry.Name]=$hash
}
if($ExpectedMsiSHA256){Require ($hashes[$msiName]-eq$ExpectedMsiSHA256) 'MSI hash does not match the expected candidate.'}
if($ExpectedZipSHA256){Require ($hashes[$zipName]-eq$ExpectedZipSHA256) 'ZIP hash does not match the expected candidate.'}
$lines=@(Get-Content -LiteralPath (Join-Path $directory 'SHA256SUMS.txt'));Require ($lines.Count-eq3) 'Expected three checksum entries.';$names=@()
foreach($line in $lines){Require ($line-match'^([0-9a-fA-F]{64})  ([^/\\]+)$') 'Invalid checksum entry.';$name=$Matches[2];Require ($name-in$expected -and $name-ne'SHA256SUMS.txt') 'Unexpected checksum filename.';Require ((Get-FileHash (Join-Path $directory $name)).Hash-eq$Matches[1]) "Checksum mismatch: $name";$names+=$name}
Require (@($names|Select-Object -Unique).Count-eq3) 'Duplicate checksum entry.'

Add-Type -AssemblyName System.IO.Compression.FileSystem
$runtime=@('SC2Switcher.Wpf.exe','SC2Switcher.Wpf.dll','SC2Switcher.Wpf.deps.json','SC2Switcher.Wpf.runtimeconfig.json')
$zipPath=Join-Path $directory $zipName;$zip=[IO.Compression.ZipFile]::OpenRead($zipPath);$zipExpected=@('README.md')+$runtime;if([version]$version-ge[version]'3.4.1'){$zipExpected+='README.zh-CN.md'};$zipExpected=@($zipExpected|Sort-Object)
try{Require ((@($zip.Entries.FullName|Sort-Object)-join'|')-eq($zipExpected-join'|')) 'Portable ZIP contains unexpected files.'}finally{$zip.Dispose()}

$installer=New-Object -ComObject WindowsInstaller.Installer;$db=$installer.OpenDatabase((Join-Path $directory $msiName),0)
function Rows([string]$sql,[int]$count){$view=$db.OpenView($sql);try{[void]$view.Execute();while($row=$view.Fetch()){try{$values=@(for($i=1;$i-le$count;$i++){$row.StringData($i)});Write-Output -NoEnumerate $values}finally{[Runtime.InteropServices.Marshal]::FinalReleaseComObject($row)|Out-Null}}}finally{[void]$view.Close();[Runtime.InteropServices.Marshal]::FinalReleaseComObject($view)|Out-Null}}
function Map($rows){$m=@{};foreach($row in $rows){$m[[string]$row[0]]=[string]$row[1]};$m}
try{
 $props=Map @(Rows 'SELECT `Property`,`Value` FROM `Property`' 2)
 $sha=[Security.Cryptography.SHA256]::Create();try{$identityBytes=$sha.ComputeHash([Text.Encoding]::UTF8.GetBytes("SC2RegionSwitcher/per-user/x64/product/$version"))}finally{$sha.Dispose()};$expectedProduct='{'+([guid]::new([byte[]]$identityBytes[0..15])).ToString().ToUpperInvariant()+'}'
 Require ($props.ProductVersion-eq$version -and $props.ProductCode-eq$expectedProduct -and $props.UpgradeCode-eq'{4A67EAD9-86CA-450C-ABDC-5D6C1E4A4CCD}' -and $props.MSIINSTALLPERUSER-eq'1' -and $props.SecureCustomProperties-eq'OLDPRODUCTS;NEWERPRODUCTS' -and $props.MSIRESTARTMANAGERCONTROL-eq'Disable') 'MSI identity, version, context or upgrade properties are incorrect.'
 $dirs=Map @(Rows 'SELECT `Directory`,`DefaultDir` FROM `Directory`' 2);$legacy=@(Rows "SELECT ``Directory``,``Directory_Parent``,``DefaultDir`` FROM ``Directory`` WHERE ``Directory``='LegacyInstallDir'" 3);Require ($dirs.INSTALLDIR-eq'app' -and ($dirs.LegacyInstallDir-split'\|')[-1]-eq'3.3.0' -and $legacy.Count-eq1 -and $legacy[0][1]-eq'ProductRoot' -and !$dirs.ContainsKey('DesktopFolder')) 'MSI install, legacy cleanup or shortcut directory is incorrect.'
 $userRegistryKeyPath=[version]$version -ge [version]'3.4.1'
 if($userRegistryKeyPath){
  $columns=@(Rows 'SELECT `Table`,`Name`,`Type` FROM `_Columns`' 3)
  $columnTypes=@{};foreach($column in $columns){$columnTypes[($column[0]+'|'+$column[1])]=[int]$column[2]}
  foreach($rule in @(@('Property|Value',3840),@('CustomAction|Target',7679),@('LaunchCondition|Description',4095),@('File|Sequence',260),@('Media|LastSequence',260))){Require ($columnTypes[$rule[0]]-eq$rule[1]) "Nonstandard MSI column definition: $($rule[0])"}
  $validation=@{};foreach($rule in @(Rows 'SELECT `Table`,`Column` FROM `_Validation`' 2)){$validation[($rule[0]+'|'+$rule[1])]=$true}
  foreach($column in $columns){if($column[0]-ne'_Validation'){Require ($validation.ContainsKey(($column[0]+'|'+$column[1]))) "Missing MSI column validation: $($column[0]).$($column[1])"}}
  Require (!$props.ContainsKey('ALLUSERS')) 'Current-user package must not author ALLUSERS.'
  $summary=$db.SummaryInformation(0)
  try{Require ($summary.Property(7)-eq'x64;1033' -and $summary.Property(14)-eq500 -and $summary.Property(15)-eq10) 'MSI architecture, engine version or non-elevated installation contract changed.'}finally{[Runtime.InteropServices.Marshal]::FinalReleaseComObject($summary)|Out-Null}
  $fileMetadata=@(Rows 'SELECT `File`,`Version`,`Language` FROM `File`' 3)
  foreach($file in $fileMetadata){
   if($file[0]-in@('AppExe','AppDll')){Require ($file[1]-eq($version+'.0') -and $file[2]-eq'0') 'Versioned binaries must use the application version and neutral language.'}
   else{Require ([string]::IsNullOrEmpty($file[1]) -and [string]::IsNullOrEmpty($file[2])) 'JSON runtime files must remain unversioned.'}
  }
 }
 $applicationComponentId=if($userRegistryKeyPath){'{6AAC927A-CAEB-4A06-B4F4-AF6F4EBE5DC6}'}else{'{9543F8B1-925C-4250-B5B6-E513266751F0}'}
 $applicationKeyPath=if($userRegistryKeyPath){'ApplicationMarker'}else{'AppExe'}
 $applicationAttributes=if($userRegistryKeyPath){'260'}else{'256'}
 $components=@(Rows 'SELECT `Component`,`ComponentId`,`Directory_`,`Attributes`,`KeyPath` FROM `Component`' 5);Require ($components.Count-eq3) 'Expected exactly three components.';$cm=@{};foreach($r in $components){$cm[[string]$r[0]]=$r};Require (@($cm.Keys|Where-Object{$_-notin@('AppRegistration','Application','StartMenu')}).Count-eq0) 'Unexpected component set.';Require ($cm.Application[1]-eq$applicationComponentId -and $cm.StartMenu[1]-eq'{95AD008A-E416-4E6A-8E52-0D0A0FCA3B94}' -and $cm.AppRegistration[1]-eq'{D2F017E8-3B57-4DD8-BD70-456D91E101D1}') 'Stable component identities changed.';Require ($cm.Application[2]-eq'INSTALLDIR' -and $cm.Application[3]-eq$applicationAttributes -and $cm.Application[4]-eq$applicationKeyPath -and $cm.StartMenu[3]-eq'260' -and $cm.AppRegistration[3]-eq'260') 'Component key path or attributes are incorrect.'
 $files=@(Rows 'SELECT `File`,`Component_`,`FileName`,`FileSize` FROM `File`' 4);Require ($files.Count-eq4 -and @($files|Where-Object{$_[1]-ne'Application'}).Count-eq0) 'MSI runtime file component is incorrect.';$fileIds=@{};foreach($f in $files){$fileIds[($f[2]-split'\|')[-1]]=$f[0]};Require ((@($fileIds.Keys|Sort-Object)-join'|')-eq(@($runtime|Sort-Object)-join'|')) 'MSI runtime file set is incorrect.'
 $short=@(Rows 'SELECT `Shortcut`,`Directory_`,`Name`,`Component_`,`Target` FROM `Shortcut`' 5);Require ($short.Count-eq1 -and $short[0][1]-eq'MenuGroup' -and $short[0][3]-eq'StartMenu' -and $short[0][4]-eq'[INSTALLDIR]SC2Switcher.Wpf.exe') 'Start menu shortcut contract is incorrect.'
 $reg=@(Rows 'SELECT `Registry`,`Root`,`Key`,`Name`,`Value`,`Component_` FROM `Registry`' 6);Require (@($reg|Where-Object{$_[0]-eq$cm.StartMenu[4] -and $_[1]-eq'1' -and $_[5]-eq'StartMenu'}).Count-eq1) 'StartMenu HKCU key path is missing.';Require (@($reg|Where-Object{$_[0]-eq$cm.AppRegistration[4] -and $_[1]-eq'1' -and $_[2]-eq'Software\Microsoft\Windows\CurrentVersion\App Paths\SC2Switcher.Wpf.exe' -and $_[4]-eq'[INSTALLDIR]SC2Switcher.Wpf.exe' -and $_[5]-eq'AppRegistration'}).Count-eq1) 'App Paths registration is incorrect.'
 if($userRegistryKeyPath){Require ($reg.Count-eq3 -and @($reg|Where-Object{$_[0]-eq'ApplicationMarker' -and $_[1]-eq'1' -and $_[2]-eq'Software\SC2RegionSwitcher\Installer' -and $_[3]-eq'Application' -and $_[4]-eq'#1' -and $_[5]-eq'Application'}).Count-eq1) 'Application HKCU key path is missing or incorrect.'}else{Require ($reg.Count-eq2) 'Unexpected legacy registry resources.'}
 $expectedRemovalCount=if($userRegistryKeyPath){5}else{4}
 $remove=@(Rows 'SELECT `FileKey`,`Component_`,`FileName`,`DirProperty`,`InstallMode` FROM `RemoveFile`' 5);Require ($remove.Count-eq$expectedRemovalCount) 'Unexpected RemoveFile rule count.';$rm=@{};foreach($r in $remove){Require ([string]::IsNullOrEmpty($r[2])) 'RemoveFile must not use a file name or wildcard.';$rm[[string]$r[0]]=$r};Require ($rm.RemoveLegacyApplicationFolder[1]-eq'Application' -and $rm.RemoveLegacyApplicationFolder[3]-eq'LegacyInstallDir' -and $rm.RemoveLegacyApplicationFolder[4]-eq'1') 'Legacy cleanup must remove only the known empty 3.3.0 directory.';Require ($rm.RemoveMenuGroup[1]-eq'StartMenu' -and $rm.RemoveMenuGroup[3]-eq'MenuGroup' -and $rm.RemoveMenuGroup[4]-eq'2' -and $rm.RemoveApplicationFolder[1]-eq'Application' -and $rm.RemoveApplicationFolder[3]-eq'INSTALLDIR' -and $rm.RemoveApplicationFolder[4]-eq'2' -and $rm.RemoveProductFolder[1]-eq'Application' -and $rm.RemoveProductFolder[3]-eq'ProductRoot' -and $rm.RemoveProductFolder[4]-eq'2') 'Uninstall folder cleanup rules are incorrect.'
 if($userRegistryKeyPath){Require ($rm.RemoveUserProgramsFolder[1]-eq'Application' -and $rm.RemoveUserProgramsFolder[3]-eq'UserProgramsDir' -and $rm.RemoveUserProgramsFolder[4]-eq'2') 'Shared Programs cleanup must remove an empty folder only during uninstall.'}
 $upgrade=@(Rows 'SELECT `UpgradeCode`,`VersionMin`,`VersionMax`,`Attributes`,`ActionProperty` FROM `Upgrade`' 5);Require ($upgrade.Count-eq2) 'Expected two upgrade rules.';$um=@{};foreach($r in $upgrade){$um[[string]$r[4]]=$r};Require ($um.OLDPRODUCTS[0]-eq$props.UpgradeCode -and $um.OLDPRODUCTS[1]-eq'3.3.0' -and $um.OLDPRODUCTS[2]-eq$version -and ([int]$um.OLDPRODUCTS[3] -band 256)-ne0 -and ([int]$um.OLDPRODUCTS[3] -band 512)-eq0) 'OLDPRODUCTS rule is incorrect.';Require ($um.NEWERPRODUCTS[0]-eq$props.UpgradeCode -and $um.NEWERPRODUCTS[1]-eq$version -and [string]::IsNullOrEmpty($um.NEWERPRODUCTS[2]) -and ([int]$um.NEWERPRODUCTS[3] -band 2)-ne0 -and ([int]$um.NEWERPRODUCTS[3] -band 256)-eq0) 'NEWERPRODUCTS rule is incorrect.'
 $ca=Map @(Rows 'SELECT `Action`,`Type` FROM `CustomAction`' 2);Require ($ca.Count-eq2 -and $ca.SetInstallLocation-eq'51' -and $ca.RejectNewerProduct-eq'19') 'Custom action contract is incorrect.';$seq=Map @(Rows 'SELECT `Action`,`Sequence` FROM `InstallExecuteSequence`' 2);Require ($seq.FindRelatedProducts-eq'200' -and $seq.RejectNewerProduct-eq'210' -and $seq.LaunchConditions-eq'400' -and $seq.InstallInitialize-eq'1500' -and $seq.RemoveExistingProducts-eq'1510' -and $seq.ProcessComponents-eq'1600') 'Upgrade action sequence is incorrect.';$cond=Map @(Rows 'SELECT `Action`,`Condition` FROM `InstallExecuteSequence`' 2);Require ($cond.RejectNewerProduct-eq'NEWERPRODUCTS') 'Newer-product condition is incorrect.';$launch=@(Rows 'SELECT `Condition`,`Description` FROM `LaunchCondition`' 2);Require ($launch.Count-eq1 -and $launch[0][0]-eq'NOT ALLUSERS') 'Per-machine launch condition is missing.'
 $v=$db.OpenView("SELECT ``Data`` FROM ``_Streams`` WHERE ``Name``='app.cab'");try{[void]$v.Execute();$row=$v.Fetch();Require ($null-ne$row) 'Embedded cabinet is missing.';try{$raw=[string]$row.ReadStream(1,[int]$row.DataSize(1),1);$cab=[byte[]]::new($raw.Length);for($i=0;$i-lt$raw.Length;$i++){$cab[$i]=[byte][int]$raw[$i]}}finally{[Runtime.InteropServices.Marshal]::FinalReleaseComObject($row)|Out-Null}}finally{[void]$v.Close();[Runtime.InteropServices.Marshal]::FinalReleaseComObject($v)|Out-Null}
}finally{[Runtime.InteropServices.Marshal]::FinalReleaseComObject($db)|Out-Null;[Runtime.InteropServices.Marshal]::FinalReleaseComObject($installer)|Out-Null}
$tempRoot=[IO.Path]::GetFullPath([IO.Path]::GetTempPath());$temp=Join-Path $tempRoot ('sc2-package-'+[guid]::NewGuid().ToString('N'))
try{$cabDir=Join-Path $temp 'cab';$zipDir=Join-Path $temp 'zip';[IO.Directory]::CreateDirectory($cabDir)|Out-Null;[IO.Directory]::CreateDirectory($zipDir)|Out-Null;$cabPath=Join-Path $temp 'app.cab';[IO.File]::WriteAllBytes($cabPath,$cab);& "$env:WINDIR\System32\expand.exe" '-F:*' $cabPath $cabDir|Out-Null;Require ($LASTEXITCODE-eq0) 'Embedded cabinet extraction failed.';[IO.Compression.ZipFile]::ExtractToDirectory($zipPath,$zipDir);foreach($file in $runtime){Require ((Get-FileHash (Join-Path $zipDir $file)).Hash-eq(Get-FileHash (Join-Path $cabDir $fileIds[$file])).Hash) "ZIP and MSI bytes differ: $file"}}finally{if(Test-Path -LiteralPath $temp){$item=Get-Item -LiteralPath $temp -Force;$resolved=[IO.Path]::GetFullPath($item.FullName);$rootPrefix=$tempRoot.TrimEnd('\')+'\';Require ($resolved.StartsWith($rootPrefix,[StringComparison]::OrdinalIgnoreCase) -and $item.Name.StartsWith('sc2-package-',[StringComparison]::Ordinal) -and ($item.Attributes-band[IO.FileAttributes]::ReparsePoint)-eq0) 'Refusing to remove an unexpected temporary directory.';Remove-Item -LiteralPath $resolved -Recurse -Force}}
Write-Output "Release package checks passed for ${version}: provenance, hashes, ZIP/MSI identity, upgrade rules, components and shortcut. MSI was not installed."
