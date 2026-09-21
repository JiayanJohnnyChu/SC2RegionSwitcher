[简体中文](README.zh-CN.md)

# Current-user MSI

`scripts/Package.ps1` publishes the application and builds its packages without installing them. To package existing published files, use:

```powershell
.\tools\Installer\Build-CurrentUserMsi.ps1 -AppDirectory <directory> -OutputDirectory <empty-directory>
```

The builder uses Windows Installer COM and `makecab`. It reads the three-part application version through `scripts/Release-Common.ps1`, checks the executable manifest and runtime versions, and packages four runtime files. Installation creates one Start menu shortcut and a current-user uninstall entry. Microsoft .NET 10 Desktop Runtime x64 is a separate dependency.

## Resource ownership

Since 3.4.0, application files use `%LOCALAPPDATA%\Programs\SC2RegionSwitcher\app`. Profiles, preferences, backups and recovery journals use `%LOCALAPPDATA%\SC2RegionSwitcherV2`; they are outside MSI ownership and survive uninstall.

The stable UpgradeCode identifies the product family. Each numerical version has a distinct deterministic ProductCode, and each newly built MSI has a new PackageCode. Three components separately own application files, the Start menu shortcut and HKCU App Paths.

In 3.4.1, the application component uses an HKCU marker as its key path to satisfy ICE38 for user-profile resources. The key-path change introduces a new component GUID. The shortcut and App Paths component identities are retained. Component GUIDs remain stable while resource identities remain compatible.

## Upgrade sequence

The Upgrade table detects lower versions from 3.3.0 onward. `RemoveExistingProducts` runs immediately after `InstallInitialize`, before `ProcessComponents`, placing old-product removal within the rollback transaction. A Type 19 action rejects a detected newer version; a Type 51 action sets the displayed install location. Neither runs application code or a script.

The package rejects ALLUSERS and stays in the current-user context. Restart Manager automatic shutdown is disabled, so the switcher should be closed before installation. The same original MSI supports ordinary maintenance invocation.

This sequence describes authoring intent. Failed-upgrade recovery has not passed acceptance in the standard-user test environment. See [validation status](../../docs/VALIDATION-3.4.1.md).

## Schema and validation

The database uses standard MSI column definitions and imports constraints from [metadata/_Validation.idt](metadata/_Validation.idt). The [metadata notice](metadata/README.md) describes its source and license. `scripts/Setup-InstallerTools.ps1` prepares pinned, hash-verified WiX 3.14.1 validation tools in `.tools/` without installing them system-wide.

| Check | Scope |
| --- | --- |
| `scripts/Test-Package.ps1 -ReleaseDirectory <directory>` | Package files, content and hashes |
| `scripts/Test-ReleaseGuards.ps1 -ReleaseDirectory <directory>` | Release rejection cases |
| `scripts/Test-InstallerSchema.ps1 -ReleaseDirectory <directory>` | Independent standard ICE suite |
| Isolated lifecycle/recovery tests | Actual installation behavior using separate product, directory, shortcut and registry identities |

No ICE is suppressed. The recorded result has zero errors and four ICE91 warnings about hypothetical per-machine use of files in fixed per-user directories. The script rejects other warnings, checks the candidate hash and confirms validation did not alter the MSI. These checks do not install the product.

Actual installation results identify the tested MSI hash. Local and CI builds need separate evidence even when they share a source version. The dedicated CI recovery test is designed to exercise native file-copy failure after old-product removal. Its latest run was blocked during baseline installation by runner policy; see the validation status for the recorded scope.

## Version policy

Every distributed preview increments the three-part ProductVersion. Preview suffixes do not change the MSI version. Preserve original candidate files once a version is tagged or distributed. The original 3.3.0 MSI has no downgrade protection; uninstall a newer application before deliberately returning to that package.

See the [release workflow](../../docs/GITHUB-RELEASE.md) for provenance and promotion requirements.
