[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$CandidateMsi,
    [Parameter(Mandatory)][string]$PortableZip,
    [Parameter(Mandatory)][string]$EvidenceDirectory
)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$runtime=@('SC2Switcher.Wpf.exe','SC2Switcher.Wpf.dll','SC2Switcher.Wpf.deps.json','SC2Switcher.Wpf.runtimeconfig.json')

Add-Type -TypeDefinition @'
using System.Runtime.InteropServices;
using System.Text;
public static class MachineLifecycleMsiNative {
    [DllImport("msi.dll", CharSet=CharSet.Unicode)]
    public static extern uint MsiGetShortcutTarget(string shortcut, StringBuilder product, StringBuilder feature, StringBuilder component);
    [DllImport("msi.dll", CharSet=CharSet.Unicode)]
    public static extern int MsiGetComponentPath(string product, string component, StringBuilder path, ref uint length);
}
'@

if($env:GITHUB_ACTIONS-ne'true'-or$env:RUNNER_ENVIRONMENT-ne'github-hosted'-or$env:CI-ne'true'){
    throw 'This harness may run only on a GitHub-hosted Actions runner.'
}
$os=Get-CimInstance Win32_OperatingSystem
if($os.ProductType-ne1-or$env:RUNNER_ARCH-ne'ARM64'){throw 'This harness requires the GitHub-hosted Windows 11 ARM64 client runner.'}
$identity=[Security.Principal.WindowsIdentity]::GetCurrent()
$principal=[Security.Principal.WindowsPrincipal]::new($identity)
if(!$principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'The hosted runner token must be an administrator.'}

$CandidateMsi=[IO.Path]::GetFullPath($CandidateMsi)
$PortableZip=[IO.Path]::GetFullPath($PortableZip)
$EvidenceDirectory=[IO.Path]::GetFullPath($EvidenceDirectory)
foreach($path in @($CandidateMsi,$PortableZip)){if(!(Test-Path -LiteralPath $path -PathType Leaf)){throw "Missing input: $path"}}
[void][IO.Directory]::CreateDirectory($EvidenceDirectory)
$logs=Join-Path $EvidenceDirectory 'logs';$fixturesDir=Join-Path $EvidenceDirectory 'fixtures';$portableDir=Join-Path $EvidenceDirectory 'portable'
foreach($directory in @($logs,$fixturesDir,$portableDir)){[void][IO.Directory]::CreateDirectory($directory)}
$candidateHash=(Get-FileHash -LiteralPath $CandidateMsi -Algorithm SHA256).Hash
$zipHash=(Get-FileHash -LiteralPath $PortableZip -Algorithm SHA256).Hash
$steps=[Collections.Generic.List[object]]::new();$cleanupErrors=[Collections.Generic.List[string]]::new();$failure=$null;$status='running';$ownedLegacyKey=$false;$collisionHash=$null

