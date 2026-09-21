[English](README.md)

# 当前用户 MSI

`scripts/Package.ps1` 发布运行文件并生成安装包，不执行安装。已有发布文件时，可运行：

```powershell
.\tools\Installer\Build-CurrentUserMsi.ps1 -AppDirectory <目录> -OutputDirectory <空目录>
```

构建器使用 Windows Installer COM 和 `makecab`，通过 `scripts/Release-Common.ps1` 读取三段应用版本，检查 EXE 清单与运行文件版本，打包四个运行文件。安装创建单一开始菜单快捷方式和当前用户卸载登记。Microsoft .NET 10 Desktop Runtime x64 需单独安装。

## 资源管理范围

自 3.4.0 起，程序文件使用 `%LOCALAPPDATA%\Programs\SC2RegionSwitcher\app`。配置、偏好、备份和恢复记录使用 `%LOCALAPPDATA%\SC2RegionSwitcherV2`，不属于 MSI 管理范围，卸载后保留。

稳定的 UpgradeCode 标识产品族。每个数值版本有不同且确定的 ProductCode，每次新构建 MSI 有新的 PackageCode。三个组件分别管理应用文件、开始菜单快捷方式和 HKCU App Paths。

3.4.1 中，应用组件改用 HKCU 标记作为键路径，以满足用户目录资源的 ICE38 要求。这项变化引入新的组件 GUID，快捷方式和 App Paths 组件标识保留。资源标识保持兼容时，组件 GUID 保持稳定。

## 升级顺序

Upgrade 表检测自 3.3.0 起的较低版本。`RemoveExistingProducts` 紧接 `InstallInitialize`、位于 `ProcessComponents` 之前执行，将旧产品移除置于回滚事务内。Type 19 动作拒绝检测到的更高版本，Type 51 动作设置显示的安装位置；两者均不执行应用代码或脚本。

安装包拒绝 ALLUSERS，保持当前用户安装。Restart Manager 自动关闭应用已禁用，因此安装前应关闭切换器。同一原始 MSI 支持正常维护调用。

上述顺序描述的是安装器设计。标准用户测试环境中的升级失败恢复尚未通过验收，参阅[验证状态](../../docs/VALIDATION-3.4.1.zh-CN.md)。

## 表结构与验证

数据库采用标准 MSI 列定义，并导入 [metadata/_Validation.idt](metadata/_Validation.idt) 中的约束。[元数据说明](metadata/README.zh-CN.md)记录来源与许可。`scripts/Setup-InstallerTools.ps1` 将固定且经过哈希校验的 WiX 3.14.1 验证工具准备到 `.tools/`，不安装为系统工具。

| 检查 | 范围 |
| --- | --- |
| `scripts/Test-Package.ps1 -ReleaseDirectory <目录>` | 包文件、内容和哈希 |
| `scripts/Test-ReleaseGuards.ps1 -ReleaseDirectory <目录>` | 发布拒绝情形 |
| `scripts/Test-InstallerSchema.ps1 -ReleaseDirectory <目录>` | 独立标准 ICE 套件 |
| 隔离生命周期／恢复测试 | 使用独立产品、目录、快捷方式和注册表标识的实际安装行为 |

没有屏蔽 ICE 规则。已记录结果为零错误，以及四条 ICE91 警告，涉及固定用户目录中的文件假设用于全机安装的情形。脚本拒绝其他警告，记录候选哈希并确认验证未修改 MSI。这些检查不安装产品。

实际安装结果须标明实测 MSI 哈希。即使源码版本相同，本地构建和 CI 构建也需各自证据。专用 CI 恢复测试用于检查旧产品移除后的原生文件复制失败。最近一次运行在基线安装阶段被运行器策略阻止，实际范围见验证状态。

## 版本规则

每个分发预览递增三段 ProductVersion，预览后缀不会改变 MSI 版本。打标签或分发后须保留候选原文件。原始 3.3.0 MSI 没有降级保护；若需要返回该包，应先卸载新版应用。

来源与提升要求见[发布流程](../../docs/GITHUB-RELEASE.zh-CN.md)。
