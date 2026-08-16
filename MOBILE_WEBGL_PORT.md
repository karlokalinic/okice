# OKICE / GRADOMRAZ — mobile WebGL port methodology

## Scope

Target a separate production Web build for modern phones without degrading the existing Windows or desktop WebGL targets.

Primary acceptance devices:

- Android baseline: Samsung Galaxy A36 5G, especially the 6 GB RAM model, Chrome.
- iOS baseline: iPhone Safari, Safari 15+ compatibility floor.
- Desktop WebGL remains a separate build and keeps the existing desktop-oriented behavior.

The mobile build is not a naive copy of the PC WebGL build. It has separate thermal, memory, transfer, input and rendering budgets.

## One-command local workflow

From the repository root on Windows:

```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\MobileWeb\mobile.ps1 All
```

`All` performs the complete local loop:

1. reads the exact Unity editor version from `ProjectSettings/ProjectVersion.txt`;
2. locates that Unity editor from Unity Hub or `UNITY_PATH`;
3. runs the production Mobile WebGL build in batch mode;
4. writes the Unity log under `Builds/Logs/`;
5. analyzes the produced payload and writes JSON + Markdown reports under `Builds/Reports/`;
6. starts a dependency-free local/LAN server with the correct Brotli and Wasm headers;
7. opens the desktop test URL and prints LAN URLs for a phone on the same network.

Individual modes are also available:

```powershell
.\Tools\MobileWeb\mobile.ps1 Build
.\Tools\MobileWeb\mobile.ps1 Analyze
.\Tools\MobileWeb\mobile.ps1 Serve
```

The server is for development/device testing. Production and iOS persistence/cache validation should still use an HTTPS deployment.

## CI automation

`.github/workflows/mobile-webgl.yml` runs on relevant pull-request changes, relevant pushes to `master`, and manual dispatch.

Once repository secret `UNITY_LICENSE` is configured, CI automatically:

1. checks out Git LFS content;
2. caches the Unity `Library` directory;
3. uses the project Unity version automatically;
4. invokes `Karlolegend.Gradomraz.Editor.GradomrazBuild.BuildMobileWebGL` through GameCI;
5. runs `Tools/MobileWeb/analyze-build.mjs`;
6. verifies `_headers`, Brotli encoding metadata and Wasm MIME metadata;
7. uploads the mobile build plus reports as a seven-day workflow artifact;
8. places the payload report in the GitHub Actions job summary.

If `UNITY_LICENSE` is absent, the workflow does not create a false failing build; it records that the Unity build was skipped and explains the one missing prerequisite.

## Repository baseline before the port

The project is Unity 6000.4.0f1 and uses URP 17.4.0. The enabled build sequence is Boot -> MainMenu -> SampleScene.

The existing WebGL build was desktop-oriented:

- WebGL template: `PROJECT:Itch`;
- Brotli and data caching already enabled;
- initial WebAssembly heap: 512 MB;
- maximum heap: 2048 MB;
- one `PC` quality level used by WebGL;
- runtime target frame rate: 60 FPS;
- URP HDR, depth texture, opaque texture, 2x MSAA, 40 m shadow distance, two cascades and additional-light shadows;
- full-resolution active Screen Space Ambient Occlusion feature;
- movement through PlayMaker `GetAxisVector` reading `Horizontal` / `Vertical`;
- camera look through PlayMaker `MouseLook` reading `Mouse X` / `Mouse Y`;
- interactions through PlayMaker `GetButtonDown`;
- no mobile touch layer.

Simply opening the old itch build on a phone therefore was not a real mobile port.

## Mobile production build

Use the Unity menu:

`KARLOLEGEND > GRADOMRAZ > Build Mobile WebGL (iOS Safari + Android Chrome)`

Output:

- `Builds/WebGL-Mobile/`
- `Builds/GRADOMRAZ-by-KARLOLEGEND-WebGL-mobile.zip`

The build applies the mobile configuration temporarily and restores the previous desktop WebGL configuration in `finally`, even when the build fails.

Mobile release policy:

