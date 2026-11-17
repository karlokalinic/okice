# GRADOMRAZ — Visual Streaming + URP Material Lab (experimental)

**Status:** source implementation on an audit branch, not a replacement for the live WebGL player. Target authoring Editor: **Unity 6000.4.0f1**, URP **17.4.0**. Use a duplicate working tree for the first Unity import. The Windows machine inspected on 17 November 2026 only had **Unity 6000.6.9f1**, so the exact target editor compile/build has **not** been verified.

## Why entire chapter roots must not be moved

A serialized YAML reference audit of `Assets/Scenes/SampleScene.unity` found:

| Candidate root | GameObjects | Serialized block count | Incoming references | Outgoing references |
|---|---:|---:|---:|---:|
| `Bridge` (fileID 8215) | 6,604 | 25,414 | 9 | 7 |
| `Outside` (fileID 17856) | 11,256 | 42,208 | 12 | 2 |
| `MaddieRoom` (fileID 12239) | 1,699 | 6,549 | 20 | 12 |
| `Side2` (fileID 15480) | 772 | 3,183 | 42 | 25 |

The counts include ordinary transform-parent edges. Non-transform references from PlayMaker objects such as `JetCrash`, `Unloader`, `BunkerExitTrigger` and `BridgeEventsTrigger` are confirmed. Moving these gameplay roots to another scene would break normal serialized references.

## Real pure-visual candidates within those roots

The read-only `Tools/gradomraz-visual-subtree-candidates.mjs` scanner found specific **visual-only** subtrees with no serialized external cross-references (transform parent-child relationships ignored in this phase):

| Candidate | Objects | MeshRenderers | Other serialized components | Cross references |
|---|---:|---:|---:|---:|
| `Bridge/buildings` (fileID 16099) | 6,337 | 5,832 | 0 | 0 |
| `Outside/backgroundbuildings (8)` (fileID 1741) | 1,056 | 972 | 0 | 0 |
| `Outside/backgroundbuildings (14)` (fileID 2423) | 1,056 | 972 | 0 | 0 |

There are numerous overlapping child candidates; do **not** sum all candidate rows. These are extraction *candidates*, not guaranteed safe migrations: dynamic `GameObject.Find`, PlayMaker strings, animator parent transforms, chapter toggles, external scripts and occlusion must be checked in Unity before a scene is edited.

**Safest first target:** a duplicate of the `Bridge/buildings` pure-render subtree, with its existing `Bridge` parent and all gameplay scripts left in `SampleScene`. The actual source scene has NOT been modified.

## 1. Reproduce preflight (read-only)

From the repository root:

```bash
node Tools/gradomraz-audit-source.mjs . gradomraz-source-audit.json
node Tools/gradomraz-audit-renderers.mjs Assets/Scenes/SampleScene.unity
node Tools/gradomraz-scene-reference-audit.mjs Assets/Scenes/SampleScene.unity ./scene-refscan.json Outside Bridge MaddieRoom Side2 Bunker
node Tools/gradomraz-visual-subtree-candidates.mjs Assets/Scenes/SampleScene.unity Bridge
node Tools/gradomraz-visual-subtree-candidates.mjs Assets/Scenes/SampleScene.unity Outside
```

The scanners do not mutate Unity data. Cross-scene links for original PlayMaker objects are still unsupported; do not convert `Bridge`, `Outside` or `MaddieRoom` into additive scenes.

## Tested non-destructive Bridge extraction (new Editor command)

In the Unity Editor open a **saved scene**, then select:

**Tools → GRADOMRAZ → Streaming → Stage Bridge Buildings (Non-Destructive)**

The command is deliberately restricted to the **exact source layout** (6,337 objects and 5,832 MeshRenderers). It checks the child subtree contains no nonvisual components; if the hierarchy has changed, it refuses to proceed until the reference audit is updated.

It creates the following two **new assets**, while leaving `Assets/Scenes/SampleScene.unity` byte-for-byte untouched:

- `Assets/GRADOMRAZ/StreamingScenes/BridgeBackground.unity` — isolated copied render-only geometry for a downloadable AssetBundle.
- `Assets/GRADOMRAZ/ExperimentalBuilds/SampleScene.unity` — copied gameplay scene **without** that geometry, with a disabled-by-empty-URL `GradomrazRemoteVisualRegion` under Bridge. The scene filename stays `SampleScene.unity` so any existing logic relying on that scene name is not silently changed.

**Before using the experimental scene as a WebGL build:** build and publish the hashed BridgeBackground bundle; set its exact URL in the Bridge streaming component; then manually switch that build profile's gameplay scene path from the canonical scene to the **experimental** scene. Do not ship a build in which the streaming URL is empty: the Bridge visuals will be missing. The source gameplay scene and original PlayMaker actors remain intact.

**Verified in an isolated Unity 6000.6.9f1 / URP 17.6 scratch project** with a generated synthetic scene matching the real candidate structure: both output scenes were saved; the original sample scene SHA-256 did not change; the output visual scene retained 5,832 renderers and the experimental scene retained the streaming anchor. This is an **Editor workflow test**, not a validated GRADOMRAZ WebGL build, not a measured download reduction, and not proof of PlayMaker behavior on the real source.

The original project targets **Unity 6000.4.0f1 and URP 17.4**; compile and verify there before release. All three shader sources imported in the scratch Editor, synchronous first-pass compilation reported zero shader messages/errors under its Null graphics backend, and the material-lab preview scene was generated. WebGL fragment compilation and GPU visuals still require a graphical build.

