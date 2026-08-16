# OKICE / GRADOMRAZ — mobile WebGL port methodology

## Scope

Target a separate production Web build for modern phones without degrading the existing Windows or desktop WebGL targets.

Primary acceptance devices:

- Android baseline: Samsung Galaxy A36 5G, especially the 6 GB RAM model, Chrome.
- iOS baseline: iPhone Safari, Safari 15+ compatibility floor.
- Desktop WebGL remains a separate build and keeps the existing desktop-oriented behavior.

The mobile build is not a naive copy of the PC WebGL build. It is a separate thermal, memory, download and input budget.

## What the repository looked like before this port

The project is Unity 6000.4.0f1 and uses URP 17.4.0. The enabled build sequence is Boot -> MainMenu -> SampleScene.

The existing WebGL build was desktop-oriented:

- WebGL template: `PROJECT:Itch`.
- Brotli and data caching already enabled.
- Initial WebAssembly heap: 512 MB.
- Maximum heap: 2048 MB.
- One `PC` quality level is used by WebGL.
- Runtime target frame rate: 60 FPS.
- URP keeps HDR, depth texture, opaque texture, 2x MSAA, 40 m shadow distance, two cascades and additional-light shadows.
- The renderer has a full-resolution active Screen Space Ambient Occlusion feature.
- Gameplay movement is PlayMaker `GetAxisVector` reading `Horizontal` / `Vertical`.
- Camera look is PlayMaker `MouseLook` reading `Mouse X` / `Mouse Y`.
- Interaction uses PlayMaker `GetButtonDown`, including the `Use` input.
- There was no mobile touch control layer.

Those details are why simply opening the existing itch build on a phone is not a real mobile port.

## Architecture added by this port

### Separate build target

Use:

`KARLOLEGEND > GRADOMRAZ > Build Mobile WebGL (iOS Safari + Android Chrome)`

Output:

- `Builds/WebGL-Mobile/`
- `Builds/GRADOMRAZ-by-KARLOLEGEND-WebGL-mobile.zip`

The build temporarily applies mobile publishing settings and restores the previous desktop WebGL settings afterward.

Mobile release settings:

- Brotli compression.
- Unity data caching enabled.
- ASTC Web texture subtarget.
- Disk Size with LTO code optimization.
- 384 MB initial heap.
- 1024 MB maximum heap.
- geometric heap growth.
- `KARLOLEGEND_MOBILE_WEB` compile define only for this build.
- Development Build remains off.
- WebGL threading is deliberately not introduced in this first mobile profile. Broad Safari compatibility is more important than a theoretical CPU win that would add cross-origin isolation and SharedArrayBuffer deployment constraints.

The 384/1024 MB values are a conservative starting envelope, not a final measurement. The initial heap must be changed after profiling real peak/typical heap usage.

### Mobile HTML shell

`Assets/WebGLTemplates/Mobile/index.html` adds:

- no build download until the player explicitly presses **Pokreni igru**;
- safe-area support for iPhone notches and browser chrome;
- touch-first full-viewport canvas;
- dynamic virtual joystick;
- independent right-side drag camera control;
- `KORISTI`, `SKOK`, `ALT` and menu buttons;
- capped device-pixel ratio rather than rendering blindly at the phone's native DPR;
- three explicit performance profiles;
- fullscreen when the browser exposes the Fullscreen API;
- landscape recommendation without preventing portrait fallback.

Deferring the download until a deliberate press matters on mobile: visiting the page must not automatically burn tens of megabytes of mobile data or start heating the phone.

### Input bridge

`MobileWebInputBridge` receives browser pointer state through `unityInstance.SendMessage` and exposes one coherent input frame to the existing PlayMaker actions.

The existing gameplay logic is preserved. The port patches only the lowest input seam:

- `GetAxisVector` can read mobile move input.
- `MouseLook` can read mobile look delta.
- `GetButtonDown` can read mobile action presses.

Keyboard, mouse and controller behavior remain available because mobile values are combined with, rather than replacing, the existing Input Manager calls.

### Runtime profiles

#### Eco

Use when battery life, thermal stability or an older phone matters most.

- 30 FPS.
- HTML DPR cap 1.0.
- URP render scale 0.75.
- MSAA disabled.
- 16 m shadow distance.
- one shadow cascade.
- one additional light per object.
- HDR off.
- SSAO off.

#### Balanced — default

This is the production default for a Galaxy A36-class device.

- 30 FPS.
- HTML DPR cap 1.5.
- URP render scale 0.90.
- 2x MSAA.
- 24 m shadow distance.
- one shadow cascade.
- two additional lights per object.
- HDR retained to preserve the authored grading/bloom response.
- SSAO off.

#### Quality

This is opt-in because a 120 Hz phone display does not mean a Unity Web game should continuously render at 120 FPS.

- 60 FPS.
- HTML DPR cap 1.75.
- URP render scale 1.0.
- 2x MSAA.
- 32 m shadow distance.
- two shadow cascades.
- HDR on.
- SSAO on.

Quality is intentionally not the default. Doubling 30 -> 60 FPS approximately doubles the number of frames the CPU/GPU must prepare in the same time and is one of the fastest ways to increase battery drain and thermal throttling.

## Why the Galaxy A36 is a useful baseline

