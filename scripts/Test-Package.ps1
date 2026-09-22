[CmdletBinding()]
param([Parameter(Mandatory)][string]$ReleaseDirectory,[string]$ExpectedVersion,[string]$ExpectedSourceCommit,[string]$ExpectedMsiSHA256,[string]$ExpectedZipSHA256)
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'Release-Common.ps1')
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
$machineInstall=[version]$version -ge [version]'3.4.1'
$licensePayload=@(if([version]$version -ge [version]'3.4.2'){Get-LicensePayload})
if($machineInstall){Require ($manifest.InstallScope-eq'per-machine' -and $manifest.RequiresAdministrator-eq$true -and $manifest.ApplicationExecutionLevel-eq'asInvoker') 'Installation and application privilege metadata are incorrect.'}
$msiName=if($machineInstall){"SC2Switcher-$version-x64.msi"}else{"SC2Switcher-$version-current-user.msi"};$zipName="SC2Switcher-$version-win-x64-preview.zip"
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
$zipPath=Join-Path $directory $zipName;$zip=[IO.Compression.ZipFile]::OpenRead($zipPath);$zipExpected=@('README.md')+$runtime;if([version]$version-ge[version]'3.4.1'){$zipExpected+='README.zh-CN.md'};$zipExpected+=@($licensePayload|ForEach-Object RelativePath);$zipExpected=@($zipExpected|Sort-Object)
try{Require ((@($zip.Entries.FullName|Sort-Object)-join'|')-eq($zipExpected-join'|')) 'Portable ZIP contains unexpected files.'}finally{$zip.Dispose()}

