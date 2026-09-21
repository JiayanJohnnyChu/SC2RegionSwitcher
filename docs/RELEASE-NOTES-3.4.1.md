[简体中文](RELEASE-NOTES-3.4.1.zh-CN.md)

# SC2 Region Switcher 3.4.1 development preview

This revision is under validation. Installer recovery acceptance is incomplete, and no final release candidate has been promoted.

## Changes

- The Global destination card shows EU, US or KR instead of INTERNATIONAL. These values identify the selected Battle.net login region. Current configuration remains separate; the actual StarCraft II server is selected in Battle.net.
- The application MSI component uses an HKCU registry key path with a renewed component identity. The product-family UpgradeCode and stable application directory remain unchanged.
- Corrected MSI column definitions, legacy-directory naming and language-neutral file metadata. Added standard validation metadata and an independent ICE check without suppressions.
- Provided paired English and Simplified Chinese documentation, including two README files in the portable ZIP.

The switching engine, configuration transactions and independent interface-language setting are unchanged.

## Validation and distribution

Local compilation completed with zero warnings and errors; 52 isolated regressions, package checks and five release rejection cases passed. The independent ICE suite reported zero errors and four ICE91 warnings relating to hypothetical per-machine use of fixed per-user directories. Basic English and Chinese label checks passed at 200% scaling in the standard window.

The full online roundtrip and four-scale DPI results belong to 3.4.0. Installation-failure recovery remains unresolved for the current preview. See [3.4.1 validation](VALIDATION-3.4.1.md).

The original 3.4.0 files remain unchanged. Version 3.4.1 uses a new three-part MSI version and requires its own CI provenance, package hashes and installation evidence. Packages remain unsigned and require Microsoft .NET 10 Desktop Runtime x64. Configuration instructions are in [Setup](SETUP.md).
