[English](README.md)

# SC2 Region Switcher — 便携包

运行要求：Windows x64、Microsoft .NET 10 Desktop Runtime x64、Battle.net，以及独立安装的国服和国际服《星际争霸 II》。国服使用中文游戏数据，国际服使用英文游戏数据。

1. 将压缩包解压到一个目录，保持四个应用文件位于同一目录。
2. 运行 `SC2Switcher.Wpf.exe`。
3. 打开**设置**，选择战网程序、两套游戏安装目录及共享的《星际争霸 II》`Variables.txt` 文件。
4. 选择目标服务；国际服还需选择战网登录区域。关闭《星际争霸 II》后执行切换，并在战网中完成所需的账号登录。
5. 启动游戏前，在战网中选择实际的《星际争霸 II》游戏服务器。切换器的 EU、US 和 KR 标识表示战网登录区域。

界面支持英文和简体中文，可在设置中选择；界面语言独立于游戏语言。

配置和备份保存在 `%LOCALAPPDATA%\SC2RegionSwitcherV2`。移动应用目录不会移动这些设置。便携包不创建开始菜单快捷方式或 Windows 卸载登记。

安装路径与操作细节见[配置指南](https://github.com/JiayanJohnnyChu/SC2RegionSwitcher/blob/main/docs/SETUP.zh-CN.md)。
