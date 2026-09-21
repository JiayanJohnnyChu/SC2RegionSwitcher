[English](HANDOFF.md)

# 开发状态

更新于 2026-09-22。当前版本为 **3.4.1 开发预览版**。

## 当前工作

国际服卡片现显示所选战网登录区域 EU、US 或 KR。当前配置与所选目标仍分别显示，切换引擎和配置事务未改。

本地编译通过，零警告、零错误；52 项隔离回归、包内容检查、五项发布拒绝情形及独立 ICE 验证均通过。已完成 200% 缩放下的基本中英文界面检查。这些结果不等于安装失败恢复通过，也不构成最终 CI 候选。参阅 [3.4.1 验证](VALIDATION-3.4.1.zh-CN.md)。

原始 3.4.0 候选仍保留为未发布的私有草稿。完整四档 DPI 矩阵和国服 → 欧洲 → 国服在线往返仅适用于该候选。参阅[历史验证](VALIDATION.zh-CN.md)。

## 剩余验收工作

1. 完成标准用户安装器恢复检查，解决发现的失败。
2. 生成干净的最终 CI 候选，核验来源和哈希，并针对这些确切文件执行所需安装生命周期测试。
3. 根据最终变更重新评估界面与在线覆盖，提升候选前重跑受影响检查。
4. 在版本说明中保留多显示器、跨电脑及签名方面的限制。

提交 `d3a179bbbb1ec55f9850f1c0846b79d126ffac4d` 通过 [CI 运行 35667524580](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35667524580)。Windows 11 ARM64 配对恢复测试使用相同候选和测试程序：[管理员上下文运行](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35668661867) 完整恢复旧安装；[标准用户运行](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35668664128) 恢复了可见资源，但旧产品处于已播发状态，MSI 清理检查失败。两次均到达旧产品移除后的原生文件复制失败阶段。输入 MSI 原文件不变，临时测试环境已移除。权限条件原型通过了结构检查，但尚未采用或完成全生命周期测试。发布验收仍未完成，详见 [3.4.1 验证记录](VALIDATION-3.4.1.zh-CN.md)。

## 发布约束

仓库为私有，尚未选择应用许可证。MSI 和 ZIP 均未签名，依赖外部运行环境。每个分发预览递增三段版本号；已分发的 3.4.0 候选须保留原文件。发布提升复制实测 CI 制品，不重新构建。原始 3.3.0 MSI 没有降级保护。

## 维护参考

- [开发命令](DEVELOPMENT.zh-CN.md)
- [架构说明](ARCHITECTURE.zh-CN.md)
- [安装器说明](../tools/Installer/README.zh-CN.md)
- [候选提升](GITHUB-RELEASE.zh-CN.md)
- [界面设计](UI-DESIGN.zh-CN.md)
