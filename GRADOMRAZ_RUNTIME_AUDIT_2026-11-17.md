# GRADOMRAZ — runtime, loading, shader and scene audit
**17. 11. 2026. · Unity 6000.4.0f1 · URP 17.4 · WebGL / Cloudflare R2**

This report separates **measured shipping bytes**, **serialized scene state**, **current runtime visibility**, and **unverified optimization hypotheses**.

## What was actually scanned

- Every path in the `okice` master checkout (6,675 tracked files); extension sizes and import metadata are summarized by the reproducible, read-only scripts in `Tools/`.
- All three enabled build scenes (`Boot`, `MainMenu`, `SampleScene`) and their serialized component inventories.
- A hierarchy-aware pass over the 85,660 serialized component blocks in `SampleScene` (including disabled ancestor GameObjects).
- Texture and audio importer settings, material shader references and .meta GUID duplication.
- Live Cloudflare response headers and transfer size of the current .data, .wasm and .framework.js files.
- Shader candidates in `STATIKA`, `BeforetheTHUNDER` and `My-project-2026-09-15_18-31-03`.

This is an automated asset inventory, **not** a Unity Profiler capture or a line-by-line manual review of every third-party C# file.

## Live WebGL transfer: measured from the Worker

| Asset | Transferred | Evidence |
|---|---:|---|
| .data | 101,895,274 bytes | HTTP 200, no Content-Encoding |
| .wasm | 11,676,344 bytes | HTTP 200, Content-Encoding: br; uncompressed 38,671,650 bytes |
| .framework.js | 103,124 bytes | HTTP 200, Content-Encoding: br |
| **Total of these three** | **113,674,742 bytes** | Excludes small loader/HTML and optional assets |

The Unity source build preset already requests Brotli; the currently published Cloudflare build is served through a **legacy uncompressed recovery workflow** in `karlokalinic/game-dev-console/.github/workflows/deploy-gradomraz.yml`. That workflow was previously invoked on ordinary Worker changes, causing `.data` to be restored as raw.

A local Brotli level-7 round-trip benchmark on the exact 101,895,274-byte live .data asset returned **81,081,533 bytes**, a verified **20.4% .data reduction**, or **20,813,741 fewer bytes transferred** if deployed with a correct `Content-Encoding: br`. Other assets remaining equal, the three-asset subtotal would fall to approximately **92.86 MB** (decimal). This is a measured hypothetical payload; the public game has **not** yet been migrated.

The live HTML also constructs duplicate `?delivery=` query parameters in its effective resource URLs. The original Unity template's helper is fixed on the audit branch; the production R2 index still needs replacement as part of a validated build.

## Source and scene inventory

| Measure | Count |
|---|---:|
| Tracked files | 6,675 |
| Raw Git blob contents | ~288 MB (decimal; includes version-control pointers where applicable) |
| Expanded checkout files | ~448 MB (decimal; including materialized Git LFS content) |
| Imported PNGs | 220 |
| Texture importers with `streamingMipmaps: 1` | **0 / 220** |
| Texture importers with mipmaps enabled | 127 / 220 |
| Audio importers | 146 |
| Audio `Decompress On Load` | 130 |
| Audio preload enabled | 78 |
| Materials | 184 |
| Duplicate .meta GUIDs | **0** |

The 140 materials with the same shader GUID are **not** necessarily missing shaders: URP/package shaders are external to the local repo. Do not replace them based on missing local GUID matches.

### Crucial correction: enabled is not necessarily visible

| SampleScene serialized state | Count |
|---|---:|
| GameObjects | 22,333 |
| MeshRenderers | 19,082 |
| MeshRenderers with `m_Enabled: 1` | 18,930 |
| Enabled MeshRenderers whose own GameObject is active | 18,838 |
| **Enabled MeshRenderers active through the *whole parent hierarchy*** | **454** |
| Enabled active-in-hierarchy shadow-casting renderers | 415 |
| Serialized Light components | 109 |
| Light components active through hierarchy | 18 |
| Scene GameObjects with static flags | 337 |

**Do not infer 19,000 live draw calls.** Many renderers are saved below inactive chapter/level roots. Largest disabled ancestor roots by child renderers include `Outside` (9,790), `Bridge` (5,944), `MaddieRoom` (1,456), `Side2` (592) and `Bunker` (157). PlayMaker or other gameplay logic may activate these during progression. Deleting or merging them blindly could destroy the game.

