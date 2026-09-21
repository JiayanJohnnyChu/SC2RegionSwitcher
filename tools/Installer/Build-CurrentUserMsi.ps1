[CmdletBinding()]
param(
 [string]$AppDirectory=(Join-Path $PSScriptRoot '..\..\artifacts\publish\win-x64'),
 [string]$IconFile=(Join-Path $PSScriptRoot '..\..\assets\icon\switcher.ico'),
 [string]$OutputDirectory=(Join-Path $PSScriptRoot '..\..\artifacts\packages\msi')
)
$ErrorActionPreference='Stop'
$stage=[IO.Path]::GetFullPath($OutputDirectory)
if((Test-Path -LiteralPath $stage) -and @(Get-ChildItem -LiteralPath $stage -Force).Count){throw 'Use an empty output directory.'}
$msi=Join-Path $stage 'SC2Switcher-3.3.0-current-user.msi'
if(Test-Path -LiteralPath $msi){throw 'An MSI build already exists; preserve it before rebuilding.'}
New-Item -ItemType Directory -Path $stage -Force | Out-Null
$files=@(
 @{Id='AppExe';Name='SC2Switcher.Wpf.exe';Short='SC2APP.EXE';Version='3.3.0.0'},
 @{Id='AppDll';Name='SC2Switcher.Wpf.dll';Short='SC2APP.DLL';Version='3.3.0.0'},
 @{Id='DepsJson';Name='SC2Switcher.Wpf.deps.json';Short='DEPS.JSN';Version=$null},
 @{Id='RuntimeJson';Name='SC2Switcher.Wpf.runtimeconfig.json';Short='RUNTIME.JSN';Version=$null}
)
$ddl=@('.OPTION EXPLICIT','.Set CabinetNameTemplate=app.cab',('.Set DiskDirectoryTemplate="'+$stage+'"'),'.Set CompressionType=MSZIP','.Set Cabinet=on','.Set Compress=on')
foreach($file in $files){$file.Source=Join-Path $AppDirectory $file.Name;$ddl+=('"'+$file.Source+'" '+$file.Id)}
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
Sql 'CREATE TABLE `Property` (`Property` CHAR(72) NOT NULL, `Value` CHAR(0) LOCALIZABLE PRIMARY KEY `Property`)'
Sql 'CREATE TABLE `Directory` (`Directory` CHAR(72) NOT NULL, `Directory_Parent` CHAR(72), `DefaultDir` CHAR(255) NOT NULL LOCALIZABLE PRIMARY KEY `Directory`)'
Sql 'CREATE TABLE `Component` (`Component` CHAR(72) NOT NULL, `ComponentId` CHAR(38), `Directory_` CHAR(72) NOT NULL, `Attributes` SHORT NOT NULL, `Condition` CHAR(255), `KeyPath` CHAR(72) PRIMARY KEY `Component`)'
Sql 'CREATE TABLE `Feature` (`Feature` CHAR(38) NOT NULL, `Feature_Parent` CHAR(38), `Title` CHAR(64) LOCALIZABLE, `Description` CHAR(255) LOCALIZABLE, `Display` SHORT, `Level` SHORT NOT NULL, `Directory_` CHAR(72), `Attributes` SHORT NOT NULL PRIMARY KEY `Feature`)'
Sql 'CREATE TABLE `FeatureComponents` (`Feature_` CHAR(38) NOT NULL, `Component_` CHAR(72) NOT NULL PRIMARY KEY `Feature_`, `Component_`)'
Sql 'CREATE TABLE `File` (`File` CHAR(72) NOT NULL, `Component_` CHAR(72) NOT NULL, `FileName` CHAR(255) NOT NULL LOCALIZABLE, `FileSize` LONG NOT NULL, `Version` CHAR(72), `Language` CHAR(20), `Attributes` SHORT, `Sequence` SHORT NOT NULL PRIMARY KEY `File`)'
Sql 'CREATE TABLE `Media` (`DiskId` SHORT NOT NULL, `LastSequence` SHORT NOT NULL, `DiskPrompt` CHAR(64) LOCALIZABLE, `Cabinet` CHAR(255), `VolumeLabel` CHAR(32), `Source` CHAR(72) PRIMARY KEY `DiskId`)'
Sql 'CREATE TABLE `Shortcut` (`Shortcut` CHAR(72) NOT NULL, `Directory_` CHAR(72) NOT NULL, `Name` CHAR(128) NOT NULL LOCALIZABLE, `Component_` CHAR(72) NOT NULL, `Target` CHAR(72) NOT NULL, `Arguments` CHAR(255), `Description` CHAR(255) LOCALIZABLE, `Hotkey` SHORT, `Icon_` CHAR(72), `IconIndex` SHORT, `ShowCmd` SHORT, `WkDir` CHAR(72) PRIMARY KEY `Shortcut`)'
Sql 'CREATE TABLE `Icon` (`Name` CHAR(72) NOT NULL, `Data` OBJECT NOT NULL PRIMARY KEY `Name`)'
Sql 'CREATE TABLE `Registry` (`Registry` CHAR(72) NOT NULL, `Root` SHORT NOT NULL, `Key` CHAR(255) NOT NULL LOCALIZABLE, `Name` CHAR(255) LOCALIZABLE, `Value` CHAR(0) LOCALIZABLE, `Component_` CHAR(72) NOT NULL PRIMARY KEY `Registry`)'
Sql 'CREATE TABLE `RemoveFile` (`FileKey` CHAR(72) NOT NULL, `Component_` CHAR(72) NOT NULL, `FileName` CHAR(255) LOCALIZABLE, `DirProperty` CHAR(72) NOT NULL, `InstallMode` SHORT NOT NULL PRIMARY KEY `FileKey`)'
Sql 'CREATE TABLE `InstallExecuteSequence` (`Action` CHAR(72) NOT NULL, `Condition` CHAR(255), `Sequence` SHORT PRIMARY KEY `Action`)'
Sql 'CREATE TABLE `CustomAction` (`Action` CHAR(72) NOT NULL, `Type` SHORT NOT NULL, `Source` CHAR(72), `Target` CHAR(0) PRIMARY KEY `Action`)'

