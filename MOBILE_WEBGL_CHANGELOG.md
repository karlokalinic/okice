# OKICE / GRADOMRAZ — Mobile WebGL optimization changelog

This log records the source-level mobile WebGL work on `chatgpt/mobile-webgl-port` relative to `master`.

Important distinction: **expected impact** below is an engineering expectation from the code path that changed. It is not a measured FPS/battery claim. Physical-device telemetry from a newly generated build remains the authority.

## 1. Dedicated mobile production target

- Added `Build Mobile WebGL (iOS Safari + Android Chrome)`.
- Output is isolated under `Builds/WebGL-Mobile` with a separate ZIP.
- Mobile build settings are temporary and restored in `finally` so desktop/Windows behavior is not silently mutated.
- Uses Brotli, Data Caching, ASTC Web texture target, Wasm Disk Size + LTO, low-power WebGL preference, content-hashed filenames, no decompression fallback, no threads, no shipping exception/debug-symbol overhead.
- Uses 512 MB initial / 1024 MB max WebAssembly heap with 10% geometric growth and a 64 MB growth cap.

Expected impact: lower network payload than development/default builds, more deterministic mobile memory growth, and less accidental desktop configuration leakage.

## 2. Dedicated lightweight URP mobile renderer

Added:

- `Assets/MonoBehaviour/Mobile_RPAsset.asset`
- `Assets/MonoBehaviour/Mobile_Renderer.asset`

Mobile pipeline policy includes:

- classic Forward renderer instead of the PC Forward+ renderer;
- no renderer features;
- default depth texture off;
- opaque texture off;
- HDR off;
- 0.75 baseline render scale;
- MSAA off by default;
- 512 main-light shadow map;
- one additional light per object;
- no additional-light shadows;
- 8 m default shadow distance;
- one cascade;
- soft shadows off;
- SRP + dynamic batching on;
- small color-grading LUT.

Why this matters: the PC renderer is Forward+ and the scene contains a very large authored light population. Forward+ ignores the classic additional-lights-per-object limit, so a true separate Forward mobile renderer is materially different from merely lowering one quality slider.

## 3. Runtime rendering profiles

All mobile profiles currently target 30 FPS. There is no automatic 60 FPS phone mode.

### Stabilno (default)

- DPR cap 1.0;
- render scale about 0.75, adaptive down toward 0.55;
- MSAA off;
- HDR / SSAO / post-processing / depth / opaque texture off;
- short main-light shadows;
- one additional light per object.

### Štednja

- DPR cap 1.0;
- render scale about 0.60, adaptive floor 0.50;
- no realtime shadow distance;
- zero additional lights;
- more aggressive texture mip limit.

### Oštrije

- DPR cap 1.25;
- 30 FPS;
- render scale about 0.85;
- 2x MSAA;
- limited depth/post-processing;
- still uses mobile shadow/light budgets.

Expected impact: the largest direct GPU reduction comes from fewer rendered pixels, fewer full-screen passes, reduced shadow work and controlled additional lighting.

## 4. Adaptive runtime governor

`MobileWebInputBridge` samples frame pacing in two-second windows.

Pressure response is staged:

1. reduce internal render scale;
2. enter emergency mode and remove expensive depth/post/shadow/light work;
3. if still critically unstable, fall from 30 FPS to a 24 FPS safety target rather than letting uncontrolled frame pacing/thermal collapse continue.

Background page behavior:

- reset held mobile input;
- reduce target to 5 FPS;
- pause audio;
- restore the active target when visible again.

Expected impact: less progressive thermal collapse and fewer long catch-up spirals after a period of overload.

## 5. Touch input architecture repair

Original mobile attempt forwarded raw pointer movement directly to Unity through `SendMessage` on every browser `pointermove`.

Current path:

- move/look state is accumulated in JavaScript;
- JS -> Wasm messages are flushed at a maximum 30 Hz;
- pointer cancellation, blur, pagehide and lost capture clear state;
- no full-screen transparent move/look DIV sits above the Unity canvas;
- canvas-level gestures preserve normal Unity pointer delivery;
- HTML controls map to actual gameplay inputs rather than generic assumptions.

