[English](README.md)

# Windows Installer 验证元数据

`_Validation.idt` 包含本构建器使用的十八张表的标准列约束。其中十六张表（包括 Signature）的约束从 WiX 3.14.1.8722 生成的对照包中导出；AppSearch 和 RegLocator 的约束来自标准 MSI 定义。包中包含 AppSearch 所需的空 Signature 表。

构建器在插入应用数据行之前导入约束，与 SQL 列类型共同支持 Windows Installer ICE 验证。[安装器维护](../README.zh-CN.md)说明结构检查及独立的安装、恢复测试。

来源：[WiX Toolset 3.14.1](https://github.com/wixtoolset/wix3/releases/tag/wix3141rtm)。上游版权与 Microsoft Reciprocal License（MS-RL）原文保留在 [LICENSE-WiX.txt](LICENSE-WiX.txt)。分发包同时提供许可证和元数据源码，具体范围见[第三方声明](../../../THIRD-PARTY-NOTICES.zh-CN.md)。
