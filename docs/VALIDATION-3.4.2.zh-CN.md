[English](VALIDATION-3.4.2.md)

# 3.4.2 验证状态

记录于 2026-09-22。3.4.2 修改许可与分发内容，应用行为不变。

验证范围包括许可来源与文本、MSI 和 ZIP 中的声明及 WiX 元数据源码，以及既有构建、包检查和静态 CI 检查。已发布候选的标识如下。

| 发布标识 | 记录 |
| --- | --- |
| 公开预发布版 | [v3.4.2-preview.1](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/releases/tag/v3.4.2-preview.1) |
| 发布时间 | 2026-09-22 12:44:51 UTC |
| 源码提交 | `1e89600a713f650376f081af3146a6e3825d7e7e` |
| CI | [35727903561](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35727903561)，通过 |
| 提升流程 | [35728297411](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35728297411)，通过 |

发布状态为 `draft=false`、`prerelease=true`。公开仓库与四个发布附件均可匿名访问。匿名下载的 MSI、便携 ZIP、`release-manifest.json` 和 `SHA256SUMS.txt` 的 SHA-256 均与原始文件相符。公开发布未重建或替换候选制品。

一次本地打包及 `Test-Package.ps1` 已通过，覆盖来源、哈希、ZIP／MSI 字节一致性，以及文件、组件、升级和快捷方式定义。MSI 包含十个文件，ZIP 包含十二个文件；六个许可／源码文件的 SHA-256 均与仓库原文件相符。记录保存在 `artifacts/validation/3.4.2/local-package.json`。

项目 MIT 文本采用 [Open Source Initiative 文本](https://opensource.org/license/mit)。Radix 许可证复制自[上游来源](https://raw.githubusercontent.com/radix-ui/colors/main/LICENSE)，保留 Modulz 和 WorkOS 声明。已有 WiX 许可证与元数据源码保持不变。

[3.4.1 验证记录](VALIDATION-3.4.1.zh-CN.md)保留此前的安装器、UAC 迁移和基本界面证据。3.4.2 验证不包含新的生命周期、升级、卸载、界面、DPI 或在线测试。原始 3.4.1 候选文件保持不变。
