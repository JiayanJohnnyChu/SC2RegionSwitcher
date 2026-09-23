"""Keep full Regular CJK faces; retain common text at 500 and UI text above 500.

Requires fontTools 4.65.0. Input is the full static output of
Prepare-SwissFonts.py. Normal application builds use the committed fonts.
"""
import argparse
import copy
import hashlib
import json
import shutil
from pathlib import Path
import xml.etree.ElementTree as ET

import fontTools
from fontTools import subset
from fontTools.ttLib import TTFont


ROOT = Path(__file__).resolve().parents[1]
APP = ROOT / 'src/SC2Switcher.Wpf'


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def ui_characters():
    text = []
    for path in sorted(APP.glob('Strings.*.json')):
        catalog = json.loads(path.read_text(encoding='utf-8-sig'))
        text.extend(catalog.keys())
        text.extend(catalog.values())
    for path in sorted(APP.glob('*.cs')):
        if not path.name.endswith('Diagnostics.cs'):
            text.append(path.read_text(encoding='utf-8-sig'))
    for path in sorted(APP.glob('*.xaml')):
        for element in ET.parse(path).iter():
            text.extend(element.attrib.values())
            text.append(element.text or '')
    return set(map(ord, ''.join(text)))


def common_characters(encoding):
    chars = set()
    for lead in range(0xa1, 0xff):
        for trail in range(0xa1, 0xff):
            try:
                chars.update(map(ord, bytes([lead, trail]).decode(encoding)))
            except UnicodeDecodeError:
                pass
    return chars


def is_ideograph_or_syllable(cp):
    return (0x3400 <= cp <= 0x4dbf or 0x4e00 <= cp <= 0x9fff
            or 0xf900 <= cp <= 0xfaff or 0x20000 <= cp <= 0x323af
            or 0xac00 <= cp <= 0xd7af)


def encoding_for(record):
    if record['family'].startswith('Switcher Hangul'):
        return 'euc_kr'
    if record['family'].startswith('Switcher Han'):
        return 'gb2312'
    return None


def required_characters(full, encoding, ui, weight):
    # Preserve existing Latin, Greek, Cyrillic, Jamo, kana and symbols.
    common = common_characters(encoding) if weight == 500 else set()
    return full & (common | ui
                   | {cp for cp in full if not is_ideograph_or_syllable(cp)})


def check(directory):
    records = json.loads((directory / 'manifest.json').read_text(encoding='utf-8'))
    ui = ui_characters()
    full_maps = {}
    for record in records:
        if record['weight'] == 400 and encoding_for(record):
            with TTFont(directory / record['file']) as font:
                full_maps[encoding_for(record)] = set(font.getBestCmap())
    for record in records:
        path = directory / record['file']
        if sha256(path) != record['sha256']:
            raise ValueError(f'Hash mismatch: {path.name}')
        with TTFont(path) as font:
            actual = set(font.getBestCmap())
            if len(actual) != record['glyphs']:
                raise ValueError(f'Character count mismatch: {path.name}')
            encoding = encoding_for(record)
            if encoding and record['weight'] != 400:
                required = required_characters(full_maps[encoding], encoding, ui, record['weight'])
                missing = required - actual
                if missing:
                    codes = ', '.join(f'U+{cp:04X}' for cp in sorted(missing))
                    raise ValueError(f'{path.name} requires regeneration: {codes}')
    print(f'PASS: {len(records)} font hashes and current UI/common-character coverage')


def generate(source, output):
    if source.resolve() == output.resolve():
        raise ValueError('Input and output directories must differ.')
    if fontTools.__version__ != '4.65.0':
        raise ValueError('Generation requires fontTools 4.65.0.')
    records = json.loads((source / 'manifest.json').read_text(encoding='utf-8'))
    if any(record.get('subset') for record in records):
        raise ValueError('Input must contain full static fonts, not previous subsets.')
    ui = ui_characters()
    output.mkdir(parents=True, exist_ok=True)
    for record in records:
        original = source / record['file']
        target = output / record['file']
        original_hash = sha256(original)
        if original_hash != record['sha256']:
            raise ValueError(f'Input hash mismatch: {original.name}')
        encoding = encoding_for(record)
        if encoding and record['weight'] != 400:
            with TTFont(original, recalcTimestamp=False) as font:
                full = set(font.getBestCmap())
                original_names = copy.deepcopy(font['name'])
                keep = required_characters(full, encoding, ui, record['weight'])
                options = subset.Options()
                options.name_IDs = ['*']
                options.name_languages = ['*']
                options.name_legacy = True
                options.layout_features = ['*']
                options.notdef_outline = True
                options.recommended_glyphs = True
                options.glyph_names = True
                options.recalc_timestamp = False
                worker = subset.Subsetter(options=options)
                worker.populate(unicodes=keep)
                worker.subset(font)
                font['name'] = original_names
                if set(font.getBestCmap()) != keep:
                    raise ValueError(f'Unexpected character coverage: {original.name}')
                font.save(target)
                record.update(subset=encoding + '+ui' if record['weight'] == 500 else 'ui', fullSha256=original_hash,
                              fullCharacters=len(full), glyphs=len(keep), sha256=sha256(target))
        else:
            shutil.copyfile(original, target)
        print(f"{target.name}: {original.stat().st_size} -> {target.stat().st_size} bytes", flush=True)
    (output / 'manifest.json').write_text(json.dumps(records, indent=2) + '\n', encoding='utf-8')
    check(output)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--input', type=Path, help='Full static fonts and their manifest')
    parser.add_argument('--output', type=Path, help='Separate output directory')
    parser.add_argument('--check', type=Path, help='Verify an existing font directory')
    args = parser.parse_args()
    if args.check and not (args.input or args.output):
        check(args.check)
    elif args.input and args.output and not args.check:
        generate(args.input, args.output)
    else:
        parser.error('Use --input with --output, or --check.')
