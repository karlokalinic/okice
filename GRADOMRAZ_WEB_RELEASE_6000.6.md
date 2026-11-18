# GRADOMRAZ — Unity 6000.6 Web release candidate

This is an **actual source/Unity Editor-tested release candidate**, NOT an already
published or browser-playtested build.

## Changes made

- Unity 6000.6.9f1 / URP 17.6.0, preserves the existing Boot → MainMenu → SampleScene sequence.
- **Unity 6.6 Progressive Asset Loading**: per-scene data instead of one large
  startup .data file; runs in the background for later scenes. Disables
  WebGL 'Name Files as Hashes' because Unity 6.6 says they are incompatible.
- Brotli, decompression fallback disabled, IndexedDB Data Caching enabled.
- Uses **a separate Boot scene** with H.264 Constrained Baseline 1080p30
  intro (5,325,615 bytes versus original 14,391,606 bytes, **63.0% smaller**),
  preserving 10.005 seconds, 30fps and AAC sound. No need to put the
  desktop original 14.39MB intro into the Web startup scene.
- Isolated HDR/URP renderer, 2×MSAA, 2048 main shadows, 1024 additional shadows,
  2 cascades, 32m shadow distance. The redundant Opaque Texture copy is
  disabled after auditing all 24 authored shaders (none samples it);
  Depth Texture stays on for the existing SSAO renderer feature.
  Canonical desktop pipeline untouched.
- **Web-only cinematic post processing** (ACES, cool restrained grade, mild
  bloom/vignette/grain), quality governor with dynamic 0.72–0.94 renderScale,
  60fps cap, with optional passes disabled under sustained slow frame times.
- **Four named material candidates only** (window, road, asphalt, newspaper),
  sharing existing textures and mapping to the actual compiled URP glass,
  damp-concrete and lit-paper shaders; no replacement of 19k materials.
  Runtime makes at most four Material clones per gameplay scene and destroys
  them on scene unload. 400 renderers are scanned per frame and a reusable
  list avoids 19k per-renderer material-array allocations.
- Texture mip streaming newly enabled on **126 pre-mipmapped textures**,
  with an explicit 256 MB texture budget (effective runtime saving needs
  profiling on WebGL). UI textures/non-mipmapped assets unchanged.
- **10 long audio clips** formerly decoded on load now use CompressedInMemory
  (no audio quality/sample rate/channel changes). Short, timing-sensitive sound
  effects unaffected.
- Default Web release uses the original game scene **with all PlayMaker FSMs**.
  The separate BridgeBackground visual-only scene is NOT shipped until a
  verified WebGL bundle and transition QA exist.
- Three Web-only shaders are deliberately retained in the Web build, avoiding
  missing/pink runtime shaders when instantiated using Shader.Find.
- Canonical `Assets/Scenes/Boot.unity` and `SampleScene.unity` unchanged.

## Verification (performed)

Unity 6000.6.9f1 headless Editor:
- Full C# compilation PASS; Tundra success.
- Memory importer tuning PASS: 211 textures and 146 AudioClips inspected;
  126 streaming-mip flags and 10 long-audio import settings changed.
- Web release Stage menu PASS; Unity import of the Web-only Boot/video/URP asset PASS.
- Strict release validator PASS: original scene SHA-256, 22,333 GameObjects,
  19,082 MeshRenderers, **392 PlayMaker FSMs, zero missing components**, source
  scenes and build scene order unchanged; 3 Web shaders present.
- Private Cloudflare Worker delivery tests: **11/11 PASS**. One-year cache for
  *unhashed* Unity 6.6 progressive data files was fixed to **revalidate**
  instead. Hashed bundles and hashed core payloads remain immutable.
- Existing live `https://gradomraz.karlolegend.workers.dev/unity/game-01/`
  returned HTTP 200 with a Unity loader/canvas; this is still the OLD release.

## What is NOT complete

**There is NO valid new WebGL player build.** Unity WebGL Build Support for
6000.6.9f1 is missing. The installer was verified Unity-signed and previously
failed with INSTALL_ERROR; latest Windows UAC admin dialog was reported
cancelled by the operating system. Respect this decision — do not bypass UAC.
Therefore **NO** browser FPS, WebGL shader variant, scene transition,
WebAssembly binary size or WebGPU/WebGL GPU measurements are claimed.

The current Unity Editor compiles successfully, but source and Editor test
success is NOT proof that the WebGL player starts or the 392 FSM transitions
behave correctly in a browser.

## Build and publish, once the OS module is installed

1. In **Unity Hub → Installs → Unity 6000.6.9f1 → Add modules**, install
   **Web Build Support** with Windows administrator approval.
   Verify `C:\Program Files\Unity\Hub\Editor\6000.6.9f1\Editor\Data\PlaybackEngines\WebGLSupport\Variations`
   exists. Do not treat an incomplete WebGLSupport folder as a valid installation.
2. Open this branch/project with Unity 6000.6.9f1.
3. Run `Tools → GRADOMRAZ → Web Release → 0 - Tune memory imports`
   if needed; the build menu invokes it automatically.
4. Run `Tools → GRADOMRAZ → Web Release → 1 - Stage progressive Web release`
   to generate the Web Boot scene and the Web-only URP pipeline.
5. Run the `GradomrazWebReleaseValidator.Validate` Editor method, or
   Unity batch mode with `-executeMethod GradomrazWebReleaseValidator.Validate`.
6. Run `Tools → GRADOMRAZ → Web Release → 2 - Build production WebGL`.
   The build method explicitly checks WebGL support and refuses to claim
   success without `index.html` and a successful Unity BuildReport.
   Expected local build root: `Builds/GRADOMRAZ-Web6/`.
7. Inspect all generated `Build/` files and their **multiple progressive
   .data chunks**. The R2 Worker route maps Unity path
   `/unity/game-01/...` to `karlo-webgl-builds/game-01/...`.
   Upload **all** chunks and dependencies with correct Brotli content-encoding.
   Do not reuse old hashed loader/index metadata from Unity 6000.4.
8. Use companion PR:
   <https://github.com/karlokalinic/game-dev-console/pull/37>,
   whose delivery.js explicitly prevents stale indefinite caching of
   unhashed progressive chunks.
   GitHub Actions runner for that private repo previously failed before
   executing steps. Resolve/enable the runner or use another authenticated
   Cloudflare release path; do not claim an unexecuted deployment.
9. Browser QA: initial Boot intro and transition; MainMenu controls;
   SampleScene spawn and input, HUD, camera, first active chapter, PlayMaker
   triggered states, audio and late-chapter transitions; window resize,
   Firefox/Safari Chromium/WebGL2; track downloads, JS heap and FPS.
   Compare startup transfer against existing ~113.7MB baseline.

**Safety:** The existing public game must remain intact until steps 6–9 pass.

## Source branch

`release/gradomraz-webgl-progressive-20261118` (stacked on the Unity 6.6
migration branch). This release branch is intentionally not production.
