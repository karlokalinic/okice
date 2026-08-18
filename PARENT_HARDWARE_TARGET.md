# OKICE / GRADOMRAZ — parent hardware performance baseline

## Why this exists

The game is intended to be playable by non-gaming family/parent users, not only on the development PC. A visually modest scene must not silently inherit a modern gaming-desktop render workload.

The current low-end acceptance reference is approximately:

- Windows laptop;
- Intel Core i3-1005G1-class CPU (2 cores / 4 logical threads);
- Intel UHD integrated graphics;
- 4 GB system RAM;
- SSD;
- ordinary Chrome/WebGL or native Windows player.

This is intentionally a hard baseline. It does not mean every setting must look identical to the high-end PC path. It means the game must choose a compatible workload automatically instead of requiring the player to understand graphics settings.

## Root problem found

The old project effectively had one desktop quality level named `PC`. That path was substantially heavier than the visible art style suggested:

- Forward+ renderer;
- depth texture copy enabled;
- opaque texture copy enabled;
- HDR enabled;
- SSAO renderer feature active;
- full internal render scale;
- high shadow distance;
- multiple cascades;
- large shadow maps;
- additional-light shadows;
- soft shadows;
- reflection-probe features;
- high LOD bias;
- very long terrain tree/detail distances;
- no texture mip streaming baseline.

The first mobile optimization work did not fix ordinary desktop WebGL or the native Windows player because the lightweight pipeline was only selected by the dedicated mobile build method. That architectural separation has now been corrected.

## Parent compatibility policy

### Ordinary desktop WebGL

`Build WebGL for itch.io` is now a compatibility-first build:

- `LowSpec_RPAsset` instead of the PC Forward+ render pipeline;
- classic Forward renderer;
- HDR / depth copy / opaque copy disabled;
- no additional-light shadows;
- one additional light per object;
- short main-light shadows;
- no soft shadows;
- baseline render scale around 0.72;
- DPR capped at 1 by the HTML shell;
- 30 FPS runtime target;
- DXT texture subtarget for desktop WebGL;
- 384 MB initial WebAssembly heap and 1024 MB ceiling;
- bounded geometric memory growth;
- Brotli, LTO, hashed output and no development diagnostics.

The browser shell can additionally send an `eco` hint when `deviceMemory`, `hardwareConcurrency`, Data Saver, or network information indicates constrained hardware.

### Native Windows

A runtime compatibility governor activates automatically when the machine is in a constrained class such as:

- <= 6 GB reported system RAM; or
- Intel integrated UHD / HD Graphics / Iris with <= 4 logical CPU threads and <= 8 GB RAM.

On a severe target it applies, before gameplay:

- 30 FPS cap;
- `LowSpec_RPAsset` runtime SRP switch;
- render scale about 0.62, adaptive floor 0.50;
- MSAA off;
- post-processing off;
- depth/opaque copies off;
- HDR off;
- punctual shadows off;
- directional shadows off on severe hardware;
- punctual light ranges capped;
- texture mip limit increased to lower texture residency;
- LOD bias reduced;
- realtime reflection probes off;
- terrain tree distance capped around 150 m;
- terrain detail distance capped around 30 m;
- earlier terrain basemap transition;
- higher terrain pixel error to reduce far geometry;
- looping particle systems capped around 96 simultaneous particles per system.

One-shot gameplay particle effects are not capped by this policy.

## Adaptive fallback

Compatibility mode samples frame pacing in two-second windows.

If average frame time cannot sustain the 30 FPS budget:

1. internal render scale drops in 0.05 steps;
2. at the floor, remaining realtime shadow work is removed;
3. the system remains at the lower stable workload rather than continuously oscillating upward.

The dedicated mobile target keeps its own separate governor and render pipeline. Parent/desktop compatibility code is explicitly excluded from `KARLOLEGEND_MOBILE_WEB` builds so the two systems cannot overwrite one another.

## Telemetry

Ordinary WebGL and low-spec native Windows players emit a compact line every ten seconds:

`[CompatibilityTelemetry]`

It includes:

- scene;
- average FPS / average frame time;
- p95 frame time;
- worst sampled frame time;
- target FPS;
- active URP render scale;
- shadow distance;
- Unity allocated/reserved memory;
- memory signal;
- CPU logical thread count;
- GPU name;
- scene transition count.

Dedicated mobile builds continue to use `[MobileWebTelemetry]` instead.

## Acceptance gate

A parent-compatible candidate is not considered finished until the low-end reference class can complete this route:

1. Boot -> Main Menu;
2. enter `SampleScene`;
3. walk and look continuously;
4. use interaction inputs;
5. complete a dialogue path;
6. traverse a light-heavy / visually dense area;
7. play for at least 10 minutes;
8. no progressive stutter collapse, tab reload or native player freeze;
9. telemetry shows frame pacing centered around the 30 FPS budget rather than sustained long-frame operation.

If this still fails after the compatibility renderer is active, the next optimization must be evidence-driven from Build Report / telemetry: resident textures, geometry/draw calls, overdraw, audio residency, physics or a specific gameplay system. The response must not be another blind global quality reduction.

## One-command local builds

Build both parent targets:

```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\Performance\parent.ps1 Both
```

Build only the browser target:

```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\Performance\parent.ps1 Web
```

Serve the generated browser target on the LAN:

```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\Performance\parent.ps1 Serve
```

Generated build logs are placed under `Builds/Logs`, and source/scene/payload reports under `Builds/Reports`.
