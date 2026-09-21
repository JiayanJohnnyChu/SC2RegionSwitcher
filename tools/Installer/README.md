# Current-user MSI

Run `scripts/Package.ps1` to publish the app and build packages without installing them. To package existing published files, run `tools/Installer/Build-CurrentUserMsi.ps1 -AppDirectory <directory> -OutputDirectory <empty-directory>`.

The builder uses Windows Installer COM and makecab. It reads the three-field application version through `scripts/Release-Common.ps1`, checks the executable manifest and runtime file versions, and creates four runtime files, one Start menu shortcut and a current-user uninstall entry. The .NET 10 Desktop Runtime x64 must already be available.

## Installation and upgrades

Starting with 3.4.0, files live in `%LOCALAPPDATA%\Programs\SC2RegionSwitcher\app`. Profiles, interface preferences, backups and recovery journals remain under `%LOCALAPPDATA%\SC2RegionSwitcherV2` and are never MSI resources.

The stable UpgradeCode identifies the product family. Each numerical version has a different, deterministic ProductCode. Each newly built MSI has its own PackageCode. The three components own application files, the Start menu shortcut, and HKCU App Paths separately. Their GUIDs remain stable while their resource identities remain compatible.

The Upgrade table finds 3.3.0 and later lower versions. RemoveExistingProducts runs immediately after InstallInitialize, before ProcessComponents, so the upgrade participates in rollback. A Type 19 message rejects a detected newer version. A Type 51 action assigns the displayed install location. Neither action executes application code or a script. ALLUSERS installation is rejected; upgrades remain in the current-user context. Restart Manager automatic application shutdown is disabled.

Close the switcher before installation. Do not force a reinstall, suppress file-in-use errors, or change Windows Installer security policy. The same original MSI may be run again for normal maintenance.

## Version and release rules

Every distributed preview increments the three-field MSI ProductVersion. A suffix such as preview.2 is not an MSI version increase. Once a version is tagged or distributed, promote its original candidate bytes; do not rebuild or replace that version.

The original 3.3.0 package contains no downgrade protection. A newer package cannot retrofit that old file. Do not run the old package over a newer installation. To return to it deliberately, uninstall the newer version first and preserve user settings.

## Validation

Run `scripts/Test-Package.ps1 -ReleaseDirectory <directory>` and `scripts/Test-ReleaseGuards.ps1 -ReleaseDirectory <directory>`. These validate content and rejection cases; they do not install the product. The separate lifecycle lab uses copies with isolated product, directory, shortcut and registry identities. Its failure injection must never target the production product family.

Real upgrade results belong to an exact MSI hash. Local rebuilds and CI builds cannot share an installation pass merely because their source or version matches.

On the current test host, a deterministic failure after InstallExecute produced access-denied errors while Windows Installer rolled back its own registry data. The previous product was not automatically restored. The isolated test's normal uninstall/reinstall recovery worked, but automatic recovery is not marked passed. Do not change system registry ACLs or Installer security policy to make the test pass. Resolve this on an appropriate clean test environment before public release.