- custom `PROJECT:Mobile` template;
- Brotli compression;
- Unity data caching;
- ASTC Web texture subtarget;
- Disk Size with LTO Wasm optimization;
- 384 MB initial heap;
- 1024 MB maximum heap;
- geometric memory growth;
- low-power WebGL GPU preference;
- content-hashed build filenames for immutable CDN caching;
- Unity decompression fallback disabled so the browser/server path handles Brotli natively;
- WebGL threads disabled for the broadest Safari deployment path;
- WebGL exception support disabled in the shipping build;
- debug symbols and diagnostics disabled;
- `KARLOLEGEND_MOBILE_WEB` compile define only for this build;
- Development Build remains off.

The 384/1024 MB heap envelope is a conservative measurement baseline, not a final magic number. It should move only after real device heap measurements.

On a successful build the editor also emits:

- `_headers` with Brotli MIME, Wasm MIME and immutable build-cache headers;
- `DEPLOYMENT.txt` with the deployment invariants.

## Mobile HTML shell

`Assets/WebGLTemplates/Mobile/index.html` adds:

- no Unity payload download until the player explicitly presses **Pokreni igru**;
- safe-area support for iPhone notches and browser chrome;
- touch-first full-viewport canvas;
- dynamic virtual joystick;
- independent right-side drag camera control;
- `KORISTI`, `SKOK`, `ALT` and menu buttons;
- capped device-pixel ratio instead of blindly rendering at native phone DPR;
- explicit Eco / Balanced / Quality profiles;
- automatic pre-launch profile selection from available browser device/network signals;
- automatic switch to Eco when Data Saver, slow mobile network, <=4 GB memory signal or <=4 hardware threads indicate a constrained device;
- no automatic promotion to Quality: Quality is always an explicit user choice;
- automatic re-evaluation if the network changes before launch and the user has not overridden the profile;
- page visibility messages to Unity so background tabs stop wasting CPU/GPU/audio;
- fullscreen where the browser exposes the Fullscreen API;
- landscape recommendation without making portrait an artificial hard failure.

## Input bridge

`MobileWebInputBridge` receives browser pointer state through `unityInstance.SendMessage` and exposes one coherent input frame to the existing PlayMaker actions.

The existing gameplay logic remains the source of truth. The port patches only the lowest input seam:

- `GetAxisVector` can read mobile move input;
- `MouseLook` can read mobile look delta;
- `GetButtonDown` can read mobile action presses.

Keyboard, mouse and controller behavior remain available because mobile values are combined with, rather than replacing, existing Input Manager calls.

## Runtime profiles and adaptive governor

All mobile profiles are now adaptive. The selected profile defines the visual ceiling; a five-second runtime governor can reduce internal URP render scale when frame-time pressure persists and can recover it slowly after sustained headroom.

The governor does not randomly toggle the entire quality stack. It changes the cheapest high-impact control first: internal render scale.

When the page becomes hidden, the mobile build drops to 5 FPS, pauses the audio listener and clears held movement/look state. On return it restores the active target and resets its measurement window.

### Eco

- 30 FPS;
- HTML DPR cap 1.0;
- URP render scale ceiling 0.75, adaptive floor 0.65;
- MSAA disabled;
- 16 m shadow distance;
- one shadow cascade;
- one additional light per object;
- HDR off;
- SSAO off.

### Balanced — default on normal hardware

- 30 FPS;
- HTML DPR cap 1.5;
- URP render scale ceiling 0.90, adaptive floor 0.72;
- 2x MSAA;
- 24 m shadow distance;
- one shadow cascade;
- two additional lights per object;
- HDR retained to preserve the authored grading/bloom response;
- SSAO off.

### Quality — manual opt-in

- 60 FPS target;
- HTML DPR cap 1.75;
- URP render scale ceiling 1.0, adaptive floor 0.78;
- 2x MSAA;
- 32 m shadow distance;
- two shadow cascades;
- HDR on;
- SSAO on initially.

If Quality cannot sustain 60 FPS after render scale reaches its adaptive floor, the governor falls back to 30 FPS and disables SSAO instead of continuously heating/throttling the device.

## Automated build-size analysis

`Tools/MobileWeb/analyze-build.mjs` requires no npm packages. It recursively inspects the final mobile output and produces:

