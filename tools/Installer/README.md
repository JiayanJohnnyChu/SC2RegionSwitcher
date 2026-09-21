# 当前用户 MSI 构建器

推荐从项目根目录执行 `scripts/Package.ps1`。该脚本先发布 WPF，再生成 ZIP、MSI 和校验值，不自动安装。

如只需封装已有运行文件：

```powershell
.\tools\Installer\Build-CurrentUserMsi.ps1 `
  -AppDirectory 'C:\Build\PublishedApp' `
  -OutputDirectory 'C:\Build\NewMsi'
```

输入目录必须包含四个 3.3.0 运行文件，输出目录必须为空。默认读取 `artifacts/publish/win-x64` 和 `assets/icon/switcher.ico`，输出到 `artifacts/packages/msi`。构建器使用 Windows 自带的 Windows Installer COM 与 `makecab.exe`，不下载依赖。

安装范围为当前用户。程序放入 `%LOCALAPPDATA%\Programs\SC2RegionSwitcher\3.3.0`，创建一个 **SC2 Region Switcher** 开始菜单入口，并登记卸载信息。没有桌面入口、游戏文件或用户配置；不捆绑 .NET Desktop Runtime。唯一自定义操作是 Type 51 属性赋值，没有可执行或脚本自定义操作。

本构建器固定于 3.3.0，ProductCode、ComponentCode 和文件版本均固定，每次生成新 PackageCode。升级开发需要设计并验证组件规则和版本处理，不能只修改 MSI 文件名。当前没有自动升级流程；不要强行修复不同来源的同版本重建包，也不要降低 Windows Installer 安全策略。

本机安装验证对应 `artifacts/baseline/3.3.0` 中的原件。新打包文件只表示构建成功，安装测试须另外记录。测试新包时应先正常退出应用、卸载旧包，再安装；卸载设计上保留用户配置。
