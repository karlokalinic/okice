#!/usr/bin/env node

import fs from 'node:fs';
import path from 'node:path';

const rootArg = process.argv.find((value, index, list) => list[index - 1] === '--root') || '.';
const root = path.resolve(rootArg);
const failures = [];
const warnings = [];

function file(rel) {
  return path.join(root, ...rel.split('/'));
}

function read(rel) {
  const target = file(rel);
  if (!fs.existsSync(target)) {
    failures.push(`Missing required file: ${rel}`);
    return '';
  }
  return fs.readFileSync(target, 'utf8');
}

function requireText(rel, text, label = text) {
  const content = read(rel);
  if (content && !content.includes(text)) failures.push(`${rel}: missing ${label}`);
}

const version = read('ProjectSettings/ProjectVersion.txt');
if (version && !/^m_EditorVersion:\s*6000\.4\.0f1\s*$/m.test(version)) {
  failures.push('ProjectVersion.txt must pin Unity 6000.4.0f1 for deterministic mobile validation.');
}

const manifest = read('Packages/manifest.json');
if (manifest) {
  try {
    const json = JSON.parse(manifest);
    if (!json.dependencies?.['com.unity.render-pipelines.universal']) {
      failures.push('URP package is missing from Packages/manifest.json.');
    }
  } catch (error) {
    failures.push(`Packages/manifest.json is invalid JSON: ${error.message}`);
  }
}

const scenes = read('ProjectSettings/EditorBuildSettings.asset');
for (const scene of ['Assets/Scenes/Boot.unity', 'Assets/Scenes/MainMenu.unity', 'Assets/Scenes/SampleScene.unity']) {
  if (scenes && !scenes.includes(`path: ${scene}`)) failures.push(`EditorBuildSettings is missing ${scene}.`);
}
if (scenes) {
  const enabledPaths = [...scenes.matchAll(/enabled:\s*1[\s\S]*?path:\s*([^\r\n]+)/g)].map(match => match[1].trim());
  if (enabledPaths.length && enabledPaths[0] !== 'Assets/Scenes/Boot.unity') {
    failures.push(`Boot.unity must be the first enabled scene; found ${enabledPaths[0]}.`);
  }
}

const buildFile = 'Assets/Editor/KARLOLEGEND_GRADOMRAZ/BuildGame.cs';
for (const [token, label] of [
  ['PROJECT:Mobile', 'dedicated Mobile WebGL template'],
  ['MobilePipelineAssetPath', 'dedicated mobile render-pipeline path'],
  ['AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>', 'mobile pipeline asset load'],
  ['QualitySettings.renderPipeline = mobilePipeline', 'quality-level mobile pipeline override'],
  ['GraphicsSettings.defaultRenderPipeline = mobilePipeline', 'default mobile pipeline override'],
  ['QualitySettings.renderPipeline = previousQualityPipeline', 'quality pipeline restoration'],
  ['GraphicsSettings.defaultRenderPipeline = previousDefaultPipeline', 'default pipeline restoration'],
  ['WebGLCompressionFormat.Brotli', 'Brotli compression'],
  ['dataCaching = true', 'WebGL data caching'],
  ['initialMemorySize = 512', '512 MB initial heap baseline'],
  ['maximumMemorySize = 1024', '1024 MB maximum heap guardrail'],
  ['geometricMemoryGrowthStep = 0.10f', 'smaller geometric heap growth step'],
  ['memoryGeometricGrowthCap = 64', '64 MB heap growth cap'],
  ['WebGLPowerPreference.LowPower', 'low-power GPU preference'],
  ['WebGLTextureSubtarget.ASTC', 'ASTC mobile texture subtarget'],
  ['WasmCodeOptimization.DiskSizeLTO', 'Wasm disk-size LTO'],
  ['nameFilesAsHashes = true', 'hashed immutable build filenames'],
  ['threadsSupport = false', 'single-thread compatibility policy'],
  ['decompressionFallback = false', 'server-side Brotli requirement'],
  ['KARLOLEGEND_MOBILE_WEB', 'mobile scripting define'],
  ['WriteMobileHostingFiles()', 'hosting metadata generation'],
  ['finally', 'settings restoration guard']
]) requireText(buildFile, token, label);

const mobileRenderer = 'Assets/MonoBehaviour/Mobile_Renderer.asset';
for (const [token, label] of [
  ['m_RendererFeatures: []', 'renderer-feature-free mobile renderer'],
  ['m_ShadowTransparentReceive: 0', 'transparent shadow receiving disabled'],
  ['m_RenderingMode: 0', 'classic Forward rendering path'],
  ['m_DepthPrimingMode: 0', 'depth priming disabled']
]) requireText(mobileRenderer, token, label);
requireText(`${mobileRenderer}.meta`, 'guid: cec103d4d6f94f2c89a8b5c4ef90919d', 'stable Mobile_Renderer GUID');

