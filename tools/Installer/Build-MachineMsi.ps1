[CmdletBinding()]
param(
 [string]$AppDirectory=(Join-Path $PSScriptRoot '..\..\artifacts\publish\win-x64'),
 [string]$OutputDirectory=(Join-Path $PSScriptRoot '..\..\artifacts\packages\msi')
)
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot '..\..\scripts\Release-Common.ps1')
$release=Get-ReleaseMetadata
$version=$release.Version
$stage=[IO.Path]::GetFullPath($OutputDirectory)
if((Test-Path -LiteralPath $stage) -and @(Get-ChildItem -LiteralPath $stage -Force).Count){throw 'Use an empty output directory.'}
$msi=Join-Path $stage "SC2Switcher-$version-x64.msi"
if(Test-Path -LiteralPath $msi){throw 'An MSI build already exists; preserve it before rebuilding.'}
New-Item -ItemType Directory -Path $stage -Force | Out-Null
$files=@(
 @{Id='AppExe';Name='SC2Switcher.Wpf.exe';Short='SC2APP.EXE';Version=$release.FileVersion},
 @{Id='AppDll';Name='SC2Switcher.Wpf.dll';Short='SC2APP.DLL';Version=$release.FileVersion},
 @{Id='DepsJson';Name='SC2Switcher.Wpf.deps.json';Short='DEPS.JSN';Version=$null},
 @{Id='RuntimeJson';Name='SC2Switcher.Wpf.runtimeconfig.json';Short='RUNTIME.JSN';Version=$null}
)
$ddl=@('.OPTION EXPLICIT','.Set CabinetNameTemplate=app.cab',('.Set DiskDirectoryTemplate="'+$stage+'"'),'.Set CompressionType=MSZIP','.Set Cabinet=on','.Set Compress=on')
foreach($file in $files){
 $file.Source=Join-Path $AppDirectory $file.Name
 if(!(Test-Path -LiteralPath $file.Source -PathType Leaf)){throw "Missing runtime file: $($file.Name)"}
 if($file.Version -and [Diagnostics.FileVersionInfo]::GetVersionInfo($file.Source).FileVersion -ne $file.Version){throw "Runtime file version mismatch: $($file.Name)"}
 $ddl+=('"'+$file.Source+'" '+$file.Id)
}
$ddlPath=Join-Path $stage 'app.ddf'
$ddl | Set-Content -LiteralPath $ddlPath -Encoding ascii
Push-Location $stage
try {& (Join-Path $env:WINDIR 'System32\makecab.exe') /F $ddlPath | Out-Null;if($LASTEXITCODE -ne 0){throw 'CAB build failed.'}} finally {Pop-Location}

