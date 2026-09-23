[English](VALIDATION.md)

# 验证

记录日期：2026 年 9 月 23 日。结果仅适用于标明的源码或产物。详细报告保留于已忽略的 `artifacts/`。

## 已发布预览版：3.5.3

[v3.5.3-preview.1](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/releases/tag/v3.5.3-preview.1) 于 2026 年 9 月 23 日发布，包含设置对齐和字段间距修正。

| 发布身份／检查 | 结果 |
| --- | --- |
| 源码 | `e811cfbf4a2929915da13abb7640260b058b90c5` |
| CI | [35862405376](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35862405376)：构建零警告、零错误，`TOTAL 82 PASSED`，包及发布保护检查通过；无抑制项的完整 MSI ICE 验证零警告、零错误 |
| 原始产物 | MSI：16,719,872 字节；便携 ZIP：16,629,653 字节。下载的 CI 产物与清单哈希及来源记录一致 |
| 发布提升与公开下载 | [35863060946](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35863060946) 上传并重新验证四个原始 CI 附件，未重新构建。四个附件匿名公开下载后的哈希均与原值一致 |
| 安装后的设置界面 | 3.5.3 的字段高度与文字边距一致，内容与“保存并检查”按钮对齐，下方设置控件间距一致；国际服语言下拉框正常打开和关闭，所选语言未改变 |
| 实际升级安装 | 原始 MSI 将 3.5.2 升级至 3.5.3，退出码为 0；十三个已安装文件的哈希均与包一致，既有配置与恢复数据得到保留；开始菜单和卸载入口各一个，App Paths 注册正确 |

产物 SHA-256：

- MSI：`9B43838B9C5605DF7535BA7234D89EBB94834BB9EC236942F876401150E2B9E8`
- 便携 ZIP：`53294F157E9225CA6BA318FAEEFB4EAC4F6D077FA66C6CE4219D1DF850D9E19C`
- 已安装的应用 DLL：`780020B4BB85D6041F609A6215B8790791660757EB73399A0D5E7FEFE0835028`

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

这些结果仍归属于原始产物。已发布版本的针对性界面、CI 和升级检查结果见上文。

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
