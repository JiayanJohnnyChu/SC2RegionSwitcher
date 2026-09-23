[English](DEVELOPMENT.md)

# 开发

开发需要 Windows x64 和 .NET SDK **10.0.401**，版本由 `global.json` 固定。应用没有第三方 NuGet 依赖。

## 构建与检查

下列命令以仓库根目录为工作目录：

```powershell
.\scripts\Setup.ps1
.\scripts\Build.ps1
.\scripts\Test.ps1
.\scripts\Check-Repository.ps1
.\scripts\Package.ps1
```

Setup 校验 SDK 压缩包，`-ArchivePath <zip>` 支持离线准备。脚本隔离并恢复 SDK／缓存环境。Build/Test 支持 `-Configuration Debug`，Build/Test/Publish/Package 支持 `-DotNet <exe>`。

控制台回归使用模拟安装。成功须同时满足退出码 0 与 `TOTAL … PASSED`；未发现测试的 `dotnet test` 不构成等效验证。打包只生成文件，不安装 MSI 或启动战网。

| 其他检查 | 命令 |
| --- | --- |
| 工作流语法 | `scripts/Setup-WorkflowTools.ps1`，随后 `scripts/Check-Workflows.ps1` |
| 包内容与哈希 | `scripts/Test-Package.ps1 -ReleaseDirectory <directory>` |
| 发布拒绝场景 | `scripts/Test-ReleaseGuards.ps1 -ReleaseDirectory <directory>` |
| 无抑制项的 MSI ICE 验证 | `scripts/Test-InstallerSchema.ps1 -ReleaseDirectory <directory>` |
| 模拟原生界面 | `scripts/Test-Ui.ps1` |

主要版本进行一次完整验证，后续修改按影响范围检查。文档和包内容修改需要相关内容／包检查及适用 CI，无须重复无关的手动矩阵。提交前需要通过仓库检查。

## 应用结构

| 位置 | 职责 |
| --- | --- |
| `src/SC2Switcher.Wpf/Core.cs` | 安装解析、活动检查、语言事务、恢复和战网启动 |
| 同目录下的 `ConfigurationStore.cs` | 迁移、原子保存、备份和并发修改保护 |
| `SwissWindow*`、`SwissSettingsView*`、`SwissReferenceView*` 及视图模型 | 原生 WPF 视图、设置草稿和操作状态 |
| `Localization.cs`、`Strings.*.json`、`UiTypography.cs` | 独立界面语言和内嵌字体 |
| `tests/SC2Switcher.Tests` | 隔离的控制台回归 |

切换先验证路径、资源和游戏／编辑器状态，请求战网正常退出并重新检查安装，随后备份并原子修改语言键，最后以所选登录地区启动战网。待恢复事务在下次切换前处理，核对日志路径、备份位置和哈希。仅打开应用不会恢复。目标启动请求发出后，取消本身不会回滚语言。

`%LOCALAPPDATA%\SC2RegionSwitcherV2` 保存配置、界面偏好、备份与日志，不归 MSI 管理。配置格式第 2 版的国际服文字／语音字段保存所选组合；已有混合组合保留至显式选择。语言检测取当前 Windows 文字与语音资源的交集，并丢弃过期的异步结果。安装不可用时保留偏好。待恢复状态阻止路径／语言变更，但允许登录地区变更。

## 界面维护

自适应原生界面共用颜色、控件与字体，参数和主操作位于同一滚动区域。界面语言独立于游戏语言，十一份翻译的键和占位符保持一致。内嵌 Inter／Noto 的来源及哈希记录于[字体文档](../src/SC2Switcher.Wpf/Fonts/README.zh-CN.md)和 [sources.json](../src/SC2Switcher.Wpf/Fonts/sources.json)。

`Test-Ui.ps1` 使用隔离模拟数据，超时为 240 秒。`-Languages` 限定语言，`-InteractionsOnly` 运行五项 WPF 事件检查，`-StatusOnly` 捕获故障布局。清单／DPI 结论需要通过原生 EXE 启动。`--data-dir <absolute-directory>` 隔离偏好；`--ui-report <absolute-json-path>`、`--compact` 和 F12 提供诊断。真实输入、无障碍和在线行为需要单独检查。

## 发布

已忽略的 `artifacts/` 保存输出与证据，`eng/` 固定工具及哈希。发布目录包含 MSI、ZIP、`release-manifest.json` 和 `SHA256SUMS.txt`。包检查核对预期内容：MSI 13 个文件，ZIP 15 个文件。

可验收候选产物来自成功且干净的 `main` CI。清单记录源码提交、运行／尝试编号、版本与哈希；安装及真实运行检查标明原始受测文件。**Promote tested candidate to draft prerelease** 接收匹配的已有 `version_tag` 和已验收 `candidate_run_id`，核对来源与附件哈希，不重新构建。验收未完成时保持草稿。

每次分发预览均增加三段数字版本。已打标签／已分发文件保持不变，重复数字版本的发布被拒绝。[安装器维护](../tools/Installer/README.zh-CN.md)记录安装范围与升级。[验证](VALIDATION.zh-CN.md)区分构建、安装、渲染与真实运行；[更新记录](../CHANGELOG.zh-CN.md)记录公开变更。
