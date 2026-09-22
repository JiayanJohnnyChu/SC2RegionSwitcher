[简体中文](VALIDATION.zh-CN.md)

# Validation record

Updated 2026-09-22. Results apply to the identified revision and package hashes. Compilation, package structure, installation, interface rendering and online use are recorded separately.

## 3.4.1 development preview

The administrator-required, per-machine MSI passed local compilation with zero warnings/errors, all 52 regressions, package validation, eight release rejection cases and the full unsuppressed ICE suite with zero warnings/errors. CI candidate and lifecycle acceptance remain pending. Earlier basic bilingual UI checks and current-user recovery diagnostics are recorded separately. This version is not a promoted candidate. Detailed scope is in [3.4.1 validation](VALIDATION-3.4.1.md).

The full DPI matrix and online roundtrip below belong to 3.4.0, not 3.4.1.

## 3.4.0 candidate

Source commit `b3c37fce22da78db7e6865df37818eb30f5708dc` passed [CI run 35649099018](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35649099018). The tested MSI SHA-256 is:

```text
73344512F949FC4ECB851279618E0F63D1B88BD5577AC5C7BEA558D19CC59C9A
```

This candidate remains an unpublished private draft.

| Area | Verified scope |
| --- | --- |
| Build and regressions | Release compilation with zero warnings/errors; all 52 isolated regressions passed. |
| Packages | Four-file release payload, manifest provenance, hashes, ZIP/MSI runtime-file identity, upgrade tables, current-user components and one Start menu shortcut. |
| Normal installation lifecycle | Clean installation, repeated invocation of the same MSI, uninstall/reinstall, direct upgrade from the original 3.3.0 package, an occupied-executable case, and rejection when a higher product-family version was present. |
| Direct 3.3.0 upgrade | One application registration and one Start menu entry remained. The four installed runtime files matched the candidate. The old product and versioned application directory were removed; thirteen existing state and backup files retained their hashes. |
| DPI and localization | 100%, 125%, 150% and 200% groups reported 96, 120, 144 and 192 DPI. Each group contained 33 synthetic reports and one actual local-window report. Primary actions, destinations and regions remained visible; no WPF binding or layout error was recorded. |
| Online use | One China → Europe → China roundtrip reached the corresponding official game main interfaces, with normal game exits. |

### Limits and unresolved checks

Failure tests did not fully restore the previous installation in the standard-user test environment. Installation recovery therefore remains an unresolved release gate, separate from the application's game-language transaction recovery.

Manual shortcuts or App Paths entries from earlier manual deployments can point to retired directories outside MSI ownership. Their migration is separate from a clean MSI-to-MSI upgrade.

The DPI tests used one active display. Multi-display movement, other computers and different system-policy environments remain unverified. Packages are unsigned.

## Earlier records

The original 3.3.0 package used a versioned application directory and had no major-upgrade or downgrade-protection design.

The preceding interface/localization revision, commit `2cca07ad1698ec81e552f48679da1b31958a5fed`, passed [CI run 35644370416](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35644370416). The initial remote CI record is commit `5886efa9bf49312a84b59ac7e98ddc4dda6100f1`, [run 35629504891](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35629504891), covering build, regressions and package checks without MSI installation.

## Evidence interpretation

Raw candidates, screenshots, logs and configuration fixtures remain under ignored `artifacts/` directories. Shared records retain source revisions, CI links, package hashes and conclusions.

Synthetic reports carry `SyntheticState=true`. They describe presentation states; a process launch or a local region record likewise does not establish an authenticated game session. Source changes and rebuilt packages require reassessment of the affected checks.
