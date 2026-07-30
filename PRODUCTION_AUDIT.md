# OKICE / GRADOMRAZ production audit

Audit date: 2026-07-30

## Critical corrections in this branch

- Restored the complete `AFTERLIVES Dialogue Database` from the last intact blob. The current `master` version had fallen from roughly 78 conversations and more than 29,000 serialized lines to a small fragment.
- Added a build guard that refuses to create a player if the dialogue database contains fewer than 70 conversations.
- Restored `Boot.unity` as build index 0. The previous build script explicitly removed it, so the startup logo could never run in a produced build.
- Reworked TMP repair cleanup so temporary font assets do not destroy atlas textures after those textures have been attached to persistent assets.
- Added Croatian fallback-glyph validation for `ČĆŽŠĐčćžšđ`.
- Changed project health checks to inspect one scene at a time and restore the previous editor scene setup. Additive inspection created duplicate `PlayMakerGUI` and other singleton warnings that were caused by the audit itself.
- Removed generated root build output (`Data/`) and Unity Version Control workspace metadata (`.plastic/`) from Git.
- Prevented desktop startup code from running at the monitor's full refresh rate without a ceiling. Desktop runtime is capped at 120 FPS; WebGL remains controlled by its 60 FPS bootstrap.

## Git LFS checkout requirement

`Assets/Font/VT323-Regular SDF.asset` is stored through Git LFS. A checkout that contains only the small text pointer beginning with `version https://git-lfs.github.com/spec/v1` is incomplete and Unity will report the asset as corrupted.

Repair the checkout without replacing the Unity asset or its GUID:

```powershell
git lfs install
git lfs pull
git lfs checkout -- "Assets/Font/VT323-Regular SDF.asset"
```

Do not regenerate or overwrite this asset until the LFS object has been recovered. Replacing an existing native Unity asset can invalidate sub-asset references even when the path remains unchanged.

## Input system status

The Input Manager deprecation message is currently informational, not a build error. The project still contains PlayMaker and Dialogue System code that calls legacy `UnityEngine.Input` APIs. Switching the project to Input System-only before those actions are migrated would break controls. Keep legacy input enabled for this repair branch; perform input migration as a separate tested task.

## Translation findings

The Croatian text is not uniformly bad, but it is inconsistent in register and sometimes reads like a literal or context-free model rewrite.

Examples requiring contextual review:

- `Maddie će te voljeti, ako budeš hrabar.` → the comma is incorrect. Grammatically: `Maddie će te voljeti ako budeš hrabar.`
- `Nema boga. Zaključano.` is understandable but sounds translated or mechanically compressed. Depending on intent: `Nema šanse. Zaključano je.` or `Ništa od toga. Zaključano je.`
- `Sve je u kurcu. Zaključano.` does not sound like a natural immediate reaction to a locked door. More performable: `U kurac. Zaključano je.`
- Player responses alternate between `Idem.`, `Dalje.` and `DALJE`. Capitalization and voice should be standardized according to whether these are spoken lines, UI prompts or internal navigation labels.

Internal Dialogue System identifiers such as `DOOR`, `START`, and `Doors / ApartmentDoor` do not need translation unless they are shown to the player.

A full language pass should be performed conversation by conversation with speaker, scene, emotional objective and audio timing visible. Blind global replacement would damage performance and subtext.

## Remaining work requiring an actual Unity validation run

- Pull all Git LFS objects before opening Unity.
- Compile the branch in Unity 6000.4.0f1.
- Run `KARLOLEGEND > GRADOMRAZ > Run Project Health Check` and inspect the generated report.
- Resolve reported missing scripts in their original scenes or prefabs rather than deleting every missing component automatically.
- Profile the heaviest gameplay scene with Unity Profiler and Frame Debugger before changing texture sizes, shadow settings or mesh import options.
- Decide whether the embedded `Packages/com.unity.services.cloud-build` copy is still needed. It duplicates manifest version 2.0.8 and should be removed only together with a regenerated `packages-lock.json`.
