[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$projectRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
Push-Location $projectRoot
try {
    $files=@(& git ls-files --cached --others --exclude-standard)
    if($LASTEXITCODE -ne 0){throw 'Run this check in an initialized Git repository.'}
    $problems=@()
    foreach($file in $files){
        $normalized=$file.Replace('\','/')
        if($normalized -match '(^|/)(artifacts|\.tools|bin|obj|work)/' -or $normalized -match '(?i)\.(msi|cab|zip|exe|dll|pdb|pfx|p12)$'){
            $problems+="Generated or private file included: $file"
            continue
        }
        if([IO.Path]::GetFileName($file) -in @('profiles.json','ui-preferences.json','pending-language.json','Variables.txt','Battle.net.config')){
            $problems+="Runtime configuration included: $file"
        }
        if([IO.Path]::GetExtension($file) -in @('.png','.ico')){continue}
        $path=Join-Path $projectRoot $file
        if(!(Test-Path -LiteralPath $path -PathType Leaf)){continue}
        $content=Get-Content -LiteralPath $path -Raw
        if($content -match '(?i)[A-Z]:[\\/]Users[\\/](?!Public[\\/]|Default[\\/]|<)'){
            $problems+="Personal absolute path included: $file"
        }
        if($content -match '(?:gh[pousr]_[A-Za-z0-9]{20,}|github_pat_[A-Za-z0-9_]{20,})'){
            $problems+="Possible GitHub credential included: $file"
        }
        if($file -like '*.ps1'){
            $parseTokens=$null;$parseErrors=$null
            [Management.Automation.Language.Parser]::ParseFile($path,[ref]$parseTokens,[ref]$parseErrors) | Out-Null
            if($parseErrors.Count){$problems+="PowerShell syntax error: $file"}
        }
    }
    if($problems.Count){throw ($problems -join [Environment]::NewLine)}
    Write-Output "Repository checks passed ($($files.Count) source files)."
} finally {Pop-Location}
