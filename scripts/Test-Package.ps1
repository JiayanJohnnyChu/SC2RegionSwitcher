[CmdletBinding()]
param([Parameter(Mandatory)][string]$ReleaseDirectory)
$ErrorActionPreference='Stop'
$directory=[IO.Path]::GetFullPath($ReleaseDirectory)
$manifest=Get-Content -LiteralPath (Join-Path $directory 'release-manifest.json') -Raw | ConvertFrom-Json
if($manifest.SchemaVersion -ne 1 -or $manifest.Version -ne '3.3.0'){throw 'Unsupported release manifest.'}
$expected=@('SC2Switcher-3.3.0-current-user.msi','SC2Switcher-3.3.0-win-x64-preview.zip','SHA256SUMS.txt','release-manifest.json')
$actual=@(Get-ChildItem -LiteralPath $directory -File | ForEach-Object Name)
if((@($actual | Sort-Object) -join '|') -ne (@($expected | Sort-Object) -join '|')){throw 'Unexpected release payload.'}
if(@($manifest.Files).Count -ne 2){throw 'Manifest must describe exactly two packages.'}
foreach($entry in $manifest.Files){
    if($entry.Name -notin $expected[0..1]){throw 'Unexpected file in release manifest.'}
    if((Get-FileHash -LiteralPath (Join-Path $directory $entry.Name)).Hash -ne $entry.SHA256){throw "Package hash mismatch: $($entry.Name)"}
}
$sumLines=@(Get-Content -LiteralPath (Join-Path $directory 'SHA256SUMS.txt'))
if($sumLines.Count -ne 3){throw 'Expected checksums for both packages and the manifest.'}
$sumNames=@()
foreach($line in $sumLines){
    if($line -notmatch '^([a-fA-F0-9]{64})  ([^/\\]+)$'){throw 'Invalid SHA256SUMS entry.'}
    $hash=$Matches[1];$name=$Matches[2]
    if($name -notin $expected -or $name -eq 'SHA256SUMS.txt'){throw 'Unexpected checksum filename.'}
    if((Get-FileHash -LiteralPath (Join-Path $directory $name)).Hash -ne $hash){throw "Checksum mismatch: $name"}
    $sumNames+=$name
}
if(@($sumNames | Select-Object -Unique).Count -ne 3){throw 'Duplicate checksum entry.'}
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip=[IO.Compression.ZipFile]::OpenRead((Join-Path $directory $expected[1]))
try {
    $entries=@($zip.Entries | ForEach-Object FullName | Sort-Object)
    $zipExpected=@('README.md','SC2Switcher.Wpf.exe','SC2Switcher.Wpf.dll','SC2Switcher.Wpf.deps.json','SC2Switcher.Wpf.runtimeconfig.json') | Sort-Object
    if(($entries -join '|') -ne ($zipExpected -join '|')){throw 'Portable ZIP contains unexpected files.'}
} finally {$zip.Dispose()}
$installer=New-Object -ComObject WindowsInstaller.Installer
$database=$installer.OpenDatabase((Join-Path $directory $expected[0]),0)
function Read-MsiColumn([string]$Query){
    $view=$database.OpenView($Query)
    try {
        [void]$view.Execute()
        while($row=$view.Fetch()){
            try {$row.StringData(1)} finally {[Runtime.InteropServices.Marshal]::FinalReleaseComObject($row) | Out-Null}
        }
    } finally {[void]$view.Close();[Runtime.InteropServices.Marshal]::FinalReleaseComObject($view) | Out-Null}
}
try {
    $msiFiles=@(Read-MsiColumn 'SELECT `File` FROM `File`')
    $shortcuts=@(Read-MsiColumn 'SELECT `Shortcut` FROM `Shortcut`')
    $actions=@(Read-MsiColumn 'SELECT `Type` FROM `CustomAction`')
    $version=@(Read-MsiColumn "SELECT ``Value`` FROM ``Property`` WHERE ``Property`` = 'ProductVersion'")
    if($msiFiles.Count -ne 4 -or $shortcuts.Count -ne 1 -or $actions.Count -ne 1 -or $actions[0] -ne '51' -or $version[0] -ne $manifest.Version){throw 'MSI structure does not match the release contract.'}
} finally {
    [Runtime.InteropServices.Marshal]::FinalReleaseComObject($database) | Out-Null
    [Runtime.InteropServices.Marshal]::FinalReleaseComObject($installer) | Out-Null
}
Write-Output 'Release package checks passed: hashes, ZIP payload, MSI version, four files and one shortcut. MSI was not installed.'
