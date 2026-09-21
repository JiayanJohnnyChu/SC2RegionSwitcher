# SC2 Region Switcher 3.4.0 Preview

Native WPF for Windows x64. English is the default interface language; Simplified Chinese is available in Settings. China uses Simplified Chinese game data and Global uses English game data.

## Changes

- Refined typography, compact spacing and page alignment, with separate China and Global colors.
- Kept the current Battle.net configuration separate from the selected destination.
- Added current-user MSI upgrades from 3.3.0, a stable application directory and one Start menu entry.
- Added version/provenance validation and promotion of original CI candidate files without rebuilding.

## Requirements and use

Install Microsoft .NET 10 Desktop Runtime x64. Configure one official Battle.net app, two independent StarCraft II installations and the shared Variables.txt file in Settings. Login, game installation, updates and game startup remain in the official Battle.net app.

Close the switcher before running the MSI. It preserves profiles, interface preferences, backups and recovery files. The original 3.3.0 installer has no downgrade protection; do not run it over a newer version. The portable ZIP must be fully extracted.

An installation originally deployed by hand may retain a manually created shortcut or App Paths entry pointing at its retired directory. Back up and migrate those identified legacy resources before treating the MSI upgrade as verified. The test machine required this one-time migration; a clean MSI-to-MSI upgrade passed independently.

## Validation record

CI checks compilation, isolated regressions, package structure and rejection cases. The release manifest identifies the original source commit and candidate CI run. This draft's maintainer validation record separately lists actual installation, DPI and China–Global–China checks. A synthetic UI render or local region record does not establish an online game session.

The isolated installer failure test did not automatically restore the old installation on the test host: Windows Installer reported access denied while rolling back its registry data. Normal uninstall and reinstall recovered the isolated test installation. Automatic installer recovery remains an unresolved release gate; it is separate from the application's game-language recovery logic. No system ACL or security policy was weakened.

This is an unsigned preview. Cross-computer compatibility, unavailable display configurations and any outstanding authentication-dependent checks must be assessed before a public release. See [the setup guide](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/blob/v3.4.0-preview.1/docs/SETUP.zh-CN.md) and [validation scope](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/blob/v3.4.0-preview.1/docs/VALIDATION.md).

## 中文说明

本版使用原生 WPF，默认英文界面，可在设置中切换简体中文。安装器支持从 3.3.0 升级，保留用户设置，并维持一个开始菜单入口。需要 .NET 10 Desktop Runtime x64、一套官方战网和两套独立游戏安装。附件沿用接受验证的 CI 候选原文件；实际安装、缩放及双服在线测试结果在草稿的验证记录中分别列明。

隔离故障测试中，安装器自动恢复旧版本未通过，日志记录 Windows Installer 注册表回滚访问被拒绝；正常卸载、重新安装可恢复。此项仍是公开发布前的阻塞项，不能用正常升级成功替代。
