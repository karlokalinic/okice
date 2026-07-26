from pathlib import Path
import re
p = Path(r'C:\Users\kalin\Downloads\AssetRipper_win_x64\okice\okice\Assets\MonoBehaviour\AFTERLIVES Dialogue Database.asset')
text = p.read_text(encoding='utf-8')
lines = text.splitlines()
vals = []
for i, l in enumerate(lines):
    if re.match(r'^\s*- title: Dialogue Text\s*$', l):
        j = i + 1
        while j < len(lines):
            m = re.match(r'^\s+value:\s*(.*)$', lines[j])
            if m:
                v = m.group(1).strip()
                if v.startswith('"') and v.endswith('"'):
                    v = v[1:-1]
                if v:
                    vals.append(v)
                break
            j += 1

seen = []
out = []
for v in vals:
    if v not in seen:
        seen.append(v)
        out.append(v)
Path('dialogue_values.txt').write_text('\n'.join(out), encoding='utf-8')
print('count', len(vals))
print('unique', len(out))
print('\n'.join(out[:300]))
