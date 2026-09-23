[CmdletBinding()]
param([string]$DotNet,[ValidateRange(30,600)][int]$TimeoutSeconds=240,[ValidateSet('en-US','zh-CN','fr-FR','de-DE','nl-NL','ko-KR','it-IT','es-ES','pt-PT','la','el-GR')][string[]]$Languages=@('en-US','zh-CN','fr-FR','de-DE','nl-NL','ko-KR','it-IT','es-ES','pt-PT','la','el-GR'),[switch]$InteractionsOnly,[switch]$StatusOnly)
$ErrorActionPreference='Stop'
if($InteractionsOnly -and $StatusOnly){throw 'Choose either InteractionsOnly or StatusOnly.'}
. (Join-Path $PSScriptRoot 'Common.ps1')
$projectRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$runDirectory=Join-Path $projectRoot ('artifacts/ui/'+(Get-Date -Format 'yyyyMMdd-HHmmss')+'-'+[guid]::NewGuid().ToString('N').Substring(0,8))
[void][IO.Directory]::CreateDirectory($runDirectory)
& (Join-Path $PSScriptRoot 'Build.ps1') -DotNet $DotNet
$runtime=Split-Path (Resolve-ProjectDotNet -DotNet $DotNet) -Parent
$start=[Diagnostics.ProcessStartInfo]::new()
$start.FileName=Join-Path $projectRoot 'src/SC2Switcher.Wpf/bin/Release/net10.0-windows/SC2Switcher.Wpf.exe'
$start.UseShellExecute=$false
$start.CreateNoWindow=$true
$start.WindowStyle=[Diagnostics.ProcessWindowStyle]::Hidden
$start.RedirectStandardOutput=$true
$start.RedirectStandardError=$true
$start.Environment['DOTNET_ROOT']=$runtime
$start.Environment['DOTNET_ROOT_X64']=$runtime
$start.Environment['DOTNET_DISABLE_GUI_ERRORS']='1'
foreach($argument in @('--ui-report',(Join-Path $runDirectory 'initial.json'),'--matrix','--data-dir',(Join-Path $runDirectory 'state'))){$start.ArgumentList.Add($argument)}
$start.ArgumentList.Add('--matrix-languages');$start.ArgumentList.Add(($Languages -join ','))
if($InteractionsOnly){$start.ArgumentList.Add('--interactions-only')}
if($StatusOnly){$start.ArgumentList.Add('--status-only')}
$process=[Diagnostics.Process]::Start($start)
$stdout=$process.StandardOutput.ReadToEndAsync()
$stderr=$process.StandardError.ReadToEndAsync()
Write-Output "Unattended UI validation: $runDirectory"
try {
    if(!$process.WaitForExit($TimeoutSeconds*1000)){
        $process.Kill()
        throw "UI validation timed out after $TimeoutSeconds seconds. Only its own process was stopped."
    }
    [IO.File]::WriteAllText((Join-Path $runDirectory 'stdout.log'),$stdout.GetAwaiter().GetResult())
    [IO.File]::WriteAllText((Join-Path $runDirectory 'stderr.log'),$stderr.GetAwaiter().GetResult())
    if($process.ExitCode -ne 0){throw "UI validation failed with exit code $($process.ExitCode). Read the reports in $runDirectory."}
    foreach($failure in @('startup-error.json','layout-error.json')){if(Test-Path -LiteralPath (Join-Path $runDirectory $failure)){throw "UI validation reported $failure in $runDirectory"}}
    if($StatusOnly){
        $statusReports=@(Get-ChildItem -LiteralPath $runDirectory -Filter 'status-*.json')
        if($statusReports.Count -ne 5){throw 'The focused status capture set is incomplete.'}
        Write-Output "Status study captures ready for visual review: $runDirectory"
        return
    }
    if($InteractionsOnly){
        $interaction=Get-Content -LiteralPath (Join-Path $runDirectory 'interaction-results.json') -Raw|ConvertFrom-Json
        if($interaction.Failures -ne 0 -or @($interaction.Checks).Count -ne 5 -or @($interaction.Checks|Where-Object {-not $_.Passed}).Count){throw 'Targeted interaction checks are incomplete or failed.'}
        Write-Output "Targeted interaction checks passed: $(@($interaction.Checks).Count). Reports: $runDirectory"
        return
    }
    $reports=@(Get-ChildItem -LiteralPath $runDirectory -Filter '*-main-global.json')
    if($reports.Count -ne $Languages.Count){throw "Expected $($Languages.Count) language render reports; found $($reports.Count)."}
    foreach($file in $reports){
        $report=Get-Content -LiteralPath $file.FullName -Raw|ConvertFrom-Json
        if(!$report.PerMonitorV2){throw "Native executable is not PerMonitorV2: $($file.Name)"}
        if($report.Ready){throw "Unchanged configuration enabled switching: $($file.Name)"}
        foreach($font in @($report.TitleFonts)+@($report.ContinuousFonts)){
            if($font.File -notlike 'Switcher*' -or $font.MissingGlyph -or $font.Simulation -ne 'None'){throw "Unexpected font fallback or simulation in $($file.Name): $($font.File)"}
        }
        $scriptFamily=switch($report.UiLanguage){'zh-CN'{'han'} 'ko-KR'{'hangul'} default{'sans'}}
        $titleFamily=if($scriptFamily-eq'sans'){'switcherdisplay-*'}else{"switcher$($scriptFamily)display-*"}
        if(@($report.ContinuousFonts|Where-Object File -notlike "switcher$scriptFamily-*").Count -or @($report.TitleFonts|Where-Object File -notlike $titleFamily).Count){throw "Mixed-script font family mismatch in $($file.Name)"}
    }
    $summary=[ordered]@{ExitCode=$process.ExitCode;Languages=$reports.Count;Screenshots=@(Get-ChildItem -LiteralPath $runDirectory -Filter '*.png').Count;BattleNetStarted=$false;GameStarted=$false;MsiInstalled=$false;Unattended=$true}
    $summary|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $runDirectory 'summary.json') -Encoding utf8
    Write-Output "UI validation passed: $($summary.Screenshots) native renders across $($reports.Count) languages."
} finally {$process.Dispose()}
