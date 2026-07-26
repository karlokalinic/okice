# HLADAN GRAD — Quality Upgrade Log

Project: **HLADAN GRAD** (KARLOLEGEND) · Unity **6000.4.0f1** · **URP** (Forward+)
All changes below are **safe & reversible**. A pristine backup of every touched
file was made **before** any edit, and a one‑click undo is in the editor menu:

> **Tools ▸ HLADAN GRAD Quality ▸ Restore Original Settings (UNDO Upgrade)**

Pristine backup location (outside `Assets/` so no duplicate GUIDs):
`_QualityUpgradeBackup/`

> **Revision 2 (2026‑07‑13):** Linear color space was **reverted** — on this
> Gamma‑authored project it washed the colors out grey. **Color space stays
> Gamma (`m_ActiveColorSpace: 0`).** Added 2560×1440 @ 200 Hz targeting, kept only
> color‑neutral quality bumps, and added a Debug chapter‑jump menu.

> **Revision 3 (2026‑07‑13):** Found the real cause of the grey — the **CRT
> post‑effect shader was failing to load** (stale import artifact), so the retro
> scanline/contrast/color grade was dead, flattening everything. Fixed the CRT
> shader and **re‑enabled Linear** (`m_ActiveColorSpace: 1`) as requested — Linear
> + the working CRT grade preserves/improves the look. Added a **Toggle Color
> Space** menu and a **Reimport Shaders** menu. Also revised the Croatian
> translation to a more polished, literary register.

---

## What was NOT done (and why)
- **No HDRP / High‑Definition package.** URP already has HDR enabled
  (`m_SupportsHDR: 1`). Converting a working URP + CRT/retro project to HDRP is
  destructive (materials go magenta, post FX break, light units change) and was
  explicitly avoided. Instead HDR quality was raised *within URP* (below).
- **No package downloads required.** Every upgrade uses settings already present
  in the project.
- **Color grading mode left at LDR**, tonemapping volumes untouched — avoids
  shifting the authored CRT/film‑grain look.

---

## Changes applied (small patches)

### 1. Color space — **Linear** (with the CRT effect fixed)
`ProjectSettings/ProjectSettings.asset` `m_ActiveColorSpace: 1`.
The earlier "grey/washed out" was **not** Linear itself — it was the **CRT
post‑effect shader failing to load** (`Assets/Resources/shaders/crt.shader`,
a stale import artifact), so the scanline/contrast/tint grade was missing.
With the CRT shader fixed, Linear now looks correct/better.
- Fix used: force‑reimport of the shader (**Tools ▸ HLADAN GRAD Quality ▸
  Reimport Shaders**) + a content bump on the shader to trigger a clean import.
- Instant A/B compare: **Tools ▸ HLADAN GRAD Quality ▸ Toggle Color Space**.
- If Linear ever looks off, flip back to Gamma with that toggle (one click).

