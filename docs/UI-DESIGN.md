# English-first interface

This is the working UI revision of SC2 Region Switcher 3.4.0. The design uses a shared grid, deliberate type hierarchy, distinct China/Global colors and a neutral Sand surface. It remains a native WPF application.

## Language policy

- Develop the English copy and layout first. Maintain Simplified Chinese alongside every user-facing change.
- A missing, unreadable or unsupported interface preference falls back to `en-US`. A valid saved `zh-CN` or `en-US` preference is preserved.
- Settings applies and saves the interface language immediately. Directory changes still require **Save & check**.
- Interface language never changes the China `zhCN` or Global `enUS` game-language profiles.
- English uses Segoe UI; Chinese uses Microsoft YaHei UI. Chinese display headings use regular weight to avoid substituting heavy bold for Latin semibold.
- Existing resource keys remain stable. Both JSON catalogs must have matching keys and format placeholders; the regression runner checks their translations.

## Visual and interaction structure

The main view has three responsibilities: show the current local configuration, select a destination, and report the operation. Current configuration is a compact, separate block. The two destinations sit side by side; China uses Tomato 11 and Global uses Indigo 9. The selected option has a solid region color, opaque white text and a check mark. An unselected option retains its region color in its small label and selection ring. Global login regions use a shared baseline below them. The action area stays visible at the bottom.

Settings and reference pages share the same paper surface, typography, separators and footer alignment. Inputs, language selection, expansion and vertical scrolling have WPF templates. The original application icon remains unchanged.

| Token | Value |
| --- | --- |
| Paper / input surface | Sand 2 `#F9F9F8` / Sand 1 `#FDFDFC` |
| Primary / secondary text | Sand 12 `#21201C` / Sand 11 `#63635E` |
| Separator / soft surface | Sand 7 `#CFCECA` / Sand 3 `#F1F0EF` |
| China identity | Tomato 11 `#D13415` |
| Global identity | Indigo 9 `#3E63DD` |
| Selected region text | Opaque white `#FFFFFF` |
| Standard window | 760 × 620 DIP |
| Minimum window | 520 × 560 DIP |
| Body / supporting text | 13–14 / 11–12 DIP |
| Page / destination title | 30 / 24 DIP; main heading reduces to 26 at narrow widths |
| Input / primary action height | 40 / 46 DIP |
| Content margins | 32 DIP normally, 22 on the compact main view, 32 on forms, reducing to 22 in compact windows |

The compact main layout retains both destinations, all three Global login regions and the primary action. Forms and reference content may scroll; their bottom actions remain fixed. Do not replace wrapping with whole-window scaling.

## Region palette

Colors are maintained in `src/SC2Switcher.Wpf/Palette.xaml`, a WPF resource dictionary merged by `App.xaml`. The sRGB values come from [Radix Colors](https://github.com/radix-ui/colors/blob/main/src/light.ts); the combination and region assignment are specific to this app. No Radix UI framework or web runtime is included.

Tomato 11 is deliberately used as a solid background to support the small white labels: white-on-Tomato-11 is approximately 4.98:1 and white-on-Indigo-9 is 5.21:1. Selected labels and their parent tile retain full opacity, including while busy. These ratios describe the specified text/background pairs, not an accessibility certification for the application.

The current-configuration marker uses `CurrentLoginRegion`, which is updated only by `SetCurrent`. Selecting another target, changing the preferred Global region or changing interface language cannot recolor the current configuration. Unknown current regions use the neutral separator color. General buttons, focus rings, forms and progress remain neutral; status messages retain their own meanings and now include a text symbol as well as color.

## Behavior retained

- Local configuration is not online account status. A completed switch means the target Battle.net was opened and its local region record checked.
- Checking or switching disables the relevant actions. There is no user cancellation control.
- A dirty settings page blocks Back, Escape and window close until the user saves or discards changes.
- Saving locks language, paths, browse, discard and navigation. Invalid input does not replace the previous saved configuration.
- Reference pages restore keyboard focus when returning. Error details remain selectable and available in full.
- The switch engine and configuration transaction code are unchanged by this UI revision.

## Validation and screenshots

Run `scripts/Build.ps1` and `scripts/Test.ps1` for compilation and the isolated regression suite. Real UI interaction is separate from these tests.

The existing `--ui-report <absolute-json-path>` diagnostics write layout facts and a WPF self-rendered PNG. F12 also captures the currently open main, settings or reference page. `--compact` requests the minimum window size. Errors from WPF bindings are logged next to the report when present.

`--matrix` exports both languages across destination, narrow-window, busy, error, success, settings, unsaved-edit, advanced-settings, installation and help states. It requires an explicit `--data-dir <absolute-path>` and blocks real switching and directory saving while the synthetic presentation is active. Reports set `SyntheticState=true`; sample paths and versions must never be represented as observed installations.

Launch the generated **EXE** for DPI validation so its application manifest is used. Starting the DLL through the dotnet host can produce valid layout renders but does not establish the executable's PerMonitorV2 behavior. A self-rendered image also does not prove that external window capture works.

Keep raw images and logs under `artifacts/validation/english-first-ui`. Refer to `VALIDATION.md` for the tested scope and outstanding checks. Installer and release validation are recorded separately from UI rendering.