The Galaxy A36 5G is a realistic mid-range rather than flagship-only target. Samsung sells 6 GB and 8 GB RAM variants, with a 6.7-inch 1080 x 2340 120 Hz display and 5000 mAh battery.

The physical 1080 x 2340 panel must not be treated as a mandate to render Unity at the browser's full physical pixel count. On a high-DPR phone that is an expensive amount of fragment work for very limited visible gain, particularly with the project's deliberately retro/CRT presentation. The DPR cap plus URP render scale is the main fill-rate control.

## Browser constraints

Unity 6 currently lists mobile Web support for iOS Safari 15+ and Chrome 58+ on Android.

For iOS, serve the game as a top-level page when persistence/cache reliability matters. Safari does not support IndexedDB for content running inside an iframe, so an iframe-only deployment is a bad canonical mobile URL.

Brotli should be served over HTTPS with correct `Content-Encoding: br` headers. Do not enable Unity's decompression fallback unless the host cannot provide the correct compressed-file headers; native browser decompression is more efficient.

## Performance acceptance budgets

These are gates, not claims about the current unmeasured build.

### Galaxy A36 6 GB / Chrome / Balanced

- Startup: no automatic download before user gesture.
- Gameplay target: stable 30 FPS; frame budget <= 33.3 ms.
- 10-minute run: no progressive FPS collapse caused by thermal throttling.
- No browser tab kill or out-of-memory reload.
- Unity heap should normally fit close to the configured 384 MB initial allocation; if routine gameplay immediately grows far above it, measure and reset the initial heap instead of guessing.
- Total tab memory should remain comfortably below the point where Android begins aggressive reclamation; practical engineering target: keep sustained use below roughly 800 MB and investigate anything approaching 1 GB.
- Compressed first-load transfer target: <= 80 MB for the first shipping pass. If the build exceeds that, asset streaming/Addressables becomes Phase 2 rather than accepting a giant monolithic `.data` file.

### iPhone / Safari / Balanced

- Same 30 FPS target.
- No WebGL context loss during scene transitions.
- No reload caused by memory pressure.
- Safe-area UI must remain tappable in landscape on devices with a notch/Dynamic Island.
- Audio must start after the user's launch gesture.
- Persistent saves/cache must be tested on a top-level page, not only inside an iframe.

## Test procedure

1. Build `WebGL-Mobile` from a clean Unity import.
2. Record the Build Report: code size, `.data` size, compressed transfer size and stripped assemblies.
3. Host over HTTPS with Brotli headers and WebAssembly MIME type configured correctly.
4. Android Chrome remote-debug the device from desktop Chrome.
5. Run the same deterministic route for at least 10 minutes:
   - Main Menu;
   - enter SampleScene;
   - continuous movement + camera rotation;
   - repeated `Use` interactions;
   - one dialogue sequence;
   - pause/cancel;
   - revisit the heaviest visible area.
6. Record FPS/frame time, JS/Wasm memory, Unity heap growth, GC spikes, network transfer and context errors.
7. Repeat Eco and Quality profiles.
8. Repeat on iOS Safari.
9. Only then change heap limits, render scale, texture policy or profile defaults.

For Android thermal validation, pair the browser measurements with ADB battery/thermal diagnostics where possible. A stable 30 FPS after 15 minutes is more valuable than a 60 FPS screenshot followed by throttling.

## Phase 2 if the first build is still too large or hot

Do these in order; do not destroy visual fidelity indiscriminately.

1. Build Report-driven texture audit: identify the largest textures actually included in the player.
2. Audio audit: convert long ambience/music to streaming-friendly compressed formats and lower sample rate only where inaudible.
3. Addressables/AssetBundles: move content that is not needed at startup out of the initial `.data` payload.
4. Scene/mesh audit: static batching, occlusion data, duplicated materials and overdraw.
5. Shader-variant stripping based on variants actually used by the project.
6. Reassess depth/opaque texture requirements only after checking the CRT/retro shaders that consume them.
7. Consider separate lighter renderer data if SSAO and other full-screen passes proliferate.

## Deliberate non-changes

The port does **not** globally lower the PC quality asset. Desktop rendering is not sacrificed to make the phone build work.

It does **not** blindly disable the depth or opaque texture because the project contains custom retro/CRT rendering and those dependencies need a visual regression test first.

It does **not** enable WebGL threads in the first mobile release.

It does **not** immediately migrate the whole project to Addressables. The current project is a single PlayMaker-driven gameplay scene, so a large asset-loading refactor should happen only if Build Report and device memory measurements justify it.

## Estimated work from this point

- First bootable/touchable mobile build: this branch contains the required first-pass infrastructure; the remaining risk is Unity compilation/build validation and device behavior.
- One real A36 + one iPhone playtest/fix cycle: approximately 4–8 focused engineering hours if no third-party WebGL incompatibility appears.
- Production thermal/memory polish: approximately 1–3 additional days.
- If the monolithic `.data` payload forces an Addressables refactor or a third-party plugin fails on mobile WebGL: add approximately 1–3 days depending on the offending asset/plugin graph.

The largest uncertainty is no longer "can Unity Web run on mobile?". In Unity 6 it is an officially supported target. The remaining uncertainty is the project's actual memory/build-size profile and whether every third-party gameplay/rendering component survives Safari's tighter browser constraints.
