[简体中文](UI-DESIGN.zh-CN.md)

# Interface design and localization

The interface is native WPF. Its main page separates the current local Battle.net configuration, the selected destination and the operation result. Version 3.4.1 uses EU, US or KR on the Global destination card to identify the selected login region; China uses CN. Game-server selection remains in Battle.net.

## Language behavior

- Missing, unreadable or unsupported preferences fall back to `en-US`. Valid `en-US` and `zh-CN` preferences are preserved.
- Settings applies and saves interface language immediately. Path changes require **Save & check**.
- Interface language is independent of China `zhCN` and Global `enUS` game-language profiles.
- English uses Segoe UI; Chinese uses Microsoft YaHei UI. Chinese display headings use regular weight.
- Both JSON catalogs have matching keys and format placeholders, checked by the regression runner.

## Layout and colors

China and Global destinations sit side by side. A selected card has a solid region color, opaque white text and a check mark. An unselected card retains its identity color in its small label and selection ring. Global region controls share a baseline below the cards. The main action stays visible at the bottom.

Settings and reference pages use the same surface, typography, separators and footer alignment. Long form and reference content scrolls while bottom actions remain fixed. Compact layouts retain both destinations, all three Global login regions and the main action; text wrapping handles narrow windows.

| Token | Value |
| --- | --- |
| Paper / input | Sand 2 `#F9F9F8` / Sand 1 `#FDFDFC` |
| Primary / secondary text | Sand 12 `#21201C` / Sand 11 `#63635E` |
| Separator / soft surface | Sand 7 `#CFCECA` / Sand 3 `#F1F0EF` |
| China / Global | Tomato 11 `#D13415` / Indigo 9 `#3E63DD` |
| Selected card text | `#FFFFFF`, fully opaque |
| Standard / minimum window | 760 × 620 / 520 × 560 DIP |
| Body / supporting text | 13–14 / 11–12 DIP |
| Page / destination title | 30 / 24 DIP; main heading reduces to 26 at narrow widths |
| Input / primary action height | 40 / 46 DIP |
| Content margins | 32 DIP normally, 22 on the compact main view; forms reduce from 32 to 22 in compact windows |

Colors are defined in `src/SC2Switcher.Wpf/Palette.xaml` and merged by `App.xaml`. The sRGB values come from [Radix Colors](https://github.com/radix-ui/colors/blob/main/src/light.ts); their assignment to application roles is specific to this project. No Radix framework or web runtime is included.

White text on Tomato 11 has approximately 4.98:1 contrast; white on Indigo 9 has approximately 5.21:1. Selected labels and their parent cards retain full opacity while busy. These values describe those color pairs, not overall accessibility conformance.

## State and interaction

`CurrentLoginRegion` changes through `SetCurrent`. Destination selection, Global-region preference and interface-language changes do not recolor the current-configuration marker. Unknown current regions use a neutral color. Status messages include a text symbol as well as color.

Checking and switching disable the relevant actions; there is no cancellation control. Unsaved settings block Back, Escape and window closing until saved or discarded. Saving locks path, language and navigation controls, and invalid input leaves the saved configuration intact. Reference pages restore keyboard focus on return; error details remain selectable in full.

## Diagnostics and coverage

`--ui-report <absolute-json-path>` writes layout facts and a WPF self-rendered image. F12 captures the current page; `--compact` requests minimum dimensions. Binding errors are logged alongside reports when present.

`--matrix` exports both languages across normal, narrow, busy, error, success, settings, unsaved-edit and reference states. It requires an explicit isolated `--data-dir` and disables real switching and path saving during the presentation. Synthetic reports set `SyntheticState=true`.

For DPI validation, launch the EXE so its PerMonitorV2 manifest applies. Self-rendering and external window capture are separate observations. Store raw images and reports under ignored `artifacts/` directories.

The complete 100%, 125%, 150% and 200% bilingual matrix belongs to the original 3.4.0 candidate. Version 3.4.1 has basic bilingual label checks at 200% in the standard window. Multi-display movement and cross-computer behavior remain unverified. See [Validation](VALIDATION.md).
