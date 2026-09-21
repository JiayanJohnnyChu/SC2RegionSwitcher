[English](README.md)

# SC2 Region Switcher — 便携包

运行需要 Windows x64、Microsoft .NET 10 Desktop Runtime x64、一套战网，以及独立安装的国服和国际服《星际争霸 II》。国服文本与语音使用简体中文，国际服使用英文。

1. 将压缩包完整解压到独立目录，保持四个应用文件在同一目录。
2. 运行 `SC2Switcher.Wpf.exe`。
3. 在**设置**中选择包含 `Battle.net.exe` 的文件夹、两套游戏安装目录，并在高级选项中选择实际共享的 `Variables.txt`，点击**保存并检查**。
4. 关闭《星际争霸 II》和地图编辑器，等待战网下载、更新结束。选择目标；国际服还需选择战网登录区域。开始切换，并在战网中完成所需登录。
5. 启动游戏前，在战网中选择实际《星际争霸 II》游戏服务器。切换器的 EU、US 和 KR 标签表示登录区域。

可在设置中选择英文或简体中文，界面语言独立于游戏语言。切换期间没有取消操作。

配置和备份保存在 `%LOCALAPPDATA%\SC2RegionSwitcherV2`。移动应用目录不会移动这些设置。便携包不创建开始菜单快捷方式或 Windows 卸载登记。

本包为未签名预览版。操作细节和实测范围见[配置指南](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/blob/main/docs/SETUP.zh-CN.md)和[验证记录](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/blob/main/docs/VALIDATION.zh-CN.md)。仓库为私有时，查看这些页面需要仓库访问权限。
