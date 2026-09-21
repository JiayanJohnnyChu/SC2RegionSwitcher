[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidatePattern('^[1-9][0-9]*$')][string]$CandidateRunId,
    [Parameter(Mandatory)][ValidatePattern('^v[0-9]+\.[0-9]+\.[0-9]+-preview\.[1-9][0-9]*$')][string]$VersionTag
)
$ErrorActionPreference='Stop'
if(!$env:GH_TOKEN -or $env:GITHUB_ACTIONS -ne 'true'){throw 'Promote candidates through the GitHub release workflow.'}
if($env:GITHUB_REPOSITORY -notmatch '^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$'){throw 'Invalid repository context.'}
$repository=$env:GITHUB_REPOSITORY
$env:GH_REPO=$repository
function Invoke-GhJson([string[]]$Arguments){
    $result=& gh @Arguments
    if($LASTEXITCODE -ne 0){throw 'GitHub request failed.'}
    ($result -join "`n") | ConvertFrom-Json
}
. (Join-Path $PSScriptRoot 'Release-Common.ps1')
$metadata=Get-ReleaseMetadata
if($VersionTag -notmatch ('^v'+[regex]::Escape($metadata.Version)+'-preview\.[1-9][0-9]*$')){throw 'Tag does not match the checked-out application version.'}
$source=(& git rev-parse HEAD).Trim()
if($LASTEXITCODE -ne 0){throw 'Cannot read checked-out revision.'}
$tagCommit=(& git rev-parse "$VersionTag^{commit}").Trim()
if($LASTEXITCODE -ne 0 -or $tagCommit -ne $source){throw 'Checkout must be the release tag commit.'}
$run=Invoke-GhJson @('api',"repos/$repository/actions/runs/$CandidateRunId")
if($run.status -ne 'completed' -or $run.conclusion -ne 'success' -or $run.event -ne 'push' -or $run.head_branch -ne 'main' -or $run.path -ne '.github/workflows/ci.yml' -or $run.head_sha -ne $source -or $run.head_repository.full_name -ne $repository){throw 'Candidate must come from successful main-branch CI for this exact tag commit.'}
$existing=Invoke-GhJson @('api',"repos/$repository/releases?per_page=100",'--paginate','--slurp')
foreach($page in $existing){foreach($item in $page){if($item.tag_name -match ('^v'+[regex]::Escape($metadata.Version)+'(?:$|-)')){throw 'This numerical version already has a release or draft. Its files will not be replaced.'}}}
$artifacts=Invoke-GhJson @('api',"repos/$repository/actions/runs/$CandidateRunId/artifacts")
$candidate=@($artifacts.artifacts | Where-Object {$_.name -eq 'SC2RegionSwitcher-win-x64-preview' -and !$_.expired})
if($candidate.Count -ne 1){throw 'Expected exactly one unexpired candidate artifact.'}
$destination=Join-Path $metadata.Root ('artifacts/promote/'+$CandidateRunId+'-'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $destination | Out-Null
& gh run download $CandidateRunId --repo $repository --name 'SC2RegionSwitcher-win-x64-preview' --dir $destination
if($LASTEXITCODE -ne 0){throw 'Candidate download failed.'}
& (Join-Path $PSScriptRoot 'Test-Package.ps1') -ReleaseDirectory $destination
$manifest=Get-Content -LiteralPath (Join-Path $destination 'release-manifest.json') -Raw | ConvertFrom-Json
if($manifest.SourceCommit -ne $source -or $manifest.CiRunId -ne $CandidateRunId -or [string]$manifest.CiRunAttempt -ne [string]$run.run_attempt -or $manifest.WorkingTreeDirty -ne $false){throw 'Candidate provenance does not match the successful source run.'}
$notes=Join-Path $metadata.Root "docs/RELEASE-NOTES-$($metadata.Version).md"
if(!(Test-Path -LiteralPath $notes -PathType Leaf)){throw 'Version-specific release notes are missing.'}
$assets=@(Get-ChildItem -LiteralPath $destination -File | ForEach-Object FullName)
& gh release create $VersionTag @assets --repo $repository --verify-tag --draft --prerelease --title "SC2 Region Switcher $VersionTag" --notes-file $notes
if($LASTEXITCODE -ne 0){throw 'Draft creation failed. Do not overwrite an existing release.'}
$verified=Join-Path (Split-Path $destination -Parent) ('verified-'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $verified | Out-Null
& gh release download $VersionTag --repo $repository --dir $verified
if($LASTEXITCODE -ne 0){throw 'Uploaded attachment verification download failed.'}
& (Join-Path $PSScriptRoot 'Test-Package.ps1') -ReleaseDirectory $verified
foreach($asset in $assets){
    $name=[IO.Path]::GetFileName($asset)
    if((Get-FileHash -LiteralPath $asset).Hash -ne (Get-FileHash -LiteralPath (Join-Path $verified $name)).Hash){throw "Uploaded attachment differs from tested candidate: $name"}
}
$release=Invoke-GhJson @('release','view',$VersionTag,'--repo',$repository,'--json','isDraft,isPrerelease,url,targetCommitish,assets')
if(!$release.isDraft -or !$release.isPrerelease -or @($release.assets).Count -ne 4){throw 'Unexpected release state or attachments.'}
Write-Output "Verified draft prerelease: $($release.url)"
