[简体中文](VALIDATION.zh-CN.md)

# Validation

Recorded 23 September 2026. Results apply to the identified source or artifact. Detailed reports remain in ignored `artifacts/`.

## Release candidate: 3.5.3

The candidate includes Settings alignment and field-spacing corrections. Its CI, package and installation checks are pending. The public download remains 3.4.2.

| Focused prepackage check | Result |
| --- | --- |
| Coverage | English and Simplified Chinese, each at wide (1000 DIP) and compact (520 DIP) window widths; four captures reviewed |
| Settings alignment | Content and footer controls share the right edge; automatic scrollbar transitions preserve that alignment |
| Field spacing | Text fields and language selectors each measure 48 DIP high, with a 22 DIP text line and matching 13 DIP left/top offsets from the outer border |
| Interaction | Dropdown open, selection and close passed in all four cases |

## Evidence for unchanged components

The installer and switching logic are unchanged from the accepted 3.5.1 candidate, source `306da479814f1451779915b403ed447d15b6cd0d`, [CI 35854904452](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35854904452).

- [Installer lifecycle 35855805823](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35855805823) passed all 28 expected steps in an elevated Windows 11 ARM64 environment. The original 3.5.1 MSI also passed an installed upgrade with matching payload hashes and preserved configuration/recovery data.
- The installed 3.5.1 application detected language resources, saved and applied `frFR`, and opened Battle.net Global in Europe. StarCraft II 5.0.16.97563 reached its online French main menu. The game exited normally, Global text/speech was restored to `enUS`, and no recovery remained pending.
- Earlier interface checks covered five synthetic WPF events, selected English/Chinese/Greek layouts at 100% and 200% scaling, translation consistency and focused native font rendering. Font sources and coverage are recorded in the [font documentation](../src/SC2Switcher.Wpf/Fonts/README.md).

These results retain their original artifact attribution. The Settings corrections receive targeted interface checks; the new package receives its own CI and upgrade checks.

[Release notes](RELEASE-NOTES-3.5.3.md) · [Release process](DEVELOPMENT.md#release)

## Published preview: 3.4.2

| Identity / result | Record |
| --- | --- |
| Release | [v3.4.2-preview.1](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/releases/tag/v3.4.2-preview.1), 22 September 2026 |
| Source | `1e89600a713f650376f081af3146a6e3825d7e7e` |
| Successful CI / promotion | [35727903561](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35727903561) / [35728297411](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35728297411) |
| Verification | License/payload checks passed; anonymous downloads of all four assets matched the original hashes; promotion reused the CI artifacts without rebuilding |
| Scope | Licensing and distribution changes; installation, interface and live-operation evidence comes from earlier artifacts, not a new test of this package |

[Release notes](RELEASE-NOTES-3.4.2.md) · [Development](DEVELOPMENT.md) · [Changelog](../CHANGELOG.md)
