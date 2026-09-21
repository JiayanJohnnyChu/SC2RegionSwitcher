[English](README.md)

# SC2 Region Switcher

用于 Windows x64 的《星际争霸 II》国服／国际服切换器。程序使用一套官方 Battle.net 桌面应用和两套独立游戏安装，切换战网登录区域，并同步共享的游戏语言设置。国服使用简体中文（`zhCN`），国际服使用英文（`enUS`）。界面支持英文和简体中文，语言可独立设置。

程序采用 C#、WPF 和 .NET 10。账号登录、游戏安装、更新、游戏服务器选择及游戏启动均在战网中完成。国际服卡片上的 **EU**、**US** 和 **KR** 表示战网登录区域，不能据此确定当前选择的游戏服务器。

**当前状态：3.4.1 开发预览版。** 升级失败后恢复旧安装的验收尚未通过。3.4.1 尚未成为发布候选；此前的 3.4.0 候选仍保留为未发布的私有草稿。安装包未签名，运行需要 **Microsoft .NET 10 Desktop Runtime x64**。已验证范围见[验证记录](docs/VALIDATION.zh-CN.md)。

## 文档入口

| 任务 | 文档 |
| --- | --- |
| 准备游戏安装、配置路径及切换区域 | [配置与使用指南](docs/SETUP.zh-CN.md) |
| 编译和测试源码 | [开发说明](docs/DEVELOPMENT.zh-CN.md) |
| 理解切换与配置逻辑 | [架构说明](docs/ARCHITECTURE.zh-CN.md) |
| 维护界面及翻译 | [界面设计](docs/UI-DESIGN.zh-CN.md) |
| 查看当前工作与验证状态 | [开发状态](docs/HANDOFF.zh-CN.md)、[3.4.1 验证](docs/VALIDATION-3.4.1.zh-CN.md) |
| 维护 MSI 安装包 | [安装器说明](tools/Installer/README.zh-CN.md) |
| 准备候选包及发布草稿 | [发布流程](docs/GITHUB-RELEASE.zh-CN.md) |
| 查看版本变化 | [更新记录](CHANGELOG.zh-CN.md) |

## 构建

在项目目录中使用 PowerShell 执行：

```powershell
.\scripts\Setup.ps1
.\scripts\Build.ps1
.\scripts\Test.ps1
.\scripts\Package.ps1
```

`global.json` 固定 SDK **10.0.401**。Setup 下载并校验 SDK，将其放入 Git 忽略的 `.tools/dotnet`，其他脚本会自动使用。项目没有第三方 NuGet 依赖。回归程序使用模拟安装，不会启动战网或游戏。打包生成依赖运行环境的 MSI 和便携 ZIP，不执行安装。开发说明另有仓库、工作流及安装包检查步骤。

## 项目结构

| 路径 | 内容 |
| --- | --- |
| `SC2RegionSwitcher.slnx` | 应用、回归程序及图标工具 |
| `src/SC2Switcher.Wpf/` | WPF 界面、切换逻辑、配置及语言资源 |
| `tests/SC2Switcher.Tests/` | 隔离回归测试 |
| `tools/Installer/` | 当前用户 MSI 构建器及验证支持 |
| `tools/IconGenerator/`、`assets/icon/` | 图标生成器及应用图像资源 |
| `scripts/`、`eng/`、`.github/` | 构建命令、固定工具版本信息及 CI 工作流 |
| `docs/` | 用户与维护文档 |
| `artifacts/`、`.tools/` | Git 忽略的构建输出、证据及本地工具 |

MSI 安装目录为 `%LOCALAPPDATA%\Programs\SC2RegionSwitcher\app`，创建一个 **SC2 Region Switcher** 开始菜单入口。配置和备份另存于 `%LOCALAPPDATA%\SC2RegionSwitcherV2`，卸载时保留。

[仓库](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher)目前为私有，尚未选择应用许可证。第三方声明仅适用于其明确指明的材料。
