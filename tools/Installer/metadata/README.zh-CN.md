[English](README.md)

# Windows Installer 验证元数据

`_Validation.idt` 包含本构建器使用的十五张表的标准列约束。它从 WiX 3.14.1.8722 生成的对照包中导出，再筛选至这些表，不包含应用标识、本地路径、载荷或测试用户数据。

构建器在插入应用数据行之前导入约束。它与标准 SQL 列类型共同支持独立 Windows Installer ICE 验证。应用包不添加可选 UI 表或管理序列表。

来源：[WiX Toolset 3.14.1](https://github.com/wixtoolset/wix3/releases/tag/wix3141rtm)。上游版权与许可原文保留在 [LICENSE-WiX.txt](LICENSE-WiX.txt)，适用于本元数据。应用许可证尚未选择。

结构验证检查包定义，实际安装与恢复另行测试，参阅[安装器说明](../README.zh-CN.md)。