const mobilePipeline = 'Assets/MonoBehaviour/Mobile_RPAsset.asset';
for (const [token, label] of [
  ['guid: cec103d4d6f94f2c89a8b5c4ef90919d', 'Mobile_Renderer reference'],
  ['m_RequireDepthTexture: 0', 'depth texture disabled by default'],
  ['m_RequireOpaqueTexture: 0', 'opaque texture disabled'],
  ['m_SupportsHDR: 0', 'HDR disabled'],
  ['m_RenderScale: 0.75', 'mobile render-scale baseline'],
  ['m_MainLightShadowmapResolution: 512', 'small main-light shadow atlas'],
  ['m_AdditionalLightsPerObjectLimit: 1', 'single additional light per object'],
  ['m_AdditionalLightShadowsSupported: 0', 'additional-light shadows disabled'],
  ['m_ShadowDistance: 8', 'short mobile shadow distance'],
  ['m_ShadowCascadeCount: 1', 'single shadow cascade'],
  ['m_SoftShadowsSupported: 0', 'soft shadows disabled'],
  ['m_SupportsDynamicBatching: 1', 'dynamic batching enabled'],
  ['m_ColorGradingLutSize: 16', 'smaller color grading LUT']
]) requireText(mobilePipeline, token, label);
requireText(`${mobilePipeline}.meta`, 'guid: e446ee0aa5ae41c680427fabbb7ace72', 'stable Mobile_RPAsset GUID');

const bridgeFile = 'Assets/Scripts/MobileWeb/MobileWebInputBridge.cs';
for (const [token, label] of [
  ['KARLOLEGEND_MOBILE_WEB', 'mobile-only compile guard'],
  ['SetMove', 'touch movement bridge'],
  ['AddLookDelta', 'touch look bridge'],
  ['PressButton', 'touch button bridge'],
  ['ResetInput', 'input reset bridge'],
  ['ConfigureProfile', 'runtime quality profile bridge'],
  ['SetPageVisibility', 'page visibility bridge'],
  ['EnterEmergencyMode', 'automatic emergency degradation'],
  ['EmergencyFallbackFps = 24', '24 FPS final safety fallback'],
  ['Time.fixedDeltaTime = 1f / MobilePhysicsHz', '30 Hz mobile physics cadence'],
  ['Time.maximumDeltaTime = 1f / 15f', 'physics catch-up guardrail'],
  ['AudioSettings.GetConfiguration()', 'mobile audio configuration read'],
  ['MobileRealVoices = 24', 'mobile real audio voice budget'],
  ['MobileVirtualVoices = 96', 'mobile virtual audio voice budget'],
  ['AudioSettings.Reset(config)', 'mobile audio voice configuration apply'],
  ['QualitySettings.globalTextureMipmapLimit', 'profile-based texture memory budget'],
  ['RenderingMode.Forward', 'classic Forward renderer policy'],
  ['supportsCameraOpaqueTexture = false', 'opaque-texture bandwidth reduction'],
  ['supportsHDR = false', 'mobile HDR render-target disable'],
  ['allowPostProcessing = false', 'lightweight default profile'],
  ['allowDepthTexture = false', 'depth-free default profile'],
  ['Application.targetFrameRate = activeTargetFps', 'stable frame cap'],
  ['lightBaselines', 'reversible authored light-range policy']
]) requireText(bridgeFile, token, label);

const pixelBridgeFile = 'Assets/Plugins/Assembly-CSharp-firstpass/PixelCrushers/MobileWebPixelCrushersInput.cs';
for (const [token, label] of [
  ['InputDeviceManager.GetButtonDelegate', 'PixelCrushers button delegate hook'],
  ['MobileWebInputBridge, Assembly-CSharp', 'late-bound gameplay bridge type'],
  ['Delegate.CreateDelegate', 'cached cross-assembly button delegate'],
  ['manager.GetButtonDown = GetButtonDownProxy', 'PixelCrushers mobile button injection'],
  ['originalGetButtonDown(buttonName)', 'desktop/original input preservation']
]) requireText(pixelBridgeFile, token, label);
requireText(`${pixelBridgeFile}.meta`, 'fileFormatVersion: 2', 'Unity .meta for PixelCrushers mobile bridge');

