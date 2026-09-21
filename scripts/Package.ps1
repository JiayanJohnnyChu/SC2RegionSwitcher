[CmdletBinding()]
param([string]$DotNet,[string]$VersionTag)
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'Release-Common.ps1')
$metadata=Get-ReleaseMetadata
$projectRoot=$metadata.Root
$version=$metadata.Version
if($VersionTag -and $VersionTag -notmatch ('^v'+[regex]::Escape($version)+'(?:-[0-9A-Za-z.-]+)?$')){
    throw 'The release tag must match the application version.'
}
$sourceCommit=(& git -C $projectRoot rev-parse HEAD).Trim()
if($LASTEXITCODE -ne 0 -or $sourceCommit -notmatch '^[a-f0-9]{40}$'){throw 'Packaging requires a committed Git source revision.'}
$treeStatus=@(& git -C $projectRoot status --porcelain --untracked-files=normal)
if($LASTEXITCODE -ne 0){throw 'Cannot determine source status.'}
$dirty=$treeStatus.Count -gt 0
if($env:GITHUB_ACTIONS -eq 'true' -and $dirty){throw 'CI packaging requires a clean working tree.'}
$existingTags=@(& git -C $projectRoot tag --list "v$version*" | Where-Object {$_ -match ('^v'+[regex]::Escape($version)+'(?:$|-)')})
if($LASTEXITCODE -ne 0){throw 'Cannot check existing version tags.'}
if($existingTags.Count){throw 'This numerical version already has a tag. Promote its original candidate or increment the version.'}
& (Join-Path $PSScriptRoot 'Publish.ps1') -DotNet $DotNet
$packageName=(Get-Date -Format 'yyyyMMdd-HHmmss')+'-'+[guid]::NewGuid().ToString('N').Substring(0,8)
$packageRoot=Join-Path $projectRoot "artifacts\packages\$packageName"
$portable=Join-Path $packageRoot 'portable'
New-Item -ItemType Directory -Path $portable -Force | Out-Null
$files=@('SC2Switcher.Wpf.exe','SC2Switcher.Wpf.dll','SC2Switcher.Wpf.deps.json','SC2Switcher.Wpf.runtimeconfig.json')
foreach($file in $files){Copy-Item -LiteralPath (Join-Path $projectRoot "artifacts\publish\win-x64\$file") -Destination $portable}
Copy-Item -LiteralPath (Join-Path $projectRoot 'docs\SETUP.zh-CN.md') -Destination (Join-Path $portable 'README.md')
$archive=Join-Path $packageRoot "SC2Switcher-$version-win-x64-preview.zip"
Compress-Archive -LiteralPath @(Get-ChildItem -LiteralPath $portable -File | ForEach-Object FullName) -DestinationPath $archive
& (Join-Path $projectRoot 'tools\Installer\Build-CurrentUserMsi.ps1') -AppDirectory $portable -OutputDirectory (Join-Path $packageRoot 'msi')
$msi=Join-Path $packageRoot "msi\SC2Switcher-$version-current-user.msi"
$manifest=[ordered]@{
    Version=$version;Status='Newly built; installation not tested';Runtime='Microsoft .NET 10 Desktop Runtime x64';Signed=$false
    Packages=@(@{File=[IO.Path]::GetFileName($archive);SHA256=(Get-FileHash -LiteralPath $archive).Hash},@{File='msi/'+[IO.Path]::GetFileName($msi);SHA256=(Get-FileHash -LiteralPath $msi).Hash})
}
$manifest | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $packageRoot 'checksums.json') -Encoding utf8
$release=Join-Path $packageRoot 'release'
New-Item -ItemType Directory -Path $release | Out-Null
Copy-Item -LiteralPath $archive,$msi -Destination $release
$publicFiles=@(Get-ChildItem -LiteralPath $release -File | Sort-Object Name)
$publicManifest=[ordered]@{
    SchemaVersion=2;Version=$version;Tag=$VersionTag;Status='Preview; installation testing recorded separately';Runtime='Microsoft .NET 10 Desktop Runtime x64';Signed=$false
    SourceCommit=$sourceCommit;WorkingTreeDirty=$dirty;CiRunId=$env:GITHUB_RUN_ID;CiRunAttempt=$env:GITHUB_RUN_ATTEMPT
    SdkVersion=(Get-Content -LiteralPath (Join-Path $projectRoot 'global.json') -Raw | ConvertFrom-Json).sdk.version
    Files=@($publicFiles | ForEach-Object {[ordered]@{Name=$_.Name;Bytes=$_.Length;SHA256=(Get-FileHash -LiteralPath $_.FullName).Hash}})
}
$publicManifest | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $release 'release-manifest.json') -Encoding utf8
@(Get-ChildItem -LiteralPath $release -File | Sort-Object Name | ForEach-Object {(Get-FileHash -LiteralPath $_.FullName).Hash.ToLowerInvariant()+'  '+$_.Name}) | Set-Content -LiteralPath (Join-Path $release 'SHA256SUMS.txt') -Encoding ascii
if($env:GITHUB_OUTPUT){'release-directory='+$release | Add-Content -LiteralPath $env:GITHUB_OUTPUT -Encoding utf8}
Write-Output "Packages created without installation: $packageRoot"
