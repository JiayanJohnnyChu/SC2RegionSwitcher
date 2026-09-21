[简体中文](ARCHITECTURE.zh-CN.md)

# Architecture

SC2 Region Switcher is a C# / WPF / .NET 10 application for Windows x64. It coordinates one official Battle.net app and two independent StarCraft II installations. It does not modify Battle.net's game-installation database.

## Source structure

All application files below are in `src/SC2Switcher.Wpf`.

| File | Responsibility |
| --- | --- |
| `App.xaml.cs` | Startup, user data directory, configuration loading and diagnostic arguments |
| `MainWindow.xaml`, `MainWindow.xaml.cs` | Main interface and operation coordination |
| `MainWindow.Diagnostics.cs` | Layout reports and isolated synthetic presentation states |
| `MainViewModel.cs` | Current region, selected destination, operation phase and action availability |
| `SettingsPage.xaml`, `SettingsPage.xaml.cs` | Path editing, native pickers, language selection and unsaved changes |
| `ConfigurationStore.cs` | Validation, migration, backups, atomic saving and concurrent-change protection |
| `Core.cs` | Installation manifests, file-activity checks, language transactions, Battle.net adapter and switch engine |
| `Localization.cs`, `Strings.*.json` | Interface language and translated resources |
| `Palette.xaml`, `app.manifest` | Shared colors and Windows/DPI declarations |

The regression project links core source files and supplies `FakePlatform` implementations. It tests behavior against simulated installations without launching Battle.net.

## Switch transaction

1. Validate configured paths, installation branches, language data and game/editor state.
2. Request a normal Battle.net exit, wait, and recheck the installations.
3. Back up `Variables.txt` and atomically update its language keys, recording original and applied hashes.
4. Launch Battle.net with the selected login-region argument and observe its local region record.
5. Commit the transaction, or attempt recovery when the failure stage and file-consistency checks permit it.

At the start of a switch, an unfinished language transaction is recovered before new language changes are made. Recovery verifies journal paths, backup placement and file hashes before restoring the settings. Merely opening the app or rechecking installations does not perform recovery.

The interface has no cancellation control. The engine retains cancellation tokens for flow boundaries and tests; after the target Battle.net launch request is issued, later cancellation does not by itself cause language rollback. Relevant settings are locked while busy.

File-activity checks are observations of local files, not an official updater-status interface. Likewise, an observed login-region record confirms local configuration rather than account authentication or the selected game server.

## Configuration and localization

State is stored under `%LOCALAPPDATA%\SC2RegionSwitcherV2`. `profiles.json` stores paths and game profiles; `ui-preferences.json` stores interface language. Configuration backups, language backups and pending recovery records share this data root. They are not package resources.

China uses `zhCN` text and speech; Global uses `enUS`. Interface language is independent. Missing or invalid language preferences fall back to `en-US`; valid `en-US` and `zh-CN` preferences are retained. Both translation catalogs must have identical keys and matching format placeholders.

The current Battle.net configuration and the selected destination are separate state. Global's EU/US/KR label reflects the selected login region. It does not set or verify the StarCraft II game server.

## Installation

Since 3.4.0, current-user MSI packages use `%LOCALAPPDATA%\Programs\SC2RegionSwitcher\app`, one Start menu entry and separate components for application files, the shortcut and App Paths. Major upgrades remove older products in the family. Uninstall preserves the separate user data directory. The package requires .NET 10 Desktop Runtime x64.

Version 3.4.1 changes UI labels and installer authoring; `Core.cs` and `ConfigurationStore.cs` are unchanged. Installation-failure recovery remains a separate unresolved acceptance item. See [Installer](../tools/Installer/README.md) and [Validation](VALIDATION-3.4.1.md).
