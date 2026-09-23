[简体中文](VALIDATION.zh-CN.md)

# Validation

Recorded 23 September 2026. Results apply to the source or artifact identified below. Detailed logs, hashes and rendering reports remain in ignored `artifacts/`.

## Release candidate: 3.5.1

`v3.5.1-preview.1` is prepared as a draft prerelease using the original accepted CI artifacts. It is not publicly available; the public download remains 3.4.2.

| Identity / check | Recorded result |
| --- | --- |
| Source | `306da479814f1451779915b403ed447d15b6cd0d` |
| Candidate CI | [35854904452](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35854904452): Release build with zero warnings/errors; `TOTAL 82 PASSED`; package identity, payload, release guards and full unsuppressed MSI ICE checks passed, with zero ICE warnings/errors |
| Installer lifecycle | [35855805823](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35855805823): all 28 expected steps passed in an elevated Windows 11 ARM64 environment; harness source `2f56c9b028aa22a23d043b5efc44bcd8af1ea125` |
| Installed upgrade | Original MSI upgraded 3.4.1 to 3.5.1 with exit 0; all thirteen installed files matched the package hashes; existing configuration and recovery data were preserved; one Start menu entry, one uninstall entry and the expected App Paths registration were present |
| Integrated live check | Installed application detected installed language resources, saved and applied `frFR`, and opened Battle.net Global in Europe. StarCraft II 5.0.16.97563 reached its online French main menu |
| State after live check | Normal game exit; Global text/speech restored to `enUS` through the app; China remained `zhCN`; profiles and interface preferences matched their original values; no pending recovery |
| Draft promotion | [35856933187](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35856933187): the four original CI assets were uploaded, downloaded again and hash-verified without rebuilding |

| Original CI artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| MSI | 16,719,872 | `51490A95C5ED7BDB17F31FD0D00D8636BE8466D3E78E56924481AB99E15B4AA2` |
| Portable ZIP | 16,629,526 | `4D79FDC9B5EF0E2D9E2CAD9F61974D6120B067CAA77982C9EC37D01BAB357458` |

Candidate application DLL SHA-256: `81DB66B10D387C65A3258000EF651D5B02006376420F5AACBD4AA32A55539548`.

The live check covers the new Global language selection in the installed application. It does not extend the earlier China roundtrip or DPI results to this package.

### Supporting interface and font checks

Earlier 3.5.1 builds passed five synthetic WPF event checks and produced 97 renders at 200% / 192 DPI and 98 at 100% / 96 DPI across English, Simplified Chinese and Greek, with selected layout samples reviewed. These event and layout results precede the font-only changes. Eleven translation catalogues had 264 matching keys and placeholders.

The final font revision passed all fifteen font hashes and per-weight coverage checks, 105 native glyph samples, and review of four Chinese/Korean compact Settings captures without font clipping. Six reduced CJK 600/650/700 subsets preserved mapped outlines and metrics, including distinct 650 display outlines registered at native WPF weight 600; the other nine font files were unchanged. Fonts total 29,759,648 bytes (28.38 MiB). These focused results belong to the font verification build; the table above records the packaged candidate's checks.

[Release notes](RELEASE-NOTES-3.5.1.md) · [Font documentation](../src/SC2Switcher.Wpf/Fonts/README.md) · [Release process](DEVELOPMENT.md#release)

## Published preview: 3.4.2

| Identity / result | Record |
| --- | --- |
| Release | [v3.4.2-preview.1](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/releases/tag/v3.4.2-preview.1), 22 September 2026 |
| Source | `1e89600a713f650376f081af3146a6e3825d7e7e` |
| Successful CI / promotion | [35727903561](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35727903561) / [35728297411](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35728297411) |
| Verification | License/payload checks passed; anonymous downloads of all four assets matched the original hashes; promotion reused the CI artifacts without rebuilding |
| Scope | Licensing and distribution changes; installation, interface and live-operation evidence comes from earlier artifacts, not a new test of this package |

[Release notes](RELEASE-NOTES-3.4.2.md) · [Development](DEVELOPMENT.md) · [Changelog](../CHANGELOG.md)
