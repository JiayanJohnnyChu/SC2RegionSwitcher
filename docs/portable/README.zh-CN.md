[English](README.md)

# SC2 Region Switcher — 便携包

应用运行需要 Windows x64、Microsoft .NET 10 Desktop Runtime x64、一套战网，以及独立安装的国服和国际服《星际争霸 II》。国服文本与语音使用简体中文，国际服使用英文。

1. 便携部署要求完整解压到独立目录，四个应用文件必须保持在同一目录。
2. 应用入口为 `SC2Switcher.Wpf.exe`。
3. **设置**指定 `Battle.net.exe` 所在文件夹、两套游戏安装目录，以及高级选项中的实际共享 `Variables.txt` 文件。**保存并检查**操作验证并保存此配置。
4. 切换要求《星际争霸 II》和地图编辑器已关闭，战网下载或更新已完成。目标为国服或国际服，国际服另需战网登录区域。切换操作打开目标区域，所需登录在战网中完成。
5. 游戏启动以战网中的实际《星际争霸 II》游戏服务器选择为前提。切换器的 EU、US 和 KR 标签表示登录区域。

设置提供英文与简体中文。界面语言独立于游戏语言，切换期间没有取消操作。

配置和备份保存在 `%LOCALAPPDATA%\SC2RegionSwitcherV2`。移动应用目录不会移动这些设置。便携包不创建开始菜单快捷方式或 Windows 卸载登记。

原创项目代码、文档和图标采用 `LICENSE` 中的 MIT 许可证。包中另含双语 `THIRD-PARTY-NOTICES` 文件及 `licenses/` 目录，包括 Radix 许可证和 WiX 许可证与元数据源码。这些文件属于解压后的分发内容。

本包为未签名预览版。操作细节和实测范围记录于[配置指南](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/blob/main/docs/SETUP.zh-CN.md)和[验证记录](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/blob/main/docs/VALIDATION.zh-CN.md)。仓库和[发布下载](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/releases/tag/v3.4.2-preview.1)可公开访问。
