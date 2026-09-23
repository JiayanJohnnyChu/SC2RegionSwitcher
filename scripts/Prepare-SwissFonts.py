"""Create WPF static fonts from the pinned, licensed design sources.

Run with fontTools[woff] installed. Source paths are explicit arguments so this
script never downloads fonts or depends on a developer's home directory.
"""
import argparse
import hashlib
import json
from pathlib import Path
from fontTools.ttLib import TTFont
from fontTools.varLib.instancer import instantiateVariableFont

parser = argparse.ArgumentParser()
parser.add_argument('--inter', required=True)
parser.add_argument('--sc', required=True)
parser.add_argument('--kr', required=True)
parser.add_argument('--output', required=True)
args = parser.parse_args()
output = Path(args.output)
output.mkdir(parents=True, exist_ok=True)
records = []
for source, family in [(args.inter, 'Switcher Sans'), (args.inter, 'Switcher Display'), (args.sc, 'Switcher Han'), (args.sc, 'Switcher Han Display'), (args.kr, 'Switcher Hangul'), (args.kr, 'Switcher Hangul Display')]:
    # Preserve the 650 outlines in a dedicated display family. WPF's legacy
    # font enumerator does not reliably expose nonstandard weight classes.
    weights = [(650, 'SemiBold')] if family.endswith('Display') else [(400, 'Regular'), (500, 'Medium'), (600, 'SemiBold'), (700, 'Bold')]
    for weight, style in weights:
        print(f'{family} {style}', flush=True)
        font = TTFont(source)
        axes = {'wght': weight}
        if 'opsz' in [a.axisTag for a in font['fvar'].axes]:
            axes['opsz'] = 32 if family == 'Switcher Display' else 14
        font = instantiateVariableFont(font, axes, inplace=True)
        font.flavor = None
        if 'STAT' in font: del font['STAT']
        native_weight = 600 if weight == 650 else weight
        # A distinct family avoids reserved-name ambiguity for modified fonts.
        # Both legacy and typographic names are set for WPF face matching.
        names = {1: family if native_weight in (400, 700) else family + ' ' + style, 2: 'Bold' if native_weight == 700 else 'Regular',
                 3: f'{family}-{style}-2', 4: f'{family} {style}',
                 6: family.replace(' ', '') + '-' + style, 16: family, 17: style}
        for name_id, value in names.items():
            font['name'].removeNames(nameID=name_id)
            font['name'].setName(value, name_id, 3, 1, 0x409)
        font['OS/2'].usWeightClass = native_weight
        font['OS/2'].fsSelection = (font['OS/2'].fsSelection & ~0x61) | (0x20 if native_weight == 700 else 0x40 if native_weight == 400 else 0)
        font['head'].macStyle = (font['head'].macStyle & ~3) | (1 if native_weight == 700 else 0)
        filename = family.replace(' ', '') + '-' + style + '.ttf'
        font.save(output / filename)
        records.append({'file': filename, 'family': family, 'weight': weight, 'nativeWeight': native_weight,
                        'glyphs': len(font.getBestCmap()),
                        'sha256': hashlib.sha256((output / filename).read_bytes()).hexdigest(),
                        'sourceSha256': hashlib.sha256(Path(source).read_bytes()).hexdigest()})
        (output / 'manifest.json').write_text(json.dumps(records, indent=2) + '\n', encoding='utf-8')
