[简体中文](VALIDATION-3.4.2.zh-CN.md)

# 3.4.2 validation status

Recorded 2026-09-22. Version 3.4.2 changes licensing and distribution contents; application behavior is unchanged.

The verification scope consists of license provenance and text, inclusion of notices and WiX metadata source in MSI and ZIP packages, and the existing build, package and static CI checks. CI results are associated with the candidate source commit in its release record.

One local package build and `Test-Package.ps1` passed. Verification covered provenance, hashes, ZIP/MSI byte identity and file, component, upgrade and shortcut definitions. The MSI contains ten files and the ZIP contains twelve; all six licensing/source files match their canonical repository sources by SHA-256. The record is retained in `artifacts/validation/3.4.2/local-package.json`.

The project MIT text follows the [Open Source Initiative text](https://opensource.org/license/mit). The Radix license is copied from its [upstream source](https://raw.githubusercontent.com/radix-ui/colors/main/LICENSE), retaining the Modulz and WorkOS notices. The existing WiX license and metadata source remain unchanged.

The [3.4.1 validation record](VALIDATION-3.4.1.md) retains the earlier installer, UAC migration and basic UI evidence. No new lifecycle, upgrade, uninstall, UI, DPI or online test is included in 3.4.2 validation. The original 3.4.1 candidate files remain immutable.
