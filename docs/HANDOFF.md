[简体中文](HANDOFF.zh-CN.md)

# Development status

Updated 2026-09-22. The active version is **3.4.1 preview**.

## Current work

The installer now targets an administrator-required, per-machine installation in 64-bit Program Files, with HKLM App Paths and one common Start menu entry. The application remains `asInvoker`; per-user configuration and backups stay outside MSI ownership and survive uninstall. Earlier current-user previews require a one-time uninstall/reinstall. Subsequent machine releases use major upgrades. The design and migration procedure are documented in [Installer](../tools/Installer/README.md) and [Setup](SETUP.md).

The Global card shows the selected Battle.net login region, EU, US or KR. Current configuration remains separate. The switching engine and configuration transactions are unchanged.

Local machine-package checks passed: compilation with zero warnings and errors, 52 regressions, package validation, eight release rejection cases and the full ICE suite with zero errors or warnings. The candidate from [CI run 35721803273](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35721803273) passed hosted machine lifecycle testing in [run 35722639637](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35722639637). Earlier current-user recovery results, including the successful administrator comparison, do not validate the new package. The final candidate from [CI run 35723586274](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35723586274), source `3337cbacbf2acc0bc8ba9a04c3431cb6e5ab819a`, passed interactive UAC migration from 3.4.0 and ordinary-user launch. Its application source and MSI authoring are identical to the hosted lifecycle candidate. Exact evidence and coverage limits are in [3.4.1 validation](VALIDATION-3.4.1.md).

## Validation scope and artifact promotion

Basic English and Simplified Chinese Settings UI checks passed with isolated configuration. Promotion requires the original final CI files without rebuilding. The existing full DPI and online roundtrip remain historical coverage; broader tests are repeated only if affected application behavior requires them. The original 3.4.0 candidate's four-scale DPI matrix and China → Europe → China roundtrip remain recorded in [historical validation](VALIDATION.md); no new online roundtrip is claimed.

## Release constraints

The repository remains private, and no application license has been selected. Packages are unsigned and require .NET 10 Desktop Runtime x64. The original 3.4.0 private draft and its files remain unchanged. Each distributed preview increments the three-part version; promotion copies tested CI artifacts without rebuilding. Multi-display, cross-computer and system-policy coverage remains limited.

## Maintainer references

- [Development commands](DEVELOPMENT.md)
- [Architecture](ARCHITECTURE.md)
- [Candidate promotion](GITHUB-RELEASE.md)
- [Interface design](UI-DESIGN.md)
