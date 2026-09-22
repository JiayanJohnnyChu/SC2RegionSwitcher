[简体中文](DEVELOPMENT.zh-CN.md)

# Development and builds

Use Windows x64 and .NET SDK **10.0.401**, pinned in `global.json`. Desktop Runtime alone cannot compile the application. Open `SC2RegionSwitcher.slnx` to load the application, regression runner and icon tool.

## Tools and commands

Run commands below in PowerShell from the project root. Scripts resolve the root from their own location, so absolute script paths also work.

| Command | Purpose |
| --- | --- |
| `.\scripts\Setup.ps1` | Prepare the pinned SDK in `.tools/dotnet` |
| `.\scripts\Setup-WorkflowTools.ps1` | Prepare the pinned actionlint binary |
| `.\scripts\Check-Workflows.ps1` | Validate workflow syntax, expressions and Action usage |
| `.\scripts\Check-Repository.ps1` | Check repository content, personal paths and PowerShell syntax |
| `.\scripts\Build.ps1` | Compile the application, tests and icon tool in Release mode |
| `.\scripts\Test.ps1` | Build and run isolated regressions |
| `.\scripts\Publish.ps1` | Produce framework-dependent x64 runtime files |
| `.\scripts\Package.ps1` | Publish and create the MSI, ZIP and release metadata |
| `.\scripts\Test-Package.ps1 -ReleaseDirectory <directory>` | Check package content and hashes |
| `.\scripts\Test-ReleaseGuards.ps1 -ReleaseDirectory <directory>` | Exercise release rejection cases |
| `.\scripts\Test-InstallerSchema.ps1 -ReleaseDirectory <directory>` | Run independent Windows Installer ICE validation |

Setup verifies the Microsoft SDK archive against `eng/dotnet-sdk.json` before extraction. `-ArchivePath <local-sdk-zip>` supports offline preparation with the same hash check. Scripts prefer the project SDK, then PATH. Build, Test, Publish and Package accept `-DotNet <path-to-dotnet.exe>`; Build and Test also accept `-Configuration Debug`. Packages use Release builds.

Tool versions and hashes are tracked in `eng/`; downloaded tools and caches are ignored. `Common.ps1` sets a project-local environment for NuGet caches, CLI state and SDK metadata lookup, then restores the environment. Restore uses the repository's `NuGet.Config`, which has no package sources because the project has no third-party NuGet dependencies. Reassess these settings if adding private packages, WinUI or native SDK dependencies.

The regression suite is a console program using `FakePlatform` and temporary installations. The recorded baseline has 52 tests and prints `TOTAL 52 PASSED` on success. A failure returns a nonzero exit code. Running `dotnet test` without discovering tests does not provide equivalent coverage.

## Outputs

Build outputs remain in each project's `bin/` and `obj/`. Publish writes to `artifacts/publish/win-x64/`. Test runs have separate directories in `artifacts/tests/`; packages are under `artifacts/packages/<build>/release`.

The release directory contains exactly four files: MSI, portable ZIP, `release-manifest.json` and `SHA256SUMS.txt`. The current ZIP contains four runtime files plus `README.md` and `README.zh-CN.md`. Package scripts create files but do not install them. The manifest records the source commit, working-tree state and CI provenance. Local development packages are not accepted CI candidates. The MSI is named `SC2Switcher-<version>-x64.msi` and uses the administrator-required machine scope; building it does not require installing it.

The separate `installer-lifecycle.yml` workflow uses `tools/Installer/Test-MachineInstall.ps1` for isolated machine installation, maintenance, upgrades, rollback and removal. Its result must identify the input candidate and hash. The older recovery workflow is retained for historical current-user diagnostics. See [Installer](../tools/Installer/README.md) for the current scope and migration rules.

## Interface diagnostics

The application normally uses `%LOCALAPPDATA%\SC2RegionSwitcherV2`. `--data-dir <absolute-directory>` redirects configuration; it does not simulate Battle.net or the game.

`--ui-report <absolute-json-path>` exports a layout report and a WPF self-rendered image. `--compact` requests the minimum window size. F12 captures the active main, settings or reference page. Store these outputs under `artifacts/`.

`--matrix` requires an explicit separate `--data-dir` and exports synthetic states in both languages. Real switching and path saving are disabled while this presentation runs. Reports mark `SyntheticState=true`. Start the generated EXE for DPI checks so the application manifest applies; rendering the DLL through the dotnet host does not establish the EXE's PerMonitorV2 behavior. See [Interface design](UI-DESIGN.md).

## Icons

Application and installer share `assets/icon/switcher.ico`; the directory also contains the vector source and preview. Generate proposed changes in a temporary location:

```powershell
. .\scripts\Common.ps1
Invoke-ProjectDotNet -Arguments @('run', '--project', '.\tools\IconGenerator\IconGenerator.csproj', '--configuration', 'Release', '--no-build', '--', '.\artifacts\icon-preview')
```

Inspect the SVG, PNG, ICO and comparison outputs before updating assets. Related geometry in the main window is maintained separately in XAML.

## CI and release maintenance

Windows CI checks workflows and repository content, builds, runs regressions, packages, and checks contents, schema and rejection cases. Actions are pinned to commits. Dependabot proposes monthly Action updates without merging them automatically. SDK upgrades update both `global.json` and `eng/dotnet-sdk.json`; validator and workflow-tool updates also require their version and hash metadata to change.

Each distributed preview needs a new three-part version. A rebuilt MSI has new bytes and a new PackageCode, so installation results must identify the exact tested hash. Promote the accepted CI files without rebuilding. See [Installer](../tools/Installer/README.md), [release workflow](GITHUB-RELEASE.md) and [current status](HANDOFF.md).
