[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateScript({ Test-Path -LiteralPath $_ -PathType Leaf })][string]$CandidateMsi,
    [Parameter(Mandatory)][ValidateScript({ Test-Path -LiteralPath $_ -PathType Leaf })][string]$BaselineMsi,
    [string]$EvidenceDirectory = (Join-Path $PSScriptRoot ("../../artifacts/validation/3.4.0/installer-lab/{0:yyyyMMdd-HHmmss}-{1}" -f (Get-Date), ([guid]::NewGuid().ToString('N').Substring(0, 8)))),
    [switch]$ExplicitShortcutTargetFixture,
    [switch]$CleanInstallOnly
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
using System.Text;
public static class LifecycleMsiNative {
    [DllImport("msi.dll", CharSet=CharSet.Unicode)]
    public static extern uint MsiGetShortcutTarget(string shortcut, StringBuilder product, StringBuilder feature, StringBuilder component);
    [DllImport("msi.dll", CharSet=CharSet.Unicode)]
    public static extern int MsiGetComponentPath(string product, string component, StringBuilder path, ref uint length);
}
'@

$production = [ordered]@{
    ProductName = 'SC2 Region Switcher'
    UpgradeCode = '{4A67EAD9-86CA-450C-ABDC-5D6C1E4A4CCD}'
    ProductRoot = 'SC2RegionSwitcher'
    MenuGroup = 'SC2 Region Switcher'
    AppPathKey = 'Software\Microsoft\Windows\CurrentVersion\App Paths\SC2Switcher.Wpf.exe'
    MarkerKey = 'Software\SC2RegionSwitcher\Installer'
}

function New-StableGuid([string]$Value) {
    $sha = [Security.Cryptography.SHA256]::Create()
    try { $bytes = $sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($Value)) } finally { $sha.Dispose() }
    return '{' + ([guid]::new([byte[]]$bytes[0..15])).ToString().ToUpperInvariant() + '}'
}

function Open-Msi([string]$Path, [int]$Mode) {
    $installer = New-Object -ComObject WindowsInstaller.Installer
    $database = $installer.OpenDatabase($Path, $Mode)
    return [pscustomobject]@{ Installer = $installer; Database = $database }
}