$installer=New-Object -ComObject WindowsInstaller.Installer;$db=$installer.OpenDatabase((Join-Path $directory $msiName),0)
function Rows([string]$sql,[int]$count){$view=$db.OpenView($sql);try{[void]$view.Execute();while($row=$view.Fetch()){try{$values=@(for($i=1;$i-le$count;$i++){$row.StringData($i)});Write-Output -NoEnumerate $values}finally{[Runtime.InteropServices.Marshal]::FinalReleaseComObject($row)|Out-Null}}}finally{[void]$view.Close();[Runtime.InteropServices.Marshal]::FinalReleaseComObject($view)|Out-Null}}
function Map($rows){$m=@{};foreach($row in $rows){$m[[string]$row[0]]=[string]$row[1]};$m}
try{
 $props=Map @(Rows 'SELECT `Property`,`Value` FROM `Property`' 2)
 $scope=if($machineInstall){'per-machine'}else{'per-user'}
 $sha=[Security.Cryptography.SHA256]::Create();try{$identityBytes=$sha.ComputeHash([Text.Encoding]::UTF8.GetBytes("SC2RegionSwitcher/$scope/x64/product/$version"))}finally{$sha.Dispose()};$expectedProduct='{'+([guid]::new([byte[]]$identityBytes[0..15])).ToString().ToUpperInvariant()+'}'
 $secureProperties=if($machineInstall){'OLDPRODUCTS;NEWERPRODUCTS;LEGACYUSERINSTALL'}else{'OLDPRODUCTS;NEWERPRODUCTS'}
 Require ($props.ProductVersion-eq$version -and $props.ProductCode-eq$expectedProduct -and $props.UpgradeCode-eq'{4A67EAD9-86CA-450C-ABDC-5D6C1E4A4CCD}' -and $props.SecureCustomProperties-eq$secureProperties -and $props.MSIRESTARTMANAGERCONTROL-eq'Disable') 'MSI identity, version, context or upgrade properties are incorrect.'
 if($machineInstall){Require ($props.ALLUSERS-eq'1' -and !$props.ContainsKey('MSIINSTALLPERUSER') -and $props.MSIDEPLOYMENTCOMPLIANT-eq'1') 'Machine installation properties are incorrect.'}else{Require ($props.MSIINSTALLPERUSER-eq'1') 'Current-user installation property is missing.'}
 $dirs=Map @(Rows 'SELECT `Directory`,`DefaultDir` FROM `Directory`' 2)
 Require ($dirs.INSTALLDIR-eq'app' -and !$dirs.ContainsKey('DesktopFolder')) 'MSI install or shortcut directory is incorrect.'
 if($machineInstall){
  $parents=Map @(Rows 'SELECT `Directory`,`Directory_Parent` FROM `Directory`' 2)
  Require ($parents.ProductRoot-eq'ProgramFiles64Folder' -and $parents.INSTALLDIR-eq'ProductRoot' -and $parents.MenuGroup-eq'ProgramMenuFolder' -and !$dirs.ContainsKey('LocalAppDataFolder') -and !$dirs.ContainsKey('LegacyInstallDir')) 'Machine installation directory is incorrect.'
  if($licensePayload.Count){Require ($parents.LicenseDir-eq'INSTALLDIR' -and $dirs.LicenseDir-eq'licenses') 'License directory is incorrect.'}
 }else{$legacy=@(Rows "SELECT ``Directory``,``Directory_Parent``,``DefaultDir`` FROM ``Directory`` WHERE ``Directory``='LegacyInstallDir'" 3);Require (($dirs.LegacyInstallDir-split'\|')[-1]-eq'3.3.0' -and $legacy.Count-eq1 -and $legacy[0][1]-eq'ProductRoot') 'Legacy cleanup directory is incorrect.'}
 $userRegistryKeyPath=[version]$version -ge [version]'3.4.1'
 if($userRegistryKeyPath){
  $columns=@(Rows 'SELECT `Table`,`Name`,`Type` FROM `_Columns`' 3)
  $columnTypes=@{};foreach($column in $columns){$columnTypes[($column[0]+'|'+$column[1])]=[int]$column[2]}
  foreach($rule in @(@('Property|Value',3840),@('CustomAction|Target',7679),@('LaunchCondition|Description',4095),@('File|Sequence',260),@('Media|LastSequence',260))){Require ($columnTypes[$rule[0]]-eq$rule[1]) "Nonstandard MSI column definition: $($rule[0])"}
  $validation=@{};foreach($rule in @(Rows 'SELECT `Table`,`Column` FROM `_Validation`' 2)){$validation[($rule[0]+'|'+$rule[1])]=$true}
  foreach($column in $columns){if($column[0]-ne'_Validation'){Require ($validation.ContainsKey(($column[0]+'|'+$column[1]))) "Missing MSI column validation: $($column[0]).$($column[1])"}}
  $summary=$db.SummaryInformation(0)
  try{Require ($summary.Property(7)-eq'x64;1033' -and $summary.Property(14)-eq500 -and $summary.Property(15)-eq2) 'MSI architecture, engine version or elevation metadata are incorrect.'}finally{[Runtime.InteropServices.Marshal]::FinalReleaseComObject($summary)|Out-Null}
  $fileMetadata=@(Rows 'SELECT `File`,`Version`,`Language` FROM `File`' 3)
  foreach($file in $fileMetadata){
   if($file[0]-in@('AppExe','AppDll')){Require ($file[1]-eq($version+'.0') -and $file[2]-eq'0') 'Versioned binaries must use the application version and neutral language.'}
   else{Require ([string]::IsNullOrEmpty($file[1]) -and [string]::IsNullOrEmpty($file[2])) 'Data and license files must remain unversioned.'}
  }
 }
 $applicationComponentId=if($machineInstall){'{9087858A-CB27-4146-BA8D-C7E90FFB0C37}'}else{'{9543F8B1-925C-4250-B5B6-E513266751F0}'}
 $registrationComponentId=if($machineInstall){'{A64B85A6-A03A-4319-AE88-ED3B53C40952}'}else{'{D2F017E8-3B57-4DD8-BD70-456D91E101D1}'}
 $components=@(Rows 'SELECT `Component`,`ComponentId`,`Directory_`,`Attributes`,`KeyPath` FROM `Component`' 5)
 $expectedComponents=if($machineInstall){@('Application','AppRegistration')}else{@('Application','AppRegistration','StartMenu')}
 $expectedComponents+=@($licensePayload|ForEach-Object Id)
 $cm=@{};foreach($row in $components){$cm[[string]$row[0]]=$row}
 Require ((@($cm.Keys|Sort-Object)-join'|')-eq(@($expectedComponents|Sort-Object)-join'|')) 'Unexpected component set.'
 Require ($cm.Application[1]-eq$applicationComponentId -and $cm.AppRegistration[1]-eq$registrationComponentId) 'Stable component identities changed.'
 Require ($cm.Application[2]-eq'INSTALLDIR' -and $cm.Application[3]-eq'256' -and $cm.Application[4]-eq'AppExe' -and $cm.AppRegistration[2]-eq'INSTALLDIR' -and $cm.AppRegistration[3]-eq'260' -and $cm.AppRegistration[4]-eq'AppPath') 'Component key path or attributes are incorrect.'
 if(!$machineInstall){Require ($cm.StartMenu[1]-eq'{95AD008A-E416-4E6A-8E52-0D0A0FCA3B94}' -and $cm.StartMenu[3]-eq'260') 'Legacy menu component is incorrect.'}
 foreach($license in $licensePayload){$c=$cm[$license.Id];Require ($c[1]-eq$license.ComponentCode -and $c[2]-eq$license.Directory -and $c[3]-eq'256' -and $c[4]-eq$license.Id) "License component is incorrect: $($license.Id)"}
 $files=@(Rows 'SELECT `File`,`Component_`,`FileName`,`FileSize` FROM `File`' 4)
 $runtimeFiles=@($files|Where-Object{$_[1]-eq'Application'})
 Require ($files.Count-eq(4+$licensePayload.Count) -and $runtimeFiles.Count-eq4) 'MSI runtime file component is incorrect.'
 $fileIds=@{};foreach($f in $runtimeFiles){$fileIds[($f[2]-split'\|')[-1]]=$f[0]}
 Require ((@($fileIds.Keys|Sort-Object)-join'|')-eq(@($runtime|Sort-Object)-join'|')) 'MSI runtime file set is incorrect.'
 foreach($license in $licensePayload){
  $entry=@($files|Where-Object{$_[0]-eq$license.Id})
  Require ($entry.Count-eq1 -and $entry[0][1]-eq$license.Id -and ($entry[0][2]-split'\|')[-1]-eq$license.Name) "MSI license file is incorrect: $($license.RelativePath)"
  $fileIds[$license.RelativePath]=$license.Id
 }
 $shortcutComponent=if($machineInstall){'Application'}else{'StartMenu'}
 $shortcutTarget=if($machineInstall){'MainFeature'}else{'[INSTALLDIR]SC2Switcher.Wpf.exe'}
 $short=@(Rows 'SELECT `Shortcut`,`Directory_`,`Name`,`Component_`,`Target` FROM `Shortcut`' 5);Require ($short.Count-eq1 -and $short[0][1]-eq'MenuGroup' -and $short[0][3]-eq$shortcutComponent -and $short[0][4]-eq$shortcutTarget) 'Start menu shortcut contract is incorrect.'
 $registryRoot=if($machineInstall){'2'}else{'1'}
 $registryCount=if($machineInstall){1}else{2}
 $reg=@(Rows 'SELECT `Registry`,`Root`,`Key`,`Name`,`Value`,`Component_` FROM `Registry`' 6)
 Require ($reg.Count-eq$registryCount -and @($reg|Where-Object{$_[0]-eq'AppPath' -and $_[1]-eq$registryRoot -and $_[2]-eq'Software\Microsoft\Windows\CurrentVersion\App Paths\SC2Switcher.Wpf.exe' -and $_[4]-eq'[INSTALLDIR]SC2Switcher.Wpf.exe' -and $_[5]-eq'AppRegistration'}).Count-eq1) 'App Paths registration is incorrect.'
 if(!$machineInstall){Require (@($reg|Where-Object{$_[0]-eq$cm.StartMenu[4] -and $_[1]-eq'1' -and $_[5]-eq'StartMenu'}).Count-eq1) 'Legacy menu registry key path is missing.'}
 $expectedRemovalCount=if($machineInstall){3}else{4}
 if($licensePayload.Count){$expectedRemovalCount++}
 $remove=@(Rows 'SELECT `FileKey`,`Component_`,`FileName`,`DirProperty`,`InstallMode` FROM `RemoveFile`' 5);Require ($remove.Count-eq$expectedRemovalCount) 'Unexpected RemoveFile rule count.';$rm=@{};foreach($r in $remove){Require ([string]::IsNullOrEmpty($r[2])) 'RemoveFile must not use a file name or wildcard.';$rm[[string]$r[0]]=$r};Require ($rm.RemoveMenuGroup[1]-eq$shortcutComponent -and $rm.RemoveMenuGroup[3]-eq'MenuGroup' -and $rm.RemoveMenuGroup[4]-eq'2' -and $rm.RemoveApplicationFolder[1]-eq'Application' -and $rm.RemoveApplicationFolder[3]-eq'INSTALLDIR' -and $rm.RemoveApplicationFolder[4]-eq'2' -and $rm.RemoveProductFolder[1]-eq'Application' -and $rm.RemoveProductFolder[3]-eq'ProductRoot' -and $rm.RemoveProductFolder[4]-eq'2') 'Uninstall folder cleanup rules are incorrect.'
 if(!$machineInstall){Require ($rm.RemoveLegacyApplicationFolder[1]-eq'Application' -and $rm.RemoveLegacyApplicationFolder[3]-eq'LegacyInstallDir' -and $rm.RemoveLegacyApplicationFolder[4]-eq'1') 'Legacy cleanup must remove only the known empty 3.3.0 directory.'}
 if($licensePayload.Count){Require ($rm.RemoveLicenseFolder[1]-eq'RadixLicense' -and $rm.RemoveLicenseFolder[3]-eq'LicenseDir' -and $rm.RemoveLicenseFolder[4]-eq'2') 'License folder cleanup is incorrect.'}
 $minimumVersion=if($machineInstall){'3.4.0'}else{'3.3.0'}
 $upgrade=@(Rows 'SELECT `UpgradeCode`,`VersionMin`,`VersionMax`,`Attributes`,`ActionProperty` FROM `Upgrade`' 5);Require ($upgrade.Count-eq2) 'Expected two upgrade rules.';$um=@{};foreach($r in $upgrade){$um[[string]$r[4]]=$r};Require ($um.OLDPRODUCTS[0]-eq$props.UpgradeCode -and $um.OLDPRODUCTS[1]-eq$minimumVersion -and $um.OLDPRODUCTS[2]-eq$version -and ([int]$um.OLDPRODUCTS[3] -band 256)-ne0 -and ([int]$um.OLDPRODUCTS[3] -band 512)-eq0) 'OLDPRODUCTS rule is incorrect.';Require ($um.NEWERPRODUCTS[0]-eq$props.UpgradeCode -and $um.NEWERPRODUCTS[1]-eq$version -and [string]::IsNullOrEmpty($um.NEWERPRODUCTS[2]) -and ([int]$um.NEWERPRODUCTS[3] -band 2)-ne0 -and ([int]$um.NEWERPRODUCTS[3] -band 256)-eq0) 'NEWERPRODUCTS rule is incorrect.'
 $ca=Map @(Rows 'SELECT `Action`,`Type` FROM `CustomAction`' 2);Require ($ca.Count-eq2 -and $ca.SetInstallLocation-eq'51' -and $ca.RejectNewerProduct-eq'19') 'Custom action contract is incorrect.';$seq=Map @(Rows 'SELECT `Action`,`Sequence` FROM `InstallExecuteSequence`' 2);Require ($seq.FindRelatedProducts-eq'200' -and $seq.RejectNewerProduct-eq'210' -and $seq.LaunchConditions-eq'400' -and $seq.InstallInitialize-eq'1500' -and $seq.RemoveExistingProducts-eq'1510' -and $seq.ProcessComponents-eq'1600') 'Upgrade action sequence is incorrect.';$cond=Map @(Rows 'SELECT `Action`,`Condition` FROM `InstallExecuteSequence`' 2);Require ($cond.RejectNewerProduct-eq'NEWERPRODUCTS') 'Newer-product condition is incorrect.';$launch=Map @(Rows 'SELECT `Condition`,`Description` FROM `LaunchCondition`' 2)
 if($machineInstall){
  Require ($seq.AppSearch-eq'300' -and $launch.Count-eq2 -and $launch.ContainsKey('ALLUSERS=1') -and $launch.ContainsKey('Installed OR NOT LEGACYUSERINSTALL')) 'Machine scope or preview migration condition is missing.'
  $tables=@(Rows 'SELECT `Name` FROM `_Tables`' 1 | ForEach-Object {$_[0]})
  Require ('Signature'-in$tables) 'AppSearch Signature table is missing.'
  Require (@(Rows 'SELECT `Signature` FROM `Signature`' 1).Count-eq0) 'Registry-only preview detection must not define a file signature.'
  $search=Map @(Rows 'SELECT `Property`,`Signature_` FROM `AppSearch`' 2)
  $locator=@(Rows 'SELECT `Signature_`,`Root`,`Key`,`Name`,`Type` FROM `RegLocator`' 5)
  Require ($search.Count-eq1 -and $search.LEGACYUSERINSTALL-eq'LegacyUserAppPath' -and $locator.Count-eq1 -and $locator[0][0]-eq'LegacyUserAppPath' -and $locator[0][1]-eq'1' -and $locator[0][2]-eq'Software\Microsoft\Windows\CurrentVersion\App Paths\SC2Switcher.Wpf.exe' -and [string]::IsNullOrEmpty($locator[0][3]) -and $locator[0][4]-eq'18') 'Current-user preview detection is incorrect.'
 }else{Require ($launch.Count-eq1 -and $launch.ContainsKey('NOT ALLUSERS')) 'Per-user launch condition is missing.'}
 $v=$db.OpenView("SELECT ``Data`` FROM ``_Streams`` WHERE ``Name``='app.cab'");try{[void]$v.Execute();$row=$v.Fetch();Require ($null-ne$row) 'Embedded cabinet is missing.';try{$raw=[string]$row.ReadStream(1,[int]$row.DataSize(1),1);$cab=[byte[]]::new($raw.Length);for($i=0;$i-lt$raw.Length;$i++){$cab[$i]=[byte][int]$raw[$i]}}finally{[Runtime.InteropServices.Marshal]::FinalReleaseComObject($row)|Out-Null}}finally{[void]$v.Close();[Runtime.InteropServices.Marshal]::FinalReleaseComObject($v)|Out-Null}
}finally{[Runtime.InteropServices.Marshal]::FinalReleaseComObject($db)|Out-Null;[Runtime.InteropServices.Marshal]::FinalReleaseComObject($installer)|Out-Null}
$tempRoot=[IO.Path]::GetFullPath([IO.Path]::GetTempPath());$temp=Join-Path $tempRoot ('sc2-package-'+[guid]::NewGuid().ToString('N'))
try{$cabDir=Join-Path $temp 'cab';$zipDir=Join-Path $temp 'zip';[IO.Directory]::CreateDirectory($cabDir)|Out-Null;[IO.Directory]::CreateDirectory($zipDir)|Out-Null;$cabPath=Join-Path $temp 'app.cab';[IO.File]::WriteAllBytes($cabPath,$cab);& "$env:WINDIR\System32\expand.exe" '-F:*' $cabPath $cabDir|Out-Null;Require ($LASTEXITCODE-eq0) 'Embedded cabinet extraction failed.';[IO.Compression.ZipFile]::ExtractToDirectory($zipPath,$zipDir);foreach($file in $fileIds.Keys){Require ((Get-FileHash (Join-Path $zipDir $file)).Hash-eq(Get-FileHash (Join-Path $cabDir $fileIds[$file])).Hash) "ZIP and MSI bytes differ: $file"}}finally{if(Test-Path -LiteralPath $temp){$item=Get-Item -LiteralPath $temp -Force;$resolved=[IO.Path]::GetFullPath($item.FullName);$rootPrefix=$tempRoot.TrimEnd('\')+'\';Require ($resolved.StartsWith($rootPrefix,[StringComparison]::OrdinalIgnoreCase) -and $item.Name.StartsWith('sc2-package-',[StringComparison]::Ordinal) -and ($item.Attributes-band[IO.FileAttributes]::ReparsePoint)-eq0) 'Refusing to remove an unexpected temporary directory.';Remove-Item -LiteralPath $resolved -Recurse -Force}}
Write-Output "Release package checks passed for ${version}: provenance, hashes, ZIP/MSI identity, upgrade rules, components and shortcut. MSI was not installed."