Expected impact: lower CPU/string-parsing overhead while touching/dragging and fewer stuck-input/UI interception bugs.

## 6. UI / modal input modes

Added browser mode bridge:

- Main Menu = UI mode;
- active gameplay = movement/look mode;
- Dialogue System conversation = dialogue mode;
- paused/modal state (`Time.timeScale == 0`) = UI mode.

This prevents a tap intended for a menu/dialogue/modal surface from simultaneously driving player movement or camera look.

## 7. PlayMaker mobile input seam

Patched only the lowest input actions instead of rewriting gameplay FSMs:

- `GetAxisVector` can consume mobile movement;
- `MouseLook` can consume mobile look delta;
- `GetButtonDown` can consume mobile button presses.

Desktop keyboard/mouse behavior remains in the non-mobile path.

## 8. PixelCrushers / Dialogue System mobile input repair

Dialogue QTE input does not use the PlayMaker button action. It goes through `DialogueManager.getInputButtonDown` and `PixelCrushers.InputDeviceManager`.

Added `MobileWebPixelCrushersInput` to inject the mobile button state while preserving the original PixelCrushers delegate.

Current HTML action mapping includes:

- `Use`;
- `Cancel`;
- `Fire1`;
- `Fire2`.

Expected impact: mobile Dialogue System / QTE input now has a real route instead of an HTML button that never reaches the plugin.

## 9. Scene light policy

The gameplay scene has a very large light population and authored shadow-casting punctual lights.

Runtime mobile policy:

- punctual realtime shadows disabled;
- punctual ranges capped to profile-appropriate distances;
- directional shadows retained only where the profile permits them;
- static point/spot lights with no attached gameplay MonoBehaviour can be temporarily disabled when the camera is farther than their effective range plus a safety margin;
- those lights are restored when the camera comes back into range;
- scripted/dynamic light GameObjects are not distance-disabled by the scene budgeter.

Expected impact: less light culling/shadow work without deliberately deleting gameplay light cues.

## 10. Particle and skinned-mesh scene budgets

Added `MobileWebSceneBudgetController`.

- Only **looping** ParticleSystems receive a population cap.
- One-shot gameplay particle effects are not modified.
- Cap scales from roughly 384 -> 256 -> 128 -> 96 particles per looping system as rendering pressure increases.
- `SkinnedMeshRenderer.updateWhenOffscreen` is forced off so offscreen skinning does not consume rendering work while scripts/animators/colliders remain alive.

Expected impact: reduced transparent overdraw/particle fill-rate and less unnecessary offscreen skinning.

## 11. Physics pacing

Mobile WebGL runtime uses:

- 30 Hz `fixedDeltaTime`;
- bounded `maximumDeltaTime` to prevent large FixedUpdate catch-up spirals after a hitch.

This is especially relevant to single-threaded WebGL when a heavy frame otherwise produces multiple physics catch-up steps.

## 12. Audio voice budget

Mobile runtime caps the available real/virtual AudioSettings voice configuration conservatively and treats failure to apply the device-specific config as non-fatal.

Expected impact: fewer simultaneously mixed voices and less unnecessary audio CPU work in a scene with many AudioSources.

## 13. Texture memory policy

Runtime profiles use `QualitySettings.globalTextureMipmapLimit`:

- sharper profile keeps full mip baseline;
- default drops one mip level where appropriate;
- Eco drops more.

This reduces texture residency/bandwidth without destructively reimporting the desktop source textures.

## 14. Boot payload stripping and repository/LFS cleanup

- Mobile scene build processing removes the desktop Boot `VideoClip` reference from the in-memory mobile scene copy.
- Desktop/Windows source scene remains unchanged.
- CI workspace has a quota-safe repair path for unavailable LFS objects.
- Removed an approximately 128 MiB unused `PerfectDOSVGA437.asset` LFS object from the branch.

