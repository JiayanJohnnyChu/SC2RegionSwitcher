[English](THIRD-PARTY-NOTICES.md)

# 第三方声明

SC2 Region Switcher 的原创代码、文档和图标采用 `LICENSE` 中的 MIT 许可证。以下第三方材料保留各自的许可证与版权声明。

## Radix Colors

部分辅助中性色源自 [Radix Colors](https://github.com/radix-ui/colors)。

上游材料采用 MIT 许可证，版权声明为：

```text
Copyright (c) 2021-2022 Modulz
Copyright (c) 2022-Present WorkOS
```

完整上游许可证原文保存在 `licenses/LICENSE-Radix.txt`，未经修改。其来源为 [Radix Colors 许可证](https://github.com/radix-ui/colors/blob/main/LICENSE)。

## Inter 与 Noto Sans 字体

应用界面打包了 Inter 4.001、Noto Sans SC 2.004、Noto Sans KR 2.004 的静态实例，采用 SIL Open Font License 1.1。派生字体为 Switcher Sans、Switcher Han、Switcher Hangul 及各自独立的标题字体。Inter 与 CJK Regular 保留完整字符覆盖，较粗 CJK 字重采用字符子集。字体保留原始版权与许可元数据。

许可证原文保存在 `licenses/LICENSE-Inter.txt`、`licenses/LICENSE-NotoSC.txt`、`licenses/LICENSE-NotoKR.txt`。固定上游版本与源地址记录在 [仓库字体来源清单](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/blob/main/src/SC2Switcher.Wpf/Fonts/sources.json)，生成文件的哈希记录在 [生成字体清单](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/blob/main/src/SC2Switcher.Wpf/Fonts/manifest.json)。这些许可证适用于字体，不替代应用代码的许可证。

## WiX Windows Installer 验证元数据

MSI 构建器使用源自 [WiX Toolset 3.14.1.8722](https://github.com/wixtoolset/wix3/releases/tag/wix3141rtm) 对照包的标准验证约束。元数据另含标准 AppSearch 与 RegLocator 定义。源码保存在 `tools/Installer/metadata/_Validation.idt`，并导入 MSI 数据库。

这些元数据采用 Microsoft Reciprocal License（MS-RL）分发，上游声明为：

```text
Copyright (c) .NET Foundation and contributors.
```

完整许可证保存在 `tools/Installer/metadata/LICENSE-WiX.txt`。MSI 与 ZIP 分发分别通过 `licenses/LICENSE-WiX.txt` 和 `licenses/WiX-Validation.idt` 提供许可证与对应元数据源码。WiX 验证可执行文件属于开发工具，不包含在应用分发中。

## 分发内容

MSI 与 ZIP 分发包含 `LICENSE`、`THIRD-PARTY-NOTICES.md`、`THIRD-PARTY-NOTICES.zh-CN.md`，以及上述 `licenses/` 下的许可证和对应源码文件。
