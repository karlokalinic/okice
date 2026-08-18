# OKICE / GRADOMRAZ — mobile WebGL port

## Current goal

The mobile target is no longer a reduced desktop preset. It is a separate WebGL runtime whose first requirement is stable control and frame pacing on Android Chrome and iOS Safari. Desktop/Windows settings stay isolated.

Primary validation target is a Galaxy A36-class Android phone plus a current iPhone Safari device. Until a physical-device run is measured, FPS, RAM, thermals and transfer size are engineering targets rather than claims.

## Build

Unity version is pinned by `ProjectSettings/ProjectVersion.txt`.

Menu command:

`KARLOLEGEND > GRADOMRAZ > Build Mobile WebGL (iOS Safari + Android Chrome)`

Output:

- `Builds/WebGL-Mobile/`
- `Builds/GRADOMRAZ-by-KARLOLEGEND-WebGL-mobile.zip`

Windows helper:

```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\MobileWeb\mobile.ps1 All
```

The mobile build temporarily applies its own WebGL settings and restores the prior desktop settings in `finally`.

## Build policy

- custom `PROJECT:Mobile` WebGL template;
- Brotli compression and Unity data caching;
- ASTC mobile texture subtarget;
- Wasm Disk Size + LTO;
- 512 MB initial WebAssembly heap;
- 1024 MB maximum heap;
- geometric heap growth with 10% growth step and 64 MB cap;
- low-power GPU preference;
- hashed output filenames and immutable cache metadata;
- WebGL threads disabled for broad Safari compatibility;
- decompression fallback, exception support, debug symbols and diagnostics disabled in the shipping target;
- `KARLOLEGEND_MOBILE_WEB` scripting define only for this target;
- the desktop 4K boot VideoClip is stripped from the in-memory Boot scene copy during mobile build.

The 512 MB starting heap is intended to avoid repeated heap-growth stalls seen with an overly small starting envelope. It is still a measurement baseline and must be adjusted from real runtime heap data rather than guessed upward indefinitely.

## Input architecture — critical performance rule

The browser must never call `unityInstance.SendMessage` on every raw `pointermove` event.

`Assets/WebGLTemplates/Mobile/index.html` now batches movement/look and crosses JS -> Wasm at a maximum cadence of 30 Hz. Raw touch events only update cheap JavaScript state between flushes.

The old full-screen transparent movement/look DIVs were removed. They intercepted pointer input above the Unity canvas and could make Main Menu/dialogue UI effectively untouchable. Gesture listeners now live on the Unity canvas itself, so Unity receives its normal pointer events while the mobile bridge also derives movement/look.

Actual action mapping from the serialized gameplay scene:

- `KORISTI` -> `Use`;
- `AKCIJA` -> `Fire1` for Dialogue System/QTE paths;
- `ALT` -> `Fire2`;
- menu -> `Cancel`.

The previous generic `Jump` mobile button was removed because the current gameplay scene does not use `Jump` as a PlayMaker button input.

`MobileWebInputBridge` feeds only the low-level seams already used by the game:

- `GetAxisVector` for movement;
- `MouseLook` for look;
- `GetButtonDown` for button presses.

Keyboard/mouse behavior is preserved outside the mobile path.

## Browser lifecycle

Blur, page hide, visibility loss, pointer cancellation and lost pointer capture clear mobile movement/look/button state. This prevents stuck walking/camera state after Safari/Chrome interrupts a touch sequence.

Hidden pages drop the Unity frame target to 5 FPS and pause the AudioListener. Returning to the page restores the active 30 FPS target and resets the adaptive governor.

## Rendering policy

All mobile profiles target 30 FPS. There is no automatic or manual 60 FPS phone mode in this branch. A 60 FPS option is not useful while the target is still failing stable 30 FPS.

Global mobile reductions:

- HDR off;
- SSAO off;
- realtime reflection probes off;
- camera opaque texture copy off;
- one shadow cascade;
- low background-loading priority;
- dynamic batching enabled;
- device DPR capped by the HTML shell instead of rendering at native high-DPI phone resolution.

### Štednja / Eco

- 30 FPS;
- HTML DPR cap 1.0;
- render scale 0.60, adaptive floor 0.50, ceiling 0.65;
- MSAA off;
- no realtime shadow distance;
- zero additional lights;
- post-processing off;
- camera depth texture off;
- LOD bias 0.85.

