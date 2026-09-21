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

## Validation record

CI checks compilation, isolated regressions, package structure and rejection cases. The release manifest identifies the original source commit and candidate CI run. This draft's maintainer validation record separately lists actual installation, DPI and China–Global–China checks. A synthetic UI render or local region record does not establish an online game session.

This is an unsigned preview. Cross-computer compatibility, unavailable display configurations and any outstanding authentication-dependent checks must be assessed before a public release. See [the setup guide](SETUP.zh-CN.md) and [validation scope](VALIDATION.md) in the source repository.

## 中文说明

本版使用原生 WPF，默认英文界面，可在设置中切换简体中文。安装器支持从 3.3.0 升级，保留用户设置，并维持一个开始菜单入口。需要 .NET 10 Desktop Runtime x64、一套官方战网和两套独立游戏安装。附件沿用接受验证的 CI 候选原文件；实际安装、缩放及双服在线测试结果在草稿的验证记录中分别列明。