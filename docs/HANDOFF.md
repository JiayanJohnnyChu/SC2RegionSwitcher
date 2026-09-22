[简体中文](HANDOFF.zh-CN.md)

# Development status

Updated 2026-09-22. The active version is **3.4.1 development preview**, not tagged or distributed.

## Current work

The installer now targets an administrator-required, per-machine installation in 64-bit Program Files, with HKLM App Paths and one common Start menu entry. The application remains `asInvoker`; per-user configuration and backups stay outside MSI ownership and survive uninstall. Earlier current-user previews require a one-time uninstall/reinstall. Subsequent machine releases use major upgrades. See [Installer](../tools/Installer/README.md) and [Setup](SETUP.md).

The Global card shows the selected Battle.net login region, EU, US or KR. Current configuration remains separate. The switching engine and configuration transactions are unchanged.

Local machine-package checks passed: compilation with zero warnings and errors, 52 regressions, package validation, seven release rejection cases and the full ICE suite with zero errors or warnings. CI candidate provenance and installation lifecycle acceptance remain pending. Earlier current-user recovery results, including the successful administrator comparison, do not validate the new package. Exact evidence and pending checks are in [3.4.1 validation](VALIDATION-3.4.1.md).

## Remaining acceptance work

1. Identify a clean CI candidate by source commit and hashes; complete its machine installation lifecycle, including failed-upgrade recovery and legacy-preview detection.
2. Complete basic launch and bilingual UI checks for the final changes. Retain the existing full DPI and online roundtrip as historical coverage; repeat broader tests only if an affected application behavior requires them.

Interactive installation from a standard desktop through UAC, followed by ordinary-user application launch, remains pending. An elevated CI lifecycle test does not establish that interactive path. The original 3.4.0 candidate's four-scale DPI matrix and China → Europe → China roundtrip remain recorded in [historical validation](VALIDATION.md); no new online roundtrip is claimed.

## Release constraints

The repository remains private, and no application license has been selected. Packages are unsigned and require .NET 10 Desktop Runtime x64. The original 3.4.0 private draft and its files remain unchanged. Each distributed preview increments the three-part version; promotion copies tested CI artifacts without rebuilding. Multi-display, cross-computer and system-policy coverage remains limited.

## Maintainer references

- [Development commands](DEVELOPMENT.md)
- [Architecture](ARCHITECTURE.md)
- [Candidate promotion](GITHUB-RELEASE.md)
- [Interface design](UI-DESIGN.md)
