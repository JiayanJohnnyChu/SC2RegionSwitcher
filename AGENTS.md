[简体中文](AGENTS.zh-CN.md)

# Repository conventions

- Repository work requires prior review of [README](README.md) and [development status](docs/HANDOFF.md). Application code is in `src/SC2Switcher.Wpf`; isolated regressions are in `tests/SC2Switcher.Tests`.
- The development environment must use Windows x64 and the pinned .NET SDK 10.0.401. SDK preparation uses `scripts/Setup.ps1`; builds, tests and packages must use the repository scripts.
- The tests use a console regression runner. Verification must include its exit code and `TOTAL … PASSED`; an empty `dotnet test` run is not equivalent.
- Build, regression and package checks must not launch Battle.net or a game, or install an MSI. Actual installation and online tests must remain separate, with their scope and exact package hashes recorded.
- Application changes must preserve independent game directories, validation, atomic writes, backups and recovery checks. Interface language must remain independent of game language.
- English and Simplified Chinese resources must remain consistent. Documentation must use paired `.md` and `.zh-CN.md` files with reciprocal language links and same-language navigation.
- Temporary outputs and raw evidence must remain under ignored `artifacts/`; local tools belong in `.tools/`. Commits must exclude real profiles, game settings, account data, backups and logs containing personal paths.
- Build scripts must isolate their tool environment and restore it afterward. Build failures must not be resolved by adding personal package credentials or changing user-directory permissions.
- Validation scope must follow the affected changes. A major application release requires one comprehensive validation pass; subsequent small revisions require targeted checks. Documentation, licensing and package-content changes require the relevant content/package checks, not a repeated installation, UI or online matrix. Necessary build/package checks and existing CI checks remain applicable. `scripts/Check-Repository.ps1` must pass before a commit.
- Compilation, installation, UI rendering and online operation must be recorded as separate validation claims. A local region record does not establish a successful login.
- Every distributed preview must increment the three-part version. Tagged or distributed versions must retain their original candidate files. Release promotion must not rebuild them.
