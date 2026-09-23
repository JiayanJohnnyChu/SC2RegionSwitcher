[English](README.md) · [简体中文](README.zh-CN.md) · [Français](README.fr-FR.md) · [Deutsch](README.de-DE.md) · [Nederlands](README.nl-NL.md)

# SC2 Region Switcher

**下载：**[v3.4.2-preview.1](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/releases/tag/v3.4.2-preview.1) · [全部版本](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/releases)

SC2 Region Switcher 供玩家通过一套战网使用两套独立的《星际争霸 II》国服和国际服安装，切换战网登录区域及游戏共享语言设置。

公开下载为 **3.4.2**，提供英文和简体中文界面，游戏语言固定为国服中文、国际服英文。截图展示使用模拟配置的 **3.5.1 开发版**。3.5.1 源码支持十一种界面语言及已安装的国际服游戏语言选择，尚无已发布的发行包。

![SC2 Region Switcher 3.5.1 开发界面，使用模拟配置](assets/screenshots/sc2-switcher-3.5.1.png)

## 安装

运行需要 Windows x64 和 **[Microsoft .NET 10 Desktop Runtime x64](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)**，游玩不需要 SDK。现有安装包未签名。

| 安装包 | 适用方式 |
| --- | --- |
| MSI | 需要管理员批准，为所有 Windows 用户安装，提供开始菜单入口和常规 Windows 卸载功能。应用本身以普通用户权限运行。 |
| 便携 ZIP | 完整解压到独立目录，所有附带文件保持在一起。`SC2Switcher.Wpf.exe` 为应用入口，不创建快捷方式或卸载登记。 |

GitHub 的“Source code”下载不是可直接运行的应用包。

## 首次配置

1. 两套游戏通过战网完成安装，使用独立目录，例如 `D:\Games\StarCraft II CN` 和 `D:\Games\StarCraft II Global`。国服需要中文文字与语音，公开版的国际服需要英文文字与语音。
2. 即使战网显示**安装**，安装前也需要核对最终目录。战网可能自动补上 `StarCraft II` 子目录，或识别另一套安装。
3. 设置页填写战网目录、两套游戏目录，以及游戏实际使用的“文档”文件夹中的 `StarCraft II\Variables.txt`。文件缺失时，一次正常游戏启动和退出会生成它。
4. **保存并检查**验证并保存路径，单独保存不会修改游戏语言。完整准备流程和路径示例见[使用指南](docs/SETUP.zh-CN.md)。

## 切换与语言

切换要求《星际争霸 II》和地图编辑器已关闭，战网下载、更新和修复已完成。目标为国服或国际服，国际服另有 EU、US 或 KR 登录区域选项。切换器正常关闭战网，备份后应用游戏语言设置，再打开所选登录区域。操作完成前相关控件保持锁定。

登录、游戏服务器选择和游戏启动均在战网中完成。**EU、US 和 KR 是战网登录区域，不代表已经选定相应游戏服务器。**界面语言独立于游戏语言。开发版在下一次切换时应用已保存的国际服语言选择，文字和语音资源均需预先通过战网安装。

## 存档、备份与更新

切换器只修改游戏共享设置中的语言键，不复制、删除或管理战役存档、录像及 `Accounts` 文件。游戏账号和区域决定进度的可用范围，切换器不提供云存档同步。

切换器设置和备份保存在 `%LOCALAPPDATA%\SC2RegionSwitcherV2`，卸载后保留。待恢复事务需要原配置和备份。MSI 更新使用较新安装器，应用须已关闭；便携更新采用新目录，并调整相关快捷方式。两种方式都不会搬移游戏安装。

## 常见问题

| 问题 | 说明 |
| --- | --- |
| 应用无法启动 | 需要 Desktop Runtime 10 x64 及全部附带应用文件。 |
| 游戏目录或语言不可用 | 所选目录须为游戏根目录，安装及所需文字、语音资源均已完成。 |
| 切换失败或等待恢复 | 错误详情指出下一步检查内容。原设置和备份仍需保留，详细恢复步骤见指南。 |

| 更多信息 | 链接 |
| --- | --- |
| 玩家指南 | [使用与排错](docs/SETUP.zh-CN.md) |
| 版本与开发 | [更新记录](CHANGELOG.zh-CN.md)、[开发说明](docs/DEVELOPMENT.zh-CN.md)、[验证记录](docs/VALIDATION.zh-CN.md) |

项目原创材料采用 [MIT 许可证](LICENSE)，[第三方声明](THIRD-PARTY-NOTICES.zh-CN.md)说明附带字体及其他许可材料。
