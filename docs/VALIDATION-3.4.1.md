[简体中文](VALIDATION-3.4.1.zh-CN.md)

# 3.4.1 validation status

Recorded 2026-09-22. Version 3.4.1 is a development preview, not a promoted release candidate. The 3.4.0 private draft remains on hold.

## Current machine installer

The adopted design requires administrator approval and installs for all users in 64-bit Program Files, with HKLM App Paths and one common Start menu entry. The application stays `asInvoker`; each user's configuration and backups remain outside MSI ownership. Earlier current-user previews require a one-time uninstall/reinstall. See [Installer](../tools/Installer/README.md).

The local development machine package passed these checks:

| Check | Result |
| --- | --- |
| Compilation | Zero warnings and zero errors |
| Isolated regressions | `TOTAL 52 PASSED` |
| Package validation | Passed |
| Release rejection cases | All seven passed |
| Independent ICE validation | Zero errors, zero warnings, no suppressions |

Schema evidence is retained at `artifacts/installer-schema/3f747e8a932b420b8ceb3a973c08e684/result.json`. These are local development-package results, not CI candidate or installation results.

CI lifecycle acceptance remains pending. The `installer-lifecycle.yml` workflow and `tools/Installer/Test-MachineInstall.ps1` exercise isolated machine installation, maintenance, upgrade, failed-upgrade recovery and uninstall. Candidate provenance, hashes, legacy-preview detection and preserved user data must be recorded against the tested files. No passing lifecycle result for the new machine package is recorded here yet.

Interactive installation from a standard desktop through UAC and subsequent ordinary-user launch remain pending. An elevated CI run does not establish this interactive path. Follow-up application coverage is limited to basic launch and bilingual UI checks unless changed behavior requires broader testing. The completed full UI/online round remains attributed to 3.4.0; no new online roundtrip is claimed.

## Historical current-user evidence

The results below predate the machine-scope change. The administrator comparison is evidence about the old current-user package under that privilege context, not proof that the new machine package passes.

### Local checks

| Check | Result and scope |
| --- | --- |
| Compilation | Passed with zero warnings and zero errors. |
| Isolated regressions | All 52 passed. |
| Package validation | Package content and all five release rejection cases passed. |
| Independent MSI schema validation | Full standard ICE suite, no suppressions, zero errors. Four ICE91 warnings concern hypothetical per-machine use of fixed per-user directories; the package rejects ALLUSERS. |
| Basic UI | English and Simplified Chinese, standard 760 × 620 window at 200% scaling. EU → US → KR selection updated the Global card while current-China status remained unchanged; relevant controls stayed visible. |

The Global card now identifies the selected Battle.net login region. `Core.cs` and `ConfigurationStore.cs` are unchanged. No 3.4.1 online game roundtrip or complete four-scale DPI matrix is recorded. Those completed tests belong to the [original 3.4.0 candidate](VALIDATION.md).

### Development CI

Commit `d3a179bbbb1ec55f9850f1c0846b79d126ffac4d` passed [CI run 35667524580](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35667524580), including compilation, regressions and package checks. This candidate predates the adopted machine installation route.

### Current-user recovery diagnostics

#### Paired privilege-context comparison

The [administrator-context run 35668661867](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35668661867) and [standard-user run 35668664128](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35668664128) used the same original candidate and test harness. Both ran on hosted Windows 11 ARM64, build 26200, with Windows Installer 5.0.26100.9444. Isolated product identities kept these tests separate from the production installation. They exercised installer behavior, not application or game operation.

| Evidence | Identity |
| --- | --- |
| Candidate source | `d3a179bbbb1ec55f9850f1c0846b79d126ffac4d` |
| Recovery workflow and harness | `f512175ef8697632cd240a2df1722ed4c3e8a406` |
| Original MSI SHA-256 | `A8C62B46FFA4DF577D6E64CE1EEA1A36025D0012F1D2D530D25A8FE0DACFB89A` |

Both tests reached the intended native file-copy failure, error 1312, after old-product removal.

| Observation | Administrator context | Standard user |
| --- | --- | --- |
| Registry operations reporting error 5 (access denied) | 0 | 81 |
| Actual rollback failures | 0 | 80 |
| Old product `ProductState` | 5 → 5, remained installed | 5 → 1, changed from installed to advertised |
| Candidate `ProductState` | -1, absent | -1, absent |
| Restored resources | All four old file hashes, shortcut, App Paths and authored markers matched; the configuration test file remained unchanged | Visible old application resources returned, but product registration was not restored |
| MSI cleanup | All resource-removal checks passed | Uninstall returned success, but old files, menu entry, App Paths and markers remained; cleanup assertions failed |

After the standard-user test, removal of the disposable account, profile and working directory completed. The administrator test also completed its cleanup. Both runs left the original input MSI unchanged. The separate cleanup steps do not convert the failed MSI cleanup into a passing result.

The results strongly associate the recovery failure with privilege context in this environment. They do not establish a specific root cause or an officially confirmed Windows defect. The administrator result is a diagnostic comparison, not acceptance of standard-user recovery. Normal MSI action-end return values were excluded from the rollback-failure count.

#### Earlier evidence

The complete code and documentation revision `a3d0cf55acb694f31f3c3b3eccef8adb0978339b` passed [CI run 35664932110](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35664932110). Its Windows 11 ARM64 [standard-user recovery run 35666330406](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35666330406) independently reproduced the same installed-to-advertised transition after the native failure. The earlier Windows Server [run 35663705665](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35663705665) stopped at baseline installation with policy error 1625 and did not enter rollback.

#### Isolated authoring probes

Local metadata prototypes using `WordCount = 2` alone, or `ALLUSERS = 2`, `MSIINSTALLPERUSER = 1` and `WordCount = 2` together, did not resolve the failure. The combined prototype still recorded registry access-denied events.

A separate prototype combined `WordCount = 2` with an explicit `Privileged` launch condition. Its quiet standard-user probe rejected installation before `InstallInitialize`, with no registry rollback errors or installed test resources. This tests early rejection, not successful recovery.

A separately built guarded MSI using the unchanged application payload from CI run 35664932110 passed the full unsuppressed ICE suite with zero errors and four expected ICE91 warnings. Validation left its bytes unchanged. That package has not been installed, tested through the full lifecycle or promoted. The guard has not been adopted in production authoring.

Raw packages and diagnostic records remain under ignored `artifacts/validation/3.4.1/` directories.

## Release status

The release remains on hold. These diagnostics did not change application code, install the production 3.4.1 package locally, or publish a release. The administrator-required machine route is now defined; acceptance still requires complete lifecycle evidence for its exact candidate files. Schema validation, early rejection and normal installation success do not establish recovery after a failed upgrade.
