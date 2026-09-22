[English](README.md)

# 全机 MSI

`scripts/Package.ps1` 发布运行文件并生成安装包，不执行安装。已有发布文件可通过以下命令打包：

```powershell
.\tools\Installer\Build-MachineMsi.ps1 -AppDirectory <目录> -OutputDirectory <空目录>
```

构建器使用 Windows Installer COM 和 `makecab`，通过 `scripts/Release-Common.ps1` 读取三段版本，检查 EXE 清单与运行文件版本，打包四个运行文件和六个许可／源码文件。当前文件名为 `SC2Switcher-3.4.2-x64.msi`。Microsoft .NET 10 Desktop Runtime x64 需单独安装，包未签名。

## 安装与资源范围

MSI 设置 `ALLUSERS=1`，需要管理员批准。程序安装到 `ProgramFiles64Folder\SC2RegionSwitcher\app`，登记 HKLM App Paths，创建单一公共开始菜单入口。应用仍声明 `asInvoker`，以普通用户权限运行。配置、偏好、备份和恢复记录仍保存在每位用户的 `%LOCALAPPDATA%\SC2RegionSwitcherV2`，不属于 MSI 管理范围，卸载后保留。

Application 组件管理四个运行文件和已播发开始菜单快捷方式，以 EXE 为键路径。AppRegistration 组件管理 HKLM App Paths 项。六个独立的文件键组件分别管理 `LICENSE`、两份第三方声明及 `licenses/` 下的三个文件，其标识在兼容的包修订中保持稳定。产品族 UpgradeCode 保留，全机安装使用新的 ProductCode 和组件标识。每个数值版本使用不同的 ProductCode，每次构建生成新的 PackageCode。

## 迁移与升级

此前的 3.3.0 和 3.4.0 当前用户包属于内部预览。迁移要求旧包先由原安装用户移除，再安装新 MSI，同时保留独立用户数据目录。Windows Installer 无法跨安装上下文执行大版本升级，这一限制记录于 Microsoft 的 [Major Upgrades](https://learn.microsoft.com/en-us/windows/win32/msi/major-upgrades)。共用 UpgradeCode 不改变这一限制。

AppSearch 和 RegLocator 检查发起安装的用户旧有的 HKCU App Paths 项；发现后阻止新安装，提示先卸载旧预览版。这一检查不扫描其他用户的配置。完成这次一次性卸载重装后，后续全机版本通过正常大版本升级更新。

全机升级时，`RemoveExistingProducts` 紧接 `InstallInitialize`，在 `ProcessComponents` 和新文件安装之前执行，将旧产品移除置于回滚事务内。检测到更高的相关全机版本时拒绝安装。Restart Manager 自动关闭应用已禁用，安装要求切换器已关闭。原始 MSI 支持维护调用。

## 验证

数据库采用标准 MSI 定义，并导入 [metadata/_Validation.idt](metadata/_Validation.idt) 中的约束。[元数据说明](metadata/README.zh-CN.md)记录来源与许可。`scripts/Setup-InstallerTools.ps1` 在 `.tools/` 下准备固定且经过哈希校验的 WiX 3.14.1 工具。

| 检查 | 范围 |
| --- | --- |
| `scripts/Test-Package.ps1 -ReleaseDirectory <目录>` | 包内容、安装器定义和哈希 |
| `scripts/Test-ReleaseGuards.ps1 -ReleaseDirectory <目录>` | 发布拒绝情形 |
| `scripts/Test-InstallerSchema.ps1 -ReleaseDirectory <目录>` | 不屏蔽规则的完整 ICE 套件；全机包要求零错误、零警告 |
| `tools/Installer/Test-MachineInstall.ps1`、`installer-lifecycle.yml` | 隔离的全机安装、维护、升级、恢复和移除 |

3.4.2 验证覆盖许可／源码内容及既有构建、包检查和静态 CI 检查，本地打包与内容检查已通过，记录于 [3.4.2 验证](../../docs/VALIDATION-3.4.2.zh-CN.md)。CI 结果通过发布记录与候选源码提交关联。此前全机生命周期、UAC 和界面证据保留于 [3.4.1 验证](../../docs/VALIDATION-3.4.1.zh-CN.md)。各项结果均由确切包哈希和源码提交标识。

每个分发预览递增三段 ProductVersion。已打标签或分发的版本必须保留原文件，提升必须使用已接受的 CI 制品，不得重新构建。原始 3.3.0 MSI 没有降级保护。流程记录于[发布流程](../../docs/GITHUB-RELEASE.zh-CN.md)。