$installer=New-Object -ComObject WindowsInstaller.Installer
$database=$installer.OpenDatabase($msi,3)
function Sql([string]$Statement){$view=$database.OpenView($Statement);try{$view.Execute()}finally{$view.Close()}}
function Insert([string]$Table,[string[]]$Columns,[object[]]$Values){
 $columnsSql=($Columns | ForEach-Object {'`'+$_+'`'}) -join ','
 $parameters=($Values | ForEach-Object {'?'}) -join ','
 $view=$database.OpenView(('INSERT INTO `'+$Table+'` ('+$columnsSql+') VALUES ('+$parameters+')'))
 $record=$installer.CreateRecord($Values.Count)
 for($i=0;$i -lt $Values.Count;$i++){
  if($null -eq $Values[$i]){continue}
  if($Values[$i] -is [int]){$record.IntegerData($i+1)=$Values[$i]}else{$record.StringData($i+1)=[string]$Values[$i]}
 }
 try{$view.Execute($record)}finally{$view.Close()}
}
Sql 'CREATE TABLE `Property` (`Property` CHAR(72) NOT NULL, `Value` CHAR(0) NOT NULL LOCALIZABLE PRIMARY KEY `Property`)'
Sql 'CREATE TABLE `Directory` (`Directory` CHAR(72) NOT NULL, `Directory_Parent` CHAR(72), `DefaultDir` CHAR(255) NOT NULL LOCALIZABLE PRIMARY KEY `Directory`)'
Sql 'CREATE TABLE `Component` (`Component` CHAR(72) NOT NULL, `ComponentId` CHAR(38), `Directory_` CHAR(72) NOT NULL, `Attributes` SHORT NOT NULL, `Condition` CHAR(255), `KeyPath` CHAR(72) PRIMARY KEY `Component`)'
Sql 'CREATE TABLE `Feature` (`Feature` CHAR(38) NOT NULL, `Feature_Parent` CHAR(38), `Title` CHAR(64) LOCALIZABLE, `Description` CHAR(255) LOCALIZABLE, `Display` SHORT, `Level` SHORT NOT NULL, `Directory_` CHAR(72), `Attributes` SHORT NOT NULL PRIMARY KEY `Feature`)'
Sql 'CREATE TABLE `FeatureComponents` (`Feature_` CHAR(38) NOT NULL, `Component_` CHAR(72) NOT NULL PRIMARY KEY `Feature_`, `Component_`)'
Sql 'CREATE TABLE `File` (`File` CHAR(72) NOT NULL, `Component_` CHAR(72) NOT NULL, `FileName` CHAR(255) NOT NULL LOCALIZABLE, `FileSize` LONG NOT NULL, `Version` CHAR(72), `Language` CHAR(20), `Attributes` SHORT, `Sequence` LONG NOT NULL PRIMARY KEY `File`)'
Sql 'CREATE TABLE `Media` (`DiskId` SHORT NOT NULL, `LastSequence` LONG NOT NULL, `DiskPrompt` CHAR(64) LOCALIZABLE, `Cabinet` CHAR(255), `VolumeLabel` CHAR(32), `Source` CHAR(72) PRIMARY KEY `DiskId`)'
Sql 'CREATE TABLE `Shortcut` (`Shortcut` CHAR(72) NOT NULL, `Directory_` CHAR(72) NOT NULL, `Name` CHAR(128) NOT NULL LOCALIZABLE, `Component_` CHAR(72) NOT NULL, `Target` CHAR(72) NOT NULL, `Arguments` CHAR(255), `Description` CHAR(255) LOCALIZABLE, `Hotkey` SHORT, `Icon_` CHAR(72), `IconIndex` SHORT, `ShowCmd` SHORT, `WkDir` CHAR(72), `DisplayResourceDLL` CHAR(255), `DisplayResourceId` SHORT, `DescriptionResourceDLL` CHAR(255), `DescriptionResourceId` SHORT PRIMARY KEY `Shortcut`)'
Sql 'CREATE TABLE `Icon` (`Name` CHAR(72) NOT NULL, `Data` OBJECT NOT NULL PRIMARY KEY `Name`)'
Sql 'CREATE TABLE `Registry` (`Registry` CHAR(72) NOT NULL, `Root` SHORT NOT NULL, `Key` CHAR(255) NOT NULL LOCALIZABLE, `Name` CHAR(255) LOCALIZABLE, `Value` CHAR(0) LOCALIZABLE, `Component_` CHAR(72) NOT NULL PRIMARY KEY `Registry`)'
Sql 'CREATE TABLE `RemoveFile` (`FileKey` CHAR(72) NOT NULL, `Component_` CHAR(72) NOT NULL, `FileName` CHAR(255) LOCALIZABLE, `DirProperty` CHAR(72) NOT NULL, `InstallMode` SHORT NOT NULL PRIMARY KEY `FileKey`)'
Sql 'CREATE TABLE `InstallExecuteSequence` (`Action` CHAR(72) NOT NULL, `Condition` CHAR(255), `Sequence` SHORT PRIMARY KEY `Action`)'
Sql 'CREATE TABLE `CustomAction` (`Action` CHAR(72) NOT NULL, `Type` SHORT NOT NULL, `Source` CHAR(72), `Target` CHAR(255), `ExtendedType` LONG PRIMARY KEY `Action`)'
Sql 'CREATE TABLE `Upgrade` (`UpgradeCode` CHAR(38) NOT NULL, `VersionMin` CHAR(20), `VersionMax` CHAR(20), `Language` CHAR(255), `Attributes` LONG NOT NULL, `Remove` CHAR(255), `ActionProperty` CHAR(72) NOT NULL PRIMARY KEY `UpgradeCode`, `VersionMin`, `VersionMax`, `Language`, `Attributes`)'
Sql 'CREATE TABLE `LaunchCondition` (`Condition` CHAR(255) NOT NULL, `Description` CHAR(255) NOT NULL LOCALIZABLE PRIMARY KEY `Condition`)'
Sql 'CREATE TABLE `AppSearch` (`Property` CHAR(72) NOT NULL, `Signature_` CHAR(72) NOT NULL PRIMARY KEY `Property`, `Signature_`)'
Sql 'CREATE TABLE `RegLocator` (`Signature_` CHAR(72) NOT NULL, `Root` SHORT NOT NULL, `Key` CHAR(255) NOT NULL, `Name` CHAR(255), `Type` SHORT PRIMARY KEY `Signature_`)'
# AppSearch opens this table even for a registry-only search. Keep it empty.
Sql 'CREATE TABLE `Signature` (`Signature` CHAR(72) NOT NULL, `FileName` CHAR(255) NOT NULL, `MinVersion` CHAR(20), `MaxVersion` CHAR(20), `MinSize` LONG, `MaxSize` LONG, `MinDate` LONG, `MaxDate` LONG, `Languages` CHAR(255) PRIMARY KEY `Signature`)'
# Import standard column constraints so the finished database can run the ICE suite.
# These describe database columns only and contain no application or user data.
$database.Import((Join-Path $PSScriptRoot 'metadata'),'_Validation.idt')

