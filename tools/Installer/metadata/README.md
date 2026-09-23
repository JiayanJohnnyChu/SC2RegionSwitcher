[简体中文](README.zh-CN.md)

# Windows Installer validation metadata

`_Validation.idt` contains standard column constraints for the eighteen tables used by the package builder. Constraints for sixteen tables, including Signature, were exported from a control package generated with WiX 3.14.1.8722. AppSearch and RegLocator constraints come from standard MSI definitions. The package includes the empty Signature table required by AppSearch.

The builder imports these constraints before application rows. Together with SQL column types, they support Windows Installer ICE validation. [Installer maintenance](../README.md) covers schema checks and separate installation/recovery testing.

Source: [WiX Toolset 3.14.1](https://github.com/wixtoolset/wix3/releases/tag/wix3141rtm). The upstream copyright and Microsoft Reciprocal License (MS-RL) are preserved in [LICENSE-WiX.txt](LICENSE-WiX.txt). Distribution includes both the license and metadata source, as specified in [third-party notices](../../../THIRD-PARTY-NOTICES.md).
