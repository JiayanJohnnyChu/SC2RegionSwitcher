[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$projectRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$manifest=Get-Content -LiteralPath (Join-Path $projectRoot 'eng\actionlint.json') -Raw | ConvertFrom-Json
$toolsRoot=Join-Path $projectRoot '.tools'
$destination=Join-Path $toolsRoot 'actionlint'
$executable=Join-Path $destination 'actionlint.exe'
if(Test-Path -LiteralPath $executable){
    $version=@(& $executable -version)
    if($LASTEXITCODE -ne 0 -or $version[0].Trim() -ne $manifest.version){throw 'Existing actionlint version differs. Preserve it before updating.'}
    Write-Output "actionlint $($manifest.version) is already prepared."
    return
}
if(Test-Path -LiteralPath $destination){throw 'Incomplete actionlint destination; preserve it before retrying.'}
New-Item -ItemType Directory -Path $toolsRoot -Force | Out-Null
if((Get-Item -LiteralPath $toolsRoot -Force).Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'The tools directory must not be a link.'}
$downloads=Join-Path $projectRoot 'artifacts\downloads'
New-Item -ItemType Directory -Path $downloads -Force | Out-Null
$archive=Join-Path $downloads "actionlint-$($manifest.version)-windows-amd64.zip"
if(!(Test-Path -LiteralPath $archive)){Invoke-WebRequest -Uri $manifest.url -OutFile $archive -UseBasicParsing}
if((Get-FileHash -LiteralPath $archive).Hash -ne $manifest.sha256){throw 'actionlint archive checksum mismatch.'}
Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::ExtractToDirectory($archive,$destination)
$version=@(& $executable -version)
if($LASTEXITCODE -ne 0 -or $version[0].Trim() -ne $manifest.version){throw 'actionlint version verification failed.'}
Write-Output "actionlint $($manifest.version) prepared."
