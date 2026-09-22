[简体中文](README.zh-CN.md)

# Per-machine MSI

`scripts/Package.ps1` publishes the application and builds packages without installing them. To package existing published files, use:

```powershell
.\tools\Installer\Build-MachineMsi.ps1 -AppDirectory <directory> -OutputDirectory <empty-directory>
```

The builder uses Windows Installer COM and `makecab`. It reads the three-part version through `scripts/Release-Common.ps1`, checks the executable manifest and runtime versions, and packages four runtime files. The current filename is `SC2Switcher-3.4.1-x64.msi`. Microsoft .NET 10 Desktop Runtime x64 is a separate dependency; packages are unsigned.

## Installation and ownership

The MSI sets `ALLUSERS=1` and requires administrator approval. It installs to `ProgramFiles64Folder\SC2RegionSwitcher\app`, registers HKLM App Paths and creates one common Start menu entry. The application remains `asInvoker` and runs with ordinary user permissions. Profiles, preferences, backups and recovery journals stay in each user's `%LOCALAPPDATA%\SC2RegionSwitcherV2`, outside MSI ownership and preserved on uninstall.

The Application component owns the four runtime files and the advertised Start menu shortcut, with the EXE as its key path. AppRegistration owns the HKLM App Paths entry. The product-family UpgradeCode is retained, while machine installation uses new ProductCode and component identities. Each numerical version has a distinct ProductCode; each build has a new PackageCode.

## Migration and upgrades

The earlier 3.3.0 and 3.4.0 current-user packages were internal previews. Uninstall the old package as its owning user, then install the new MSI. Keep the separate user data directory. Windows Installer does not perform major upgrades across installation contexts; see Microsoft's [Major Upgrades](https://learn.microsoft.com/en-us/windows/win32/msi/major-upgrades). A shared UpgradeCode does not remove this restriction.

AppSearch and RegLocator check the invoking user's old HKCU App Paths entry. If found, the installer blocks the new installation and asks for the old preview to be uninstalled. This check does not search other users' profiles. The transition is a one-time uninstall/reinstall, followed by ordinary major upgrades between later machine releases.

For machine upgrades, `RemoveExistingProducts` runs immediately after `InstallInitialize`, before `ProcessComponents` and new file installation, so removal is inside the rollback transaction. Newer related machine versions are rejected. Restart Manager automatic shutdown is disabled; close the switcher before installation. The original MSI supports maintenance invocation.

## Validation

The database uses standard MSI definitions and imports constraints from [metadata/_Validation.idt](metadata/_Validation.idt). The [metadata notice](metadata/README.md) records its source and license. `scripts/Setup-InstallerTools.ps1` prepares pinned, hash-verified WiX 3.14.1 tools under `.tools/`.

| Check | Scope |
| --- | --- |
| `scripts/Test-Package.ps1 -ReleaseDirectory <directory>` | Package contents, authoring and hashes |
| `scripts/Test-ReleaseGuards.ps1 -ReleaseDirectory <directory>` | Release rejection cases |
| `scripts/Test-InstallerSchema.ps1 -ReleaseDirectory <directory>` | Full unsuppressed ICE suite; machine packages require zero errors and warnings |
| `tools/Installer/Test-MachineInstall.ps1`, `installer-lifecycle.yml` | Isolated machine installation, maintenance, upgrade, recovery and removal |

Lifecycle acceptance for the new package is pending. Record the exact tested MSI hash and source revision. The old `installer-recovery.yml` workflow and administrator/standard-user comparisons describe the earlier current-user design; they do not validate this machine package. See [3.4.1 validation](../../docs/VALIDATION-3.4.1.md).

Every distributed preview increments the three-part ProductVersion. Preserve original files once tagged or distributed, and promote accepted CI artifacts without rebuilding. The original 3.3.0 MSI lacks downgrade protection. See the [release workflow](../../docs/GITHUB-RELEASE.md).