### Stabilno / Balanced — default

- 30 FPS;
- HTML DPR cap 1.0;
- render scale 0.75, adaptive floor 0.55, ceiling 0.80;
- MSAA off;
- 8 m shadow distance;
- one additional light;
- post-processing off;
- camera depth texture off;
- LOD bias 1.05.

This is deliberately much lighter than the PC renderer. The default profile exists to be playable first.

### Oštrije / Quality — manual opt-in

- 30 FPS;
- HTML DPR cap 1.25;
- render scale 0.85, adaptive floor 0.65, ceiling 0.90;
- 2x MSAA;
- 16 m shadow distance;
- one additional light;
- post-processing/depth texture allowed;
- HDR and SSAO still remain off;
- LOD bias 1.30.

## Adaptive governor

The governor samples two-second windows. Persistent frame pressure reduces internal URP render scale in 0.05 increments.

If the selected profile is still too slow at its normal floor, emergency mode disables realtime shadows, additional lights, depth-dependent camera work and post-processing, then allows render scale to fall as low as 0.50. Quality only recovers after sustained stable windows to avoid oscillating between sharp/hot and blurry/cool states.

## Web shell costs

The mobile overlay intentionally avoids CSS `backdrop-filter`/blur over the WebGL canvas. Those effects create additional browser compositing work unrelated to the actual game renderer.

Unity payload loading begins only after an explicit user gesture. The HTML shell uses safe-area insets, prevents browser zoom/scroll gestures on the game surface, and supports fullscreen where exposed by the browser.

## Hosting

Successful mobile builds emit `_headers` and `DEPLOYMENT.txt`.

Production requirements:

- HTTPS;
- preserve `Content-Encoding: br` for Brotli files;
- serve `.wasm.br` with the Wasm MIME type;
- prefer a top-level page on iOS Safari rather than an iframe when persistence/cache reliability matters;
- hashed build files may use immutable caching while `index.html` must not be pinned forever.

## GitHub Actions policy

`.github/workflows/mobile-webgl.yml` is **manual-only** (`workflow_dispatch`). It does not run on branch pushes or pull requests.

This is intentional. Iterative mobile tuning must not generate repeated failed-build notification spam. If `UNITY_LICENSE` is not configured, a manually started job records that the Unity build was skipped rather than deliberately producing a red failure.

The workflow still provides source preflight, LFS-pointer auditing, quota-safe CI workspace preparation, Unity build/smoke test when licensed, payload analysis and artifact upload.

## Git LFS constraint

The repository's Git LFS quota currently prevents CI from downloading every LFS object. The mobile CI preparation script therefore repairs only its temporary CI checkout when needed:

- the desktop boot video reference is removed from the temporary workspace;
- the unavailable VT323 SDF dependency is rewired to an available font asset for CI validation.

Those CI substitutions do not mutate the source branch. The production source still needs its real assets available on the development machine.

## Acceptance route

A candidate is not called finished until a newly rebuilt mobile artifact passes this route on physical hardware:

1. launch from a top-level HTTPS page;
2. Main Menu buttons respond to direct touch;
3. load `SampleScene` without context loss/reload;
4. walk continuously while dragging camera for several minutes;
5. repeatedly trigger `Use` interactions;
6. complete a dialogue/QTE path using `Fire1`/`Fire2` where applicable;
7. open/cancel menus;
8. background/foreground the browser and confirm no stuck movement/look/audio;
9. remain around the heaviest visible area for a 10–15 minute soak;
10. verify stable 30 FPS behavior, memory does not creep toward tab kill, and performance does not progressively collapse from thermal pressure.

If Stable still misses 30 FPS, the next work is not to add another browser trick. Use the Build Report and profiler evidence to attack the largest resident textures/audio/meshes, overdraw, shader passes and objects actually present in the expensive frame.

## Instrumentation

`Tools/MobileWeb/analyze-build.mjs` reports build/data/Wasm/JS/category sizes and largest files. `Tools/MobileWeb/smoke-build.mjs` validates the generated WebGL shell/hosting contract. `Tools/MobileWeb/preflight.mjs` prevents regression back to the old heavy/mobile-spam policy.

Do not merge PR #3 merely because source preflight is clean. A green source check is not a physical-device performance result.
