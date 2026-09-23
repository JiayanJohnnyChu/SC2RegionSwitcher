[English](README.md)

# 内嵌字体

应用内嵌十五个字体文件，共 28.38 MiB。Inter 4.001 分别命名为 **Switcher Sans**（400/500/600/700，光学字号 14）和 **Switcher Display**（650 字形以 600 字重注册，光学字号 32）。Noto Sans SC、KR 2.004 分别生成 **Switcher Han** 和 **Switcher Hangul**，各含 400/500/600/700 字重及独立的 Switcher Han Display、Switcher Hangul Display 标题字体。标题字体保留独立的 650 字形轮廓，以原生 600 字重注册供 WPF 使用。

五个 Inter 文件及两个 CJK Regular 400 文件保留完整字符覆盖。八个较粗的 CJK 字体采用子集：

- 500 字重保留中文 GB2312 字符集，以及韩文 EUC-KR 双字节 A1–FE 字符集，后者包括 2,350 个常用韩文音节及常用汉字、符号。
- 600、650 标题和 700 字重保留当前界面字符。
- 所有子集还保留当前界面字符和原字体中汉字、韩文音节以外的字符，包括韩文字母、拉丁字母、希腊字母、西里尔字母、假名及标点。

保留字形的轮廓、字重、微调指令、前进宽度和行度量，以及字体名称与版权／许可元数据。中文和韩文句子使用对应字体。路径使用 Consolas，并以内嵌 Han／Hangul Regular 回退；正文和错误详情使用 400 字重。超出完整内嵌字体覆盖的字符由 Windows 字体回退处理。

## 生成

以下命令在仓库根目录运行，需要 fontTools 4.65.0 及 WOFF 支持。[Prepare-SwissFonts.py](../../../scripts/Prepare-SwissFonts.py) 先在独立目录生成完整静态字体，再由 [Subset-SwissFonts.py](../../../scripts/Subset-SwissFonts.py) 生成分发字体：

```text
python scripts/Prepare-SwissFonts.py --inter <InterVariable.woff2> --sc <NotoSansSC.ttf> --kr <NotoSansKR.ttf> --output <full-static-dir>
python scripts/Subset-SwissFonts.py --input <full-static-dir> --output <subset-dir>
python scripts/Subset-SwissFonts.py --check src/SC2Switcher.Wpf/Fonts
```

输入文件须与 sources.json 中记录的版本和哈希一致。新增界面字符不在子集中时需要重新生成。常规构建直接使用仓库中的字体。

[来源清单](sources.json)记录上游版本、地址及哈希。[字体清单](manifest.json)记录生成文件哈希，子集条目另含 `subset`、`fullSha256` 和 `fullCharacters`；500 字重的 `subset` 策略为 `gb2312+ui` 或 `euc_kr+ui`，更高字重为 `ui`。`glyphs` 字段统计 Unicode cmap 条目数。原始 [Inter](../../../licenses/LICENSE-Inter.txt)、[Noto SC](../../../licenses/LICENSE-NotoSC.txt) 和 [Noto KR](../../../licenses/LICENSE-NotoKR.txt) 许可证随分发包提供。
