#!/usr/bin/env node

import fs from 'node:fs';
import path from 'node:path';

const rootArg = process.argv.find((value, index, list) => list[index - 1] === '--root') || '.';
const root = path.resolve(rootArg);
const failures = [];
const warnings = [];

const toPath = rel => path.join(root, ...rel.split('/'));
function read(rel) {
  const target = toPath(rel);
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
function requireAll(rel, entries) {
  for (const [token, label] of entries) requireText(rel, token, label);
}

const version = read('ProjectSettings/ProjectVersion.txt');
if (version && !/^m_EditorVersion:\s*6000\.4\.0f1\s*$/m.test(version)) {
  failures.push('ProjectVersion.txt must pin Unity 6000.4.0f1 for deterministic mobile validation.');
}

const manifest = read('Packages/manifest.json');
if (manifest) {
  try {
    const json = JSON.parse(manifest);
    if (!json.dependencies?.['com.unity.render-pipelines.universal']) failures.push('URP package is missing.');
  } catch (error) {
    failures.push(`Packages/manifest.json is invalid JSON: ${error.message}`);
  }
}

const scenes = read('ProjectSettings/EditorBuildSettings.asset');
for (const scene of ['Assets/Scenes/Boot.unity', 'Assets/Scenes/MainMenu.unity', 'Assets/Scenes/SampleScene.unity']) {
  if (scenes && !scenes.includes(`path: ${scene}`)) failures.push(`EditorBuildSettings is missing ${scene}.`);
}
if (scenes) {
  const enabled = [...scenes.matchAll(/enabled:\s*1[\s\S]*?path:\s*([^\r\n]+)/g)].map(match => match[1].trim());
  if (enabled.length && enabled[0] !== 'Assets/Scenes/Boot.unity') failures.push(`Boot.unity must be first; found ${enabled[0]}.`);
}

requireAll('Assets/Editor/KARLOLEGEND_GRADOMRAZ/BuildGame.cs', [
  ['PROJECT:Mobile', 'dedicated Mobile template'],
  ['MobilePipelineAssetPath', 'dedicated mobile pipeline path'],
  ['AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>', 'mobile pipeline load'],
  ['QualitySettings.renderPipeline = mobilePipeline', 'quality pipeline override'],
  ['GraphicsSettings.defaultRenderPipeline = mobilePipeline', 'default pipeline override'],
  ['QualitySettings.renderPipeline = previousQualityPipeline', 'quality pipeline restoration'],
  ['GraphicsSettings.defaultRenderPipeline = previousDefaultPipeline', 'default pipeline restoration'],
  ['WebGLCompressionFormat.Brotli', 'Brotli'],
  ['dataCaching = true', 'data caching'],
  ['initialMemorySize = 512', '512 MB initial heap'],
  ['maximumMemorySize = 1024', '1024 MB maximum heap'],
  ['geometricMemoryGrowthStep = 0.10f', '10% heap growth'],
  ['memoryGeometricGrowthCap = 64', '64 MB growth cap'],
  ['WebGLPowerPreference.LowPower', 'low-power preference'],
  ['WebGLTextureSubtarget.ASTC', 'ASTC'],
  ['WasmCodeOptimization.DiskSizeLTO', 'Wasm LTO'],
  ['nameFilesAsHashes = true', 'hashed output'],
  ['threadsSupport = false', 'single-thread compatibility'],
  ['decompressionFallback = false', 'native Brotli hosting'],
  ['KARLOLEGEND_MOBILE_WEB', 'mobile scripting define'],
  ['finally', 'settings restoration guard']
]);

requireAll('Assets/MonoBehaviour/Mobile_Renderer.asset', [
  ['m_RendererFeatures: []', 'feature-free renderer'],
  ['m_ShadowTransparentReceive: 0', 'transparent shadow receive off'],
  ['m_RenderingMode: 0', 'classic Forward renderer'],
  ['m_DepthPrimingMode: 0', 'depth priming off']
]);
requireText('Assets/MonoBehaviour/Mobile_Renderer.asset.meta', 'guid: cec103d4d6f94f2c89a8b5c4ef90919d', 'stable Mobile_Renderer GUID');

requireAll('Assets/MonoBehaviour/Mobile_RPAsset.asset', [
  ['guid: cec103d4d6f94f2c89a8b5c4ef90919d', 'Mobile_Renderer reference'],
  ['m_RequireDepthTexture: 0', 'depth off'],
  ['m_RequireOpaqueTexture: 0', 'opaque texture off'],
  ['m_SupportsHDR: 0', 'HDR off'],
  ['m_RenderScale: 0.75', 'render scale baseline'],
  ['m_MainLightShadowmapResolution: 512', 'small main shadowmap'],
  ['m_AdditionalLightsPerObjectLimit: 1', 'one additional light'],
  ['m_AdditionalLightShadowsSupported: 0', 'punctual shadows off'],
  ['m_ShadowDistance: 8', 'short shadow distance'],
  ['m_ShadowCascadeCount: 1', 'single cascade'],
  ['m_SoftShadowsSupported: 0', 'soft shadows off'],
  ['m_SupportsDynamicBatching: 1', 'dynamic batching'],
  ['m_ColorGradingLutSize: 16', 'small LUT']
]);
requireText('Assets/MonoBehaviour/Mobile_RPAsset.asset.meta', 'guid: e446ee0aa5ae41c680427fabbb7ace72', 'stable Mobile_RPAsset GUID');

requireAll('Assets/Scripts/MobileWeb/MobileWebInputBridge.cs', [
  ['SetMove', 'movement bridge'],
  ['AddLookDelta', 'look bridge'],
  ['PressButton', 'button bridge'],
  ['SetPageVisibility', 'page lifecycle bridge'],
  ['EmergencyFallbackFps = 24', '24 FPS emergency fallback'],
  ['Time.fixedDeltaTime = 1f / MobilePhysicsHz', '30 Hz physics'],
  ['Time.maximumDeltaTime = 1f / 15f', 'physics catch-up guard'],
  ['MobileRealVoices = 24', 'audio real-voice budget'],
  ['MobileVirtualVoices = 96', 'audio virtual-voice budget'],
  ['QualitySettings.globalTextureMipmapLimit', 'texture mip budget'],
  ['RenderingMode.Forward', 'Forward runtime guard'],
  ['supportsCameraOpaqueTexture = false', 'opaque texture runtime off'],
  ['supportsHDR = false', 'HDR runtime off'],
  ['allowPostProcessing = false', 'light default profile'],
  ['allowDepthTexture = false', 'depth-free default'],
  ['lightBaselines', 'light range/shadow policy']
]);

requireAll('Assets/Scripts/MobileWeb/MobileWebSceneBudgetController.cs', [
  ['ReauditIntervalSeconds = 2f', 'low-frequency scene budget'],
  ['main.loop', 'looping-particle-only cap'],
  ['main.maxParticles = capped', 'particle cap application'],
  ['Application.targetFrameRate <= 24', 'emergency particle cap'],
  ['ApplySafeStaticLightCulling', 'safe static light distance culling'],
  ['light.gameObject.isStatic', 'static-only light culling'],
  ['TryGetComponent<MonoBehaviour>', 'scripted light protection']
]);
if (read('Assets/Scripts/MobileWeb/MobileWebSceneBudgetController.cs').includes('updateWhenOffscreen = false')) {
  failures.push('Scene budget must not disable SkinnedMeshRenderer.updateWhenOffscreen automatically; it can alter offscreen animation updates.');
}

requireAll('Assets/Scripts/MobileWeb/MobileWebTelemetry.cs', [
  ['ReportIntervalSeconds = 10f', '10 second telemetry cadence'],
  ['SampleCapacity = 300', 'bounded frame sample buffer'],
  ['Profiler.GetTotalAllocatedMemoryLong()', 'allocated memory telemetry'],
  ['Profiler.GetTotalReservedMemoryLong()', 'reserved memory telemetry'],
  ['p95Ms=', 'p95 frame timing'],
  ['renderScale=', 'render scale telemetry'],
  ['[MobileWebTelemetry]', 'console prefix']
]);

requireAll('Assets/Scripts/MobileWeb/MobileWebBrowserModeBridge.cs', [
  ['DialogueManager.isConversationActive', 'dialogue UI mode'],
  ['Time.timeScale <= 0.001f', 'paused/modal UI mode'],
  ['Karlolegend_SetInputMode(nextMode)', 'browser mode handoff']
]);

requireAll('Assets/Plugins/Assembly-CSharp-firstpass/PixelCrushers/MobileWebPixelCrushersInput.cs', [
  ['InputDeviceManager.GetButtonDelegate', 'PixelCrushers hook'],
  ['manager.GetButtonDown = GetButtonDownProxy', 'mobile button injection'],
  ['originalGetButtonDown(buttonName)', 'original input preservation']
]);

requireAll('Assets/WebGLTemplates/Mobile/index.html', [
  ['viewport-fit=cover', 'safe area viewport'],
  ['createUnityInstance', 'Unity bootstrap'],
  ['ConfigureProfile', 'profile handoff'],
  ['INPUT_INTERVAL_MS = 1000 / 30', '30 Hz JS-Wasm input batching'],
  ['queueLook', 'batched look'],
  ['queueMove', 'batched movement'],
  ["canvas.addEventListener('pointerdown'", 'canvas gestures'],
  ['primary-button', 'Fire1 control'],
  ['pointercancel', 'pointer cancellation'],
  ['navigator.maxTouchPoints', 'touch detection']
]);

const template = read('Assets/WebGLTemplates/Mobile/index.html');
if (template.includes('id="move-zone"') || template.includes('id="look-zone"')) failures.push('Do not restore pointer-intercepting move/look overlay zones.');
if (template.includes('backdrop-filter')) failures.push('Do not add backdrop-filter over WebGL canvas.');
if (template.includes('60 FPS')) failures.push('Do not advertise a 60 FPS mobile profile before stable 30 is proven.');

for (const meta of [
  'Assets/Scripts/MobileWeb/MobileWebInputBridge.cs.meta',
  'Assets/Scripts/MobileWeb/MobileWebSceneBudgetController.cs.meta',
  'Assets/Scripts/MobileWeb/MobileWebTelemetry.cs.meta',
  'Assets/WebGLTemplates/Mobile.meta',
  'Assets/WebGLTemplates/Mobile/index.html.meta'
]) requireText(meta, 'fileFormatVersion: 2', `Unity metadata for ${meta}`);

const workflow = read('.github/workflows/mobile-webgl.yml');
if (workflow) {
  if (!/workflow_dispatch:\s*/.test(workflow)) failures.push('Mobile workflow must remain manually dispatchable.');
  if (/\bpull_request\s*:/.test(workflow) || /\bpush\s*:/.test(workflow)) failures.push('Mobile workflow must remain manual-only to prevent notification spam.');
  if (/\blfs:\s*true\b/.test(workflow)) warnings.push('Workflow asks checkout to download Git LFS.');
}

console.log('=== Mobile WebGL source preflight ===');
console.log(`Root: ${root}`);
console.log(`Failures: ${failures.length}`);
console.log(`Warnings: ${warnings.length}`);
for (const warning of warnings) console.warn(`WARN: ${warning}`);
for (const failure of failures) console.error(`FAIL: ${failure}`);

if (process.env.GITHUB_STEP_SUMMARY) {
  const lines = ['## Mobile WebGL source preflight', '', `- Failures: **${failures.length}**`, `- Warnings: **${warnings.length}**`];
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
