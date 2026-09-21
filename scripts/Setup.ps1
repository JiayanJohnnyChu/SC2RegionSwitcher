[CmdletBinding()]
param([string]$ArchivePath)
$ErrorActionPreference='Stop'
$projectRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if([Environment]::OSVersion.Platform -ne 'Win32NT' -or ![Environment]::Is64BitOperatingSystem){throw 'Windows x64 is required.'}
$manifest=Get-Content -LiteralPath (Join-Path $projectRoot 'eng\dotnet-sdk.json') -Raw | ConvertFrom-Json
$sdkVersion=(Get-Content -LiteralPath (Join-Path $projectRoot 'global.json') -Raw | ConvertFrom-Json).sdk.version
if($manifest.version -ne $sdkVersion){throw 'global.json and the SDK download manifest disagree.'}
$toolsRoot=Join-Path $projectRoot '.tools'
$destination=Join-Path $toolsRoot 'dotnet'
if(Test-Path -LiteralPath (Join-Path $destination 'dotnet.exe')){
    . (Join-Path $PSScriptRoot 'Common.ps1')
    Invoke-ProjectDotNet -DotNet (Join-Path $destination 'dotnet.exe') -Arguments @('--version')
    Write-Output 'Project SDK is already prepared.'
    return
}
if(Test-Path -LiteralPath $destination){throw 'The SDK destination already exists but is incomplete. Preserve it before retrying.'}
New-Item -ItemType Directory -Path $toolsRoot -Force | Out-Null
if((Get-Item -LiteralPath $toolsRoot -Force).Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'The local tools directory must not be a link.'}
if(!$ArchivePath){
    $downloads=Join-Path $projectRoot 'artifacts\downloads'
    New-Item -ItemType Directory -Path $downloads -Force | Out-Null
    $ArchivePath=Join-Path $downloads "dotnet-sdk-$sdkVersion-win-x64.zip"
    if(!(Test-Path -LiteralPath $ArchivePath)){
        Invoke-WebRequest -Uri $manifest.url -OutFile $ArchivePath -UseBasicParsing
    }
}
$archive=[IO.Path]::GetFullPath($ArchivePath)
if((Get-FileHash -LiteralPath $archive -Algorithm SHA512).Hash -ne $manifest.sha512){throw 'SDK archive SHA-512 does not match the pinned Microsoft release metadata.'}
$stage=Join-Path $toolsRoot ('sdk-stage-'+[guid]::NewGuid().ToString('N'))
Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::ExtractToDirectory($archive,$stage)
if(!(Test-Path -LiteralPath (Join-Path $stage "sdk\$sdkVersion\dotnet.dll"))){throw 'The archive does not contain the required SDK.'}
$allowedRoot=[IO.Path]::GetFullPath($toolsRoot).TrimEnd('\')+'\'
foreach($target in @($stage,$destination)){
    if(![IO.Path]::GetFullPath($target).StartsWith($allowedRoot,[StringComparison]::OrdinalIgnoreCase)){throw 'SDK move would leave the project tools directory.'}
}
Move-Item -LiteralPath $stage -Destination $destination
. (Join-Path $PSScriptRoot 'Common.ps1')
Invoke-ProjectDotNet -Arguments @('--version')
Write-Output 'Project SDK prepared. Build, Test and Package now select it automatically.'
