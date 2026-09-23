[简体中文](README.zh-CN.md)

# Embedded fonts

The application embeds fifteen font files totaling 28.38 MiB. Inter 4.001 is provided as **Switcher Sans** (400/500/600/700, optical size 14) and **Switcher Display** (650 outlines registered as 600, optical size 32). Noto Sans SC and KR 2.004 provide **Switcher Han** and **Switcher Hangul**, each with 400/500/600/700 weights and a separate display family, Switcher Han Display or Switcher Hangul Display. These families preserve distinct 650 outlines and register as native weight 600 for WPF.

All five Inter files and both CJK Regular 400 files retain full coverage. The eight heavier CJK files are subsets:

- Weight 500 retains GB2312 for Chinese and the EUC-KR two-byte A1–FE repertoire for Korean, including 2,350 common Hangul syllables and common Hanja/symbols.
- Weights 600, 650 display and 700 retain current UI characters.
- Every subset also retains current UI characters and the original non-Han/non-Hangul repertoire, including Jamo, Latin, Greek, Cyrillic, kana and punctuation.

Retained glyph outlines, weights, hinting, advance widths and line metrics are preserved, as are font names and copyright/license metadata. Chinese and Korean sentences use their respective families. Paths use Consolas with embedded Han/Hangul Regular fallback; body text and detailed errors use weight 400. Characters outside full embedded-font coverage rely on Windows fallback.

## Generation

Commands run from the repository root with fontTools 4.65.0 and WOFF support. [Prepare-SwissFonts.py](../../../scripts/Prepare-SwissFonts.py) generates full static fonts into a separate directory. [Subset-SwissFonts.py](../../../scripts/Subset-SwissFonts.py) then creates the distributable set:

```text
python scripts/Prepare-SwissFonts.py --inter <InterVariable.woff2> --sc <NotoSansSC.ttf> --kr <NotoSansKR.ttf> --output <full-static-dir>
python scripts/Subset-SwissFonts.py --input <full-static-dir> --output <subset-dir>
python scripts/Subset-SwissFonts.py --check src/SC2Switcher.Wpf/Fonts
```

Input files must match the versions and hashes in sources.json. New UI characters missing from a subset require regeneration. Normal builds use checked-in fonts.

[Sources](sources.json) records upstream versions, URLs and hashes. [The manifest](manifest.json) records generated hashes; subset entries add `subset`, `fullSha256` and `fullCharacters`. The `subset` policy is `gb2312+ui` or `euc_kr+ui` for weight 500 and `ui` for higher weights. Its `glyphs` field counts Unicode cmap entries. Original [Inter](../../../licenses/LICENSE-Inter.txt), [Noto SC](../../../licenses/LICENSE-NotoSC.txt) and [Noto KR](../../../licenses/LICENSE-NotoKR.txt) licenses accompany distributions.
