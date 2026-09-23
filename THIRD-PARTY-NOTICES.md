[简体中文](THIRD-PARTY-NOTICES.zh-CN.md)

# Third-party notices

Original SC2 Region Switcher code, documentation and icons are licensed under the MIT License in `LICENSE`. The third-party materials identified below retain their respective licenses and copyright notices.

## Radix Colors

Some supporting neutral colors derive from [Radix Colors](https://github.com/radix-ui/colors).

The upstream material is licensed under MIT, with these copyright notices:

```text
Copyright (c) 2021-2022 Modulz
Copyright (c) 2022-Present WorkOS
```

The complete upstream license is included unchanged in `licenses/LICENSE-Radix.txt`. Its source is the [Radix Colors license](https://github.com/radix-ui/colors/blob/main/LICENSE).

## Inter and Noto Sans fonts

The application interface embeds static instances of Inter 4.001, Noto Sans SC 2.004 and Noto Sans KR 2.004 under the SIL Open Font License 1.1. The derived families are Switcher Sans, Switcher Han and Switcher Hangul, with separate display families. Inter and CJK Regular retain full character coverage; heavier CJK weights use character subsets. Original copyright and license metadata is retained.

Unmodified licenses are provided in `licenses/LICENSE-Inter.txt`, `licenses/LICENSE-NotoSC.txt` and `licenses/LICENSE-NotoKR.txt`. Exact upstream revisions and source URLs are recorded in [repository font sources](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/blob/main/src/SC2Switcher.Wpf/Fonts/sources.json); generated hashes are in [generated font manifest](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/blob/main/src/SC2Switcher.Wpf/Fonts/manifest.json). These licenses govern the fonts, not the application code.

## WiX Windows Installer validation metadata

The MSI builder incorporates standard validation constraints derived from a control package generated with [WiX Toolset 3.14.1.8722](https://github.com/wixtoolset/wix3/releases/tag/wix3141rtm). It also includes standard AppSearch and RegLocator definitions. The source is maintained in `tools/Installer/metadata/_Validation.idt` and is imported into the MSI database.

This metadata is distributed under the Microsoft Reciprocal License (MS-RL), with the upstream notice:

```text
Copyright (c) .NET Foundation and contributors.
```

The complete license is maintained in `tools/Installer/metadata/LICENSE-WiX.txt`. MSI and ZIP distributions include the license as `licenses/LICENSE-WiX.txt` and the corresponding metadata source as `licenses/WiX-Validation.idt`. WiX validation executables are development tools and are not included in the application distribution.

## Distribution contents

MSI and ZIP distributions include `LICENSE`, `THIRD-PARTY-NOTICES.md`, `THIRD-PARTY-NOTICES.zh-CN.md` and the license and source files under `licenses/` identified above.
