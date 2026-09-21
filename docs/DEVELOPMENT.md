# 开发与构建

## 开发环境

使用 Windows x64、.NET 10 SDK 及其 WPF 目标包。只安装 .NET Desktop Runtime 可以运行程序，但不能编译。根目录 `global.json` 固定 SDK 10.0.401，不自动切换到其他版本。首次执行 `scripts/Setup.ps1`，SDK 将准备到 `.tools/dotnet`；构建脚本自动优先使用它，然后才查找 PATH。没有第三方 NuGet 依赖。

Setup 使用 `eng/dotnet-sdk.json` 中 Microsoft 官方下载地址和 SHA-512 校验值，校验通过后才解压。它支持 `-ArchivePath <本地 SDK ZIP>` 离线准备，同样核对哈希。不会改写系统 SDK、全局 PATH 或 Git 身份。SDK 和下载缓存分别在 `.tools`、`artifacts` 下，均不进入仓库。[Microsoft 手动安装说明](https://learn.microsoft.com/en-us/dotnet/core/install/windows#install-with-windows-installer)。

后续可将整个项目文件夹加入 Project；编辑器入口为 `SC2RegionSwitcher.slnx`。所有源文件引用均相对于项目目录。项目没有自动创建远程仓库、提交或发布。

## 标准命令

在项目根目录运行以下 PowerShell 脚本。脚本也可以通过绝对路径从其他目录调用；它们依据自身位置寻找项目根目录。

| 命令 | 作用 | 输出 |
| --- | --- | --- |
| `.\scripts\Setup.ps1` | 准备并核验项目 SDK，重复调用复用已安装版本 | `.tools/dotnet/` |
| `.\scripts\Setup-WorkflowTools.ps1` | 准备并核验固定版本 actionlint | `.tools/actionlint/` |
| `.\scripts\Check-Workflows.ps1` | 检查 GitHub 工作流语法、表达式与 Action 使用 | 控制台结果 |
| `.\scripts\Check-Repository.ps1` | 检查待提交内容、个人路径和脚本语法 | 控制台结果 |
| `.\scripts\Build.ps1` | Release 编译应用、测试和图标工具 | 各项目 `bin/`、`obj/` |
| `.\scripts\Test.ps1` | 编译并运行隔离回归 | `artifacts/tests/<本次运行>/` |
| `.\scripts\Publish.ps1` | 生成框架依赖的 x64 运行文件 | `artifacts/publish/win-x64/` |
| `.\scripts\Package.ps1` | 先发布运行文件，再生成 ZIP 与 MSI | `artifacts/packages/<本次构建>/` |
| `.\scripts\Test-Package.ps1 -ReleaseDirectory <目录>` | 核对发布文件、哈希、ZIP 与 MSI 内容 | 控制台结果，不安装 MSI |

Build、Test、Publish、Package 支持 `-DotNet 'C:\path\to\dotnet.exe'` 显式覆盖；通常无需使用。Build 和 Test 另支持 `-Configuration Debug`；安装包固定使用 Release。Package 不安装 MSI，也不覆盖基线安装包。

Common.ps1 仅在命令执行期间设置工具环境，退出后恢复。NuGet 缓存、CLI 状态和用于工具的 APPDATA 指向项目 `artifacts`；恢复命令显式采用项目 NuGet.Config。Windows 扩展 SDK 查找范围限制在所选 SDK 的 packs，避免枚举个人 SDK 目录。当前 WPF 项目使用 SDK 自带引用包；将来引入 WinUI、原生 Windows SDK 或私有包源时，应重新评估这一隔离方式，不能直接加入用户凭据。[NuGet 环境目录实现](https://github.com/NuGet/NuGet.Client/blob/dev/src/NuGet.Core/NuGet.Common/PathUtil/NuGetEnvironment.cs)、[MSBuild SDK 查找实现](https://github.com/dotnet/msbuild/blob/main/src/Utilities/ToolLocationHelper.cs)。

测试是独立控制台程序，使用 `FakePlatform` 和每次新建的模拟文件夹。当前基线为 52 项测试，成功输出 `TOTAL 52 PASSED`，失败返回非零退出码。不能用一次没有发现测试用例的 `dotnet test` 命令替代它。

## 图标资源

应用编译和安装器共用 `assets/icon/switcher.ico`。WPF 将其链接为 `switcher.ico`，保留原 XAML 资源 URI。矢量原稿与预览见同目录的 `icon.svg`、`preview.png`。

生成工具通过命令行接收输出目录：

```powershell
. .\scripts\Common.ps1
Invoke-ProjectDotNet -Arguments @('run', '--project', '.\tools\IconGenerator\IconGenerator.csproj', '--configuration', 'Release', '--no-build', '--', '.\artifacts\icon-preview')
```

它生成 SVG、多尺寸 PNG、ICO 和对照图。先在临时输出中检查，再更新 `assets/icon`。主窗口中的几何图形还定义在 XAML 中，修改图标时应同步核对，生成器不会自动改写 XAML。

## 配置与真实测试

程序默认读取 `%LOCALAPPDATA%\SC2RegionSwitcherV2`。开发诊断可以指定 `--data-dir <绝对目录>` 隔离配置；它只改变配置位置，**不自动模拟战网或游戏**。不要把诊断启动当作隔离测试程序。

`--ui-report <绝对 JSON 路径> --compact` 用于输出窗口诊断与自渲染图；相关文件写入 `artifacts`。实际双服验证与用户登录需要另行安排，构建和自动回归不包含这些操作。

English-first 界面的 `--matrix` 诊断必须同时指定独立的 `--data-dir`，会导出两种界面的模拟状态，并在导出期间禁止切换和目录保存。F12 可捕获主页面、设置页及参考页。进行 DPI 验证时应启动生成的 EXE，确保应用清单生效；通过 `dotnet` 运行 DLL 的渲染不能代替 EXE 的 PerMonitorV2 验证。详细设计与证据边界见 [UI-DESIGN.md](UI-DESIGN.md)。

## 安装版本与构建产物

重新编译会改变文件哈希和 MSI PackageCode。3.4.0 起采用稳定应用目录与当前用户 major upgrade；每个分发预览递增三段数值版本。不要直接覆盖已安装文件，也不要将其他包的安装测试结论套用到新包上。发布必须提升经过实测的 CI 候选原文件。

MSI 具体限制见 [安装器说明](../tools/Installer/README.md)。`artifacts` 中的二进制、夹具、日志和本机校验文件已由 `.gitignore` 排除。公开文档只记录可共享的结论；原始现场记录保留在旧工作区。

## GitHub 与工具更新

CI 在 Windows runner 上安装 global.json 指定的 SDK，依次检查工作流和源码、编译、运行隔离测试、打包和验证制品。Action 版本固定到已核实的提交；Dependabot 每月为 Action 更新提出 PR，不自动合并。SDK 升级需要同时更新 global.json 与 eng/dotnet-sdk.json，并重跑上述验证。actionlint 版本和 SHA-256 在 `eng/actionlint.json` 固定，二进制保存在 Git 忽略的 `.tools`，下载后核验再执行。[actionlint 官方安装说明](https://github.com/rhysd/actionlint/blob/main/docs/install.md)。

首次提交及预发布步骤见 [GITHUB-RELEASE.md](GITHUB-RELEASE.md)。仓库以 main 为主分支，提交作者信息在本地 Git 配置中管理。
