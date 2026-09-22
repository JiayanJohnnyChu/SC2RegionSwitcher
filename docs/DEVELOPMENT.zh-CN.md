[English](DEVELOPMENT.md)

# 开发与构建

开发环境要求 Windows x64 和 `global.json` 固定的 .NET SDK **10.0.401**。仅有 Desktop Runtime 无法编译。`SC2RegionSwitcher.slnx` 解决方案包含应用、回归程序及图标工具。

## 工具与命令

下列命令的执行环境为项目根目录中的 PowerShell。脚本按自身位置定位根目录，因此也可用脚本绝对路径调用。

| 命令 | 用途 |
| --- | --- |
| `.\scripts\Setup.ps1` | 在 `.tools/dotnet` 准备固定版本 SDK |
| `.\scripts\Setup-WorkflowTools.ps1` | 准备固定版本 actionlint |
| `.\scripts\Check-Workflows.ps1` | 校验工作流语法、表达式及 Action 使用 |
| `.\scripts\Check-Repository.ps1` | 检查仓库内容、个人路径和 PowerShell 语法 |
| `.\scripts\Build.ps1` | 以 Release 编译应用、测试及图标工具 |
| `.\scripts\Test.ps1` | 编译并运行隔离回归 |
| `.\scripts\Publish.ps1` | 生成依赖运行环境的 x64 程序文件 |
| `.\scripts\Package.ps1` | 发布运行文件，生成 MSI、ZIP 及发布元数据 |
| `.\scripts\Test-Package.ps1 -ReleaseDirectory <目录>` | 检查包内容及哈希 |
| `.\scripts\Test-ReleaseGuards.ps1 -ReleaseDirectory <目录>` | 测试发布拒绝情形 |
| `.\scripts\Test-InstallerSchema.ps1 -ReleaseDirectory <目录>` | 运行独立 Windows Installer ICE 验证 |

Setup 按 `eng/dotnet-sdk.json` 校验 Microsoft SDK 压缩包后才解压。`-ArchivePath <本地SDK压缩包>` 支持离线准备，仍执行相同哈希检查。脚本优先使用项目 SDK，再查找 PATH。Build、Test、Publish 和 Package 支持 `-DotNet <dotnet.exe路径>`；Build 和 Test 另支持 `-Configuration Debug`。安装包固定使用 Release。

工具版本和哈希记录在 `eng/`，下载的工具与缓存由 Git 忽略。`Common.ps1` 在执行期间为 NuGet 缓存、CLI 状态和 SDK 元数据查找设置项目内环境，结束后恢复。还原使用仓库的 `NuGet.Config`；项目没有第三方 NuGet 依赖，因此未配置包源。引入私有包、WinUI 或原生 SDK 依赖时需重新评估这些设置。

回归套件是使用 `FakePlatform` 和临时安装的控制台程序。已记录基线有 52 项测试，成功时输出 `TOTAL 52 PASSED`，失败返回非零退出码。未发现测试的 `dotnet test` 不具有相同覆盖范围。

## 输出

编译输出位于各项目的 `bin/` 和 `obj/`，发布文件位于 `artifacts/publish/win-x64/`。每次测试在 `artifacts/tests/` 下有独立目录，安装包位于 `artifacts/packages/<本次构建>/release`。

发布目录恰有四个文件：MSI、便携 ZIP、`release-manifest.json` 和 `SHA256SUMS.txt`。当前 ZIP 包含四个运行文件及 `README.md`、`README.zh-CN.md`。打包脚本生成文件，不执行安装。清单记录源码提交、工作树状态及 CI 来源。本地开发包不属于已接受的 CI 候选。MSI 文件名为 `SC2Switcher-<version>-x64.msi`，采用需要管理员权限的全机范围；构建时无需安装它。

独立的 `installer-lifecycle.yml` 工作流通过 `tools/Installer/Test-MachineInstall.ps1` 隔离测试全机安装、维护、升级、回滚和移除，结果须标明输入候选及哈希。较早的恢复工作流保留用于历史当前用户设计诊断。当前范围与迁移规则见[安装器说明](../tools/Installer/README.zh-CN.md)。

## 界面诊断

程序默认使用 `%LOCALAPPDATA%\SC2RegionSwitcherV2`。`--data-dir <绝对目录>` 可重定向配置，但不会模拟战网或游戏。

`--ui-report <绝对JSON路径>` 导出布局报告和 WPF 自渲染图。`--compact` 请求最小窗口尺寸。F12 捕获当前主页面、设置页或参考页。输出应放入 `artifacts/`。

`--matrix` 必须同时指定独立的 `--data-dir`，用于导出两种语言的模拟状态。展示期间禁止真实切换和路径保存，报告标记 `SyntheticState=true`。DPI 检查要求通过生成的 EXE 启动应用，以使应用清单生效；经 dotnet 宿主运行 DLL 的渲染不能证明 EXE 的 PerMonitorV2 行为。诊断流程记录于[界面设计](UI-DESIGN.zh-CN.md)。

## 图标

应用与安装器共用 `assets/icon/switcher.ico`，同目录含矢量原稿和预览。以下命令在临时位置生成拟议变更：

```powershell
. .\scripts\Common.ps1
Invoke-ProjectDotNet -Arguments @('run', '--project', '.\tools\IconGenerator\IconGenerator.csproj', '--configuration', 'Release', '--no-build', '--', '.\artifacts\icon-preview')
```

资源更新以 SVG、PNG、ICO 和对照图的预先检查为前提。主窗口相关几何图形另在 XAML 中维护。

## CI 与发布维护

Windows CI 检查工作流与仓库内容，完成编译、回归、打包以及内容、结构和拒绝情形检查。Action 固定到提交；Dependabot 每月提出更新，不自动合并。升级 SDK 时同时更新 `global.json` 和 `eng/dotnet-sdk.json`；验证器及工作流工具更新也需修改对应版本与哈希元数据。

每个分发预览使用新的三段版本号。重新构建的 MSI 字节和 PackageCode 均不同，因此安装结果必须标明确切哈希。提升已接受的 CI 原文件时不得重新构建。流程与证据记录于[安装器说明](../tools/Installer/README.zh-CN.md)、[发布流程](GITHUB-RELEASE.zh-CN.md)和[当前状态](HANDOFF.zh-CN.md)。
