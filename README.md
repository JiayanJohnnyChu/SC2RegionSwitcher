[简体中文](README.zh-CN.md)

# SC2 Region Switcher

A Windows x64 application for switching between separate China and Global installations of StarCraft II. It uses one official Battle.net desktop app, changes its login region, and updates the shared game-language settings. China uses Simplified Chinese (`zhCN`); Global uses English (`enUS`). The interface supports English and Simplified Chinese, with its own language setting.

The application is written in C# with WPF and .NET 10. Account login, game installation, updates, game-server selection and game startup take place in Battle.net. The Global labels **EU**, **US** and **KR** identify Battle.net login regions; they do not establish which game server is selected.

**Current status: 3.4.1 development preview.** The administrator-required, per-machine MSI has passed local build and package checks; CI lifecycle acceptance is pending. Version 3.4.1 is not a release candidate; the earlier 3.4.0 candidate remains an unpublished private draft. Packages are unsigned and require **Microsoft .NET 10 Desktop Runtime x64**. See the [validation record](docs/VALIDATION.md) for the tested scope.

## Documentation

| Task | Guide |
| --- | --- |
| Prepare installations, configure paths and switch regions | [Setup and use](docs/SETUP.md) |
| Build and test the source | [Development](docs/DEVELOPMENT.md) |
| Understand the switching and configuration logic | [Architecture](docs/ARCHITECTURE.md) |
| Maintain the interface and translations | [Interface design](docs/UI-DESIGN.md) |
| Review current work and validation | [Development status](docs/HANDOFF.md), [3.4.1 validation](docs/VALIDATION-3.4.1.md) |
| Maintain MSI packages | [Installer](tools/Installer/README.md) |
| Prepare a candidate and draft release | [Release workflow](docs/GITHUB-RELEASE.md) |
| Review version changes | [Changelog](CHANGELOG.md) |

## Build

Run these commands in PowerShell from the project directory:

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

The MSI requires administrator approval, installs to `%ProgramFiles%\SC2RegionSwitcher\app` (64-bit Program Files), and creates one shared **SC2 Region Switcher** Start menu entry. The application runs with ordinary user permissions. Earlier current-user previews require a one-time uninstall before installing this package; see [Setup](docs/SETUP.md). Configuration and backups are stored separately in `%LOCALAPPDATA%\SC2RegionSwitcherV2` and are retained on uninstall.

The [repository](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher) is private. No application license has been selected. Third-party notices apply only to the materials they identify.