$productCode='{48AF7B03-7C67-4D2C-B64D-1A1B3AE450C9}'
$packageCode='{'+[guid]::NewGuid().ToString().ToUpperInvariant()+'}'
$componentCode='{F0EF532A-227F-4436-85F4-CD6A02A0444C}'
$properties=[ordered]@{
 ProductCode=$productCode;ProductVersion='3.3.0';ProductLanguage='1033';ProductName='SC2 Region Switcher';Manufacturer='SC2 Region Switcher';
 UpgradeCode='{4A67EAD9-86CA-450C-ABDC-5D6C1E4A4CCD}';INSTALLLEVEL='1';MSIINSTALLPERUSER='1';ARPNOMODIFY='1';ARPNOREPAIR='1';ARPPRODUCTICON='SwitcherIcon';ARPCOMMENTS='StarCraft II CN and Global region switcher';
}
foreach($name in $properties.Keys){Insert Property @('Property','Value') @($name,$properties[$name])}
foreach($dir in @(
 @('TARGETDIR',$null,'SourceDir'),@('LocalAppDataFolder','TARGETDIR','.'),@('UserProgramsDir','LocalAppDataFolder','Programs'),
 @('ProductRoot','UserProgramsDir','SC2REG~1|SC2RegionSwitcher'),@('INSTALLDIR','ProductRoot','3.3.0'),
 @('ProgramMenuFolder','TARGETDIR','.'),@('MenuGroup','ProgramMenuFolder','SC2REG~1|SC2 Region Switcher')
)){Insert Directory @('Directory','Directory_Parent','DefaultDir') $dir}
Insert Component @('Component','ComponentId','Directory_','Attributes','Condition','KeyPath') @('Application',$componentCode,'INSTALLDIR',[int]256,$null,'AppExe')
Insert Feature @('Feature','Feature_Parent','Title','Description','Display','Level','Directory_','Attributes') @('MainFeature',$null,'SC2 Region Switcher','Application and one Start menu shortcut',[int]1,[int]1,'INSTALLDIR',[int]0)
Insert FeatureComponents @('Feature_','Component_') @('MainFeature','Application')
$sequence=1
foreach($file in $files){
 Insert File @('File','Component_','FileName','FileSize','Version','Language','Attributes','Sequence') @($file.Id,'Application',($file.Short+'|'+$file.Name),[int](Get-Item -LiteralPath $file.Source).Length,$file.Version,$null,[int]512,[int]$sequence)
 $sequence++
}
Insert Media @('DiskId','LastSequence','DiskPrompt','Cabinet','VolumeLabel','Source') @([int]1,[int]4,$null,'#app.cab',$null,$null)
Insert Shortcut @('Shortcut','Directory_','Name','Component_','Target','Arguments','Description','Hotkey','Icon_','IconIndex','ShowCmd','WkDir') @('StartMenu','MenuGroup','SC2REG~1|SC2 Region Switcher','Application','MainFeature',$null,'StarCraft II CN and Global region switcher',$null,'SwitcherIcon',[int]0,[int]1,'INSTALLDIR')
Insert Registry @('Registry','Root','Key','Name','Value','Component_') @('AppPath',[int]1,'Software\Microsoft\Windows\CurrentVersion\App Paths\SC2Switcher.Wpf.exe',$null,'[#AppExe]','Application')
Insert RemoveFile @('FileKey','Component_','FileName','DirProperty','InstallMode') @('RemoveMenuGroup','Application',$null,'MenuGroup',[int]2)
Insert CustomAction @('Action','Type','Source','Target') @('SetInstallLocation',[int]51,'ARPINSTALLLOCATION','[INSTALLDIR]')
foreach($step in @(
 @('ValidateProductID',700),@('CostInitialize',800),@('FileCost',900),@('CostFinalize',1000),@('SetInstallLocation',1100),@('InstallValidate',1400),@('InstallInitialize',1500),
 @('ProcessComponents',1600),@('UnpublishFeatures',1800),@('RemoveShortcuts',3200),@('RemoveRegistryValues',3300),@('RemoveFiles',3500),@('RemoveFolders',3600),
 @('CreateFolders',3700),@('InstallFiles',4000),@('CreateShortcuts',4500),@('WriteRegistryValues',5000),@('RegisterUser',6000),@('RegisterProduct',6100),
 @('PublishFeatures',6300),@('PublishProduct',6400),@('InstallFinalize',6600)
)){Insert InstallExecuteSequence @('Action','Condition','Sequence') @($step[0],$null,[int]$step[1])}

