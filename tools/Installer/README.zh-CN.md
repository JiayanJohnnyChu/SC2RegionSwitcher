[English](README.md)

# 全用户 MSI 维护

`scripts/Package.ps1` 发布并打包，不执行安装。已有运行文件可通过以下命令打包：

```powershell
.\tools\Installer\Build-MachineMsi.ps1 -AppDirectory <directory> -OutputDirectory <empty-directory>
```

构建器使用 Windows Installer COM 与 `makecab`，从 `scripts/Release-Common.ps1` 读取三段版本并检查可执行清单／运行时版本。当前定义在一个内嵌 cabinet 中包含四个运行文件和九个许可／源文件。3.5.1 的正式打包与托管 CI 尚未执行。包未签名，.NET 10 Desktop Runtime x64 为独立依赖。

## 所有权与升级

| 项目 | 定义 |
| --- | --- |
| 安装 | `ALLUSERS=1`，需要管理员批准，目录为 `ProgramFiles64Folder\SC2RegionSwitcher\app` |
| 入口 | HKLM App Paths 和一个公用的公布式开始菜单快捷方式 |
| 应用 | `asInvoker`，普通用户运行 |
| 用户数据 | `%LOCALAPPDATA%\SC2RegionSwitcherV2`；配置、偏好、备份和恢复日志不归 MSI 管理，卸载后保留 |
| 身份 | 产品族 UpgradeCode 稳定；每个数字版本使用新 ProductCode，每次构建使用新 PackageCode |
| 组件 | Application 管理运行文件／快捷方式，AppRegistration 管理 App Paths，九个以文件为键的组件管理许可／源文件 |

早期当前用户预览需要由所属用户卸载后再进行全用户安装。安装器发现调用用户的旧 HKCU App Paths 记录时会阻止安装，不搜索其他用户配置。相同 UpgradeCode 不支持跨安装上下文升级。

后续全用户版本使用正常主要升级。`RemoveExistingProducts` 位于 `InstallInitialize` 之后，使旧版移除处于回滚事务内。已安装较新关联版本时拒绝安装。Restart Manager 自动关闭被禁用，应用需要预先关闭。原始 MSI 支持维护操作。

## 检查

| 命令／工作流 | 范围 |
| --- | --- |
| `scripts/Test-Package.ps1 -ReleaseDirectory <directory>` | 内容、定义与哈希 |
| `scripts/Test-ReleaseGuards.ps1 -ReleaseDirectory <directory>` | 拒绝场景 |
| `scripts/Test-InstallerSchema.ps1 -ReleaseDirectory <directory>` | 无抑制项的完整 ICE，要求零错误／警告 |
| `tools/Installer/Test-MachineInstall.ps1`、`installer-lifecycle.yml` | 隔离的安装、维护、升级、恢复和移除 |

`scripts/Setup-InstallerTools.ps1` 准备经哈希校验的 WiX 3.14.1 工具。[元数据来源](metadata/README.zh-CN.md)记录导入的验证约束。

[开发](../../docs/DEVELOPMENT.zh-CN.md)规定版本与发布流程；[验证](../../docs/VALIDATION.zh-CN.md)记录各项检查覆盖的包和源码版本。
