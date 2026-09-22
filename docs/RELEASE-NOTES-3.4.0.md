[简体中文](RELEASE-NOTES-3.4.0.zh-CN.md)

# SC2 Region Switcher 3.4.0 Preview

This candidate is retained as an unpublished private draft. Installation-failure recovery has not passed acceptance testing.

## Changes

- The interface received revised typography, spacing and page alignment, with distinct China and Global colors.
- The interface separated the current Battle.net configuration from the selected destination.
- The installer introduced current-user MSI major upgrades from 3.3.0, a stable application directory and one Start menu entry.
- The release workflow introduced version and source-provenance checks and promotion of original CI candidate files.

The interface supports English and Simplified Chinese. China uses `zhCN` game data and Global uses `enUS`; interface language is independent. Configuration requires one Battle.net app, two separate game installations and the actual shared `Variables.txt`. Account login and game-server selection remain in Battle.net. The configuration procedure is documented in [Setup](SETUP.md).

## Installation

Packages are unsigned and require Microsoft .NET 10 Desktop Runtime x64. MSI installation requires the application to be closed. Profiles, preferences, backups and recovery records are retained separately. Portable operation requires complete ZIP extraction.

The original 3.3.0 package has no downgrade protection. Earlier manually created shortcuts or App Paths entries require separate review if they still point to retired directories.

## Validation

The exact candidate identified in [Validation](VALIDATION.md) passed compilation, 52 regressions, package checks, normal installation lifecycle checks, the 100%/125%/150%/200% bilingual DPI matrix and one China → Europe → China online roundtrip with normal game exits.

Failure-path testing did not fully restore the previous installation. This unresolved installer recovery result keeps the draft on hold; it is separate from the application's language transaction recovery. Multi-display movement, other computers and different system-policy environments remain unverified. The candidate's original files are retained unchanged.
