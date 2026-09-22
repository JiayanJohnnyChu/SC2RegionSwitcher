[简体中文](README.zh-CN.md)

# SC2 Region Switcher

SC2 Region Switcher is a Windows x64 application for switching between separate China and Global installations of StarCraft II. It uses one official Battle.net desktop app, changes its login region, and updates the shared game-language settings. China uses Simplified Chinese (`zhCN`); Global uses English (`enUS`). The interface supports English and Simplified Chinese, with its own language setting.

The application is written in C# with WPF and .NET 10. Account login, game installation, updates, game-server selection and game startup take place in Battle.net. The Global labels **EU**, **US** and **KR** identify Battle.net login regions; they do not establish which game server is selected.

**Current status: 3.4.1 development preview.** The administrator-required, per-machine MSI has passed build, package and hosted lifecycle checks for the candidate identified in the validation record. Interactive UAC installation and subsequent ordinary-user launch remain pending. Version 3.4.1 is not a release candidate; the earlier 3.4.0 candidate remains an unpublished private draft. Packages are unsigned and require **Microsoft .NET 10 Desktop Runtime x64**. The tested scope is documented in the [validation record](docs/VALIDATION.md).

## Documentation

| Task | Guide |
| --- | --- |
| Installation preparation, configuration and region switching | [Setup and use](docs/SETUP.md) |
| Source compilation and testing | [Development](docs/DEVELOPMENT.md) |
| Switching and configuration logic | [Architecture](docs/ARCHITECTURE.md) |
| Interface and translation maintenance | [Interface design](docs/UI-DESIGN.md) |
| Development and validation status | [Development status](docs/HANDOFF.md), [3.4.1 validation](docs/VALIDATION-3.4.1.md) |
| MSI maintenance | [Installer](tools/Installer/README.md) |
| Candidate and draft preparation | [Release workflow](docs/GITHUB-RELEASE.md) |
| Version history | [Changelog](CHANGELOG.md) |

## Build

The following PowerShell commands prepare the SDK, build the source, run regressions and create packages from the project directory:

```powershell
.\scripts\Setup.ps1
.\scripts\Build.ps1
.\scripts\Test.ps1
.\scripts\Package.ps1
```

`global.json` pins SDK **10.0.401**. Setup downloads and verifies the SDK into the ignored `.tools/dotnet` directory; the other scripts use it automatically. The project has no third-party NuGet dependencies. The regression runner uses simulated installations and does not start Battle.net or the game. Packaging produces a framework-dependent MSI and portable ZIP without installing them. Additional repository, workflow and package checks are described in the development guide.

## Project layout

| Path | Contents |
| --- | --- |
| `SC2RegionSwitcher.slnx` | Application, regression runner and icon tool |
| `src/SC2Switcher.Wpf/` | WPF interface, switching logic, configuration and language resources |
| `tests/SC2Switcher.Tests/` | Isolated regression tests |
| `tools/Installer/` | Per-machine MSI builder and validation support |
| `tools/IconGenerator/`, `assets/icon/` | Icon generator and application artwork |
| `scripts/`, `eng/`, `.github/` | Build commands, pinned tool metadata and CI workflows |
| `docs/` | User and maintainer documentation |
| `artifacts/`, `.tools/` | Ignored build outputs, evidence and local tools |

The MSI requires administrator approval, installs to `%ProgramFiles%\SC2RegionSwitcher\app` (64-bit Program Files), and creates one shared **SC2 Region Switcher** Start menu entry. The application runs with ordinary user permissions. Earlier current-user previews require a one-time uninstall before installation of this package. The migration procedure is documented in [Setup](docs/SETUP.md). Configuration and backups are stored separately in `%LOCALAPPDATA%\SC2RegionSwitcherV2` and are retained on uninstall.

The [repository](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher) is private. No application license has been selected. Third-party notices apply only to the materials they identify.