For the currently active-in-hierarchy renderers, there are 100 distinct mesh/material combinations; 411 of 454 renderers belong to repeated combinations. Confirm real draw calls and batching with Unity Frame Debugger before rewriting scene serialization.

## Quality configuration

The committed PC URP asset has 4x MSAA, 4096 main-light shadows, 2048 additional-light shadows, four cascades, 50-unit shadow distance, LOD bias 3, render scale 1 and SRP Batcher enabled. Its `m_SupportsHDR` is **0**, contrary to an older QUALITY_UPGRADE_LOG description. Do not enable HDR output as a blind visual fix.

## Shader reuse audit: which cross-project assets are real?

| Other project shader | Verdict | Best potential use |
|---|---|---|
| `BeforetheTHUNDER/Assets/OG/Resources/Humak/HumakGlass.shader` | Real URP shader; target 3.5 | Opt-in glass, damaged/dirty translucent props after scene/material review |
| `BeforetheTHUNDER/Assets/OG/Resources/Humak/HumakLitPaper.shader` | Real URP shader; target 3.5 | Lit paper, signs and readable close-up documents |
| `My-project-2026-09-15_18-31-03/Assets/Shader/RecoveredTriplanarDynamic.shader` | URP shader **dependent on RecoveredSurface.hlsl** | Potential concrete/industrial triplanar mapping after dependency/compile audit |
| `My-project-2026-09-15_18-31-03/Assets/Shader/RecoveredGlass.shader` | URP, target 5.0 and custom includes | **Not a safe WebGL drop-in** |
| `STATIKA/Assets/Resources/fogplus.shader`, `outlineplus.shader`, `pixelnostalgia.shader` | **Fullscreen copy placeholders**, not recovered effects | Do not import |
| `STATIKA/Assets/Shader/Custom_RainShader.shader` | Legacy Built-In pipeline | Port to URP first |
| `STATIKA/Assets/Shader/FX_SimpleWater4.shader` | Legacy Surface Shader | Port to URP first |
| `BeforetheTHUNDER` HDRP shaders | Wrong render pipeline | Do not import |

The strongest visual gains are likely to come from **lighting composition, high-quality material layering, atmospheric depth, wet surfaces, occlusion and authored post-processing** at a known frame budget, not from importing dozens of unrelated shaders. A 100x immersion improvement or AAA fidelity is not numerically verifiable from source inspection.

## Recommended implementation order

1. **Delivery:** Fix double query strings; test strict single-layer Brotli output; remove automatic destructive raw R2 republishing. Publish compressed assets only after verifying content encoding, Unity startup, dialogue, WebGL 2, mobile touch and rollback.
2. **Profile before modifying:** Capture startup timings, WebGL heap, CPU/GPU frame time, draw calls, shader variants, and texture memory for *Boot, MainMenu, SampleScene* and at least one later chapter where `Outside`/`Bridge` are enabled.
3. **Split content by chapter (high-impact, high-risk):** Design an additive scene/asset-loading migration, preserve PlayMaker object references and transitions. Do not delete inactive roots.
4. **Texture/audio optimization (medium-impact):** Enable texture mip streaming only where appropriate and supported in the target WebGL build; review 2K textures and DXT import settings individually. Use audio streaming/compressed-in-memory for long ambiences after latency tests. Protect short UI/dialogue clips.
5. **Rendering:** Use real visibility data to decide GPU instancing, LOD groups, occlusion culling and selective shadow distances. Avoid maxing every setting.
6. **Atmosphere:** Implement a small, benchmarked URP weather/material preview using a copy of a proven compatible shader, then assign it to a reviewed subset of surfaces and compare matched screenshots/FPS.

## Reproduce the inventory

From the repo root (Node.js 20+; neither script edits assets):

```powershell
node Tools/gradomraz-audit-source.mjs . gradomraz-source-audit.json
node Tools/gradomraz-audit-renderers.mjs Assets/Scenes/SampleScene.unity
```

The source scanner writes a local JSON report. The renderer scanner prints hierarchy-aware diagnostics. Both operate directly on serialized Unity YAML; Unity may still override components/visibility at runtime.

## Validation status

- Cloudflare live HEAD/GET measured, and the actual .data payload Brotli round-trip checked.
- Four Brotli normalization tests pass in Node; workflow YAML parses.
- No Unity player was rebuilt or deployed from this audit branch. The connected Windows machine has Unity 6000.6.9f1; the project requires 6000.4.0f1 for a non-upgrading validation build.
- No FPS or screenshot-based AAA comparison is asserted without an actual GPU/browser profile.