function Invoke-MsiSql($Database, [string]$Statement, [object[]]$Values = @()) {
    $view = $Database.OpenView($Statement)
    $params = $null
    $installer = $null
    try {
        if (-not $Values.Count) { $view.Execute(); return }
        $installer = New-Object -ComObject WindowsInstaller.Installer
        $params = $installer.CreateRecord($Values.Count)
        for ($index = 0; $index -lt $Values.Count; $index++) {
            if ($Values[$index] -is [int]) { $null = ($params.IntegerData($index + 1) = $Values[$index]) }
            elseif ($null -ne $Values[$index]) { $null = ($params.StringData($index + 1) = [string]$Values[$index]) }
        }
        [void]$view.Execute($params)
    } finally {
        if ($null -ne $view) { [void]$view.Close() }
        foreach ($item in @($params, $installer)) {
            if ($null -ne $item -and [Runtime.InteropServices.Marshal]::IsComObject($item)) {
                [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($item)
            }
        }
    }
}

function Get-MsiRows([string]$Path, [string]$Query, [int]$Columns) {
    $opened = Open-Msi $Path 0
    $view = $opened.Database.OpenView($Query)
    $rows = @()
    try {
        [void]$view.Execute()
        while ($record = $view.Fetch()) {
            $values = for ($column = 1; $column -le $Columns; $column++) { $record.StringData($column) }
            $rows += [pscustomobject]@{ C1=$values[0]; C2=$(if ($Columns -ge 2) { $values[1] } else { $null }) }
            [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($record)
        }
    } finally {
        [void]$view.Close()
        [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($view)
        [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($opened.Database)
        [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($opened.Installer)
    }
    return $rows
}

function Set-MsiValue($Database, [string]$Table, [string]$Column, [string]$KeyColumn, [string]$Key, [string]$Value) {
    Invoke-MsiSql $Database ("UPDATE ``$Table`` SET ``$Column`` = ? WHERE ``$KeyColumn`` = ?") @($Value, $Key)
}

function Set-MsiUpgradeCode($Database, [string]$UpgradeCode) {
    $view = $Database.OpenView('SELECT * FROM `Upgrade`')
    try {
        [void]$view.Execute()
        while ($record = $view.Fetch()) {
            try {
                [void]$view.Modify(6, $record) # msiViewModifyDelete removes the row under its old composite key.
                $null = ($record.StringData(1) = $UpgradeCode)
                [void]$view.Modify(1, $record) # msiViewModifyInsert adds it under the isolated family key.
            } finally { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($record) }
        }
    } finally { [void]$view.Close(); [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($view) }
}

function New-FixtureMsi([string]$Source, [string]$Destination, [string]$Role, [string]$Version, [bool]$FailAfterExecute) {
    Copy-Item -LiteralPath $Source -Destination $Destination
    $opened = Open-Msi $Destination 1
    $summary = $null
    try {
        $productCode = New-StableGuid "$runId/product/$Role/$Version"
        $packageCode = New-StableGuid "$runId/package/$Role/$Version"
        foreach ($entry in @{
            ProductCode=$productCode; ProductVersion=$Version; ProductName=$fixtureName; Manufacturer=$fixtureName;
            UpgradeCode=$fixtureUpgrade; ARPCOMMENTS='Isolated installer lifecycle fixture; never launch.'
        }.GetEnumerator()) { Set-MsiValue $opened.Database Property Value Property $entry.Key $entry.Value }
        # The historical 3.3.0 baseline predates the Upgrade table. The 3.4+
        # package discovers and removes it through the candidate's Upgrade rows.
        if ($Role -ne 'baseline') { Set-MsiUpgradeCode $opened.Database $fixtureUpgrade }
        Set-MsiValue $opened.Database Directory DefaultDir Directory ProductRoot ("SC2QA|$fixtureRoot")
        Set-MsiValue $opened.Database Directory DefaultDir Directory MenuGroup ("SC2QA|$fixtureMenu")
        Set-MsiValue $opened.Database Shortcut Name Shortcut StartMenu ("SC2QA|$fixtureShortcut")
        Set-MsiValue $opened.Database Shortcut Description Shortcut StartMenu 'Isolated SC2 Region Switcher installer test fixture'
        Set-MsiValue $opened.Database Registry Key Registry AppPath $fixtureAppPathKey
        Set-MsiValue $opened.Database Registry Key Registry MenuMarker $fixtureMarkerKey
        Set-MsiValue $opened.Database Registry Key Registry ApplicationMarker $fixtureMarkerKey
        if ($ExplicitShortcutTargetFixture) {
            Set-MsiValue $opened.Database Shortcut Target Shortcut StartMenu '[INSTALLDIR]SC2Switcher.Wpf.exe'
            Set-MsiValue $opened.Database Registry Value Registry AppPath '[INSTALLDIR]SC2Switcher.Wpf.exe'
        }
        foreach ($component in @('Application','StartMenu','AppRegistration')) {
            $componentGeneration = if ($Role -eq 'baseline') { 'legacy-3.3.0' } else { 'current' }
            Set-MsiValue $opened.Database Component ComponentId Component $component (New-StableGuid "$runId/component/$componentGeneration/$component")
        }
        Set-MsiValue $opened.Database Feature Title Feature MainFeature $fixtureName
        Set-MsiValue $opened.Database Feature Description Feature MainFeature 'Isolated lifecycle-test application and one shortcut'
        if ($FailAfterExecute) {
            Invoke-MsiSql $opened.Database 'INSERT INTO `InstallExecuteSequence` (`Action`,`Condition`,`Sequence`) VALUES (?,?,?)' @('InstallExecute',$null,[int]6450)
            Invoke-MsiSql $opened.Database 'INSERT INTO `CustomAction` (`Action`,`Type`,`Source`,`Target`) VALUES (?,?,?,?)' @('FixtureRollbackFailure',[int]19,$null,'Intentional isolated lifecycle rollback test failure.')
            Invoke-MsiSql $opened.Database 'INSERT INTO `InstallExecuteSequence` (`Action`,`Condition`,`Sequence`) VALUES (?,?,?)' @('FixtureRollbackFailure','NOT Installed',[int]6500)
        }
        $summary = $opened.Database.SummaryInformation(20)
        $null = ($summary.Property(3) = $fixtureName)
        $null = ($summary.Property(4) = $fixtureName)
        $null = ($summary.Property(8) = $fixtureName)
        $null = ($summary.Property(9) = $packageCode)
        $null = ($summary.Property(18) = 'Isolated lifecycle-test fixture builder')
        [void]$summary.Persist()
        [void]$opened.Database.Commit()
        $fixtureInstallDirectory = Join-Path $env:LOCALAPPDATA ("Programs\$fixtureRoot\" + $(if ($Role -eq 'baseline') { '3.3.0' } else { 'app' }))
    } finally {
        foreach ($item in @($summary, $opened.Database, $opened.Installer)) {
            if ($null -ne $item -and [Runtime.InteropServices.Marshal]::IsComObject($item)) { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($item) }
        }
        [GC]::Collect(); [GC]::WaitForPendingFinalizers()
    }
    return [pscustomobject]@{ Role=$Role; Version=$Version; ProductCode=$productCode; PackageCode=$packageCode; Path=$Destination; InstallDirectory=$fixtureInstallDirectory; SHA256=(Get-FileHash -LiteralPath $Destination -Algorithm SHA256).Hash }
}

function Invoke-Msi([string]$Label, [string[]]$Arguments, [int[]]$ExpectedExitCodes = @(0)) {
    $log = Join-Path $logs "$Label.log"
    $allArguments = @($Arguments) + @('/qn','/norestart','REBOOT=ReallySuppress','/L*v', $log)
    $process = Start-Process -FilePath (Join-Path $env:WINDIR 'System32/msiexec.exe') -ArgumentList $allArguments -WindowStyle Hidden -Wait -PassThru
    $exitCode = [uint32]$process.ExitCode
    $script:results += [pscustomobject]@{ Step=$Label; ExitCode=$exitCode; Expected=($ExpectedExitCodes -contains $exitCode); Log=$log }
    if ($ExpectedExitCodes -notcontains $exitCode) { throw "$Label returned unexpected MSI exit code $exitCode. See $log" }
    return $exitCode
}

function Get-MsiInstalled([string]$ProductCode) {
    $installer = New-Object -ComObject WindowsInstaller.Installer
    try { $installed = ([int]$installer.ProductState($ProductCode) -eq 5) } finally { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($installer) }
    return $installed
}

function Test-Installed([string]$ProductCode, [bool]$Expected) {
    $installed = Get-MsiInstalled $ProductCode
    if ($installed -ne $Expected) { throw "Product state mismatch for $ProductCode. Expected installed=$Expected; actual=$installed." }
}

function Get-MsiProductInfo([string]$ProductCode, [string]$Property) {
    $installer = New-Object -ComObject WindowsInstaller.Installer
    try { return [string]$installer.ProductInfo($ProductCode, $Property) }
    finally { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($installer) }
}

function Get-FileHashes([string]$Directory) {
    $hashes = [ordered]@{}
    foreach ($name in @('SC2Switcher.Wpf.exe','SC2Switcher.Wpf.dll','SC2Switcher.Wpf.deps.json','SC2Switcher.Wpf.runtimeconfig.json')) {
        $path = Join-Path $Directory $name
        if (!(Test-Path -LiteralPath $path -PathType Leaf)) { throw "Expected payload file is missing: $path" }
        $hashes[$name] = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
    }
    return $hashes
}

function Get-MsiPayloadHashes([string]$Path, [string]$ExtractDirectory) {
    $opened = Open-Msi $Path 0
    try {
        $fileMap = @{}
        $fileView = $opened.Database.OpenView('SELECT `File`,`FileName` FROM `File`')
        try {
            [void]$fileView.Execute()
            while ($fileRecord = $fileView.Fetch()) {
                try { $fileMap[($fileRecord.StringData(2) -split '\|')[-1]] = $fileRecord.StringData(1) }
                finally { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($fileRecord) }
            }
        } finally { [void]$fileView.Close(); [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($fileView) }
        $streamView = $opened.Database.OpenView("SELECT `Data` FROM `_Streams` WHERE `Name`='app.cab'")
        try {
            [void]$streamView.Execute()
            $streamRecord = $streamView.Fetch()
            if ($null -eq $streamRecord) { throw 'Embedded app.cab is missing.' }
            try {
                $raw = [string]$streamRecord.ReadStream(1, [int]$streamRecord.DataSize(1), 1)
                $cabBytes = [byte[]]::new($raw.Length)
                for ($index = 0; $index -lt $raw.Length; $index++) { $cabBytes[$index] = [byte][int]$raw[$index] }
            } finally { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($streamRecord) }
        } finally { [void]$streamView.Close(); [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($streamView) }
    } finally {
        [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($opened.Database)
        [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($opened.Installer)
    }
    $cabPath = Join-Path $ExtractDirectory 'app.cab'
    [IO.File]::WriteAllBytes($cabPath, $cabBytes)
    & (Join-Path $env:WINDIR 'System32/expand.exe') '-F:*' $cabPath $ExtractDirectory | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Cabinet extraction failed for $Path" }
    $hashes = [ordered]@{}
    foreach ($name in @('SC2Switcher.Wpf.exe','SC2Switcher.Wpf.dll','SC2Switcher.Wpf.deps.json','SC2Switcher.Wpf.runtimeconfig.json')) {
        $hashes[$name] = (Get-FileHash -LiteralPath (Join-Path $ExtractDirectory $fileMap[$name]) -Algorithm SHA256).Hash
    }
    return $hashes
}

function Assert-HashesEqual($Expected, $Actual, [string]$Context) {
    foreach ($name in $Expected.Keys) { if ($Expected[$name] -ne $Actual[$name]) { throw "$Context SHA-256 mismatch: $name" } }
}

function Assert-InstalledResources($Fixture, $ExpectedHashes) {
    Test-Installed $Fixture.ProductCode $true
    Assert-HashesEqual $ExpectedHashes (Get-FileHashes $Fixture.InstallDirectory) $Fixture.Role
    $shortcuts = @(Get-ChildItem -LiteralPath $menuDir -Filter '*.lnk' -File -ErrorAction Stop)
    if ($shortcuts.Count -ne 1 -or $shortcuts[0].BaseName -ne $fixtureShortcut) { throw "Expected exactly one isolated Start menu shortcut." }
    $expectedTarget = [IO.Path]::GetFullPath((Join-Path $Fixture.InstallDirectory 'SC2Switcher.Wpf.exe'))
    if ($Fixture.Role -ne 'baseline') {
    $wsh = New-Object -ComObject WScript.Shell
    $shell = New-Object -ComObject Shell.Application
    try {
        $wshTarget = [string]$wsh.CreateShortcut($shortcuts[0].FullName).TargetPath
        $folder = $shell.Namespace($shortcuts[0].Directory.FullName)
        $shellTarget = [string]$folder.ParseName($shortcuts[0].Name).ExtendedProperty('System.Link.TargetParsingPath')
    } finally {
        [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($wsh)
        [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($shell)
    }
    if ($wshTarget -and [IO.Path]::GetFullPath($wshTarget) -eq $expectedTarget) { $shortcutTarget = $expectedTarget }
    elseif ($shellTarget -and [IO.Path]::GetFullPath($shellTarget) -eq $expectedTarget) { $shortcutTarget = $expectedTarget }
    else {
    # Advertised shortcuts require MSI's product/component resolution.
    $shortcutProduct = [Text.StringBuilder]::new(39)
    $shortcutFeature = [Text.StringBuilder]::new(39)
    $shortcutComponent = [Text.StringBuilder]::new(39)
    $shortcutResult = [LifecycleMsiNative]::MsiGetShortcutTarget($shortcuts[0].FullName, $shortcutProduct, $shortcutFeature, $shortcutComponent)
    if ($shortcutResult -ne 0 -or $shortcutProduct.ToString() -ne $Fixture.ProductCode) { throw "Fixture shortcut target mismatch. WSH=$wshTarget; Shell=$shellTarget; MSI error=$shortcutResult" }
    $targetLength = [uint32]32767
    $targetBuffer = [Text.StringBuilder]::new([int]$targetLength)
    $componentState = [LifecycleMsiNative]::MsiGetComponentPath($Fixture.ProductCode, $shortcutComponent.ToString(), $targetBuffer, [ref]$targetLength)
    if ($componentState -ne 3) { throw "Fixture shortcut component is not installed locally (state $componentState)." }
    $shortcutTarget = [IO.Path]::GetFullPath($targetBuffer.ToString())
    }
    if ($shortcutTarget -ne $expectedTarget) { throw "Fixture shortcut target mismatch. Expected $expectedTarget; actual $shortcutTarget" }
    }
    $appPath = "HKCU:\$fixtureAppPathKey"
    if (!(Test-Path -LiteralPath $appPath)) { throw "Fixture App Paths registration is missing: $appPath" }
    $registeredTarget = [IO.Path]::GetFullPath([string](Get-ItemPropertyValue -LiteralPath $appPath -Name '(default)'))
    if ($registeredTarget -ne $expectedTarget) { throw "Fixture App Paths target mismatch. Expected $expectedTarget; actual $registeredTarget" }
    # Windows Installer may store per-user ARP data in a packed Installer key
    # instead of HKCU's conventional Uninstall key. ProductInfo reads the
    # published registration that Add/Remove Programs consumes.
    if ((Get-MsiProductInfo $Fixture.ProductCode 'ProductName') -ne $fixtureName) { throw 'Fixture ARP product registration is missing or has the wrong display name.' }
}

$CandidateMsi = [IO.Path]::GetFullPath($CandidateMsi)
$BaselineMsi = [IO.Path]::GetFullPath($BaselineMsi)
$EvidenceDirectory = [IO.Path]::GetFullPath($EvidenceDirectory)
if ($CandidateMsi -eq $BaselineMsi) { throw 'Candidate and baseline MSI paths must differ.' }
$candidateOriginalHash = (Get-FileHash -LiteralPath $CandidateMsi -Algorithm SHA256).Hash
$baselineOriginalHash = (Get-FileHash -LiteralPath $BaselineMsi -Algorithm SHA256).Hash
$runId = Split-Path -Leaf $EvidenceDirectory
$safeToken = ([regex]::Replace($runId, '[^A-Za-z0-9]', '')).ToUpperInvariant()
if ($safeToken.Length -lt 8) { throw 'Evidence directory leaf must provide at least eight alphanumeric characters.' }
$safeToken = $safeToken.Substring([Math]::Max(0, $safeToken.Length - 12))
$fixtureName = "SC2 Region Switcher Installer QA $safeToken"
$fixtureRoot = "SC2RSQA$safeToken"
$fixtureMenu = "SC2 Installer QA $safeToken"
$fixtureShortcut = "SC2 Installer QA $safeToken"
$fixtureUpgrade = New-StableGuid "$runId/isolated-upgrade-family"
$fixtureAppPathKey = "Software\Microsoft\Windows\CurrentVersion\App Paths\SC2Switcher.QA.$safeToken.exe"
$fixtureMarkerKey = "Software\SC2RegionSwitcherInstallerQA\$safeToken\Installer"
$installDir = Join-Path $env:LOCALAPPDATA "Programs\$fixtureRoot\app"
$menuDir = Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs\$fixtureMenu"
$canaryDir = Join-Path $env:LOCALAPPDATA "SC2RegionSwitcherInstallerQA\$safeToken\Configuration"
$canaryPath = Join-Path $canaryDir 'preserve-canary.json'
$fixturesDir = Join-Path $EvidenceDirectory 'fixtures'
$logs = Join-Path $EvidenceDirectory 'logs'
$images = Join-Path $EvidenceDirectory 'administrative-images'
foreach ($directory in @($EvidenceDirectory,$fixturesDir,$logs,$images,$canaryDir)) { [void](New-Item -ItemType Directory -Path $directory -Force) }
$results = @()
$status = 'not-run'
$failure = $null
$lockResult = $null
$scenarioFailures = @()

$baseline = @(New-FixtureMsi $BaselineMsi (Join-Path $fixturesDir 'baseline-3.3.0-isolated.msi') 'baseline' '3.3.0' $false) | Where-Object { $_ -is [pscustomobject] -and $_.PSObject.Properties['Path'] } | Select-Object -Last 1
$candidate = @(New-FixtureMsi $CandidateMsi (Join-Path $fixturesDir 'candidate-3.4.0-isolated.msi') 'candidate' '3.4.0' $false) | Where-Object { $_ -is [pscustomobject] -and $_.PSObject.Properties['Path'] } | Select-Object -Last 1
$rollback = @(New-FixtureMsi $CandidateMsi (Join-Path $fixturesDir 'rollback-3.4.0-isolated.msi') 'rollback' '3.4.0' $true) | Where-Object { $_ -is [pscustomobject] -and $_.PSObject.Properties['Path'] } | Select-Object -Last 1
$newer = @(New-FixtureMsi $CandidateMsi (Join-Path $fixturesDir 'newer-3.5.0-isolated.msi') 'newer' '3.5.0' $false) | Where-Object { $_ -is [pscustomobject] -and $_.PSObject.Properties['Path'] } | Select-Object -Last 1
$fixtures = @($baseline,$candidate,$rollback,$newer)

# Refuse execution if a fixture retains any production family identity or target.
foreach ($fixture in $fixtures) {
    $properties = Get-MsiRows $fixture.Path 'SELECT `Property`,`Value` FROM `Property`' 2
    $directories = Get-MsiRows $fixture.Path 'SELECT `Directory`,`DefaultDir` FROM `Directory`' 2
    $registry = Get-MsiRows $fixture.Path 'SELECT `Registry`,`Key` FROM `Registry`' 2
    $shortcuts = Get-MsiRows $fixture.Path 'SELECT `Shortcut`,`Name` FROM `Shortcut`' 2
    $components = Get-MsiRows $fixture.Path 'SELECT `Component`,`ComponentId` FROM `Component`' 2
    $propertyMap = @{}; foreach ($row in $properties) { $propertyMap[$row.C1] = $row.C2 }
    $directoryMap = @{}; foreach ($row in $directories) { $directoryMap[$row.C1] = $row.C2 }
    $registryMap = @{}; foreach ($row in $registry) { $registryMap[$row.C1] = $row.C2 }
    $shortcutMap = @{}; foreach ($row in $shortcuts) { $shortcutMap[$row.C1] = $row.C2 }
    if ($propertyMap.UpgradeCode -eq $production.UpgradeCode -or $propertyMap.ProductCode -match '^\{(4A67EAD9|9543F8B1|95AD008A|D2F017E8)') { throw "Fixture $($fixture.Role) retains a production identity." }
    foreach ($row in $directories) { if ($row.C2 -match '(^|\|)SC2RegionSwitcher$' -or $row.C2 -match '(^|\|)SC2 Region Switcher$') { throw "Fixture $($fixture.Role) retains a production directory." } }
    foreach ($row in $registry) { if ($row.C2 -eq $production.AppPathKey -or $row.C2 -eq $production.MarkerKey -or $row.C2 -like 'Software\SC2RegionSwitcher\*') { throw "Fixture $($fixture.Role) retains a production registry resource." } }
    if ($shortcutMap.StartMenu -match '(^|\|)SC2 Region Switcher$') { throw "Fixture $($fixture.Role) retains the production shortcut." }
    foreach ($row in $components) { if ($row.C2 -in @('{9543F8B1-925C-4250-B5B6-E513266751F0}','{6AAC927A-CAEB-4A06-B4F4-AF6F4EBE5DC6}','{95AD008A-E416-4E6A-8E52-0D0A0FCA3B94}','{D2F017E8-3B57-4DD8-BD70-456D91E101D1}')) { throw "Fixture $($fixture.Role) retains a production component GUID." } }
}
if ($fixtureUpgrade -eq $production.UpgradeCode) { throw 'Fixture UpgradeCode overlaps production.' }

# Extract the embedded cabinet directly; the packages intentionally have no
# AdminExecuteSequence, and payload inspection must not register a product.
$expected = @{}
foreach ($fixture in @($baseline,$candidate,$newer)) {
    $image = Join-Path $images $fixture.Role
    [void](New-Item -ItemType Directory -Path $image)
    $expected[$fixture.Role] = Get-MsiPayloadHashes $fixture.Path $image
}

[IO.File]::WriteAllText($canaryPath, '{"owner":"installer-lifecycle-lab","preserve":true}', [Text.UTF8Encoding]::new($false))
$canaryHash = (Get-FileHash -LiteralPath $canaryPath -Algorithm SHA256).Hash

try {
    foreach ($fixture in $fixtures) { Invoke-Msi "preclean-$($fixture.Role)" @('/x', $fixture.Path) @(0,1605) }

    Invoke-Msi 'clean-install-candidate' @('/i', $candidate.Path)
    Assert-InstalledResources $candidate $expected.candidate
    if ($CleanInstallOnly) {
        $installedShortcut = Get-ChildItem -LiteralPath $menuDir -Filter '*.lnk' -File | Select-Object -First 1
        $evidenceShortcut = Join-Path $EvidenceDirectory 'installed-shortcut.lnk'
        Copy-Item -LiteralPath $installedShortcut.FullName -Destination $evidenceShortcut
        $wsh = New-Object -ComObject WScript.Shell; $shell = New-Object -ComObject Shell.Application
        try {
            $folder = $shell.Namespace($installedShortcut.Directory.FullName)
            [ordered]@{
                SHA256=(Get-FileHash -LiteralPath $evidenceShortcut -Algorithm SHA256).Hash
                WshTargetPath=[string]$wsh.CreateShortcut($installedShortcut.FullName).TargetPath
                ShellTargetParsingPath=[string]$folder.ParseName($installedShortcut.Name).ExtendedProperty('System.Link.TargetParsingPath')
                PrintableStrings=[Text.Encoding]::Unicode.GetString([IO.File]::ReadAllBytes($evidenceShortcut)) -replace '[^\x20-\x7E]',' '
            } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $EvidenceDirectory 'shortcut-probe.json') -Encoding utf8
        } finally { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($wsh); [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($shell) }
        $status = 'passed-clean-install-only'
        return
    }
    Invoke-Msi 'exact-msi-rerun' @('/i', $candidate.Path)
    Assert-InstalledResources $candidate $expected.candidate
    Invoke-Msi 'clean-uninstall-candidate' @('/x', $candidate.Path)
    Test-Installed $candidate.ProductCode $false
    Invoke-Msi 'reinstall-candidate' @('/i', $candidate.Path)
    Assert-InstalledResources $candidate $expected.candidate
    Invoke-Msi 'uninstall-after-reinstall' @('/x', $candidate.Path)

    Invoke-Msi 'install-baseline-for-rollback' @('/i', $baseline.Path)
    Assert-InstalledResources $baseline $expected.baseline
    Invoke-Msi 'intentional-rollback' @('/i', $rollback.Path) @(1603)
    $baselineRestored = Get-MsiInstalled $baseline.ProductCode
    $failedProductAbsent = !(Get-MsiInstalled $rollback.ProductCode)
    if ($baselineRestored -and $failedProductAbsent) {
        Assert-InstalledResources $baseline $expected.baseline
        $results += [pscustomobject]@{Step='rollback-restoration';ExitCode=$null;Expected=$true;Outcome='passed';Log=(Join-Path $logs 'intentional-rollback.log')}
    } else {
        $scenarioFailures += "Rollback restoration failed: baseline restored=$baselineRestored; failed product absent=$failedProductAbsent. The MSI log records registry rollback access-denied errors; installer security policy was not changed."
        $results += [pscustomobject]@{Step='rollback-restoration';ExitCode=$null;Expected=$false;Outcome='failed';Log=(Join-Path $logs 'intentional-rollback.log')}
        # Recover only through the known fixture MSIs so independent lifecycle
        # scenarios can continue without masking the rollback failure.
        Invoke-Msi 'rollback-failure-remove-new' @('/x', $rollback.Path) @(0,1605)
        Invoke-Msi 'rollback-failure-remove-old' @('/x', $baseline.Path) @(0,1605)
        Invoke-Msi 'rollback-failure-restore-baseline' @('/i', $baseline.Path)
        Assert-InstalledResources $baseline $expected.baseline
    }

    Invoke-Msi 'direct-upgrade' @('/i', $candidate.Path)
    Test-Installed $baseline.ProductCode $false
    Assert-InstalledResources $candidate $expected.candidate
    Invoke-Msi 'uninstall-after-upgrade' @('/x', $candidate.Path)

    Invoke-Msi 'install-baseline-for-lock' @('/i', $baseline.Path)
    $lockPath = Join-Path $baseline.InstallDirectory 'SC2Switcher.Wpf.exe'
    $lock = [IO.File]::Open($lockPath, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::Read)
    try {
        $lockResult = Invoke-Msi 'locked-file-upgrade' @('/i', $candidate.Path) @(0,1603,3010)
        if (!$lock.CanRead) { throw 'The test file handle was unexpectedly closed during the upgrade.' }
    } finally { $lock.Dispose() }
    if ($lockResult -eq 1603) { Assert-InstalledResources $baseline $expected.baseline }
    else { Test-Installed $baseline.ProductCode $false; Assert-InstalledResources $candidate $expected.candidate }
    foreach ($fixture in @($candidate,$baseline)) { Invoke-Msi "postlock-clean-$($fixture.Role)" @('/x', $fixture.Path) @(0,1605) }

    Invoke-Msi 'install-newer' @('/i', $newer.Path)
    Assert-InstalledResources $newer $expected.newer
    Invoke-Msi 'blocked-downgrade' @('/i', $candidate.Path) @(1603)
    Assert-InstalledResources $newer $expected.newer
    Test-Installed $candidate.ProductCode $false
    Invoke-Msi 'uninstall-newer' @('/x', $newer.Path)

    if ((Get-FileHash -LiteralPath $canaryPath -Algorithm SHA256).Hash -ne $canaryHash) { throw 'Dedicated configuration canary changed.' }
    if ((Get-FileHash -LiteralPath $CandidateMsi -Algorithm SHA256).Hash -ne $candidateOriginalHash -or (Get-FileHash -LiteralPath $BaselineMsi -Algorithm SHA256).Hash -ne $baselineOriginalHash) { throw 'An input MSI changed.' }
    if ($scenarioFailures.Count) { $status = 'failed-scenarios'; $failure = $scenarioFailures -join "`n" }
    else { $status = 'passed' }
} catch {
    $status = 'failed'
    $failure = $_.Exception.ToString()
    throw
} finally {
    foreach ($fixture in $fixtures) {
        try { Invoke-Msi "final-clean-$($fixture.Role)" @('/x', $fixture.Path) @(0,1605) } catch { }
    }
    $cleanupSucceeded = @($results | Where-Object { $_.Step -like 'final-clean-*' -and !$_.Expected }).Count -eq 0
    $report = [ordered]@{
        SchemaVersion=1; Status=$status; CompletedUtc=(Get-Date).ToUniversalTime().ToString('o'); RunId=$runId
        Inputs=[ordered]@{ Candidate=[ordered]@{Path=$CandidateMsi;SHA256=$candidateOriginalHash}; Baseline=[ordered]@{Path=$BaselineMsi;SHA256=$baselineOriginalHash} }
        Isolation=[ordered]@{ FixtureName=$fixtureName;UpgradeCode=$fixtureUpgrade;InstallDirectory=$installDir;MenuDirectory=$menuDir;AppPathRegistry=$fixtureAppPathKey;MarkerRegistry=$fixtureMarkerKey;Canary=$canaryPath;ProductionFamilyRejected=$true }
        Fixtures=$fixtures; Results=$results; LockedFileResult=$lockResult; CleanupSucceeded=$cleanupSucceeded; Failure=$failure
    }
    $report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $EvidenceDirectory 'result.json') -Encoding utf8
}

$report | ConvertTo-Json -Depth 8
if ($status -eq 'failed-scenarios') { throw $failure }
