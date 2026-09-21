[CmdletBinding()]
param([string]$ArchivePath)
$ErrorActionPreference='Stop'
$projectRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$manifest=Get-Content -LiteralPath (Join-Path $projectRoot 'eng\wix-validation.json') -Raw | ConvertFrom-Json
$toolsRoot=Join-Path $projectRoot '.tools'
$destination=Join-Path $toolsRoot 'wix-validation'
New-Item -ItemType Directory -Path $toolsRoot -Force | Out-Null
if((Get-Item -LiteralPath $toolsRoot -Force).Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'The tools directory must not be a link.'}
if(!(Test-Path -LiteralPath $destination)){
    if(!$ArchivePath){
        $downloads=Join-Path $projectRoot 'artifacts\downloads'
        New-Item -ItemType Directory -Path $downloads -Force | Out-Null
        $ArchivePath=Join-Path $downloads 'wix314-binaries.zip'
        if(!(Test-Path -LiteralPath $ArchivePath)){Invoke-WebRequest -Uri $manifest.url -OutFile $ArchivePath}
    }
    if((Get-FileHash -LiteralPath $ArchivePath -Algorithm SHA256).Hash -ne $manifest.sha256){throw 'WiX validation archive checksum mismatch.'}
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [IO.Compression.ZipFile]::ExtractToDirectory([IO.Path]::GetFullPath($ArchivePath),$destination)
}
if((Get-Item -LiteralPath $destination -Force).Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'The validator directory must not be a link.'}
foreach($file in $manifest.files.PSObject.Properties){
    $path=Join-Path $destination $file.Name
    if(!(Test-Path -LiteralPath $path -PathType Leaf) -or (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $file.Value){throw "WiX validation tool differs: $($file.Name). Preserve the directory before replacing it."}
}
if([Diagnostics.FileVersionInfo]::GetVersionInfo((Join-Path $destination 'smoke.exe')).FileVersion -ne $manifest.version){throw 'WiX validator version mismatch.'}
Write-Output "WiX $($manifest.version) validation tools are ready. No installer was run."