function Stable-Guid([string]$Text){$sha=[Security.Cryptography.SHA256]::Create();try{$bytes=$sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($Text))}finally{$sha.Dispose()};'{'+([guid]::new([byte[]]$bytes[0..15])).ToString().ToUpperInvariant()+'}'}
function Open-Msi([string]$Path,[int]$Mode=0){$installer=New-Object -ComObject WindowsInstaller.Installer;$database=$installer.OpenDatabase($Path,$Mode);[pscustomobject]@{Installer=$installer;Database=$database}}
function Rows([string]$Path,[string]$Query,[int]$Count){$opened=Open-Msi $Path;try{$view=$opened.Database.OpenView($Query);try{[void]$view.Execute();while($record=$view.Fetch()){try{,$(@(for($n=1;$n-le$Count;$n++){$record.StringData($n)}))}finally{[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($record)}}}finally{[void]$view.Close();[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($view)}}finally{[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($opened.Database);[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($opened.Installer)}}
function Map($InputRows){$map=@{};foreach($row in $InputRows){$map[[string]$row[0]]=[string]$row[1]};$map}
function Sql($Database,[string]$Statement,[object[]]$Values=@()){$view=$Database.OpenView($Statement);$installer=$null;$record=$null;try{if(!$Values.Count){[void]$view.Execute();return};$installer=New-Object -ComObject WindowsInstaller.Installer;$record=$installer.CreateRecord($Values.Count);for($n=0;$n-lt$Values.Count;$n++){if($Values[$n]-is[int]){$record.IntegerData($n+1)=$Values[$n]}elseif($null-ne$Values[$n]){$record.StringData($n+1)=[string]$Values[$n]}};[void]$view.Execute($record)}finally{[void]$view.Close();foreach($item in @($record,$installer,$view)){if($null-ne$item-and[Runtime.InteropServices.Marshal]::IsComObject($item)){[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($item)}}}}
function Set-Value($Database,[string]$Table,[string]$Column,[string]$KeyColumn,[string]$Key,[string]$Value){Sql $Database ("UPDATE ``$Table`` SET ``$Column``=? WHERE ``$KeyColumn``=?") @($Value,$Key)}
function Set-FixtureUpgrade($Database,[string]$Code,[string]$Version){$view=$Database.OpenView('SELECT * FROM `Upgrade`');try{[void]$view.Execute();while($record=$view.Fetch()){try{[void]$view.Modify(6,$record);$record.StringData(1)=$Code;if($record.StringData(7)-eq'OLDPRODUCTS'){$record.StringData(2)='3.4.0';$record.StringData(3)=$Version}else{$record.StringData(2)=$Version};[void]$view.Modify(1,$record)}finally{[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($record)}}}finally{[void]$view.Close();[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($view)}}
function Product-State([string]$Code){$installer=New-Object -ComObject WindowsInstaller.Installer;try{[int]$installer.ProductState($Code)}finally{[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($installer)}}
function Invoke-Msi([string]$Label,[string[]]$Arguments,[int[]]$Expected=@(0)){$log=Join-Path $logs "$Label.log";$args=@($Arguments)+@('/qn','/norestart','REBOOT=ReallySuppress','/L*v',$log);$quoted=@($args|ForEach-Object{if($_-match'\s'){ '"'+$_.Replace('"','\"')+'"' }else{$_}});$process=Start-Process (Join-Path $env:WINDIR 'System32\msiexec.exe') -ArgumentList $quoted -WindowStyle Hidden -Wait -PassThru;$exit=[uint32]$process.ExitCode;$steps.Add([pscustomobject]@{Step=$Label;ExitCode=$exit;Expected=$Expected-contains$exit;Log=$log});if($Expected-notcontains$exit){throw "$Label returned $exit"};$exit}
function File-Hashes([string]$Directory,[bool]$Require=$true){$hashes=[ordered]@{};foreach($name in $runtime){$path=Join-Path $Directory $name;if($Require-and!(Test-Path -LiteralPath $path -PathType Leaf)){throw "Missing installed file: $path"};$hashes[$name]=if(Test-Path -LiteralPath $path -PathType Leaf){(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash}else{$null}};$hashes}
function Assert-Hashes($Expected,$Actual,[string]$Label){foreach($name in $runtime){if($Expected[$name]-ne$Actual[$name]){throw "$Label hash mismatch: $name"}}}
function Registry-Default([Microsoft.Win32.RegistryHive]$Hive,[string]$SubKey){$base=[Microsoft.Win32.RegistryKey]::OpenBaseKey($Hive,[Microsoft.Win32.RegistryView]::Registry64);try{$key=$base.OpenSubKey($SubKey);if(!$key){return $null};try{[string]$key.GetValue($null)}finally{$key.Dispose()}}finally{$base.Dispose()}}
function Shortcut-Target([string]$Path){if(!(Test-Path -LiteralPath $Path)){return $null};$product=[Text.StringBuilder]::new(39);$feature=[Text.StringBuilder]::new(39);$component=[Text.StringBuilder]::new(39);$result=[MachineLifecycleMsiNative]::MsiGetShortcutTarget($Path,$product,$feature,$component);if($result-ne0){throw "MsiGetShortcutTarget failed with $result for $Path"};$length=[uint32]32767;$target=[Text.StringBuilder]::new([int]$length);$state=[MachineLifecycleMsiNative]::MsiGetComponentPath($product.ToString(),$component.ToString(),$target,[ref]$length);if($state-ne3){throw "Advertised shortcut component state is $state for $Path"};[IO.Path]::GetFullPath($target.ToString())}
function Assert-Canary([string]$Path,[string]$Hash){if(!(Test-Path -LiteralPath $Path)-or(Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash-ne$Hash){throw 'User-state canary changed.'}}

function New-Fixture([string]$Role,[string]$Version,[string]$Token){
    $destination=Join-Path $fixturesDir "$Role.msi";Copy-Item -LiteralPath $CandidateMsi -Destination $destination
    $product=Stable-Guid "$Token/product/$Role";$upgrade=Stable-Guid "$Token/upgrade";$package=Stable-Guid "$Token/package/$Role"
    $opened=Open-Msi $destination 1
    try{
        foreach($pair in @(@('ProductCode',$product),@('ProductVersion',$Version),@('ProductName',"SC2 Machine QA $Token"),@('Manufacturer','SC2 Machine QA'),@('UpgradeCode',$upgrade))){Set-Value $opened.Database Property Value Property $pair[0] $pair[1]}
        Set-FixtureUpgrade $opened.Database $upgrade $Version
        Set-Value $opened.Database Directory DefaultDir Directory ProductRoot "SC2MQA|SC2MachineQA$Token"
        Set-Value $opened.Database Directory DefaultDir Directory INSTALLDIR $(if($Role-eq'baseline'){'old'}else{'app'})
        Set-Value $opened.Database Directory DefaultDir Directory MenuGroup "SC2MQA|SC2 Machine QA $Token"
        Set-Value $opened.Database Shortcut Name Shortcut StartMenu "SC2MQA|SC2 Machine QA $Token"
        Set-Value $opened.Database Registry Key Registry AppPath "Software\Microsoft\Windows\CurrentVersion\App Paths\SC2MachineQA.$Token.exe"
        Set-Value $opened.Database RegLocator Key Signature_ LegacyUserAppPath "Software\SC2MachineQA\$Token\LegacyPreview"
        foreach($component in @('Application','AppRegistration')){Set-Value $opened.Database Component ComponentId Component $component (Stable-Guid "$Token/component/$Role/$component")}
        $summary=$opened.Database.SummaryInformation(20);try{$summary.Property(3)="SC2 Machine QA $Token";$summary.Property(9)=$package;[void]$summary.Persist()}finally{[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($summary)}
        [void]$opened.Database.Commit()
    }finally{[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($opened.Database);[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($opened.Installer);[GC]::Collect();[GC]::WaitForPendingFinalizers()}
    [pscustomobject]@{Role=$Role;Version=$Version;Path=$destination;ProductCode=$product;UpgradeCode=$upgrade;PackageCode=$package;SHA256=(Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash}
}

Expand-Archive -LiteralPath $PortableZip -DestinationPath $portableDir
$portableHashes=File-Hashes $portableDir
$properties=Map @(Rows $CandidateMsi 'SELECT `Property`,`Value` FROM `Property`' 2)
$productCode=$properties.ProductCode
$installDir=Join-Path $env:ProgramFiles 'SC2RegionSwitcher\app'
$shortcut=Join-Path $env:ProgramData 'Microsoft\Windows\Start Menu\Programs\SC2 Region Switcher\SC2 Region Switcher.lnk'
$appPathKey='Software\Microsoft\Windows\CurrentVersion\App Paths\SC2Switcher.Wpf.exe'
$canary=Join-Path $env:LOCALAPPDATA 'SC2RegionSwitcherV2\installer-lifecycle-canary.json'
[void][IO.Directory]::CreateDirectory((Split-Path $canary));[IO.File]::WriteAllText($canary,'{"owner":"machine-installer-lifecycle","preserve":true}',[Text.UTF8Encoding]::new($false));$canaryHash=(Get-FileHash -LiteralPath $canary -Algorithm SHA256).Hash
$token=('R'+$env:GITHUB_RUN_ID+'A'+$env:GITHUB_RUN_ATTEMPT).ToUpperInvariant()
$fixtureRoot=Join-Path $env:ProgramFiles "SC2MachineQA$Token";$fixtureOld=Join-Path $fixtureRoot 'old';$fixtureApp=Join-Path $fixtureRoot 'app'
$fixtureShortcut=Join-Path $env:ProgramData "Microsoft\Windows\Start Menu\Programs\SC2 Machine QA $Token\SC2 Machine QA $Token.lnk"
$fixtureAppPath="Software\Microsoft\Windows\CurrentVersion\App Paths\SC2MachineQA.$Token.exe"
$baseline=New-Fixture baseline '3.4.1' $token;$candidate=New-Fixture candidate '3.4.2' $token;$newer=New-Fixture newer '3.5.0' $token;$rollback=New-Fixture rollback '3.4.2' $token
$fixtures=@($baseline,$candidate,$newer,$rollback)
foreach($fixture in $fixtures){
    $components=@(Rows $fixture.Path 'SELECT `Component`,`Attributes`,`KeyPath` FROM `Component`' 3)
    $registry=@(Rows $fixture.Path 'SELECT `Registry`,`Root`,`Component_` FROM `Registry`' 3)
    $shortcutRows=@(Rows $fixture.Path 'SELECT `Shortcut`,`Component_`,`Target` FROM `Shortcut`' 3)
    $componentMap=@{};foreach($row in $components){$componentMap[[string]$row[0]]=$row}
    if($components.Count-ne2-or$componentMap.Application[1]-ne'256'-or$componentMap.Application[2]-ne'AppExe'-or$componentMap.AppRegistration[1]-ne'260'-or$componentMap.AppRegistration[2]-ne'AppPath'){throw "Fixture component contract mismatch: $($fixture.Role)"}
    if($registry.Count-ne1-or$registry[0][0]-ne'AppPath'-or$registry[0][1]-ne'2'-or$registry[0][2]-ne'AppRegistration'){throw "Fixture registry contract mismatch: $($fixture.Role)"}
    if($shortcutRows.Count-ne1-or$shortcutRows[0][1]-ne'Application'-or$shortcutRows[0][2]-ne'MainFeature'){throw "Fixture advertised shortcut contract mismatch: $($fixture.Role)"}
}

function Assert-MachineResources($Fixture,[string]$Directory,$ExpectedHashes){
    if((Product-State $Fixture.ProductCode)-ne5){throw "$($Fixture.Role) is not registered installed."}
    Assert-Hashes $ExpectedHashes (File-Hashes $Directory) $Fixture.Role
    $expectedExe=Join-Path $Directory 'SC2Switcher.Wpf.exe'
    if((Shortcut-Target $fixtureShortcut)-ne$expectedExe){throw 'Common Start menu shortcut target mismatch.'}
    if((Registry-Default LocalMachine $fixtureAppPath)-ne$expectedExe){throw 'HKLM App Paths target mismatch.'}
}
function Assert-ProductionAbsent(){if((Product-State $productCode)-ne-1-or@($runtime|Where-Object{Test-Path -LiteralPath (Join-Path $installDir $_)}).Count-or(Test-Path -LiteralPath $shortcut)-or$null-ne(Registry-Default LocalMachine $appPathKey)){throw 'Production uninstall left product registration, payload, shortcut, or HKLM App Paths state.'}}
function Assert-FixturesAbsent(){foreach($fixture in $fixtures){if((Product-State $fixture.ProductCode)-ne-1){throw "Fixture remains registered: $($fixture.Role)"}};if(@($runtime|Where-Object{(Test-Path -LiteralPath (Join-Path $fixtureOld $_))-or(Test-Path -LiteralPath (Join-Path $fixtureApp $_))}).Count-or(Test-Path -LiteralPath $fixtureShortcut)-or$null-ne(Registry-Default LocalMachine $fixtureAppPath)){throw 'Fixture cleanup left payload, shortcut, or HKLM App Paths state.'}}

try{
    if($properties.ALLUSERS-ne'1'){throw 'Candidate is not authored per-machine.'}
    if((Product-State $productCode)-ne-1-or(Test-Path -LiteralPath $installDir)-or(Test-Path -LiteralPath $shortcut)-or$null-ne(Registry-Default LocalMachine $appPathKey)){throw 'Fresh hosted runner already contains production product state; refusing to remove or overwrite it.'}
    foreach($item in @($CandidateMsi,$PortableZip)+@($fixtures.Path)){Copy-Item -LiteralPath $item -Destination (Join-Path $EvidenceDirectory ('input-'+[IO.Path]::GetFileName($item))) -Force}
    [ordered]@{CandidateSHA256=$candidateHash;PortableSHA256=$zipHash;FixtureSHA256=@($fixtures|ForEach-Object{[ordered]@{Role=$_.Role;SHA256=$_.SHA256}})}|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $EvidenceDirectory 'input-hashes.json') -Encoding utf8
    [void](Invoke-Msi production-install @('/i',$CandidateMsi));$productionInstallLog=@(Get-Content -LiteralPath (Join-Path $logs 'production-install.log'));if(!($productionInstallLog-match'Property\(S\): MsiRunningElevated = 1')){throw 'Exact install log does not prove MsiRunningElevated=1.'};if((Product-State $productCode)-ne5){throw 'Production candidate is not installed.'};Assert-Hashes $portableHashes (File-Hashes $installDir) 'production install';if((Registry-Default LocalMachine $appPathKey)-ne(Join-Path $installDir 'SC2Switcher.Wpf.exe')){throw 'Production HKLM App Paths mismatch.'};if((Shortcut-Target $shortcut)-ne(Join-Path $installDir 'SC2Switcher.Wpf.exe')){throw 'Production common advertised shortcut mismatch.'};$arp=@(Get-ChildItem 'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall'|Where-Object{($_.GetValue('DisplayName'))-eq'SC2 Region Switcher'});if($arp.Count-ne1){throw "Expected exactly one machine ARP entry; found $($arp.Count)."};Assert-Canary $canary $canaryHash
    [void](Invoke-Msi production-maintenance @('/i',$CandidateMsi));Assert-Hashes $portableHashes (File-Hashes $installDir) 'production maintenance';Assert-Canary $canary $canaryHash
    [void](Invoke-Msi production-uninstall @('/x',$CandidateMsi));Assert-ProductionAbsent;Assert-Canary $canary $canaryHash
    [void](Invoke-Msi production-reinstall @('/i',$CandidateMsi));Assert-Hashes $portableHashes (File-Hashes $installDir) 'production reinstall';[void](Invoke-Msi production-final-uninstall @('/x',$CandidateMsi));Assert-ProductionAbsent;Assert-Canary $canary $canaryHash

    if(Test-Path -LiteralPath "HKCU:\$appPathKey"){throw 'Current user already owns the preview App Paths key; refusing to overwrite it.'};New-Item -Path "HKCU:\$appPathKey" -Force|Out-Null;Set-Item -LiteralPath "HKCU:\$appPathKey" -Value 'synthetic-preview-owned-by-lifecycle-test';$ownedLegacyKey=$true
    [void](Invoke-Msi preview-guard @('/i',$CandidateMsi) @(1603));if((Product-State $productCode)-ne-1-or(Test-Path -LiteralPath $installDir)){throw 'Preview guard mutated production installation state.'};Remove-Item -LiteralPath "HKCU:\$appPathKey" -Recurse -Force;$ownedLegacyKey=$false;Assert-Canary $canary $canaryHash

    foreach($fixture in $fixtures){[void](Invoke-Msi "fixture-preclean-$($fixture.Role)" @('/x',$fixture.Path) @(0,1605))}
    $fixtureHashes=$portableHashes
    [void](Invoke-Msi fixture-baseline-install @('/i',$baseline.Path));Assert-MachineResources $baseline $fixtureOld $fixtureHashes
    [void](Invoke-Msi fixture-success-upgrade @('/i',$candidate.Path));if((Product-State $baseline.ProductCode)-ne-1){throw 'Baseline remains after successful upgrade.'};Assert-MachineResources $candidate $fixtureApp $fixtureHashes
    [void](Invoke-Msi fixture-success-clean @('/x',$candidate.Path))

    [void](Invoke-Msi fixture-newer-install @('/i',$newer.Path));Assert-MachineResources $newer $fixtureApp $fixtureHashes;[void](Invoke-Msi fixture-downgrade-reject @('/i',$candidate.Path) @(1603));Assert-MachineResources $newer $fixtureApp $fixtureHashes;if((Product-State $candidate.ProductCode)-ne-1){throw 'Rejected downgrade registered candidate.'};[void](Invoke-Msi fixture-newer-clean @('/x',$newer.Path))

    [void](Invoke-Msi fixture-lock-baseline @('/i',$baseline.Path));$lock=[IO.File]::Open((Join-Path $fixtureOld 'SC2Switcher.Wpf.exe'),[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read);try{$lockExit=Invoke-Msi fixture-file-in-use @('/i',$candidate.Path) @(0,1603,3010);if(!$lock.CanRead){throw 'Installer closed the test-owned file handle.'}}finally{$lock.Dispose()};if($lockExit-eq1603){Assert-MachineResources $baseline $fixtureOld $fixtureHashes}else{Assert-MachineResources $candidate $fixtureApp $fixtureHashes};foreach($fixture in @($candidate,$baseline)){[void](Invoke-Msi "fixture-lock-clean-$($fixture.Role)" @('/x',$fixture.Path) @(0,1605))}

    [void](Invoke-Msi fixture-rollback-baseline @('/i',$baseline.Path));$before=File-Hashes $fixtureOld
    if(Test-Path -LiteralPath $fixtureApp){throw 'Rollback collision path already exists.'};[IO.File]::WriteAllText($fixtureApp,'native-file-collision',[Text.UTF8Encoding]::new($false));$collisionHash=(Get-FileHash -LiteralPath $fixtureApp -Algorithm SHA256).Hash
    [void](Invoke-Msi fixture-native-failure @('/i',$rollback.Path) @(1603));$lines=@(Get-Content -LiteralPath (Join-Path $logs 'fixture-native-failure.log'));$remove=@($lines|Select-String 'Action ended .*RemoveExistingProducts\. Return value 1\.'|Select-Object -First 1);$copy=@($lines|Select-String 'Executing op: FileCopy\(.*DestName=SC2Switcher\.Wpf\.exe'|Where-Object{$remove-and$_.LineNumber-gt$remove.LineNumber}|Select-Object -First 1);$target=@($lines|Select-String -SimpleMatch ("File: "+(Join-Path $fixtureApp 'SC2Switcher.Wpf.exe')+';')|Where-Object{$copy-and$_.LineNumber-gt$copy.LineNumber}|Select-Object -First 1);$error1312=@($lines|Select-String 'Error 1312\.'|Where-Object{$target-and$_.LineNumber-gt$target.LineNumber-and$_.Line.IndexOf($fixtureApp,[StringComparison]::OrdinalIgnoreCase)-ge0}|Select-Object -First 1);$finalize=@($lines|Select-String 'Action ended .*InstallFinalize\. Return value 3\.'|Where-Object{$error1312-and$_.LineNumber-gt$error1312.LineNumber}|Select-Object -First 1);$rollbackStart=@($lines|Select-String 'Executing op: RollbackInfo\('|Where-Object{$finalize-and$_.LineNumber-gt$finalize.LineNumber}|Select-Object -First 1);if(!($lines-match'Property\(S\): MsiRunningElevated = 1')-or!$remove-or!$copy-or!$target-or!$error1312-or!$finalize-or!$rollbackStart){throw 'Native failure log lacks ordered elevation, removal, candidate file copy/target, error 1312, failed InstallFinalize, or rollback evidence.'};if((Product-State $baseline.ProductCode)-ne5-or(Product-State $rollback.ProductCode)-ne-1){throw 'Native failure product-state restoration failed.'};Assert-Hashes $before (File-Hashes $fixtureOld) 'native rollback';if((Shortcut-Target $fixtureShortcut)-ne(Join-Path $fixtureOld 'SC2Switcher.Wpf.exe')-or(Registry-Default LocalMachine $fixtureAppPath)-ne(Join-Path $fixtureOld 'SC2Switcher.Wpf.exe')){throw 'Native rollback entry points were not restored.'};Assert-Canary $canary $canaryHash
    if(!(Test-Path -LiteralPath $fixtureApp -PathType Leaf)-or(Get-FileHash -LiteralPath $fixtureApp).Hash-ne$collisionHash){throw 'Collision sentinel changed unexpectedly.'};Remove-Item -LiteralPath $fixtureApp -Force;[void](Invoke-Msi fixture-rollback-clean @('/x',$baseline.Path));Assert-FixturesAbsent
    if((Get-FileHash -LiteralPath $CandidateMsi -Algorithm SHA256).Hash-ne$candidateHash-or(Get-FileHash -LiteralPath $PortableZip -Algorithm SHA256).Hash-ne$zipHash){throw 'Original candidate inputs changed.'}
    $status='passed'
}catch{$status='failed';$failure=$_.Exception.ToString();throw}finally{
    if($ownedLegacyKey-and(Test-Path -LiteralPath "HKCU:\$appPathKey")){try{if([string](Get-Item -LiteralPath "HKCU:\$appPathKey").GetValue('')-eq'synthetic-preview-owned-by-lifecycle-test'){Remove-Item -LiteralPath "HKCU:\$appPathKey" -Recurse -Force}else{$cleanupErrors.Add('Owned preview key value changed; cleanup refused.')}}catch{$cleanupErrors.Add($_.Exception.Message)}}
    foreach($fixture in @($rollback,$newer,$candidate,$baseline)){try{[void](Invoke-Msi "final-clean-$($fixture.Role)" @('/x',$fixture.Path) @(0,1605))}catch{$cleanupErrors.Add($_.Exception.Message)}}
    try{[void](Invoke-Msi production-emergency-clean @('/x',$CandidateMsi) @(0,1605))}catch{$cleanupErrors.Add($_.Exception.Message)}
    if(Test-Path -LiteralPath $fixtureApp -PathType Leaf){try{if($collisionHash-and(Get-FileHash -LiteralPath $fixtureApp).Hash-eq$collisionHash){Remove-Item -LiteralPath $fixtureApp -Force}else{$cleanupErrors.Add('Collision sentinel bytes changed; cleanup refused.')}}catch{$cleanupErrors.Add($_.Exception.Message)}}
    try{Assert-ProductionAbsent}catch{$cleanupErrors.Add($_.Exception.Message)}
    try{Assert-FixturesAbsent}catch{$cleanupErrors.Add($_.Exception.Message)}
    try{Assert-Canary $canary $canaryHash}catch{$cleanupErrors.Add($_.Exception.Message)}
    if($cleanupErrors.Count){$status='failed';$cleanupText='Cleanup verification failed: '+($cleanupErrors -join ' | ');$failure=if($failure){$failure+"`n"+$cleanupText}else{$cleanupText}}
    $report=[ordered]@{SchemaVersion=1;Status=$status;CompletedUtc=(Get-Date).ToUniversalTime().ToString('o');Environment=[ordered]@{OS=$os.Caption;Version=$os.Version;Architecture=$env:RUNNER_ARCH;Identity=$identity.Name;Administrator=$true};Inputs=[ordered]@{Candidate=[ordered]@{Path=$CandidateMsi;SHA256=$candidateHash};Portable=[ordered]@{Path=$PortableZip;SHA256=$zipHash}};ProductionLifecycleSeparated=$true;ElevationEvidence=[ordered]@{ExactInstallLog='logs/production-install.log';NativeRecoveryLog='logs/fixture-native-failure.log';RequiredProperty='Property(S): MsiRunningElevated = 1'};Fixtures=$fixtures;Canary=[ordered]@{Path=$canary;SHA256=$canaryHash};Steps=$steps;Failure=$failure}
    $report|ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $EvidenceDirectory 'result.json') -Encoding utf8
}

$report|ConvertTo-Json -Depth 8
if($status-ne'passed'){throw $failure}
