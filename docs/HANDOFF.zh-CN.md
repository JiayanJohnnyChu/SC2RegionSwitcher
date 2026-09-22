[English](HANDOFF.md)

# 开发状态

更新于 2026-09-22。[**v3.4.2-preview.1**](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/releases/tag/v3.4.2-preview.1) 已作为公开预发布版发布。

## 当前修订

3.4.2 为项目原创代码、文档和图标采用 MIT 许可证，版权人为 Jiayan Chu，年份为 2026。MSI 与 ZIP 分发包含项目许可证、双语第三方声明、上游 Radix MIT 许可证，以及 WiX MS-RL 许可证与对应验证源码。应用行为与界面不变。

验证仅覆盖许可与分发内容，以及既有构建、包检查和静态 CI 检查。本地打包与内容验证已通过，记录于 [3.4.2 验证](VALIDATION-3.4.2.zh-CN.md)。源码 `1e89600a713f650376f081af3146a6e3825d7e7e` 通过 [CI 运行 35727903561](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35727903561) 和[提升运行 35728297411](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35728297411)。[3.4.1 记录](VALIDATION-3.4.1.zh-CN.md)保留此前的安装器、UAC 迁移和基本界面证据；完整 DPI 矩阵及在线往返仍属于 3.4.0 的历史结果。

## 分发

原始 3.4.1 预览文件保持不变。每个分发预览递增三段版本，提升采用原始 CI 制品，不重新构建。包仍未签名，运行需要 .NET 10 Desktop Runtime x64。仓库已公开。预发布版于 2026-09-22 12:44:51 UTC 发布；四个发布附件均已匿名下载，其 SHA-256 与原始文件相符。

## 维护参考

- [开发命令](DEVELOPMENT.zh-CN.md)
- [安装器说明](../tools/Installer/README.zh-CN.md)
- [第三方声明](../THIRD-PARTY-NOTICES.zh-CN.md)
- [候选提升](GITHUB-RELEASE.zh-CN.md)
