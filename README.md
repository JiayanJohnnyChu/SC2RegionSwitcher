# SC2 Region Switcher

用于 Windows 的《星际争霸 II》国服／国际服切换器，采用 C#、WPF 和 .NET 10。当前基线为 **3.3.0 预览版**。

程序使用一套 Battle.net 和两套独立游戏目录，切换 Battle.net 登录区域并同步共享游戏语言设置。国服使用简体中文，国际服使用英文；界面语言可独立设置为简体中文或 English。账号登录、游戏安装、更新和游戏启动仍由官方战网完成。

## 使用与开发入口

- 普通用户：[环境配置与使用指南](docs/SETUP.zh-CN.md)。运行需要 .NET 10 Desktop Runtime（Windows x64）。
- 后续开发：[开发与构建](docs/DEVELOPMENT.md)、[架构说明](docs/ARCHITECTURE.md)。构建需要 Windows x64 和 .NET 10 SDK。
- 接续当前工作：[项目交接记录](docs/HANDOFF.md)、[验证范围与发布待办](docs/VALIDATION.md)。
- 安装包维护：[当前用户 MSI 构建说明](tools/Installer/README.md)。
- 接入 GitHub：[首次提交、自动检查与预发布流程](docs/GITHUB-RELEASE.md)。

本目录是后续开发的项目根目录，可以直接作为 Project 文件夹使用，也可以整体复制到其他工作位置。打开 `SC2RegionSwitcher.slnx` 可加载应用、隔离测试和图标生成工具。源码不依赖原对话工作区的绝对路径。

## 目录

```text
SC2RegionSwitcher/
├── SC2RegionSwitcher.slnx
├── global.json
├── NuGet.Config
├── .github/                  自动检查、预发布工作流与协作模板
├── eng/                      SDK 下载版本与校验信息
├── src/SC2Switcher.Wpf/       WPF 应用、切换逻辑、设置及双语资源
├── tests/SC2Switcher.Tests/   隔离回归测试
├── tools/
│   ├── Installer/            当前用户 MSI 构建器
│   └── IconGenerator/        图标 SVG、PNG、ICO 生成工具
├── assets/icon/              应用图标及矢量原稿
├── scripts/                  构建、测试、发布文件与打包入口
├── docs/                     用户指南、技术说明和开发交接
└── artifacts/                本地构建、测试和安装包；Git 忽略
```

## 快速构建

在项目根目录的 PowerShell 中执行：

```powershell
.\scripts\Setup.ps1
.\scripts\Setup-WorkflowTools.ps1
.\scripts\Check-Repository.ps1
.\scripts\Check-Workflows.ps1
.\scripts\Build.ps1
.\scripts\Test.ps1
.\scripts\Package.ps1
```

首次执行 Setup 会将经过 SHA-512 核验的 SDK 准备到 Git 忽略的 `.tools/dotnet` 中；重复运行会复用现有版本。其他脚本自动选择项目 SDK，无需手动指定旧工作区路径。已经安装对应版本 SDK 的 CI 环境也可以直接构建。

测试使用临时模拟安装，不启动战网或游戏。打包生成便携 ZIP 和当前用户 MSI，**不会自动安装**。`global.json` 固定 SDK **10.0.401**，本地与 GitHub Actions 使用相同版本。当前没有第三方 NuGet 包，`NuGet.Config` 清空包源。普通用户只需 Desktop Runtime，不需要 SDK。

## 安装包与状态

本地 `artifacts/baseline/3.3.0` 保留此前已测试安装包的原件和校验值。新构建输出位于 `artifacts/packages/<本次构建>/release`，包含 MSI、ZIP、SHA-256 校验文件和无本机路径的发布清单。其安装测试结论须单独记录。`artifacts` 和 `.tools` 整体不纳入 Git。

仓库位于 [JiayanJohnnyChu/SC2RegionSwitcher](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher)，当前为私有。首次 GitHub CI 已通过 Windows 构建、52 项隔离回归和包内容检查，并上传预览构建产物；详细记录见 [验证范围](docs/VALIDATION.md)。推送版本标签时，发布工作流配置为创建 **Draft / Pre-release**，供维护者核对后手动发布；该标签发布流程尚未实际触发。

本项目尚未公开发布，也尚未确定开源许可证。本目录整理不代表已授予某种开源许可。现有安装包未签名，跨电脑兼容性与完整升级流程仍待验证。详见 [验证范围](docs/VALIDATION.md)。