- total output size;
- served `Build/` payload size;
- data / Wasm / JavaScript / image / audio category totals;
- twenty largest files in JSON;
- fifteen largest files in the Markdown report;
- warnings for unusually large startup payloads, `.data`, Wasm, missing Brotli or missing hosting metadata;
- actionable next-stage recommendations;
- automatic GitHub Actions step-summary output when run in CI.

Warnings are informational by default. Use `--strict` only once a measured production baseline exists and the thresholds should become release gates.

This prevents premature asset destruction: Addressables, texture reduction and audio changes happen only when the generated build proves they are needed.

## Browser/deployment constraints

For iOS, serve the game as a top-level page when persistence/cache reliability matters. An iframe-only deployment is not the canonical mobile route.

Production hosting should use HTTPS and preserve `Content-Encoding: br` for Brotli assets and `application/wasm` for Wasm. The generated `_headers` file covers a Cloudflare Pages-style static deployment automatically.

## Performance acceptance gates

These are engineering gates, not claims about an unmeasured physical-device run.

### Galaxy A36-class Android / Chrome / Balanced

- no automatic build download before user gesture;
- stable 30 FPS target, <=33.3 ms frame budget;
- no progressive FPS collapse over a 10–15 minute route;
- no browser tab kill or out-of-memory reload;
- investigate routine Unity heap growth far above the 384 MB startup envelope;
- investigate sustained total tab memory approaching 1 GB;
- initial compressed/served Build payload target around <=80–90 MB before considering content streaming mandatory.

### iPhone / Safari / Balanced

- same stable 30 FPS target;
- no WebGL context loss during scene transitions;
- no reload caused by memory pressure;
- safe-area controls remain tappable in landscape;
- audio starts from the explicit launch gesture;
- persistent cache/save tested from a top-level HTTPS page.

## Deterministic physical-device test

1. Run `mobile.ps1 Build` or let CI produce the artifact.
2. Read `Builds/Reports/mobile-webgl-report.md`.
3. Deploy over HTTPS for canonical validation.
4. Android Chrome remote-debug the device.
5. Run the same 10–15 minute route:
   - Main Menu;
   - enter SampleScene;
   - continuous movement + camera rotation;
   - repeated `Use` interactions;
   - one dialogue sequence;
   - pause/cancel;
   - revisit the heaviest visible area.
6. Record FPS/frame time, JS/Wasm memory, Unity heap growth, GC spikes, transfer and context errors.
7. Repeat Eco and Quality.
8. Repeat on iOS Safari.
9. Change heap limits, asset streaming or destructive quality settings only from the resulting evidence.

For Android thermal validation, pair browser measurements with ADB battery/thermal diagnostics where practical. A stable 30 FPS after 15 minutes is more valuable than an initial 60 FPS followed by thermal collapse.

## Phase 2 only if measurements justify it

Do these in order:

1. Build Report-driven texture audit;
2. audio residency/compression audit;
3. Addressables/AssetBundles for content not needed at startup;
4. scene/mesh/static batching/occlusion/overdraw audit;
5. shader-variant stripping based on variants actually used;
6. reassess depth/opaque texture requirements only after CRT/retro visual regression tests;
7. introduce a separate lighter renderer asset if full-screen passes expand again.

## Deliberate non-changes

The port does **not** globally lower the PC quality asset.

It does **not** blindly disable depth or opaque textures because the custom retro/CRT rendering path must be regression-tested first.

It does **not** enable WebGL threads in the first production-mobile path.

It does **not** pre-emptively migrate the whole project to Addressables. The analyzer and physical-device data decide whether that complexity is warranted.

## Remaining non-automatable gate

Most repetitive engineering is now automated. The remaining gate is physical evidence:

- Unity 6000.4.0f1 must compile the branch successfully;
- Galaxy A36-class Chrome must complete the soak route;
- iOS Safari must complete the same route;
- the real Build Report and runtime memory numbers must be recorded.

No script can truthfully replace those device measurements. Everything around them — build settings, touch input, profile selection, frame-time adaptation, background throttling, compression/hosting metadata, payload analysis and CI artifact generation — is automated in this branch.
