[简体中文](CHANGELOG.zh-CN.md)

# Changelog

## 3.4.2 — preview

- Original project code, documentation and icons are licensed under MIT, copyright 2026 Jiayan Chu.
- MSI and ZIP packages include the project license, bilingual notices, the Radix MIT license, and the WiX MS-RL license with its validation source.
- Application behavior is unchanged. Local license-content and package verification passed; the original 3.4.1 preview files remain immutable.

The changes and scope are documented in [release notes](docs/RELEASE-NOTES-3.4.2.md) and [validation](docs/VALIDATION-3.4.2.md).

## 3.4.1 — preview

- Global destination labels show the selected Battle.net login region: EU, US or KR. The current configuration remains separate from the selected target.
- The installer uses an administrator-required, per-machine MSI in 64-bit Program Files, with HKLM App Paths and one common Start menu entry. The application still runs as an ordinary user; user configuration and backups remain separate and survive uninstall.
- Earlier current-user previews require a one-time uninstall/reinstall. Machine versions retain the product-family UpgradeCode and use new product/component identities.
- MSI schema definitions and validation metadata have been corrected.
- Package checks include independent Windows Installer ICE validation without suppressions.
- Documentation is available in paired English and Simplified Chinese versions, including both README files in the portable ZIP.

The administrator-required installer has passed build, package and hosted lifecycle checks for the candidate identified in the validation record. The final CI candidate passed interactive UAC migration and subsequent ordinary-user launch; basic English and Simplified Chinese Settings UI checks passed with isolated configuration. The changes and evidence are documented in [release notes](docs/RELEASE-NOTES-3.4.1.md) and [validation](docs/VALIDATION-3.4.1.md).

## 3.4.0 — private draft, on hold

- The interface received revised typography, spacing, destination colors and page alignment.
- The interface separated the current Battle.net configuration from the selected destination.
- The installer introduced current-user major upgrades, a stable application directory and one Start menu entry.
- The release workflow introduced candidate provenance checks and promotion of original CI artifacts.

The identified candidate passed normal installation lifecycle checks, the four-scale bilingual DPI matrix and one China → Europe → China online roundtrip. Failed-upgrade recovery remained unresolved. The changes and evidence are documented in [release notes](docs/RELEASE-NOTES-3.4.0.md) and [validation](docs/VALIDATION.md).

## 3.3.0 — historical preview

- The application provided a WPF interface with independent English and Simplified Chinese selection.
- Configuration supported Battle.net, China, Global and shared-settings paths.
- Configuration handling included validated saving, backups, unsaved-edit prompts and concurrent-change checks.
- The distribution included a current-user MSI, one Start menu entry and a portable ZIP.
- Development tooling included a pinned SDK, isolated regressions and package checks.

This installer used a versioned application directory and had no major-upgrade or downgrade-protection design. The version is described in [release notes](docs/RELEASE-NOTES-3.3.0.md).
