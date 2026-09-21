[简体中文](README.zh-CN.md)

# SC2 Region Switcher — portable package

Requires Windows x64, Microsoft .NET 10 Desktop Runtime x64, one Battle.net app, and separate China and Global StarCraft II installations. China uses Simplified Chinese text and speech; Global uses English.

1. Extract the archive into its own directory, keeping the four application files together.
2. Run `SC2Switcher.Wpf.exe`.
3. In **Settings**, choose the folder containing `Battle.net.exe`, both game installation folders, and the actual shared `Variables.txt` file under Advanced. Select **Save & check**.
4. Close StarCraft II and its editor, and wait for Battle.net downloads or updates to finish. Select a destination and, for Global, a Battle.net login region. Start the switch and complete any required login in Battle.net.
5. Select the actual StarCraft II game server in Battle.net before launching the game. The switcher's EU, US and KR labels identify login regions.

Choose English or Simplified Chinese in Settings. Interface language is independent of game language. There is no cancellation control during a switch.

Configuration and backups are stored in `%LOCALAPPDATA%\SC2RegionSwitcherV2`. Moving the application folder does not move these settings. The portable package creates no Start menu shortcut or Windows uninstall entry.

This is an unsigned preview. See the [setup guide](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/blob/main/docs/SETUP.md) and [validation record](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/blob/main/docs/VALIDATION.md) for operating details and tested scope. Repository access is required while the repository is private.
