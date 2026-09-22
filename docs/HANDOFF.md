[简体中文](HANDOFF.zh-CN.md)

# Development status

Updated 2026-09-22. [**v3.4.2-preview.1**](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/releases/tag/v3.4.2-preview.1) is a published public prerelease.

## Current revision

Version 3.4.2 adopts MIT for original project code, documentation and icons, copyright 2026 Jiayan Chu. MSI and ZIP distributions include the project license, bilingual third-party notices, the upstream Radix MIT license, and the WiX MS-RL license with its corresponding validation source. The application behavior and interface are unchanged.

Verification is limited to license and distribution contents and the existing build, package and static CI checks. Local packaging and content verification passed, as recorded in [3.4.2 validation](VALIDATION-3.4.2.md). Source `1e89600a713f650376f081af3146a6e3825d7e7e` passed [CI run 35727903561](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35727903561) and [promotion run 35728297411](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35728297411). The [3.4.1 record](VALIDATION-3.4.1.md) retains prior installer, UAC migration and basic UI evidence; the full DPI matrix and online roundtrip remain historical 3.4.0 results.

## Distribution

The original 3.4.1 preview files remain immutable. Every distributed preview increments the three-part version, and promotion uses the original CI artifacts without rebuilding. Packages remain unsigned and require .NET 10 Desktop Runtime x64. The repository is public. The prerelease was published on 2026-09-22 at 12:44:51 UTC; all four release assets were downloaded without authentication and matched the original SHA-256 values.

## Maintainer references

- [Development commands](DEVELOPMENT.md)
- [Installer](../tools/Installer/README.md)
- [Third-party notices](../THIRD-PARTY-NOTICES.md)
- [Candidate promotion](GITHUB-RELEASE.md)
