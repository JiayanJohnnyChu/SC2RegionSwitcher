[简体中文](THIRD-PARTY-NOTICES.zh-CN.md)

# Third-party notices

Original SC2 Region Switcher code, documentation and icons are licensed under the MIT License in `LICENSE`, copyright 2026 Jiayan Chu. The third-party materials identified below retain their respective licenses and copyright notices.

## Radix Colors

The interface uses color values derived from [Radix Colors](https://github.com/radix-ui/colors), including the Sand, Tomato and Indigo scales. Their use is reflected in `src/SC2Switcher.Wpf/Palette.xaml` and the interface-design documentation. No Radix framework or runtime is included.

The upstream material is licensed under MIT, with these copyright notices:

```text
Copyright (c) 2021-2022 Modulz
Copyright (c) 2022-Present WorkOS
```

The complete upstream license is included unchanged in `licenses/LICENSE-Radix.txt`. Its source is the [Radix Colors license](https://github.com/radix-ui/colors/blob/main/LICENSE).

## WiX Windows Installer validation metadata

The MSI builder incorporates standard validation constraints derived from a control package generated with [WiX Toolset 3.14.1.8722](https://github.com/wixtoolset/wix3/releases/tag/wix3141rtm). The metadata subset includes additional standard AppSearch and RegLocator definitions. The resulting source is maintained in `tools/Installer/metadata/_Validation.idt` and is imported into the MSI database.

This metadata is distributed under the Microsoft Reciprocal License (MS-RL), with the upstream notice:

```text
Copyright (c) .NET Foundation and contributors.
```

The complete license is maintained in `tools/Installer/metadata/LICENSE-WiX.txt`. MSI and ZIP distributions include the license as `licenses/LICENSE-WiX.txt` and the corresponding metadata source as `licenses/WiX-Validation.idt`. The metadata retains MS-RL; the project MIT license does not replace it. WiX validation executables are development tools and are not included in the application distribution.

## Distribution contents

MSI and ZIP distributions include `LICENSE`, `THIRD-PARTY-NOTICES.md`, `THIRD-PARTY-NOTICES.zh-CN.md` and the three files under `licenses/` identified above. The complete license texts govern their respective materials; this notice describes their scope.
