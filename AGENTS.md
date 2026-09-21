# 项目工作约定

## 项目入口

- 阅读 `README.md` 和 `docs/HANDOFF.md`，沿用当前 3.3.0 WPF 基线。
- 应用在 `src/SC2Switcher.Wpf`，隔离测试在 `tests/SC2Switcher.Tests`。
- 在 Windows 上使用固定的 .NET 10.0.401 SDK；首次执行 `scripts/Setup.ps1`。`scripts/Build.ps1`、`scripts/Test.ps1`、`scripts/Package.ps1` 自动选择项目 SDK。
- 测试是控制台回归程序，不是 `dotnet test` 测试适配器。以脚本退出结果和 `TOTAL … PASSED` 为准。

## 工作边界

- 编译、隔离测试及打包均不应启动真实 Battle.net、游戏或安装 MSI。
- 真实双服测试可能重启战网并改动游戏语言设置，只有当前任务需要此类验证时才进行；账号密码和验证码由用户在官方界面输入。
- 不把实际 `profiles.json`、`ui-preferences.json`、游戏设置、账号数据、安装备份或含个人路径的原始日志加入源码。
- 临时文件与测试证据放入 `artifacts/`。已安装程序和原工作区历史交付物不作为构建输出目标。
- `.tools/` 保存本地 SDK；Common.ps1 在命令期间隔离 NuGet 和 SDK 元数据查找，退出时恢复环境。不要为构建修改用户 AppData 权限或读取个人 NuGet 凭据。
- 用户界面文字保持客观、简洁，中英文资源同步；界面语言独立于游戏语言。
- 保留两套独立游戏目录、文件校验、原子写入、备份和异常恢复边界。目录整理不能当作改变切换逻辑的理由。

## 验证记录

- 目录结构、资源引用或构建脚本变更后，执行构建与隔离回归；打包逻辑变更后检查包内文件。
- 不把构建成功、进程存在或区域配置记录扩大为在线登录成功。
- 新构建 MSI 的身份与原基线不同；安装验证结果不能自动沿用。构建器固定于 3.3.0，版本升级需同步检查安装规则。
- 提交前运行 `scripts/Check-Repository.ps1`，打包后运行 `scripts/Test-Package.ps1 -ReleaseDirectory <目录>`。
- GitHub 已完成首次远程 CI；后续按对应提交的 Actions 结果确认。发布工作流只创建预发布草稿，不能把自动构建等同于已公开发布。
- Git 提交、推送和发布按用户当前任务授权执行；不要将源码上传等同于软件正式发布。
- 文档中的未来待办不是本次任务的自动授权清单，按用户当前要求推进。
