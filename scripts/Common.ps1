$script:Sc2ProjectRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))

function Resolve-ProjectDotNet {
    param([string]$DotNet)
    if($DotNet){
        return (Get-Command $DotNet -CommandType Application -ErrorAction Stop | Select-Object -First 1).Source
    }
    $localSdk=Join-Path $script:Sc2ProjectRoot '.tools\dotnet\dotnet.exe'
    if(Test-Path -LiteralPath $localSdk){return $localSdk}
    $command=Get-Command dotnet -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
    if($command){return $command.Source}
    throw 'Run scripts/Setup.ps1 to prepare the pinned .NET SDK.'
}

function Invoke-ProjectDotNet {
    param([string]$DotNet,[Parameter(Mandatory)][string[]]$Arguments)
    $executable=Resolve-ProjectDotNet -DotNet $DotNet
    $sdkRoot=Split-Path -Parent $executable
    $settings=@{
        DOTNET_CLI_HOME=(Join-Path $script:Sc2ProjectRoot 'artifacts\tool-state')
        DOTNET_CLI_TELEMETRY_OPTOUT='1'
        DOTNET_NOLOGO='1'
        DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'
        DOTNET_ROOT=$sdkRoot
        DOTNET_ROOT_X64=$sdkRoot
        NUGET_PACKAGES=(Join-Path $script:Sc2ProjectRoot 'artifacts\nuget')
        # NuGet still consults its user settings in some solution restore paths.
        # Use an isolated process-local roaming root, never the user's config.
        APPDATA=(Join-Path $script:Sc2ProjectRoot 'artifacts\tool-state\roaming')
        # This WPF project uses SDK-bundled reference packs, not extension SDKs.
        # Scope SDK discovery to readable packs instead of personal SDK folders.
        MSBUILDSDKREFERENCEDIRECTORY=(Join-Path $sdkRoot 'packs')
        MSBUILDDISABLEREGISTRYFORSDKLOOKUP='1'
    }
    New-Item -ItemType Directory -Path $settings.APPDATA -Force | Out-Null
    $previous=@{}
    foreach($name in $settings.Keys){
        $previous[$name]=[Environment]::GetEnvironmentVariable($name,'Process')
        [Environment]::SetEnvironmentVariable($name,$settings[$name],'Process')
    }
    Push-Location $script:Sc2ProjectRoot
    try {
        $required=(Get-Content -LiteralPath (Join-Path $script:Sc2ProjectRoot 'global.json') -Raw | ConvertFrom-Json).sdk.version
        $sdkVersion=(& $executable --version 2>&1 | Out-String).Trim()
        if($LASTEXITCODE -ne 0 -or $sdkVersion -ne $required){
            throw "SDK $required is required. Run scripts/Setup.ps1, or supply -DotNet with the matching dotnet.exe."
        }
        if($Arguments[0] -in @('build','publish','restore')){
            $Arguments+=('-p:RestoreConfigFile='+(Join-Path $script:Sc2ProjectRoot 'NuGet.Config'))
        }
        & $executable @Arguments
        if($LASTEXITCODE -ne 0){throw "dotnet command failed with exit code $LASTEXITCODE."}
    } finally {
        Pop-Location
        foreach($name in $previous.Keys){[Environment]::SetEnvironmentVariable($name,$previous[$name],'Process')}
    }
}
