[简体中文](HANDOFF.zh-CN.md)

# Development status

Updated 2026-09-22. The active version is **3.4.2 preview**.

## Current revision

Version 3.4.2 adopts MIT for original project code, documentation and icons, copyright 2026 Jiayan Chu. MSI and ZIP distributions include the project license, bilingual third-party notices, the upstream Radix MIT license, and the WiX MS-RL license with its corresponding validation source. The application behavior and interface are unchanged.

Verification is limited to license and distribution contents and the existing build, package and static CI checks. Local packaging and content verification passed, as recorded in [3.4.2 validation](VALIDATION-3.4.2.md). CI results are associated with the candidate source commit in its release record. The [3.4.1 record](VALIDATION-3.4.1.md) retains prior installer, UAC migration and basic UI evidence; the full DPI matrix and online roundtrip remain historical 3.4.0 results.

## Distribution

The original 3.4.1 preview files remain immutable. Every distributed preview increments the three-part version, and promotion uses the original CI artifacts without rebuilding. Packages remain unsigned and require .NET 10 Desktop Runtime x64. Repository visibility remains private.

## Maintainer references

- [Development commands](DEVELOPMENT.md)
- [Installer](../tools/Installer/README.md)
- [Third-party notices](../THIRD-PARTY-NOTICES.md)
- [Candidate promotion](GITHUB-RELEASE.md)
