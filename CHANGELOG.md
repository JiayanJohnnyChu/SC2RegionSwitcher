[简体中文](CHANGELOG.zh-CN.md)

# Changelog

## 3.4.1 — development preview

- Global destination labels show the selected Battle.net login region: EU, US or KR. The current configuration remains separate from the selected target.
- Corrected MSI schema definitions and validation metadata. The application component uses an HKCU key path with a new component identity.
- Added independent Windows Installer ICE validation without suppressions.
- Added paired English and Simplified Chinese documentation, including both README files in the portable ZIP.

Installer failure recovery has not passed acceptance testing; this version is not a release candidate. See [release notes](docs/RELEASE-NOTES-3.4.1.md) and [validation](docs/VALIDATION-3.4.1.md).

## 3.4.0 — private draft, on hold

- Revised typography, spacing, destination colors and page alignment.
- Separated the current Battle.net configuration from the selected destination.
- Added current-user major upgrades, a stable application directory and one Start menu entry.
- Added candidate provenance checks and promotion of original CI artifacts.

The identified candidate passed normal installation lifecycle checks, the four-scale bilingual DPI matrix and one China → Europe → China online roundtrip. Failed-upgrade recovery remained unresolved. See [release notes](docs/RELEASE-NOTES-3.4.0.md) and [validation](docs/VALIDATION.md).

## 3.3.0 — historical preview

- Provided a WPF interface with independent English and Simplified Chinese selection.
- Supported configurable Battle.net, China, Global and shared-settings paths.
- Included validated configuration saving, backups, unsaved-edit prompts and concurrent-change checks.
- Provided a current-user MSI, one Start menu entry and a portable ZIP.
- Included a pinned development SDK, isolated regressions and package checks.

This installer used a versioned application directory and had no major-upgrade or downgrade-protection design. See [release notes](docs/RELEASE-NOTES-3.3.0.md).
