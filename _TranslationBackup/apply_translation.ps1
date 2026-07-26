$ErrorActionPreference = 'Stop'
$root = 'c:\Users\kalin\Downloads\AssetRipper_win_x64\okice\ExportedProject'
$asset = Join-Path $root 'Assets\MonoBehaviour\AFTERLIVES Dialogue Database.asset'
$enc = New-Object System.Text.UTF8Encoding($false)

function ReadLines($path) {
    $t = [System.IO.File]::ReadAllText($path, $enc)
    $t = $t.TrimStart([char]0xFEFF)
    $arr = $t -split "`n"
    # drop a single trailing empty element from a final newline
    if ($arr.Count -gt 0 -and $arr[$arr.Count - 1] -eq '') { $arr = $arr[0..($arr.Count - 2)] }
    return ,($arr | ForEach-Object { $_.TrimEnd([char]13) })
}

$en = ReadLines (Join-Path $root '_en.txt')
$hr = ReadLines (Join-Path $root '_hr.txt')

"EN lines: $($en.Count)"
"HR lines: $($hr.Count)"
if ($en.Count -ne $hr.Count) { throw "Line count mismatch EN=$($en.Count) HR=$($hr.Count) - aborting, no changes written." }

$map = @{}
for ($i = 0; $i -lt $en.Count; $i++) {
    if (-not $map.ContainsKey($en[$i])) { $map[$en[$i]] = $hr[$i] }
}

$lines = [System.IO.File]::ReadAllText($asset, $enc) -split "`n"
$replacements = 0
$usedKeys = @{}
for ($i = 0; $i -lt $lines.Count - 1; $i++) {
    if ($lines[$i] -match '^\s+- title: Dialogue Text\s*$') {
        if ($lines[$i + 1] -match '^(?<ind>\s+)value:\s?(?<val>.*)$') {
            $val = $Matches['val']
            $ind = $Matches['ind']
            if ($val.Trim().Length -gt 0 -and $map.ContainsKey($val)) {
                $lines[$i + 1] = $ind + 'value: ' + $map[$val]
                $replacements++
                $usedKeys[$val] = $true
            }
        }
    }
}

[System.IO.File]::WriteAllText($asset, ($lines -join "`n"), $enc)
"Replacements written: $replacements"
"Distinct EN keys matched: $($usedKeys.Count) / $($map.Count)"
$unmatched = $map.Keys | Where-Object { -not $usedKeys.ContainsKey($_) }
if ($unmatched.Count -gt 0) {
    "UNMATCHED KEYS (left in English):"
    $unmatched | ForEach-Object { "  <<$_>>" }
}
