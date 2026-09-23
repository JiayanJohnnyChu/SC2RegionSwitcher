[简体中文](SETUP.zh-CN.md)

# Setup and use

SC2 Region Switcher uses one Battle.net app and two separate StarCraft II installations. It changes the Battle.net login region and the game's shared language settings.

Downloads are available from [v3.5.3-preview.1](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/releases/tag/v3.5.3-preview.1). The application supports eleven interface languages. China uses Simplified Chinese text and speech; Global uses a selected installed text/speech pair.

## 1. Download and installation

The application requires Windows x64 and **Microsoft .NET 10 Desktop Runtime x64**, available from [Microsoft's .NET 10 download page](https://dotnet.microsoft.com/en-us/download/dotnet/10.0). The SDK is unnecessary for normal use. Packages are unsigned.

| Download | Installation and startup |
| --- | --- |
| `SC2Switcher-3.5.3-x64.msi` | Installation requires the switcher to be closed and administrator approval. It installs for all Windows users and creates a **SC2 Region Switcher** Start menu entry. The application runs with ordinary user permissions. |
| `SC2Switcher-3.5.3-win-x64-preview.zip` | Complete extraction into a separate folder provides the portable application. Its entry point is `SC2Switcher.Wpf.exe`. It creates no shortcut or Windows uninstall entry. |

GitHub's **Source code** archives contain source, not a runnable application. Portable extraction retains all supplied files, including the four runtime files below and the accompanying notices and licenses:

```text
SC2Switcher.Wpf.exe
SC2Switcher.Wpf.dll
SC2Switcher.Wpf.deps.json
SC2Switcher.Wpf.runtimeconfig.json
```

An optional portable shortcut points to the extracted EXE, with its containing folder as the working directory. One application entry serves both game installations.

## 2. Preparation in Battle.net

Preparation requires access to the relevant regional services and two completed game installations. The switcher does not download game files, register accounts or change account eligibility.

The installations use separate folders, for example:

```text
D:\Games\StarCraft II CN
D:\Games\StarCraft II Global
```

Neither folder can be inside the other. Two names or directory links pointing to one installation are not two installations. Each selected game root directly contains `StarCraft II.exe`, `.build.info` and `Versions`.

**The final installation location needs checking even when Battle.net displays Install.** The launcher may append a `StarCraft II` subfolder to the chosen path or detect the other installation. The location shown immediately before installation must correspond to the intended separate game root. A path pointing into the other region's folder is unsuitable.

China requires both Simplified Chinese text and speech. Global requires both resources for the selected language. Resources are installed through Battle.net. A language shown in the switcher does not prove that an ongoing download has finished.

Each completed installation is checked by launching it through Battle.net, reaching the game main interface and exiting normally. This also creates the shared settings file. Changing a path in the switcher's settings does not move game files or change Battle.net's installation records.

### Optional login-region preparation

Normal Battle.net preparation comes first. If the required login region is unavailable, the following alternative PowerShell commands request a region from the same Battle.net executable. The example path must match the actual installation. This requires the game and editor to be closed, downloads to be complete and Battle.net to have exited normally.

```powershell
& 'C:\Program Files (x86)\Battle.net\Battle.net.exe' --setregion=CN
& 'C:\Program Files (x86)\Battle.net\Battle.net.exe' --setregion=EU
```

Only the command for the required region is applicable; `US` and `KR` are the other Global values. The resulting Battle.net login screen confirms the requested region. Support depends on the Battle.net version. If the request is not accepted, regional access in Battle.net remains a prerequisite for configuring the switcher.

## 3. First configuration

The application opens Settings when configuration is absent or invalid. The following paths are examples; each field requires the location actually used on the computer.

| Setting | Required location | Example |
| --- | --- | --- |
| Battle.net folder | Folder directly containing `Battle.net.exe` | `C:\Program Files (x86)\Battle.net` |
| China installation | China game root | `D:\Games\StarCraft II CN` |
| Global installation | Separate Global game root | `D:\Games\StarCraft II Global` |
| Shared game settings file | Actual `Variables.txt` in the game's Documents location | `D:\Documents\StarCraft II\Variables.txt`, if Documents is located there |

The file field is under **Advanced · shared game settings**. Each location can be selected with its folder or file button or entered as a full path; typed paths do not expand expressions such as `%USERPROFILE%`.

The shared file is normally `StarCraft II\Variables.txt` beneath the current Windows user's actual Documents folder. If Documents has moved, its real location applies. The file must have been generated by the game and contain the `localeidassets` and `localeiddata` language entries. A normal game session with saved settings and a normal exit creates it if missing. An empty file or a copy elsewhere does not substitute for the file the game uses.

The current path checks reject paths through directory links and other reparse points, including some redirected or synchronized folders. The actual location and its accessibility matter, not merely the folder's displayed name.

**Save & check** validates and saves the configuration. Invalid input leaves the saved configuration unchanged. Saving alone does not write the game's language settings. Unsaved changes require saving or **Discard changes** before returning, pressing Escape or closing the window.

## 4. Switching regions

1. StarCraft II and its editor are closed. Battle.net downloads, updates and repairs are complete.
2. The switcher's installation check passes. A game update requires a fresh check.
3. The destination is China or Global. Global additionally uses Europe (`EU`), Americas (`US`) or Asia (`KR`) as its Battle.net login region.
4. The switch action requests a normal Battle.net exit, backs up and updates the game-language settings, then starts Battle.net in the selected region.
5. Any login takes place in Battle.net. Game launch follows confirmation of the correct installation and selection of the actual game server there.

Controls and normal window closing remain locked until the operation finishes. There is no cancellation button. A different target can be requested after completion. Battle.net prompts are handled in Battle.net; the switcher does not force-close it.

**Login region and game server are separate choices.** EU, US and KR identify Battle.net login regions. Selecting a destination does not immediately change the current-region readout, and a local region record does not prove account authentication or the selected StarCraft II server.

The action is disabled when the observed region, text language and speech language already match the target. A required recovery or unknown state is handled separately.

## 5. Interface and game languages

The interface language changes the switcher's labels only. It takes effect immediately, is remembered for the next launch and does not change either game's language.

The **Global game language (experimental)** field selects one language for both text and speech. It lists languages with both resources declared by the installation. Additional resources require installation through Battle.net.

Opening Settings or finishing a Global path edit starts detection. **Refresh languages** checks again. An unavailable drive, unfinished update or missing language resources can disable selection without replacing the saved preference. After a path change, saving and checking the new folder precedes language selection.

A language choice remains a draft until **Save & check** succeeds and is applied only during a later switch. Older configurations with different text and speech languages remain intact until a unified language is selected. **Discard changes** restores the saved paths and language pair. A pending recovery must finish with the original configuration before the game language changes.

## 6. Saved games, backups and recovery

Within the game's shared settings file, the switcher changes only `localeidassets` and `localeiddata`. It does not copy, delete or manage campaign saves, replays or `Accounts` files, and it does not transfer progress between regions or accounts. Progress availability is determined by the game account and region. The switcher provides no cloud-save synchronization.

**Language-setting backups are not campaign-save backups.** They preserve the shared settings before a language change. Switcher data is stored separately in `%LOCALAPPDATA%\SC2RegionSwitcherV2`:

| File or folder | Contents |
| --- | --- |
| `profiles.json` | Installation paths, game-language settings and preferred Global login region |
| `ui-preferences.json` | Interface language |
| `ConfigurationBackups` | Earlier switcher configurations |
| `Backups` | Shared game-settings backups |
| `pending-language.json` | A language change awaiting completion or recovery |
| `last-success.json`, `last-error.json` | Recent operation records, when present |

An unfinished operation requires preservation of the original paths, configuration, recovery record and backups. With the game and editor closed and the original installation checks passing, a subsequent switch attempts recovery before making another language change. Opening the app or running a check alone does not start recovery.

A repeated recovery failure requires the error and files to remain available for diagnosis. Deleting the recovery record or overwriting current settings with an old backup can remove the information needed for recovery.

## 7. Updates, uninstall and moved games

MSI updates use a newer installer while the switcher is closed. Uninstall is available through Windows Installed apps. Settings and backups in `%LOCALAPPDATA%\SC2RegionSwitcherV2` remain after uninstall.

For a portable update, the app is closed, the new ZIP is extracted into a new folder and any shortcut is updated to point there. Portable files do not belong in an MSI-managed application folder. Removing a portable application folder does not remove the separately stored settings.

Older previews installed for one Windows user require a one-time uninstall by that user before the current all-users installation. Keeping the separate data folder preserves that user's configuration and backups. The old 3.3.0 installer is unsuitable for installation over a newer version.

After a game is moved, Battle.net must recognize and check its new location before the corresponding switcher path changes. Existing shortcuts may still point to an older switcher folder. A valid old `profiles.json` beside the application can be imported when no current configuration exists; the original file is retained.

## 8. Common problems

| Symptom | Explanation or next check |
| --- | --- |
| The app does not start or asks for .NET | Desktop Runtime 10 x64 and all four runtime files are required. |
| Missing game files or wrong region | The path must point directly to the intended game root, with installation or repair complete. |
| A language is unavailable | Both text and speech resources must be installed through Battle.net; a fresh language check follows completion. |
| An update is reported as incomplete | Battle.net must finish its download, update or repair before a new check. |
| `Variables.txt` is missing or invalid | The app needs the file actually generated and used by the game. A normal game session and exit creates it if absent. |
| A linked or redirected path is rejected | The current checks do not support paths through reparse points. The game's real folder and Documents location need checking. |
| The game/editor is running or Battle.net will not exit | Normal exit and resolution of any Battle.net prompts precede another attempt. |
| The login region cannot be confirmed | The Battle.net login screen and version help identify the problem; launcher updates can change supported behavior. |
| Configuration changed in another window | Further editing requires duplicate switcher windows to be closed and the app to be reopened. |
| A file cannot be written | Read-only attributes, current-user access and locks held by other programs can prevent writing. |

The two game installations do not need identical build numbers. Official updates can still change compatibility. A useful problem report includes the switcher, Windows, .NET and Battle.net versions, the steps taken and the error text, with personal paths and account information removed.

[Version history](../CHANGELOG.md) · [Current validation](VALIDATION.md)
