[简体中文](AGENTS.zh-CN.md)

# Repository conventions

- Read [README](README.md) and [development status](docs/HANDOFF.md). Application code is in `src/SC2Switcher.Wpf`; isolated regressions are in `tests/SC2Switcher.Tests`.
- Use Windows x64 and the pinned .NET SDK 10.0.401. Prepare it with `scripts/Setup.ps1`; use the repository's build, test and package scripts.
- The tests are a console regression runner. Check the exit code and `TOTAL … PASSED`; an empty `dotnet test` run is not equivalent.
- Build, regression and package checks must not launch Battle.net or a game, or install an MSI. Keep real installation and online tests separate, with their scope and exact package hashes recorded.
- Preserve independent game directories, validation, atomic writes, backups and recovery checks when changing the application. Interface language remains independent of game language.
- Keep English and Simplified Chinese resources consistent. Documentation uses paired `.md` and `.zh-CN.md` files with reciprocal language links and same-language navigation.
- Store temporary outputs and raw evidence under ignored `artifacts/`; local tools belong in `.tools/`. Do not commit real profiles, game settings, account data, backups or logs containing personal paths.
- Build scripts isolate their tool environment and restore it afterward. Do not add personal package credentials or change user-directory permissions to make a build pass.
- Run appropriate build and regression checks for source changes, and package checks for packaging changes. Run `scripts/Check-Repository.ps1` before committing.
- Treat compilation, installation, UI rendering and online operation as separate validation claims. A local region record does not establish a successful login.
- Increase the three-part version for every distributed preview. A tagged or distributed version retains its original candidate files. Release promotion does not rebuild them.
