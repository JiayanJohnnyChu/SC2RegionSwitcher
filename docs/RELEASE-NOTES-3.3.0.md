[简体中文](RELEASE-NOTES-3.3.0.zh-CN.md)

# SC2 Region Switcher 3.3.0 Preview

Historical preview for Windows x64, built with WPF and .NET 10. It provided separate China and Global game paths, configurable Battle.net and shared-settings locations, and an interface available in English and Simplified Chinese.

China uses Simplified Chinese text and speech; Global uses English. Interface language is selected independently. Login and game startup take place in the official Battle.net app. Runtime packages require Microsoft .NET 10 Desktop Runtime x64.

The unsigned distribution included a current-user MSI, a portable ZIP, release metadata and SHA-256 values. The MSI created one Start menu entry and used a versioned application directory. It did not include major-upgrade handling or downgrade protection. When deliberately returning to this historical package, uninstall the newer application first; user configuration is stored separately and is retained.

For current configuration instructions, see [Setup](SETUP.md). Later changes are listed in the [Changelog](../CHANGELOG.md).