$productCode=$release.ProductCode
$packageCode='{'+[guid]::NewGuid().ToString().ToUpperInvariant()+'}'
$componentCodes=[ordered]@{Application='{9087858A-CB27-4146-BA8D-C7E90FFB0C37}';AppRegistration='{A64B85A6-A03A-4319-AE88-ED3B53C40952}'}
$properties=[ordered]@{
 ProductCode=$productCode;ProductVersion=$version;ProductLanguage='1033';ProductName='SC2 Region Switcher';Manufacturer='SC2 Region Switcher';
 UpgradeCode=$release.UpgradeCode;INSTALLLEVEL='1';ALLUSERS='1';ARPNOMODIFY='1';ARPNOREPAIR='1';ARPPRODUCTICON='SwitcherIcon.exe';ARPCOMMENTS='StarCraft II CN and Global region switcher';
 SecureCustomProperties='OLDPRODUCTS;NEWERPRODUCTS;LEGACYUSERINSTALL';MSIRESTARTMANAGERCONTROL='Disable';MSIDEPLOYMENTCOMPLIANT='1';
}
foreach($name in $properties.Keys){Insert Property @('Property','Value') @($name,$properties[$name])}
Insert LaunchCondition @('Condition','Description') @('ALLUSERS=1','This package installs for all users and requires administrator approval.')
Insert LaunchCondition @('Condition','Description') @('Installed OR NOT LEGACYUSERINSTALL','Uninstall the earlier current-user SC2 Region Switcher preview from Windows Settings, then run this installer again. Your configuration and backups are retained.')
Insert AppSearch @('Property','Signature_') @('LEGACYUSERINSTALL','LegacyUserAppPath')
Insert RegLocator @('Signature_','Root','Key','Name','Type') @('LegacyUserAppPath',[int]1,'Software\Microsoft\Windows\CurrentVersion\App Paths\SC2Switcher.Wpf.exe',$null,[int]18)
Insert Upgrade @('UpgradeCode','VersionMin','VersionMax','Language','Attributes','Remove','ActionProperty') @($release.UpgradeCode,'3.4.0',$version,$null,[int]256,$null,'OLDPRODUCTS')
Insert Upgrade @('UpgradeCode','VersionMin','VersionMax','Language','Attributes','Remove','ActionProperty') @($release.UpgradeCode,$version,$null,$null,[int]2,$null,'NEWERPRODUCTS')
foreach($dir in @(
 @('TARGETDIR',$null,'SourceDir'),@('ProgramFiles64Folder','TARGETDIR','.'),
 @('ProductRoot','ProgramFiles64Folder','SC2REG~1|SC2RegionSwitcher'),@('INSTALLDIR','ProductRoot','app'),
 @('ProgramMenuFolder','TARGETDIR','.'),@('MenuGroup','ProgramMenuFolder','SC2REG~1|SC2 Region Switcher')
)){Insert Directory @('Directory','Directory_Parent','DefaultDir') $dir}
Insert Component @('Component','ComponentId','Directory_','Attributes','Condition','KeyPath') @('Application',$componentCodes.Application,'INSTALLDIR',[int]256,$null,'AppExe')
Insert Component @('Component','ComponentId','Directory_','Attributes','Condition','KeyPath') @('AppRegistration',$componentCodes.AppRegistration,'INSTALLDIR',[int]260,$null,'AppPath')
Insert Feature @('Feature','Feature_Parent','Title','Description','Display','Level','Directory_','Attributes') @('MainFeature',$null,'SC2 Region Switcher','Application and one Start menu shortcut',[int]1,[int]1,'INSTALLDIR',[int]0)
foreach($component in $componentCodes.Keys){Insert FeatureComponents @('Feature_','Component_') @('MainFeature',$component)}
$sequence=1
foreach($file in $files){
 $language=if($file.Version){'0'}else{$null}
 Insert File @('File','Component_','FileName','FileSize','Version','Language','Attributes','Sequence') @($file.Id,'Application',($file.Short+'|'+$file.Name),[int](Get-Item -LiteralPath $file.Source).Length,$file.Version,$language,[int]512,[int]$sequence)
 $sequence++
}
Insert Media @('DiskId','LastSequence','DiskPrompt','Cabinet','VolumeLabel','Source') @([int]1,[int]4,$null,'#app.cab',$null,$null)
Insert Shortcut @('Shortcut','Directory_','Name','Component_','Target','Arguments','Description','Hotkey','Icon_','IconIndex','ShowCmd','WkDir') @('StartMenu','MenuGroup','SC2REG~1|SC2 Region Switcher','Application','MainFeature',$null,'StarCraft II CN and Global region switcher',$null,'SwitcherIcon.exe',[int]0,[int]1,'INSTALLDIR')
Insert Registry @('Registry','Root','Key','Name','Value','Component_') @('AppPath',[int]2,'Software\Microsoft\Windows\CurrentVersion\App Paths\SC2Switcher.Wpf.exe',$null,'[INSTALLDIR]SC2Switcher.Wpf.exe','AppRegistration')
Insert RemoveFile @('FileKey','Component_','FileName','DirProperty','InstallMode') @('RemoveMenuGroup','Application',$null,'MenuGroup',[int]2)
Insert RemoveFile @('FileKey','Component_','FileName','DirProperty','InstallMode') @('RemoveApplicationFolder','Application',$null,'INSTALLDIR',[int]2)
Insert RemoveFile @('FileKey','Component_','FileName','DirProperty','InstallMode') @('RemoveProductFolder','Application',$null,'ProductRoot',[int]2)
Insert CustomAction @('Action','Type','Source','Target') @('SetInstallLocation',[int]51,'ARPINSTALLLOCATION','[INSTALLDIR]')
Insert CustomAction @('Action','Type','Source','Target') @('RejectNewerProduct',[int]19,$null,'A newer version of SC2 Region Switcher is already installed. Uninstall it before installing an older release.')
Insert InstallExecuteSequence @('Action','Condition','Sequence') @('RejectNewerProduct','NEWERPRODUCTS',[int]210)
foreach($step in @(
 @('FindRelatedProducts',200),@('AppSearch',300),@('LaunchConditions',400),
 @('ValidateProductID',700),@('CostInitialize',800),@('FileCost',900),@('CostFinalize',1000),@('SetInstallLocation',1100),@('InstallValidate',1400),@('InstallInitialize',1500),
 @('RemoveExistingProducts',1510),@('ProcessComponents',1600),@('UnpublishFeatures',1800),@('RemoveShortcuts',3200),@('RemoveRegistryValues',3300),@('RemoveFiles',3500),@('RemoveFolders',3600),
 @('CreateFolders',3700),@('InstallFiles',4000),@('CreateShortcuts',4500),@('WriteRegistryValues',5000),@('RegisterUser',6000),@('RegisterProduct',6100),
 @('PublishFeatures',6300),@('PublishProduct',6400),@('InstallFinalize',6600)
)){Insert InstallExecuteSequence @('Action','Condition','Sequence') @($step[0],$null,[int]$step[1])}

