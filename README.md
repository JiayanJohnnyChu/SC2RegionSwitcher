[English](README.md) · [简体中文](README.zh-CN.md) · [Français](README.fr-FR.md) · [Deutsch](README.de-DE.md) · [Nederlands](README.nl-NL.md)

# SC2 Region Switcher

**Downloads:** [v3.5.3-preview.1](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/releases/tag/v3.5.3-preview.1) · [All releases](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/releases)

SC2 Region Switcher helps players use separate China and Global installations of StarCraft II with one Battle.net app. It switches the Battle.net login region and the game's shared language settings.

The application supports eleven interface languages and selection of an installed Global game language.

![SC2 Region Switcher with simulated configuration](assets/screenshots/sc2-switcher-3.5.1.png)

## Installation

Windows x64 and **[Microsoft .NET 10 Desktop Runtime x64](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)** are required. The SDK is unnecessary for playing. The available packages are unsigned.

| Package | Suitable use |
| --- | --- |
| MSI | Installation for all Windows users, with administrator approval, a Start menu entry and normal Windows uninstall support. The app itself runs with ordinary user permissions. |
| Portable ZIP | Complete extraction into a separate folder, with all included files kept together. `SC2Switcher.Wpf.exe` starts the app; no shortcut or uninstall entry is created. |

GitHub's “Source code” downloads are not runnable application packages.

## First configuration

1. Battle.net contains two completed, separate game installations, for example `D:\Games\StarCraft II CN` and `D:\Games\StarCraft II Global`. China requires Simplified Chinese text and speech; Global requires both resources for the selected language.
2. The final installation path is checked in Battle.net before installation, even when **Install** is shown. Battle.net may append a `StarCraft II` subfolder or detect the other installation.
3. Settings records the Battle.net folder, both game folders and the actual `StarCraft II\Variables.txt` from the game's Documents folder. A normal game launch and exit creates this file if it is absent.
4. **Save & check** validates and saves the paths. Saving alone does not change the game's language. Full preparation and path examples are in the [setup guide](docs/SETUP.md).

## Switching and language

Switching requires StarCraft II and its editor to be closed and Battle.net downloads, updates and repairs to be complete. The selected destination is China or Global; Global also has an EU, US or KR login-region choice. The switcher closes Battle.net normally, applies the game-language settings with a backup, and opens the requested login region. Controls remain locked until the operation finishes.

Login, game-server selection and game launch take place in Battle.net. **EU, US and KR are Battle.net login regions, not a confirmation of the selected game server.** Interface language is independent of game language. The application applies a saved Global language choice on the next switch; both text and speech resources must already be installed through Battle.net.

## Saved games, backups and updates

Within the game's shared settings, the switcher changes only the language keys. It does not copy, delete or manage campaign saves, replays or `Accounts` files. Game accounts and regions determine access to progress; the switcher does not provide cloud-save synchronization.

Switcher settings and backups are stored in `%LOCALAPPDATA%\SC2RegionSwitcherV2` and remain after uninstall. A pending recovery needs the original configuration and backups. MSI updates use a newer installer with the app closed; a portable update uses a new folder and an updated shortcut. Neither method moves the game installations.

## Common questions

| Question | Answer |
| --- | --- |
| The app does not start | It needs Desktop Runtime 10 x64 and all included application files. |
| A game folder or language is unavailable | The selected folder must be the game root, with installation and both required language resources complete. |
| Switching fails or recovery is pending | The error details identify the next check. The original settings and backups remain necessary; detailed recovery steps are in the guide. |

| More information | Links |
| --- | --- |
| Player guide | [Setup and troubleshooting](docs/SETUP.md) |
| Versions and development | [Changelog](CHANGELOG.md), [Development](docs/DEVELOPMENT.md), [Validation](docs/VALIDATION.md) |

Original project material uses [MIT](LICENSE); [third-party notices](THIRD-PARTY-NOTICES.md) describe the included fonts and other licensed materials.
