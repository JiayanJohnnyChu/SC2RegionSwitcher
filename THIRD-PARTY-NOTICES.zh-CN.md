[English](THIRD-PARTY-NOTICES.md)

# 第三方声明

SC2 Region Switcher 的原创代码、文档和图标采用 `LICENSE` 中的 MIT 许可证，版权人为 Jiayan Chu，年份为 2026。以下第三方材料保留各自的许可证与版权声明。

## Radix Colors

界面使用源自 [Radix Colors](https://github.com/radix-ui/colors) 的颜色值，包括 Sand、Tomato 和 Indigo 色阶，相关内容体现在 `src/SC2Switcher.Wpf/Palette.xaml` 与界面设计文档中。应用不包含 Radix 框架或运行时。

上游材料采用 MIT 许可证，版权声明为：

```text
Copyright (c) 2021-2022 Modulz
Copyright (c) 2022-Present WorkOS
```

完整上游许可证原文保存在 `licenses/LICENSE-Radix.txt`，未经修改。其来源为 [Radix Colors 许可证](https://github.com/radix-ui/colors/blob/main/LICENSE)。

## WiX Windows Installer 验证元数据

MSI 构建器使用源自 [WiX Toolset 3.14.1.8722](https://github.com/wixtoolset/wix3/releases/tag/wix3141rtm) 对照包的标准验证约束。该元数据子集另含补入的标准 AppSearch 与 RegLocator 定义。最终源码保存在 `tools/Installer/metadata/_Validation.idt`，并导入 MSI 数据库。

这些元数据采用 Microsoft Reciprocal License（MS-RL）分发，上游声明为：

```text
Copyright (c) .NET Foundation and contributors.
```

完整许可证保存在 `tools/Installer/metadata/LICENSE-WiX.txt`。MSI 与 ZIP 分发分别通过 `licenses/LICENSE-WiX.txt` 和 `licenses/WiX-Validation.idt` 提供许可证与对应元数据源码。元数据继续采用 MS-RL，项目 MIT 许可证不替代该许可。WiX 验证可执行文件属于开发工具，不包含在应用分发中。

## 分发内容

MSI 与 ZIP 分发包含 `LICENSE`、`THIRD-PARTY-NOTICES.md`、`THIRD-PARTY-NOTICES.zh-CN.md`，以及上述 `licenses/` 下的三个文件。完整许可证文本适用于各自材料，本声明说明其范围。
