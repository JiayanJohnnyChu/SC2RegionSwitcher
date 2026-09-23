[简体中文](DEVELOPMENT.zh-CN.md)

# Development

Windows x64 and .NET SDK **10.0.401** are required; `global.json` pins the version. The application has no third-party NuGet dependencies.

## Build and checks

Commands run from the repository root:

```powershell
.\scripts\Setup.ps1
.\scripts\Build.ps1
.\scripts\Test.ps1
.\scripts\Check-Repository.ps1
.\scripts\Package.ps1
```

Setup verifies the SDK archive; `-ArchivePath <zip>` supports offline preparation. Scripts isolate and restore SDK/cache settings. Build/Test accept `-Configuration Debug`; Build/Test/Publish/Package accept `-DotNet <exe>`.

The console runner uses simulated installations. Success requires exit 0 and `TOTAL … PASSED`; an empty `dotnet test` run is not equivalent. Packaging creates files without installing an MSI or launching Battle.net.

| Additional check | Command |
| --- | --- |
| Workflow syntax | `scripts/Setup-WorkflowTools.ps1`, then `scripts/Check-Workflows.ps1` |
| Payload and hashes | `scripts/Test-Package.ps1 -ReleaseDirectory <directory>` |
| Release rejection cases | `scripts/Test-ReleaseGuards.ps1 -ReleaseDirectory <directory>` |
| Unsuppressed MSI ICE validation | `scripts/Test-InstallerSchema.ps1 -ReleaseDirectory <directory>` |
| Synthetic native interface | `scripts/Test-Ui.ps1` |

A major release receives one comprehensive pass; subsequent revisions receive checks appropriate to their changes. Documentation and package-content changes need relevant content/package checks and applicable CI, without repeating unrelated manual matrices. Repository checks precede commits.

## Application structure

| Location | Responsibility |
| --- | --- |
| `src/SC2Switcher.Wpf/Core.cs` | Installation parsing, activity checks, language transactions, recovery and Battle.net launch |
| `ConfigurationStore.cs` in the same directory | Migration, atomic saving, backups and concurrent-change protection |
| `SwissWindow*`, `SwissSettingsView*`, `SwissReferenceView*`, view models | Native WPF views, settings drafts and operation state |
| `Localization.cs`, `Strings.*.json`, `UiTypography.cs` | Independent interface language and embedded fonts |
| `tests/SC2Switcher.Tests` | Isolated console regressions |

A switch validates paths/resources and game/editor state, requests normal Battle.net exit, rechecks installations, backs up and atomically changes language keys, then launches Battle.net in the selected login region. Pending recovery runs before the next switch and verifies journal paths, backup placement and hashes. Opening the app alone does not recover. Cancellation after the target launch request does not alone roll back language changes.

`%LOCALAPPDATA%\SC2RegionSwitcherV2` holds profiles, interface preferences, backups and journals outside MSI ownership. Schema-2 Global text/speech fields store the selected pair; existing mixed pairs remain until explicit selection. Discovery intersects active Windows text/speech resources and rejects stale asynchronous results. Missing installations preserve preferences. Pending recovery blocks path/language changes but permits login-region changes.

## Interface maintenance

The responsive native interface shares colors, controls and typography; parameters and the primary action use one scroll area. Interface language remains independent of game language. All eleven catalogues require matching keys/placeholders. Embedded Inter/Noto sources and hashes are recorded in [font documentation](../src/SC2Switcher.Wpf/Fonts/README.md) and [sources.json](../src/SC2Switcher.Wpf/Fonts/sources.json).

`Test-Ui.ps1` uses isolated synthetic data and a 240-second timeout. `-Languages` limits locales, `-InteractionsOnly` runs five WPF event checks, and `-StatusOnly` captures fault layouts. Native EXE execution is required for manifest/DPI claims. `--data-dir <absolute-directory>` isolates preferences; `--ui-report <absolute-json-path>`, `--compact` and F12 provide diagnostics. Physical input, accessibility and online behavior require separate checks.

## Release

Ignored `artifacts/` holds outputs/evidence; `eng/` pins tools and hashes. A release directory contains MSI, ZIP, `release-manifest.json` and `SHA256SUMS.txt`. The package checks verify the expected payload of thirteen MSI files and fifteen ZIP files.

Accepted candidates come from successful, clean `main` CI. The manifest identifies source commit, run/attempt, version and hashes. Installation and live checks identify the original tested files. **Promote tested candidate to draft prerelease** takes the matching existing `version_tag` and accepted `candidate_run_id`, verifies provenance and attachment hashes, and never rebuilds. Incomplete acceptance keeps the draft on hold.

Every distributed preview increments the three-part numerical version. Tagged/distributed files remain immutable; duplicate numerical releases are rejected. [Installer maintenance](../tools/Installer/README.md) covers scope and upgrades. [Validation](VALIDATION.md) separates build, installation, rendering and live claims; [Changelog](../CHANGELOG.md) records public changes.
