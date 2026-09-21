# 接入 GitHub 与预发布

## 已准备的内容

- Git 仓库，主分支 `main`，目标为 `JiayanJohnnyChu/SC2RegionSwitcher` 私有仓库。
- 固定 .NET SDK 10.0.401 的本地准备脚本和 GitHub Actions 配置。
- Windows 构建、52 项隔离回归、源码内容检查、ZIP／MSI 结构及 SHA-256 校验。
- 提交和 PR 自动检查；版本标签触发的 Draft / Pre-release 工作流。
- Issue、PR 模板，版本记录和 3.3.0 预览版发布说明。

GitHub 凭据由本机凭据管理工具或 GitHub Actions 管理，不保存在项目中。源码提交不会创建公开 Release；远程构建结果应以仓库 Actions 中对应提交的运行记录为准。

## 首次连接时需要确定

1. Git 提交作者姓名和邮箱，可使用 GitHub 提供的 noreply 地址。
2. GitHub 仓库所有者、名称以及公开／私有可见性。
3. 源码许可证。尚未替用户选择许可证或添加许可文件，应在公开发布前明确。

准备工作不要求将个人访问令牌写入项目。Git 推送使用用户正常的 GitHub 登录和凭据管理；Actions 使用 GitHub 提供的临时 GITHUB_TOKEN。[自动令牌认证说明](https://docs.github.com/en/actions/security-for-github-actions/security-guides/automatic-token-authentication)。

## 首次提交与推送

新开发环境可按以下示例配置。占位符需要替换为自己的信息，不要覆盖其他开发者的作者身份。

```powershell
git config user.name "YOUR_NAME"
git config user.email "YOUR_EMAIL"
.\scripts\Check-Repository.ps1
.\scripts\Check-Workflows.ps1
.\scripts\Build.ps1
.\scripts\Test.ps1
git add .
git diff --cached --stat
git commit -m "Prepare WPF 3.3.0 preview"
```

在 GitHub 创建空仓库，不额外生成 README、许可证或 gitignore，随后添加地址并推送：

```powershell
git remote add origin https://github.com/JiayanJohnnyChu/SC2RegionSwitcher.git
git push -u origin main
```

`ci.yml` 在 main 推送、PR 和手动触发时运行。默认只有仓库读取权限，上传的是 `release` 子目录中的四个文件，不包括 SDK、构建中间文件、用户配置或本地安装记录。

## 生成预发布草稿

确认远程 CI 成功，并完成需要的实机检查后，在待发布提交上创建版本标签：

```powershell
git tag -a v3.3.0-preview.1 -m "3.3.0 preview 1"
git push origin v3.3.0-preview.1
```

`release.yml` 会重跑检查，确认标签与应用版本匹配，生成并校验以下文件：

- `SC2Switcher-3.3.0-current-user.msi`
- `SC2Switcher-3.3.0-win-x64-preview.zip`
- `release-manifest.json`
- `SHA256SUMS.txt`

工作流通过 GitHub CLI 创建 **Draft / Pre-release**，不会直接公开发布，也不会自动覆盖同名 Release。核对附件和说明后，由维护者在 GitHub 中发布草稿。只有该 job 申请 `contents: write`；本地没有保存长期 token。[GitHub CLI release create](https://cli.github.com/manual/gh_release_create)。

## 发布边界

当前仍为未签名预览版，MSI 构建器限定 3.3.0。改版时必须同时处理安装版本、组件规则、发布说明和升级验证，不能只改标签。CI 校验安装包内容但不安装程序，不模拟账号登录，也不启动游戏。

正常卸载与重新安装曾在本机基线包上验证；新 CI 包的安装、升级与在线切换结果需单独记录。D 盘离线时可以继续开发与自动测试，实机双服验证需要恢复真实安装目录。

## 本地文件与 Git 范围

`.tools`、`artifacts`、`bin`、`obj`、运行时配置和打包文件均由 .gitignore 排除。Check-Repository 是补充检查，不能代替发布前对变更内容的阅读。可以用以下命令检查本次提交范围：

```powershell
git status --short
git ls-files --cached --others --exclude-standard
```

Action 引用固定到官方仓库的具体提交，更新由 Dependabot 提出。SDK 官方下载地址与校验值在 `eng/dotnet-sdk.json` 中集中维护。[setup-dotnet](https://github.com/actions/setup-dotnet)、[upload-artifact](https://github.com/actions/upload-artifact)。
