[简体中文](HANDOFF.zh-CN.md)

# Development status

Updated 2026-09-22. The active version is **3.4.1 development preview**.

## Current work

The Global card now shows the selected Battle.net login region, EU, US or KR. Current configuration and selected destination remain separate. The switch engine and configuration transactions are unchanged.

Local checks passed compilation with no warnings or errors, 52 isolated regressions, package-content checks, five release rejection cases and the independent ICE suite. Basic English and Chinese UI checks passed at 200% scaling. These results do not establish installation-failure recovery or a final CI candidate. See [3.4.1 validation](VALIDATION-3.4.1.md).

The original 3.4.0 candidate remains an unpublished private draft. Its complete four-scale DPI matrix and China → Europe → China online roundtrip apply to that candidate only. See [historical validation](VALIDATION.md).

## Remaining acceptance work

1. Complete the standard-user installer recovery check and resolve any failure.
2. Produce a clean final CI candidate, verify its provenance and hashes, and test the required installation lifecycle against those exact files.
3. Reassess UI and online coverage for the final changes; repeat affected checks before promotion.
4. Keep multi-display, cross-computer and signing limits explicit in the release notes.

Commit `d3a179bbbb1ec55f9850f1c0846b79d126ffac4d` passed [CI run 35667524580](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35667524580). Paired Windows 11 ARM64 recovery tests used the same candidate and harness: the [administrator-context run](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35668661867) fully restored the old installation, while the [standard-user run](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35668664128) restored visible resources but left the old product advertised and failed MSI cleanup checks. Both reached the native file-copy failure after old-product removal. The original input MSI remained unchanged, and disposable test environments were removed. A privilege-condition prototype has passed schema checks but has not been adopted or tested through the full lifecycle. Release acceptance remains incomplete; see the [3.4.1 validation record](VALIDATION-3.4.1.md).

## Release constraints

The repository is private and no application license has been selected. Packages are unsigned, framework-dependent MSI and ZIP distributions. Every distributed preview increments the three-part version; the distributed 3.4.0 candidate must retain its original files. Promotion copies the tested CI artifacts without rebuilding. The original 3.3.0 MSI has no downgrade protection.

## Maintainer references

- [Development commands](DEVELOPMENT.md)
- [Architecture](ARCHITECTURE.md)
- [Installer](../tools/Installer/README.md)
- [Candidate promotion](GITHUB-RELEASE.md)
- [Interface design](UI-DESIGN.md)
