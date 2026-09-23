[English](README.md)

# SC2 Region Switcher — 便携包

应用运行需要 Windows x64、Microsoft .NET 10 Desktop Runtime x64、一套战网，以及独立安装的国服和国际服《星际争霸 II》。国服文本与语音使用简体中文，国际服使用所选的已安装语言。

1. 便携部署要求完整解压到独立目录，所有附带应用文件、声明和许可证均保留在解压目录中。
2. 应用入口为 `SC2Switcher.Wpf.exe`。
3. **设置**指定 `Battle.net.exe` 所在文件夹、两套游戏安装目录，以及**高级 · 共享游戏设置**中的实际共享 `Variables.txt` 文件。**保存并检查**操作验证并保存此配置。
4. 切换要求《星际争霸 II》和地图编辑器已关闭，战网下载、更新或修复已完成。目标为国服或国际服，国际服另需战网登录区域。切换操作打开目标区域，所需登录在战网中完成。
5. 游戏启动以战网中的实际《星际争霸 II》游戏服务器选择为前提。切换器的 EU、US 和 KR 标签表示登录区域。

设置中的**外服游戏语言（实验性）**可为文字和语音统一选择语言，列表包含安装记录中同时具备文字和语音资源的语言。新增资源需通过战网安装。游戏所在磁盘不可用时，选择器禁用，已保存的偏好保持不变。**保存并检查**保存偏好，游戏设置仅在切换时修改。旧有混合组合在选择统一语言前保留。待恢复事务必须先使用原配置完成恢复，才能修改游戏语言。

顶栏和设置提供英语、简体中文、法语、德语、荷兰语、韩语、意大利语、西班牙语、欧洲葡萄牙语、拉丁语和现代希腊语界面，独立于游戏语言。切换期间没有取消操作。

配置和备份保存在 `%LOCALAPPDATA%\SC2RegionSwitcherV2`。移动应用目录不会移动这些设置。便携包不创建开始菜单快捷方式或 Windows 卸载登记。

原创项目代码、文档和图标采用 `LICENSE` 中的 MIT 许可证。包中另含双语 `THIRD-PARTY-NOTICES` 文件及 `licenses/` 目录，包括 Inter 与 Noto 字体许可证、Radix 许可证和 WiX 许可证与元数据源码。这些文件属于解压后的分发内容。

便携发行包未签名。操作细节和实测范围记录于[配置指南](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/blob/main/docs/SETUP.zh-CN.md)和[验证记录](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/blob/main/docs/VALIDATION.zh-CN.md)。已发布版本的下载见[发布页](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/releases)。
