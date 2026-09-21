[简体中文](VALIDATION-3.4.1.zh-CN.md)

# 3.4.1 validation status

Recorded 2026-09-22. Version 3.4.1 is a development preview, not a promoted release candidate. The 3.4.0 private draft remains on hold.

## Completed local checks

| Check | Result and scope |
| --- | --- |
| Compilation | Passed with zero warnings and zero errors. |
| Isolated regressions | All 52 passed. |
| Package validation | Package content and all five release rejection cases passed. |
| Independent MSI schema validation | Full standard ICE suite, no suppressions, zero errors. Four ICE91 warnings concern hypothetical per-machine use of fixed per-user directories; the package rejects ALLUSERS. |
| Basic UI | English and Simplified Chinese, standard 760 × 620 window at 200% scaling. EU → US → KR selection updated the Global card while current-China status remained unchanged; relevant controls stayed visible. |

The Global card now identifies the selected Battle.net login region. `Core.cs` and `ConfigurationStore.cs` are unchanged. No 3.4.1 online game roundtrip or complete four-scale DPI matrix is recorded. Those completed tests belong to the [original 3.4.0 candidate](VALIDATION.md).

## Development CI

Commit `68d5d31f823de5992eb331b7bf07803557b80bc5` passed [CI run 35663508108](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35663508108), including build, regressions and package checks. Documentation was still under revision, so this run is recorded as development validation rather than the final release candidate.

## Installer recovery acceptance

Installation-failure recovery has not passed in the standard-user test environment. A separate installer control reproduced the failure; a comparison under a different privilege context completed recovery. This establishes an environment-dependent observation, not a resolved cause or a successful production upgrade.

The dedicated CI recovery test uses isolated product identities to test a native file-copy failure after old-product removal. In [run 35663705665](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/actions/runs/35663705665), the Windows Server runner policy rejected baseline MSI installation for the fresh standard-user account with error 1625. The rollback scenario was never reached, so this is a blocked test rather than a rollback result. Test account, profile and working-directory cleanup checks passed, and the input MSI hash remained unchanged. Schema validation and normal installation success do not test this failure path.

The next candidate requires a clean CI source revision, original package hashes, and installation lifecycle evidence against those exact files. Local package checks do not transfer to subsequently rebuilt candidates.
