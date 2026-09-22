[English](ARCHITECTURE.md)

# 架构说明

SC2 Region Switcher 是面向 Windows x64 的 C# / WPF / .NET 10 应用，协调一套官方战网和两套独立《星际争霸 II》安装，不修改战网的游戏安装数据库。

## 源码结构

下列应用文件均位于 `src/SC2Switcher.Wpf`。

| 文件 | 职责 |
| --- | --- |
| `App.xaml.cs` | 启动、用户数据目录、配置加载及诊断参数 |
| `MainWindow.xaml`、`MainWindow.xaml.cs` | 主界面与操作协调 |
| `MainWindow.Diagnostics.cs` | 布局报告及隔离的模拟界面状态 |
| `MainViewModel.cs` | 当前区域、所选目标、操作阶段与操作可用性 |
| `SettingsPage.xaml`、`SettingsPage.xaml.cs` | 路径编辑、原生选择器、语言选择及未保存改动 |
| `ConfigurationStore.cs` | 校验、迁移、备份、原子保存及并发变更保护 |
| `Core.cs` | 安装清单、文件活动检查、语言事务、战网适配及切换引擎 |
| `Localization.cs`、`Strings.*.json` | 界面语言及翻译资源 |
| `Palette.xaml`、`app.manifest` | 共用颜色及 Windows／DPI 声明 |

回归项目链接核心源文件，并提供 `FakePlatform` 实现，使用模拟安装测试行为，不启动战网。

## 切换事务

1. 引擎校验配置路径、安装分支、语言数据及游戏／编辑器状态。
2. 引擎请求战网正常退出，等待后重新检查安装。
3. 引擎备份 `Variables.txt`，以原子方式修改语言键，记录原始与修改后的哈希。
4. 引擎使用所选登录区域参数启动战网，读取其本地区域记录。
5. 引擎提交事务；失败时，在相应阶段和文件一致性检查允许的情况下尝试恢复。

开始切换时，先恢复尚未完成的语言事务，再进行新的语言修改。恢复设置前会核对事务路径、备份位置与文件哈希。仅打开应用或重新检查安装不会执行恢复。

界面没有取消操作。引擎保留取消令牌以处理流程边界和测试；发出目标战网启动请求后，之后收到取消本身不会导致语言回滚。忙碌期间锁定相关设置。

文件活动检查观察的是本地文件，并非官方更新状态接口。同样，登录区域记录反映本地配置，不能确认账号认证或所选游戏服务器。

## 配置与本地化

用户状态保存在 `%LOCALAPPDATA%\SC2RegionSwitcherV2`。`profiles.json` 保存路径及游戏配置，`ui-preferences.json` 保存界面语言。配置备份、语言备份和待恢复记录共用此数据根目录，不属于安装包资源。

国服文本与语音使用 `zhCN`，国际服使用 `enUS`。界面语言独立设置。缺失或无效的偏好回退到 `en-US`，有效的 `en-US` 和 `zh-CN` 偏好会保留。两份翻译资源的键和格式占位符必须一致。

战网当前配置和所选目标分别维护。国际服 EU／US／KR 标签反映所选登录区域，不设置或确认《星际争霸 II》游戏服务器。

## 安装结构

3.4.1 采用全机 MSI：`ALLUSERS=1`、`ProgramFiles64Folder\SC2RegionSwitcher\app`、HKLM App Paths 和单一公共开始菜单入口。安装需要管理员批准。应用清单仍为 `asInvoker`；配置、偏好、备份和恢复记录仍按用户保存在 `%LOCALAPPDATA%\SC2RegionSwitcherV2`，不由 MSI 管理，卸载后保留。

产品族 UpgradeCode 保留。全机安装使用新的 ProductCode 和组件标识。此前的当前用户预览版需一次性卸载重装，后续全机版本在相同上下文中执行大版本升级。`RemoveExistingProducts` 位于 `InstallInitialize` 之后、新文件安装之前。

切换引擎与配置事务未改。验证记录标明的候选已通过托管环境中的全机生命周期与升级失败恢复检查，交互式 UAC 安装仍待验证。设计与证据记录于[安装器说明](../tools/Installer/README.zh-CN.md)和[验证状态](VALIDATION-3.4.1.zh-CN.md)。