### 2. Resolution & refresh: **2560×1440 @ 200 Hz**
- `ProjectSettings/ProjectSettings.asset`: `defaultScreenWidth/Height 1366×768`
  → **2560×1440** (build's default resolution).
- `Assets/Scripts/HladanGradDisplay/HladanGradDisplayBootstrap.cs` (new, runtime):
  at startup sets `Application.targetFrameRate = 200`, `vSyncCount = 0`, and in
  builds calls `Screen.SetResolution(2560, 1440, <fullscreen>, 200 Hz)`.
- The game also has its own in‑game resolution options (720p/1080p/1440p/2160p);
  those still work and can override at runtime.

### 3. Render scale — kept at **1.0** (no supersampling)
`Assets/MonoBehaviour/PC_RPAsset.asset` `m_RenderScale: 1`. Left at 1 so the game
comfortably holds 200 fps at 1440p. Raise to `1.25`–`1.5` for extra sharpness if
your GPU has headroom (this is real SSAA supersampling and does not change color).

### 3. Anti‑aliasing (MSAA)
`Assets/MonoBehaviour/PC_RPAsset.asset`
- `m_MSAA: 1` (off) → `4` (4× MSAA) — smooth geometry edges.

### 4. HDR color buffer precision
`Assets/MonoBehaviour/PC_RPAsset.asset`
- `m_HDRColorBufferPrecision: 0` → `1` (High precision) — reduces banding in
  bloom/bright highlights.

### 5. Shadow map resolution
`Assets/MonoBehaviour/PC_RPAsset.asset`
- `m_MainLightShadowmapResolution: 2048` → `4096`
- `m_AdditionalLightsShadowmapResolution: 2048` → `4096`
- Sharper, less pixelated shadows.

### 6. Color grading LUT size
`Assets/MonoBehaviour/PC_RPAsset.asset`
- `m_ColorGradingLutSize: 32` → `64` — smoother color gradients / less banding.

### 7. LOD detail retention
`ProjectSettings/QualitySettings.asset` (PC tier)
- `lodBias: 2` → `3` — higher‑detail LODs stay visible at greater distance
  (no effect if a mesh has no LOD group; harmless).

---

## How to undo
1. In Unity: **Tools ▸ HLADAN GRAD Quality ▸ Restore Original Settings (UNDO Upgrade)**
2. Restart the Editor.

Or manually copy the files from `_QualityUpgradeBackup/` back over the originals
(the backup file name encodes the path, e.g.
`Assets__MonoBehaviour__PC_RPAsset.asset` → `Assets/MonoBehaviour/PC_RPAsset.asset`).

You can also save a **Timestamped Snapshot** of the current settings any time via
**Tools ▸ HLADAN GRAD Quality ▸ Create Timestamped Snapshot** (`_QualitySnapshots/`).

---

## Debug tools — fast testing (chapter jumps)
`Assets/Editor/HladanGradDebug/HladanGradDebugTools.cs`

The game is a **single gameplay scene** driven by **PlayMaker** global events
(no separate chapter scenes). The debug menu fires the game's **own** events, so
nothing new is introduced. **Enter Play mode and click "Igraj" to load the
gameplay scene first**, then use:

- **Tools ▸ HLADAN GRAD Debug ▸ Start at Chapter 2 (Mirror Stage 2)** → broadcasts
  the game's `debug / mirror stage 2` event.
- **Tools ▸ HLADAN GRAD Debug ▸ Start at Chapter 3 (Act 2)** → broadcasts
  `UNLOAD / ACT1` then `ACT 2 CHANGES` (the jet‑crash / bunker / Maddie act).
- **Tools ▸ HLADAN GRAD Debug ▸ Resolution ▸ Set 1440p / 2160p / 1080p** →
  broadcasts the game's resolution events.

Menu items are greyed out unless the game is playing. They use
`PlayMakerFSM.BroadcastEvent` via reflection (no compile‑time coupling).
Names map to the game's events found in `Assets/Resources/PlayMakerGlobals.asset`
(`CHAPTERS / CHAPTER1`, `debug / mirror stage 2`, `ACT 2 CHANGES`, `UNLOAD / ACT1`,
`720p`/`1080p`/`1440p`/`2160p`).

---

## Croatian translation (Hrvatski prijevod)

The full in‑game narrative was translated to Croatian. The game text lives in the
Pixel Crushers Dialogue System database:
`Assets/MonoBehaviour/AFTERLIVES Dialogue Database.asset`.

- **257 unique strings** (all `Dialogue Text` values — subtitles, notes, item
  inspections and player response options) → **451 field replacements**. This is
  the entire game's text, not just Act I.
- Only `Dialogue Text` values were changed. `Sequence`, `Conditions`, `Script`,
  actor names and all logic fields were left untouched, so game flow is identical.
- File integrity verified: line count unchanged (33,713), every translated value
  is a balanced YAML double‑quoted scalar, no leftover English key strings.

### Diacritics: now FULL Croatian orthography via Jersey 25 font
The original pixel font `PublicPixel SDF` had **no glyphs** for č ć ž š đ. Switched
to **Jersey 25** (Google Fonts, OFL) — a pixel/PSX‑style font that **includes all
Croatian diacritics** (verified: č ć ž š đ + uppercase, 332 glyphs). The
translation now uses **proper Croatian orthography** (č ć ž š đ), not ASCII folding.

- Font file: `Assets/Font/Jersey25-Regular.ttf`.
- Build/apply it with the editor menu (TMP font asset is generated in‑editor):
  - **Tools ▸ HLADAN GRAD Font ▸ 1. Enable Croatian Diacritics (safe fallback)** —
    builds the Jersey 25 TMP font asset and adds it as a **fallback** on the game
    font, so diacritics render everywhere with **no scene/prefab edits**. Do this.
  - **Tools ▸ HLADAN GRAD Font ▸ 2. Swap ENTIRE game to Jersey 25** — optional;
    changes every TMP text component to Jersey 25 for the full pixel look.
- **Run option 1 at least once**, otherwise the new diacritics show as squares
  (the font asset must be generated first).

### Backup & undo
- Original English database: `_TranslationBackup/AFTERLIVES Dialogue Database.asset`
- Original English menu scenes: `_TranslationBackup/MainMenu.unity`, `SampleScene.unity`
- Undo: **Tools ▸ HLADAN GRAD Quality ▸ Restore English Dialogue (UNDO Translation)**
  (restores the dialogue database **and** both menu scenes)
- Reference files & re‑apply tooling: `_TranslationBackup/english_strings.txt`,
  `croatian_strings.txt`, `apply_translation.ps1` (to tweak wording: restore the
  English DB, edit `croatian_strings.txt`, re‑run the script).

### Menu buttons (UI)
The title/pause menu labels in `MainMenu.unity` and `SampleScene.unity` were also
translated (13 labels): Play→Igraj, Continue→Nastavi, Options→Postavke,
Quit→Izlaz, Back→Natrag, "Playtested by"→"Igru testirali".

### Not covered
Any text baked into **textures/sprites** (e.g. a title image) or hard‑coded in
C# scripts is not part of the dialogue database and would need separate handling.
All dialogue, notes, item inspections, response options and the menu buttons ARE
translated. (The Pixel Crushers "Basic Standard UI Quest Log / Quest Tracker"
template prefabs contain English placeholder text but are unused — the database
has `items: []`, i.e. no quests.)