foreach($stream in @(@('_Streams','Name','Data','app.cab',(Join-Path $stage 'app.cab')),@('Icon','Name','Data','SwitcherIcon.exe',(Join-Path $AppDirectory 'SC2Switcher.Wpf.exe')))){
 $view=$database.OpenView(('INSERT INTO `'+$stream[0]+'` (`'+$stream[1]+'`,`'+$stream[2]+'`) VALUES (?,?)'))
 $record=$installer.CreateRecord(2);$record.StringData(1)=$stream[3];$record.SetStream(2,$stream[4]);try{$view.Execute($record)}finally{$view.Close()}
}
$summary=$database.SummaryInformation(20)
$summary.Property(1)=1252
$summary.Property(2)='Installation Database'
$summary.Property(3)='SC2 Region Switcher'
$summary.Property(4)='SC2 Region Switcher'
$summary.Property(5)='Installer'
$summary.Property(7)='x64;1033'
$summary.Property(8)='SC2 Region Switcher'
$summary.Property(9)=$packageCode
$summary.Property(14)=500
$summary.Property(15)=2
$summary.Property(18)='SC2 Region Switcher package builder'
$summary.Persist();$database.Commit()
[Runtime.InteropServices.Marshal]::FinalReleaseComObject($database) | Out-Null
[Runtime.InteropServices.Marshal]::FinalReleaseComObject($installer) | Out-Null
foreach($com in @($summary,$view,$record)){if($null -ne $com -and [Runtime.InteropServices.Marshal]::IsComObject($com)){try{[Runtime.InteropServices.Marshal]::FinalReleaseComObject($com) | Out-Null}catch{}}}
[GC]::Collect();[GC]::WaitForPendingFinalizers();[GC]::Collect();[GC]::WaitForPendingFinalizers()
$manifest=[ordered]@{ProductCode=$productCode;UpgradeCode=$release.UpgradeCode;PackageCode=$packageCode;ComponentCodes=$componentCodes;Version=$version;Architecture='x64';PerUser=$false;RequiresAdministrator=$true;Msi=$msi;SHA256=(Get-FileHash -LiteralPath $msi -Algorithm SHA256).Hash;ShortcutCount=1;CustomActionCount=2;CustomExecutableActionCount=0;CustomActionPurpose='Type 51 property assignment and Type 19 downgrade message';GameAndUserConfigurationIncluded=$false}
$manifest | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $stage 'package.json') -Encoding utf8
$manifest | ConvertTo-Json
