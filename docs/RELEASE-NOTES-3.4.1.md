[简体中文](RELEASE-NOTES-3.4.1.zh-CN.md)

# SC2 Region Switcher 3.4.1 development preview

The administrator-required MSI has passed local build and package checks; CI lifecycle acceptance is pending. Version 3.4.1 has not been tagged, distributed or promoted.

## Changes

- The Global destination card shows EU, US or KR instead of INTERNATIONAL, identifying the selected Battle.net login region. Current configuration remains separate; the actual game server is selected in Battle.net.
- The MSI installs for all users in 64-bit Program Files, registers HKLM App Paths and creates one common Start menu entry. Installation requires administrator approval; the application runs with ordinary user permissions.
- Earlier current-user previews require a one-time uninstall/reinstall. Later machine versions use MSI major upgrades. The product-family UpgradeCode is retained, with new product and component identities for machine scope.
- Corrected MSI definitions and validation metadata, with independent ICE validation and isolated machine lifecycle checks.
- Provided paired English and Simplified Chinese documentation, including both README files in the portable ZIP.

The switching engine, configuration transactions and independent interface-language setting are unchanged. Configuration and backups remain per user in `%LOCALAPPDATA%\SC2RegionSwitcherV2`, outside MSI ownership and preserved on uninstall.

## Validation and distribution

The new machine package requires its own CI provenance, hashes and lifecycle evidence. Earlier current-user package checks and the administrator recovery comparison do not establish acceptance of this package. See [3.4.1 validation](VALIDATION-3.4.1.md).

The complete DPI matrix and online roundtrip remain historical 3.4.0 results. Follow-up UI checks cover the changed labels; no new full online roundtrip is claimed. Multi-display movement and cross-computer behavior remain unverified.

The original 3.4.0 files remain unchanged. Packages are unsigned and require Microsoft .NET 10 Desktop Runtime x64. The repository remains private; no application license has been selected. Installation and migration steps are in [Setup](SETUP.md).
