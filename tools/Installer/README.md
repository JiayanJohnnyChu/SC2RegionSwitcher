[简体中文](README.zh-CN.md)

# Per-machine MSI maintenance

`scripts/Package.ps1` publishes and packages without installation. Existing runtime files can be packaged with:

```powershell
.\tools\Installer\Build-MachineMsi.ps1 -AppDirectory <directory> -OutputDirectory <empty-directory>
```

The builder uses Windows Installer COM and `makecab`, reads the three-part version through `scripts/Release-Common.ps1`, and checks the executable manifest/runtime versions. The current definition has four runtime files and nine licensing/source files in one embedded cabinet. Version 3.5.1 packaging and hosted CI remain pending. Packages are unsigned; .NET 10 Desktop Runtime x64 is a separate dependency.

## Ownership and upgrades

| Area | Definition |
| --- | --- |
| Installation | `ALLUSERS=1`; administrator approval; `ProgramFiles64Folder\SC2RegionSwitcher\app` |
| Entry points | HKLM App Paths and one common advertised Start menu shortcut |
| Application | `asInvoker`; ordinary-user execution |
| User data | `%LOCALAPPDATA%\SC2RegionSwitcherV2`; profiles, preferences, backups and recovery journals remain outside MSI ownership and survive uninstall |
| Identity | Stable product-family UpgradeCode; new ProductCode for each numerical version and PackageCode for each build |
| Components | Application owns runtime files/shortcut, AppRegistration owns App Paths, nine file-keyed components own licensing/source files |

Earlier current-user previews require uninstall by their owning user before machine installation. The invoking user's legacy HKCU App Paths entry blocks installation; other profiles are not searched. A shared UpgradeCode does not allow upgrades across installation contexts.

Later machine versions use normal major upgrades. `RemoveExistingProducts` follows `InstallInitialize`, keeping removal inside rollback. Newer related versions are rejected. Automatic Restart Manager shutdown is disabled; the application must be closed. The original MSI supports maintenance.

## Checks

| Command / workflow | Scope |
| --- | --- |
| `scripts/Test-Package.ps1 -ReleaseDirectory <directory>` | Contents, authoring and hashes |
| `scripts/Test-ReleaseGuards.ps1 -ReleaseDirectory <directory>` | Rejection cases |
| `scripts/Test-InstallerSchema.ps1 -ReleaseDirectory <directory>` | Full ICE suite without suppressions; zero errors/warnings required |
| `tools/Installer/Test-MachineInstall.ps1`, `installer-lifecycle.yml` | Isolated installation, maintenance, upgrade, recovery and removal |

`scripts/Setup-InstallerTools.ps1` prepares hash-verified WiX 3.14.1 tools. [Metadata provenance](metadata/README.md) covers the imported validation constraints.

[Development](../../docs/DEVELOPMENT.md) defines versioning and release promotion; [Validation](../../docs/VALIDATION.md) records the packages and source versions covered by each check.