Expected impact: lower repository/LFS pressure and prevention of desktop boot media being dragged into the mobile content path.

## 15. Mobile browser shell

Custom WebGL template provides:

- explicit user gesture before Unity payload download;
- safe-area handling;
- DPR caps;
- touch controls;
- no costly backdrop blur over the WebGL canvas;
- correct browser lifecycle input reset;
- profile handoff to Unity;
- top-level deployment assumptions compatible with iOS persistence/cache requirements.

## 16. Hosting output

Successful mobile build writes hosting metadata including:

- `Content-Encoding: br` for Brotli build files;
- `application/wasm` MIME for Wasm;
- immutable cache policy for content-hashed build assets;
- no-cache policy for `index.html`.

## 17. Build / scene / LFS validation tooling

Added:

- `Tools/MobileWeb/preflight.mjs`;
- `audit-scene.mjs`;
- `audit-lfs.mjs`;
- `prepare-ci-lfs.mjs`;
- `smoke-build.mjs`;
- `analyze-build.mjs`;
- `serve.mjs`;
- `mobile.ps1`.

`mobile.ps1 Build` runs source preflight + scene audit before Unity, then build + package smoke + payload analysis after Unity.

## 18. Physical-device telemetry

Added `MobileWebTelemetry`.

Every ten seconds it writes one compact browser-console line containing:

- scene;
- average FPS and frame time;
- p95 frame time;
- worst sampled frame time;
- current target FPS;
- URP render scale;
- shadow distance;
- allocated and reserved Unity memory;
- scene transition count.

Prefix:

`[MobileWebTelemetry]`

This intentionally has no analytics SDK or network transport.

## 19. GitHub Actions notification policy

Mobile workflow is **manual-only** through `workflow_dispatch`.

It does not automatically run on every branch push or PR update. Missing optional `UNITY_LICENSE` records a skip instead of deliberately producing a red job.

Reason: iterative tuning should not spam failed-build mail, and a green check must not pretend a Unity build occurred when it did not.

## 20. Current expected impact ranking

Highest likely runtime impact:

1. dedicated lightweight Forward mobile renderer;
2. DPR + internal render-scale reduction;
3. removal of mobile post/depth/HDR/SSAO work;
4. punctual shadow removal + strict light budget;
5. 30 FPS cap / adaptive governor;
6. 30 Hz JS-to-Wasm input batching;
7. looping-particle and offscreen-skinning caps;
8. 30 Hz physics / bounded catch-up;
9. texture mip and audio voice budgets.

Primarily functional/reliability impact:

- canvas touch architecture;
- UI/dialogue/pause mode switching;
- PixelCrushers QTE bridge;
- lifecycle input reset;
- correct Brotli/Wasm hosting metadata;
- mobile-only boot asset stripping.

Primarily repository/build-system impact:

- LFS cleanup/repair;
- build analyzer;
- scene audit;
- preflight contract;
- manual CI policy.

## 21. What is still not proven

Do **not** treat any of these as measured facts until a new build produced from the current branch is tested:

- actual Galaxy A36 FPS;
- actual iPhone Safari FPS;
- actual battery drain;
- actual thermal behavior;
- actual browser-tab RAM;
- actual compressed transfer size;
- actual final `.data` / Wasm size;
- absence of WebGL context loss;
- long-session memory stability.

The current branch is source-level engineering. A stale deployed mobile build does not contain these changes.

## 22. Next validation gate

Generate a completely new `Builds/WebGL-Mobile` artifact and run the same device route every time:

1. Main Menu touch;
2. enter gameplay;
3. continuous movement + look;
4. repeated `Use`;
5. Dialogue / `Fire1` / `Fire2`;
6. Pause / Cancel / modal UI;
7. visually dense/light-heavy area;
8. 10–15 minute soak.

Capture `[MobileWebTelemetry]` lines and the generated build reports. Only then tune texture imports, audio clip load type, Addressables or more destructive scene changes.
