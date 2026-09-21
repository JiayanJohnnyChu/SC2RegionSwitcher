[简体中文](README.zh-CN.md)

# SC2 Region Switcher — portable package

Requirements: Windows x64, Microsoft .NET 10 Desktop Runtime x64, Battle.net, and separate China and Global StarCraft II installations. The China installation uses Chinese game data; the Global installation uses English game data.

1. Extract the archive into a folder. Keep the four application files together.
2. Run `SC2Switcher.Wpf.exe`.
3. Open **Settings** and select the Battle.net executable, both game installation folders, and the shared StarCraft II `Variables.txt` file.
4. Select a destination and, for Global, a Battle.net login region. Start the switch after closing StarCraft II. Complete any required account login in Battle.net.
5. Select the actual StarCraft II game server in Battle.net before launching the game. The switcher's EU, US and KR labels indicate Battle.net login regions.

The interface supports English and Simplified Chinese. Interface language is selected in Settings and is independent of game language.

Configuration and backups are stored in `%LOCALAPPDATA%\SC2RegionSwitcherV2`. Moving the application folder does not move these settings. This portable package does not create a Start menu shortcut or a Windows uninstall entry.

See the [configuration guide](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/blob/main/docs/SETUP.md) for installation paths and operating details.
