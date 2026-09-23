[简体中文](VALIDATION.zh-CN.md)

# Validation

Recorded 23 September 2026. Results apply to the identified source or artifact. Detailed reports remain in ignored `artifacts/`.

## Published preview: 3.5.3

[v3.5.3-preview.1](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/releases/tag/v3.5.3-preview.1) was published on 23 September 2026. It includes Settings alignment and field-spacing corrections.

| Release identity / check | Result |
| --- | --- |
| Source | `e811cfbf4a2929915da13abb7640260b058b90c5` |
| CI | [35862405376](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35862405376): zero build warnings/errors, `TOTAL 82 PASSED`, package and release-guard checks passed; full unsuppressed MSI ICE validation reported zero warnings/errors |
| Original artifacts | MSI: 16,719,872 bytes; portable ZIP: 16,629,653 bytes. Downloaded CI artifacts matched the manifest hashes and provenance |
| Promotion and public downloads | [35863060946](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35863060946) uploaded and reverified the original four CI assets without rebuilding. All four anonymous public downloads matched the original hashes |
| Installed Settings | Version 3.5.3 displayed matching field heights and text insets, content aligned with the Save & check button, and consistent lower Settings controls; the Global language dropdown opened and closed without changing the selection |
| Installed upgrade | Original MSI upgraded 3.5.2 to 3.5.3 with exit 0; all thirteen installed files matched the package hashes; existing configuration and recovery data were preserved; one Start menu entry, one uninstall entry and the expected App Paths registration were present |

Artifact SHA-256 values:

- MSI: `9B43838B9C5605DF7535BA7234D89EBB94834BB9EC236942F876401150E2B9E8`
- Portable ZIP: `53294F157E9225CA6BA318FAEEFB4EAC4F6D077FA66C6CE4219D1DF850D9E19C`
- Installed application DLL: `780020B4BB85D6041F609A6215B8790791660757EB73399A0D5E7FEFE0835028`

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

These results retain their original artifact attribution. Targeted interface, CI and upgrade results for the published version are recorded above.

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
