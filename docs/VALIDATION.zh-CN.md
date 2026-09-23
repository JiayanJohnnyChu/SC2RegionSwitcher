[English](VALIDATION.md)

# 验证

记录日期：2026 年 9 月 23 日。结果仅适用于标明的源码或产物。详细报告保留于已忽略的 `artifacts/`。

## 发布候选版：3.5.3

候选版包含设置对齐和字段间距修正，CI、包与安装检查尚待完成。公开下载仍为 3.4.2。

| 打包前针对性检查 | 结果 |
| --- | --- |
| 范围 | 英语和简体中文，各检查宽窗口（1000 DIP）与紧凑窗口（520 DIP）；四张截图均已审阅 |
| 设置对齐 | 内容与底部控件右侧对齐，滚动条自动显隐时仍保持对齐 |
| 字段间距 | 输入框与语言选择框高度均为 48 DIP，文字行高为 22 DIP，距外边框左侧和顶部均为 13 DIP |
| 交互 | 四种情况下的下拉框打开、选择和关闭均通过 |

## 未变组件的既有证据

安装器和切换逻辑与已验收的 3.5.1 候选版相同，源码为 `306da479814f1451779915b403ed447d15b6cd0d`，对应 [CI 35854904452](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35854904452)。

- [安装器生命周期检查 35855805823](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35855805823)在提权的 Windows 11 ARM64 环境中通过全部 28 个预期步骤。原始 3.5.1 MSI 也通过了实际升级安装，已安装文件哈希与包一致，配置及恢复数据得到保留。
- 已安装的 3.5.1 应用检测到语言资源，保存并应用 `frFR`，以欧洲地区打开国际服战网；《星际争霸 II》5.0.16.97563 到达在线法语主菜单。游戏正常退出后，国际服文字／语音恢复为 `enUS`，无待恢复事务。
- 较早的界面检查涵盖五项模拟 WPF 事件、100% 和 200% 缩放下的部分英语／中文／希腊语布局、翻译一致性，以及针对性的原生字体渲染。[字体文档](../src/SC2Switcher.Wpf/Fonts/README.zh-CN.md)记录字体来源和字符覆盖。

这些结果仍归属于原始产物。设置修正接受针对性界面检查，新包单独接受 CI 和升级检查。

[版本说明](RELEASE-NOTES-3.5.3.zh-CN.md) · [发布流程](DEVELOPMENT.zh-CN.md#发布)

## 已发布预览版：3.4.2

| 身份／结果 | 记录 |
| --- | --- |
| 发布 | [v3.4.2-preview.1](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/releases/tag/v3.4.2-preview.1)，2026 年 9 月 22 日 |
| 源码 | `1e89600a713f650376f081af3146a6e3825d7e7e` |
| 成功的 CI／发布提升 | [35727903561](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35727903561)／[35728297411](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35728297411) |
| 验证 | 许可及包内容检查通过；四个附件匿名下载后均与原哈希一致；发布提升直接使用 CI 产物，未重新构建 |
| 范围 | 许可和分发修改；安装、界面与真实运行证据来自较早产物，本次未对该包重新进行这些测试 |

[版本说明](RELEASE-NOTES-3.4.2.zh-CN.md) · [开发](DEVELOPMENT.zh-CN.md) · [更新记录](../CHANGELOG.zh-CN.md)
