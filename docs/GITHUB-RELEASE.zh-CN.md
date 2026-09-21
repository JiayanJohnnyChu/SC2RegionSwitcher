[English](GITHUB-RELEASE.md)

# 候选包与发布草稿流程

[仓库](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher)目前为私有。当前版本为 3.4.1，原始 3.4.0 候选保留为未发布草稿。剩余验收工作见[当前状态](HANDOFF.zh-CN.md)。

## 构建并确定候选

1. 完成仓库、工作流、构建、回归及包检查。
2. 提交并推送到 `main`。Windows CI 使用固定 SDK，检查通过后上传 `SC2RegionSwitcher-win-x64-preview`。
3. 记录成功运行编号和完整源码提交，下载该次运行的原始制品用于安装和游戏测试。
4. 核验 `SHA256SUMS.txt` 与 `release-manifest.json`。Schema 2 记录版本、源码提交、工作树状态、CI 运行编号、运行次数和包哈希。可接受候选应来自干净的 `main` CI 运行。
5. 针对这些文件测试所需桌面、安装和在线行为。完成项与受阻项分别记录，原始证据存入 Git 忽略的 `artifacts/`。

发布集合恰有四个文件：MSI、便携 ZIP、`release-manifest.json` 和 `SHA256SUMS.txt`。当前 ZIP 包含四个运行文件以及中英文 README。较早候选保留原有内容。

## 提升原始制品

验收后，在实测源码提交上创建匹配标签，例如 `v3.4.1-preview.1`。单独推送标签不会创建发布。运行 **Promote tested candidate to draft prerelease**，填写：

- `version_tag`：指向实测提交的已有标签。
- `candidate_run_id`：生成实测文件的成功 `main` CI 运行编号。

工作流检查来源仓库、工作流、事件、分支、提交和运行次数，然后下载原始制品，不编译或重新打包。发现相同数值版本的其他发布或草稿时会拒绝；创建预发布草稿后，重新下载四个附件，与候选哈希比较。

相应版本说明描述变化与限制。将实际验证结果、源码提交、CI 运行和包哈希补入草稿。验收未完成时继续暂缓发布。

## 版本标识

Windows Installer 比较三段数值版本。因此每个分发预览都需递增该版本，仅将 `preview.1` 改成 `preview.2` 不够。每个数值版本使用新的 ProductCode，产品族 UpgradeCode 保持稳定。

打包时若已有该数值版本的本地标签则拒绝，CI 会先获取标签。提升流程也拒绝同版本的已有草稿或发布。已分发的 3.4.0 候选不得重建、移动标签或替换。

原始 3.3.0 MSI 缺少降级保护，后续包无法为已经分发的旧文件补加保护。

## 发布范围

包未签名，依赖 .NET 10 Desktop Runtime x64，应用许可证尚未选择。创建草稿不会将其发布，也不会改变仓库可见性。构建成功不能证明安装、恢复或在线运行成功。新源码版本或重建 MSI 需重新评估受影响检查。