## 2. Author a separate visual-only test scene

In **Unity 6000.4.0f1**, on a **copied working tree**:

1. Inspect one candidate hierarchy and its animation, transforms, collider behavior and dynamic references across every relevant chapter.
2. **Copy** (do not remove yet) only a fully self-contained mesh-renderer hierarchy into a newly created scene under `Assets/GRADOMRAZ/StreamingScenes/`, e.g. `BridgeBackground.unity`. Keep the copied objects at their precise original world-space transforms; the scene must contain only `Transform`, `MeshFilter` and `MeshRenderer`.
3. Exclude the new streaming scene from `EditorBuildSettings` and any addressable group included in the initial build.
4. From Unity, select **Tools → GRADOMRAZ → Streaming → Build Visual-Only WebGL Bundles**. It rejects enabled BuildSettings scenes and non-visual components; creates WebGL LZ4 bundles outside `Assets` in `_GRADOMRAZ_Streaming_Staging/`.
5. Verify the resulting `GRADOMRAZ-Streaming-Upload-Manifest.tsv`. Each published `.bundle` filename contains the first 16 hexadecimal digits of its SHA-256. The original unchanged gameplay scene remains playable.

At this point the copy provides a streaming **proof of concept**, but no startup size reduction. Main scene geometry must be removed only in a separate validated stage, after verifying its references and transitions. If the same mesh and texture are still referenced in the main build, those assets will also remain in the initial `.data`.

## 3. Upload only the test bundle (explicit, no automatic production rewrite)

The uploader is in the companion repository `karlokalinic/game-dev-console`. Its default is a **dry run**:

```bash
node scripts/upload-gradomraz-visual-bundles.mjs --stage=/absolute/path/to/_GRADOMRAZ_Streaming_Staging
```

After checking its size/hash/UnityFS results and setting the **Cloudflare secrets in your local environment**, publication of only *new*, content-hashed R2 keys is explicit:

```bash
node scripts/upload-gradomraz-visual-bundles.mjs --stage=/absolute/path/to/_GRADOMRAZ_Streaming_Staging --publish
```

The asset URL is `/unity/game-01/Streaming/gradomraz-bridgebackground.<SHA16>.bundle`. The Worker already serves `/unity/game-01/` from the `game-01/` R2 prefix. The companion change adds immutable cache headers **only** for hash-named bundles. Never publish a test into an existing filename or change the production index before QA.

## 4. Enable streaming in a test scene

Attach `GradomrazRemoteVisualRegion` to a gameplay **anchor that stays in the main scene**. Set its `assetBundleUrl` to the published hashed path. Set `loadWhenActive` only when the anchor lifecycle is understood. The loader fetches the external AssetBundle using `UnityWebRequestAssetBundle`, loads its single scene additively and reference-counts requests; optional `Release()` unloads its scene. Existing PlayMaker objects are untouched by the loader.

Load before a chapter transition to avoid visual popping. There is no automatic prefab/scene modification in this branch, and the feature is inactive unless deliberately attached and configured.

## 5. Opt-in URP visual material laboratory

**Tools → GRADOMRAZ → Visual Lab:**

1. **Validate Shader Candidates** — ensures that Unity discovers three custom URP shaders.
2. **Create Candidate Materials** — creates new assets in `Assets/GRADOMRAZ/VisualLab/` without overwriting existing material tuning.
3. **Build Isolated Preview Scene** — creates a separate `GRADOMRAZ-MaterialLab.unity` with three sample surfaces (wet concrete, paper, glass). **Not included in the player build.**

Shaders:

- `GRADOMRAZ/Atmosphere/Weathered Glass` — Fresnel, grime, controlled transparency and reflection.
- `GRADOMRAZ/Props/Lit Paper` — lit double-sided cutout paper, shadow receiving and fog.
- `GRADOMRAZ/Environment/Wet Triplanar Concrete` — world-space concrete texture with damp darkening, directional highlights and fog.

Do not wholesale replace established materials. Capture matched camera screenshots and FPS before assigning new shaders to a few relevant props. UV-less triplanar shading samples three axes per pixel; it is not free on WebGL. Avoid adding a full-screen post-processing pass until the existing CRT and URP frame budget is profiled.

## Validation gate before a release

- [ ] Import and compile shaders in **Unity 6000.4.0f1**; inspect Unity Console for unsupported passes/variants
- [ ] Build unchanged original WebGL player as a baseline
- [ ] Test standalone streamed visual scene load, same-origin R2 URL, browser compression and cache
- [ ] Verify all `JetCrash`, `Unloader`, `BridgeEventsTrigger`, `BunkerExitTrigger` and `MaddieHallwayTrigger` PlayMaker events
- [ ] Check collision, static/dynamic transforms, fog, light probes, shadows, visibility and LOD with the original scene
- [ ] Profile initial download, parse/startup time, graphics memory, WebGL heap, GPU frame time, CPU thread, draw calls and loading hitches
- [ ] Remove original visual-only duplicates **only after** verifying that runtime streams are ready in advance and gameplay references are preserved
- [ ] Run Chrome, Firefox, Edge, Safari and touch tests where applicable before updating Cloudflare production

The backend Node tests in `game-dev-console` verify hashed cache headers and upload guardrails; they do **not** prove real Unity shader compilation, FPS improvements or fully working additive gameplay.
