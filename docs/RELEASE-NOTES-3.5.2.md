[简体中文](RELEASE-NOTES-3.5.2.zh-CN.md)

# SC2 Region Switcher 3.5.2 preview

This version updates the interface and adds installed Global game-language selection for players using separate China and Global StarCraft II installations through one Battle.net app.

- The responsive interface keeps settings, error details and actions accessible in small windows. Settings content aligns with the footer controls, and path fields use consistent text insets.
- Eleven interface languages remain independent of game language. Embedded Inter/Noto fonts provide consistent text rendering; reduced font subsets retain the required character coverage and weights.
- Global game language can be selected from installed text/speech pairs. Corrected parsing recognizes language resources in actual installation records, and saved preferences are retained when an installation is unavailable.
- Matching region and language settings avoid unnecessary restarts.
- Navigation, saving after path edits, repair links and translated errors have been corrected.

Path validation, atomic language writes, backups and recovery remain in place. China uses `zhCN`; Global defaults to `enUS`. Switching requires completed downloads and updates.

The unsigned per-machine MSI and portable ZIP require Windows x64 and .NET 10 Desktop Runtime x64. MSI installation requires administrator approval; the app runs as an ordinary user. User configuration and backups remain separate from the application files and survive uninstall.

[Validation](VALIDATION.md) records verification scope and release acceptance status. [Setup](SETUP.md) explains configuration and use; [third-party notices](../THIRD-PARTY-NOTICES.md) cover included materials.
