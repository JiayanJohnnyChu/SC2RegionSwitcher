function Get-ReleaseMetadata {
    $root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
    [xml]$project=Get-Content -LiteralPath (Join-Path $root 'src/SC2Switcher.Wpf/SC2Switcher.Wpf.csproj') -Raw
    $version=[string]$project.Project.PropertyGroup.Version
    if($version -notmatch '^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$'){throw 'Release version must have exactly three numeric fields.'}
    $parts=$version.Split('.')
    if([int]$parts[0] -gt 255 -or [int]$parts[1] -gt 255 -or [int]$parts[2] -gt 65535 -or [version]$version -lt [version]'3.4.0'){throw 'Version is outside the supported MSI range.'}
    [xml]$applicationManifest=Get-Content -LiteralPath (Join-Path $root 'src/SC2Switcher.Wpf/app.manifest') -Raw
    if($applicationManifest.assembly.assemblyIdentity.version -ne "$version.0"){throw 'Application and executable manifest versions differ.'}
    if($applicationManifest.assembly.trustInfo.security.requestedPrivileges.requestedExecutionLevel.level -ne 'asInvoker'){throw 'The application must retain ordinary-user execution.'}
    $hash=[Security.Cryptography.SHA256]::Create()
    try {$bytes=$hash.ComputeHash([Text.Encoding]::UTF8.GetBytes("SC2RegionSwitcher/per-machine/x64/product/$version"))} finally {$hash.Dispose()}
    # Stable product identity for a version; a changed, distributed payload requires a new version.
    $product='{'+([guid]::new([byte[]]$bytes[0..15])).ToString().ToUpperInvariant()+'}'
    [pscustomobject]@{Root=$root;Version=$version;FileVersion="$version.0";ProductCode=$product;UpgradeCode='{4A67EAD9-86CA-450C-ABDC-5D6C1E4A4CCD}'}
}
