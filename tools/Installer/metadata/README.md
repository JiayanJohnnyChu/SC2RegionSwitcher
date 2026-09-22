[简体中文](README.zh-CN.md)

# Windows Installer validation metadata

`_Validation.idt` contains standard column constraints for the seventeen tables used by this package builder. The original fifteen-table subset was exported from a control package generated with WiX 3.14.1.8722. Seven rows for AppSearch and RegLocator were added from standard MSI definitions and checked by the unsuppressed ICE suite. The file contains no application identity, local paths, payload or test-user data.

The builder imports these constraints before inserting application rows. Together with standard SQL column types, they support independent Windows Installer ICE validation. Optional UI and administrative sequence tables are not added to the application package.

Source: [WiX Toolset 3.14.1](https://github.com/wixtoolset/wix3/releases/tag/wix3141rtm). The upstream copyright and license are preserved in [LICENSE-WiX.txt](LICENSE-WiX.txt), which applies to this metadata. No application license has been selected.

Schema validation checks package structure. Actual installation and recovery require separate testing; see the [installer guide](../README.md).
