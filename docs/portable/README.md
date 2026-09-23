[简体中文](README.zh-CN.md)

# SC2 Region Switcher — portable package

The application requires Windows x64, Microsoft .NET 10 Desktop Runtime x64, one Battle.net app, and separate China and Global StarCraft II installations. China uses Simplified Chinese text and speech; Global uses the selected installed language.

1. Portable deployment requires complete extraction into a separate directory, with all supplied application files, notices and licenses kept together.
2. The application entry point is `SC2Switcher.Wpf.exe`.
3. **Settings** specifies the folder holding `Battle.net.exe`, both game installation folders, and the actual shared `Variables.txt` file under **Advanced · shared game settings**. **Save & check** validates and saves this configuration.
4. Switching requires StarCraft II and its editor to be closed and Battle.net downloads, updates or repairs to be complete. The selected destination is China or Global; Global also requires a Battle.net login region. The switch action opens the target region, where any required login takes place in Battle.net.
5. Game launch follows selection of the actual StarCraft II game server in Battle.net. The switcher's EU, US and KR labels identify login regions.

**Global game language (experimental)** in Settings selects one language for both text and speech. The list contains languages whose text and speech resources are both recorded as installed. Additional resources must be installed through Battle.net. An unavailable game drive disables selection without changing the saved preference. **Save & check** saves the preference; game settings change only during a switch. Legacy mixed pairs remain intact until a unified language is selected. Pending recovery must complete with the original configuration before changing game language.

Eleven interface languages are available in the header and Settings: English, Simplified Chinese, French, German, Dutch, Korean, Italian, Spanish, European Portuguese, Latin and Modern Greek. They are independent of game language. There is no cancellation control during a switch.

Configuration and backups are stored in `%LOCALAPPDATA%\SC2RegionSwitcherV2`. Moving the application folder does not move these settings. The portable package creates no Start menu shortcut or Windows uninstall entry.

Original project code, documentation and icons use the MIT License in `LICENSE`. The package also contains bilingual `THIRD-PARTY-NOTICES` files and the `licenses/` directory, including the Inter and Noto font licenses, the Radix license and the WiX license with its metadata source. These files remain part of the extracted distribution.

Portable distributions are unsigned. Operating details and the tested scope are documented in the [setup guide](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/blob/main/docs/SETUP.md) and [validation record](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/blob/main/docs/VALIDATION.md). Published downloads are available on the [releases page](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/releases).
