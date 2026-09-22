[简体中文](RELEASE-NOTES-3.4.1.zh-CN.md)

# SC2 Region Switcher 3.4.1 preview

The administrator-required MSI has passed build, package and hosted lifecycle checks for the candidate identified in the validation record. The final CI candidate passed interactive UAC migration and subsequent ordinary-user launch. Basic English and Simplified Chinese Settings UI checks passed with isolated configuration. Distribution remains private.

## Changes

- The Global destination card shows EU, US or KR instead of INTERNATIONAL, identifying the selected Battle.net login region. Current configuration remains separate; the actual game server is selected in Battle.net.
- The MSI installs for all users in 64-bit Program Files, registers HKLM App Paths and creates one common Start menu entry. Installation requires administrator approval; the application runs with ordinary user permissions.
- Earlier current-user previews require a one-time uninstall/reinstall. Later machine versions use MSI major upgrades. The product-family UpgradeCode is retained, with new product and component identities for machine scope.
- MSI definitions and validation metadata have been corrected, with independent ICE validation and isolated machine lifecycle checks.
- Documentation is available in paired English and Simplified Chinese versions, including both README files in the portable ZIP.

The switching engine, configuration transactions and independent interface-language setting are unchanged. Configuration and backups remain per user in `%LOCALAPPDATA%\SC2RegionSwitcherV2`, outside MSI ownership and preserved on uninstall.

## Validation and distribution

The identified machine candidate passed hosted installation, maintenance, upgrade, failed-upgrade recovery and removal checks. The final candidate from CI run 35723586274 passed interactive UAC migration and ordinary-user launch. The full hosted lifecycle result belongs to an earlier package with identical application source and MSI authoring; the final revision changes documentation and the test harness. Earlier current-user package checks and the administrator recovery comparison do not establish acceptance of this package. The evidence is documented in [3.4.1 validation](VALIDATION-3.4.1.md).

The complete DPI matrix and online roundtrip remain historical 3.4.0 results. The final installed application passed basic English and Simplified Chinese Settings UI checks with isolated configuration; no new full DPI matrix or online roundtrip is claimed. Multi-display movement and cross-computer behavior remain unverified.

The original 3.4.0 files remain unchanged. Packages are unsigned and require Microsoft .NET 10 Desktop Runtime x64. The repository remains private; no application license has been selected. Installation and migration steps are in [Setup](SETUP.md).
