[简体中文](VALIDATION.zh-CN.md)

# Validation

Recorded 23 September 2026. Results apply to the source or artifact identified below. Detailed logs, hashes and rendering reports remain in ignored `artifacts/`.

## Development version: 3.5.1

The current source has local build, regression and rendering results. It has no formal package, hosted CI result, installation verification, integrated live WPF/game verification, tag or publication. Release acceptance requires identified CI artifacts, package and installer checks, and an integrated live check under the [release process](DEVELOPMENT.md#release).

| Scope | Recorded result |
| --- | --- |
| Functional baseline, before the font-only changes | Release build with SDK 10.0.401: zero warnings/errors; regression exit 0 with `TOTAL 82 PASSED`; five synthetic WPF event checks passed |
| Baseline layout | English, Simplified Chinese and Greek: 97 renders at 200% / 192 DPI and 98 at 100% / 96 DPI, with selected layout samples reviewed |
| Translation consistency | Eleven catalogues with 264 matching keys and placeholders |
| Final font build | Release build: zero warnings/errors; all fifteen font hashes and per-weight character coverage verified |
| Font preservation | Six CJK 600/650/700 subsets retain mapped outlines, advance widths, hinting, vertical metrics and name metadata; distinct 650 outlines remain in the display family registered at native WPF weight 600. The other nine font files are unchanged |
| Focused native rendering | 105 glyph samples passed without empty/missing glyphs or simulated weights; four Chinese/Korean compact Settings captures were reviewed without font clipping |
| Font size | 29,759,648 bytes (28.38 MiB); a compression probe of the complete fifteen-file portable payload produced a ZIP of 16,630,678 bytes (15.86 MiB), not a formal release package |

Final application DLL SHA-256: `BA293765A5DCA23166F36BBD904AE168B9D641417B8F6670500E0E62AAD59B6E`.

The functional and baseline DPI results precede the font-only changes; the final build received the focused font checks above. These results cover simulated data and rendering, with live acceptance still pending. [Development notes](RELEASE-NOTES-3.5.1.md) describe the changes; [font documentation](../src/SC2Switcher.Wpf/Fonts/README.md) records font sources and coverage.

## Published preview: 3.4.2

| Identity / result | Record |
| --- | --- |
| Release | [v3.4.2-preview.1](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/releases/tag/v3.4.2-preview.1), 22 September 2026 |
| Source | `1e89600a713f650376f081af3146a6e3825d7e7e` |
| Successful CI / promotion | [35727903561](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35727903561) / [35728297411](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35728297411) |
| Verification | License/payload checks passed; anonymous downloads of all four assets matched the original hashes; promotion reused the CI artifacts without rebuilding |
| Scope | Licensing and distribution changes; installation, interface and live-operation evidence comes from earlier artifacts, not a new test of this package |

[Release notes](RELEASE-NOTES-3.4.2.md) · [Development](DEVELOPMENT.md) · [Changelog](../CHANGELOG.md)