const mouseLookFile = 'Assets/Scripts/Assembly-CSharp/HutongGames/PlayMaker/Actions/MouseLook.cs';
for (const [token, label] of [
  ['#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR', 'mobile-only look branch'],
  ['float input = MobileWebInputBridge.LookDelta.x * MobileLookScale;', 'touch-only mobile horizontal look'],
  ['float input = MobileWebInputBridge.LookDelta.y * MobileLookScale;', 'touch-only mobile vertical look'],
  ['float input = Input.GetAxis("Mouse X");', 'desktop horizontal mouse preservation'],
  ['float input = Input.GetAxis("Mouse Y");', 'desktop vertical mouse preservation']
]) requireText(mouseLookFile, token, label);

const templateFile = 'Assets/WebGLTemplates/Mobile/index.html';
for (const [token, label] of [
  ['viewport-fit=cover', 'safe-area viewport support'],
  ['{{{ LOADER_FILENAME }}}', 'Unity loader placeholder'],
  ['{{{ DATA_FILENAME }}}', 'Unity data placeholder'],
  ['{{{ FRAMEWORK_FILENAME }}}', 'Unity framework placeholder'],
  ['{{{ CODE_FILENAME }}}', 'Unity Wasm placeholder'],
  ['createUnityInstance', 'Unity bootstrap'],
  ['ConfigureProfile', 'profile handoff'],
  ['SetPageVisibility', 'visibility handoff'],
  ['INPUT_INTERVAL_MS = 1000 / 30', '30 Hz JS-to-WASM input throttle'],
  ['queueLook', 'batched touch look input'],
  ['queueMove', 'batched touch movement input'],
  ['ResetInput', 'browser-to-Unity input reset'],
  ["canvas.addEventListener('pointerdown'", 'canvas-level gesture input'],
  ['primary-button', 'Fire1 action control'],
  ['pointercancel', 'touch cancellation handling'],
  ['navigator.maxTouchPoints', 'touch capability detection']
]) requireText(templateFile, token, label);

const template = read(templateFile);
if (template.includes('id="move-zone"') || template.includes('id="look-zone"')) {
  failures.push('Mobile template must not restore full-screen pointer-intercepting move/look overlay zones.');
}
if (template.includes('backdrop-filter')) {
  failures.push('Mobile template must not add backdrop-filter compositing over the WebGL canvas.');
}
if (template.includes('60 FPS')) {
  failures.push('Mobile template must not advertise a 60 FPS phone profile while stable 30 FPS is the release target.');
}

requireText('Assets/Scripts/MobileWeb/MobileWebInputBridge.cs.meta', 'fileFormatVersion: 2', 'Unity .meta for mobile bridge');
requireText('Assets/WebGLTemplates/Mobile.meta', 'fileFormatVersion: 2', 'Unity .meta for mobile template folder');
requireText('Assets/WebGLTemplates/Mobile/index.html.meta', 'fileFormatVersion: 2', 'Unity .meta for mobile template index');

const workflow = read('.github/workflows/mobile-webgl.yml');
if (workflow) {
  if (!/workflow_dispatch:\s*/.test(workflow)) failures.push('Mobile workflow must remain manually dispatchable.');
  if (/\bpull_request\s*:/.test(workflow) || /\bpush\s*:/.test(workflow)) {
    failures.push('Mobile workflow must remain manual-only; automatic PR/push runs create notification spam.');
  }
  if (/\blfs:\s*true\b/.test(workflow)) warnings.push('Workflow asks actions/checkout to download Git LFS.');
}

console.log('=== Mobile WebGL source preflight ===');
console.log(`Root: ${root}`);
console.log(`Failures: ${failures.length}`);
console.log(`Warnings: ${warnings.length}`);
for (const warning of warnings) console.warn(`WARN: ${warning}`);
for (const failure of failures) console.error(`FAIL: ${failure}`);

if (process.env.GITHUB_STEP_SUMMARY) {
  const lines = [
    '## Mobile WebGL source preflight',
    '',
    `- Failures: **${failures.length}**`,
    `- Warnings: **${warnings.length}**`,
  ];
  if (failures.length) {
    lines.push('', '### Failures');
    for (const failure of failures) lines.push(`- ${failure}`);
  }
  if (warnings.length) {
    lines.push('', '### Warnings');
    for (const warning of warnings) lines.push(`- ${warning}`);
  }
  fs.appendFileSync(process.env.GITHUB_STEP_SUMMARY, `${lines.join('\n')}\n`);
}

if (failures.length) process.exit(1);
console.log('Source preflight passed.');
