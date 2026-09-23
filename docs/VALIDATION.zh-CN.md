[English](VALIDATION.md)

# 验证

记录日期：2026 年 9 月 23 日。结果仅适用于下文标明的源码或产物。详细日志、哈希和渲染报告保留于已忽略的 `artifacts/`。

## 发布候选版：3.5.1

`v3.5.1-preview.1` 已使用验收通过的原始 CI 产物创建预发布草稿，尚未公开；公开下载仍为 3.4.2。

| 身份／检查 | 已记录结果 |
| --- | --- |
| 源码 | `306da479814f1451779915b403ed447d15b6cd0d` |
| 候选版本 CI | [35854904452](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35854904452)：Release 构建零警告、零错误；`TOTAL 82 PASSED`；包身份、内容、发布保护和无抑制项的完整 MSI ICE 检查通过，ICE 零警告、零错误 |
| 安装器生命周期 | [35855805823](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35855805823)：在提权的 Windows 11 ARM64 环境中通过全部 28 个预期步骤；测试程序源码为 `2f56c9b028aa22a23d043b5efc44bcd8af1ea125` |
| 实际升级安装 | 原始 MSI 将 3.4.1 升级至 3.5.1，退出码为 0；十三个已安装文件的哈希均与包一致，既有配置与恢复数据得到保留；开始菜单和卸载入口各一个，App Paths 注册正确 |
| 整合实测 | 已安装的应用检测到已安装的语言资源，保存并应用 `frFR`，以欧洲地区打开国际服战网；《星际争霸 II》5.0.16.97563 到达在线法语主菜单 |
| 实测后状态 | 游戏正常退出；通过应用将国际服文字／语音恢复为 `enUS`，国服仍为 `zhCN`；配置和界面偏好与原值一致，无待恢复事务 |
| 草稿提升 | [35856933187](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35856933187)：四个原始 CI 附件完成上传、重新下载和哈希验证，未重新构建 |

| 原始 CI 产物 | 字节数 | SHA-256 |
| --- | ---: | --- |
| MSI | 16,719,872 | `51490A95C5ED7BDB17F31FD0D00D8636BE8466D3E78E56924481AB99E15B4AA2` |
| 便携 ZIP | 16,629,526 | `4D79FDC9B5EF0E2D9E2CAD9F61974D6120B067CAA77982C9EC37D01BAB357458` |

候选版应用 DLL SHA-256：`81DB66B10D387C65A3258000EF651D5B02006376420F5AACBD4AA32A55539548`。

此次实测覆盖已安装应用中的国际服语言选择新功能，不将先前的国服往返或 DPI 结果视为对此包的验证。

### 界面与字体辅助检查

较早的 3.5.1 构建通过了五项模拟 WPF 事件检查，并以英语、简体中文和希腊语完成 200%／192 DPI 下 97 次渲染、100%／96 DPI 下 98 次渲染，审阅了部分布局样本。这些事件和布局检查早于后续仅涉及字体的修改。十一份翻译的 264 个键及占位符一致。

最终字体修订通过了十五个字体的哈希及各字重字符覆盖检查、105 个原生字形样本检查，并审阅了四张中文／韩语紧凑设置截图，未见字体裁切。六个精简后的 CJK 600／650／700 子集保留映射字形轮廓与度量，包括以 WPF 原生 600 字重注册的独立 650 显示轮廓；其余九个字体文件未变。字体共 29,759,648 字节（28.38 MiB）。这些针对性结果属于字体验证构建；已打包候选版的检查记录见上表。

[版本说明](RELEASE-NOTES-3.5.1.zh-CN.md) · [字体文档](../src/SC2Switcher.Wpf/Fonts/README.zh-CN.md) · [发布流程](DEVELOPMENT.zh-CN.md#发布)

## 已发布预览版：3.4.2

| 身份／结果 | 记录 |
| --- | --- |
| 发布 | [v3.4.2-preview.1](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/releases/tag/v3.4.2-preview.1)，2026 年 9 月 22 日 |
| 源码 | `1e89600a713f650376f081af3146a6e3825d7e7e` |
| 成功的 CI／发布提升 | [35727903561](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35727903561)／[35728297411](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35728297411) |
| 验证 | 许可及包内容检查通过；四个附件匿名下载后均与原哈希一致；发布提升直接使用 CI 产物，未重新构建 |
| 范围 | 许可和分发修改；安装、界面与真实运行证据来自较早产物，本次未对该包重新进行这些测试 |

[版本说明](RELEASE-NOTES-3.4.2.zh-CN.md) · [开发](DEVELOPMENT.zh-CN.md) · [更新记录](../CHANGELOG.zh-CN.md)