foreach($stream in @(@('_Streams','Name','Data','app.cab',(Join-Path $stage 'app.cab')),@('Icon','Name','Data','SwitcherIcon',$IconFile))){
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
$summary.Property(14)=200
$summary.Property(15)=10
$summary.Property(18)='SC2 Region Switcher package builder'
$summary.Persist();$database.Commit()
[Runtime.InteropServices.Marshal]::FinalReleaseComObject($database) | Out-Null
[Runtime.InteropServices.Marshal]::FinalReleaseComObject($installer) | Out-Null
foreach($com in @($summary,$view,$record)){if($null -ne $com -and [Runtime.InteropServices.Marshal]::IsComObject($com)){try{[Runtime.InteropServices.Marshal]::FinalReleaseComObject($com) | Out-Null}catch{}}}
[GC]::Collect();[GC]::WaitForPendingFinalizers();[GC]::Collect();[GC]::WaitForPendingFinalizers()
$manifest=[ordered]@{ProductCode=$productCode;PackageCode=$packageCode;ComponentCode=$componentCode;Version='3.3.0';Architecture='x64';PerUser=$true;Msi=$msi;SHA256=(Get-FileHash -LiteralPath $msi -Algorithm SHA256).Hash;ShortcutCount=1;CustomActionCount=1;CustomExecutableActionCount=0;CustomActionPurpose='Type 51 property assignment only';GameAndUserConfigurationIncluded=$false}
$manifest | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $stage 'package.json') -Encoding utf8
$manifest | ConvertTo-Json
