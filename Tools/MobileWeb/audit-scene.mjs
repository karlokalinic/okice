#!/usr/bin/env node

import fs from 'node:fs';
import path from 'node:path';

const argAfter = name => {
  const index = process.argv.indexOf(name);
  return index >= 0 ? process.argv[index + 1] : null;
};

const root = path.resolve(argAfter('--root') || '.');
const reportDir = path.resolve(argAfter('--report-dir') || path.join(root, 'Builds', 'Reports'));
const sceneRel = argAfter('--scene') || 'Assets/Scenes/SampleScene.unity';
const scenePath = path.join(root, ...sceneRel.split('/'));

if (!fs.existsSync(scenePath)) {
  console.error(`Scene audit failed: missing ${sceneRel}`);
  process.exit(1);
}

const text = fs.readFileSync(scenePath, 'utf8');
const header = /^--- !u!(\d+) &(-?\d+)/gm;
const matches = [...text.matchAll(header)];
const components = [];

for (let index = 0; index < matches.length; index++) {
  const match = matches[index];
  const start = match.index;
  const end = index + 1 < matches.length ? matches[index + 1].index : text.length;
  components.push({
    classId: Number(match[1]),
    fileId: match[2],
    body: text.slice(start, end)
  });
}

const byClass = id => components.filter(item => item.classId === id);
const count = id => byClass(id).length;
const boolField = (body, field, value) => new RegExp(`^\\s*${field}:\\s*${value}\\s*$`, 'm').test(body);
const numericField = (body, field) => {
  const escaped = field.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  const match = body.match(new RegExp(`^\\s*${escaped}:\\s*(-?[0-9.]+(?:[eE][+-]?[0-9]+)?)\\s*$`, 'm'));
  return match ? Number(match[1]) : null;
};

const lights = byClass(108);
const audioSources = byClass(82);
const rigidbodies = byClass(54);
const meshColliders = byClass(64);
const boxColliders = byClass(65);

const metrics = {
  scene: sceneRel,
  gameObjects: count(1),
  cameras: count(20),
  meshRenderers: count(23),
  skinnedMeshRenderers: count(137),
  animators: count(95),
  particleSystems: count(198),
  reflectionProbes: count(215),
  lights: lights.length,
  enabledLights: lights.filter(item => boolField(item.body, 'm_Enabled', 1)).length,
  shadowCastingLights: lights.filter(item => /m_Shadows:\s*[\s\S]*?^\s*m_Type:\s*[12]\s*$/m.test(item.body)).length,
  rigidbodies: rigidbodies.length,
  dynamicRigidbodies: rigidbodies.filter(item => boolField(item.body, 'm_IsKinematic', 0)).length,
  meshColliders: meshColliders.length,
  nonConvexMeshColliders: meshColliders.filter(item => boolField(item.body, 'm_Convex', 0)).length,
  boxColliders: boxColliders.length,
  audioSources: audioSources.length,
  audioPlayOnAwake: audioSources.filter(item => boolField(item.body, 'm_PlayOnAwake', 1)).length,
  audioLoops: audioSources.filter(item => boolField(item.body, 'Loop', 1)).length,
  audibleLoopStarts: audioSources.filter(item => {
    const volume = numericField(item.body, 'm_Volume');
    return boolField(item.body, 'm_PlayOnAwake', 1) && boolField(item.body, 'Loop', 1) && volume !== null && volume > 0.001;
  }).length
};

const warningRules = [
  ['lights', 64, 'Large light population: mobile runtime must keep Forward + strict light/shadow budgets.'],
  ['shadowCastingLights', 16, 'Many authored shadow-casting lights: punctual shadows must remain disabled on mobile.'],
  ['dynamicRigidbodies', 24, 'Dynamic Rigidbody count is high for single-threaded WebGL physics.'],
  ['meshColliders', 256, 'MeshCollider population is high; watch broadphase/narrowphase cost on device.'],
  ['boxColliders', 1024, 'Collider population is high; 30 Hz physics cadence is mandatory.'],
  ['audioPlayOnAwake', 48, 'Many AudioSources auto-start; keep the mobile voice budget constrained.'],
  ['audioLoops', 32, 'Large looping-audio population; inspect clip load types if memory/audio thread remains hot.']
];

const warnings = warningRules
  .filter(([key, limit]) => metrics[key] > limit)
  .map(([key, limit, message]) => ({ key, value: metrics[key], limit, message }));

fs.mkdirSync(reportDir, { recursive: true });
const jsonPath = path.join(reportDir, 'mobile-scene-audit.json');
const markdownPath = path.join(reportDir, 'mobile-scene-audit.md');
fs.writeFileSync(jsonPath, JSON.stringify({ generatedAt: new Date().toISOString(), metrics, warnings }, null, 2));

const lines = [
  '# Mobile scene complexity audit',
  '',
  `Scene: \`${sceneRel}\``,
  '',
  '| Metric | Count |',
  '|---|---:|',
  ...Object.entries(metrics)
    .filter(([key]) => key !== 'scene')
    .map(([key, value]) => `| ${key} | ${value} |`),
  '',
  '## Warnings',
  ''
];

if (warnings.length === 0) {
  lines.push('No source-complexity budget warnings.');
} else {
  for (const warning of warnings) {
    lines.push(`- **${warning.key}: ${warning.value}** (budget ${warning.limit}) — ${warning.message}`);
  }
}

lines.push('', 'These are source-complexity warnings, not measured runtime timings. Physical-device profiling remains authoritative.');
fs.writeFileSync(markdownPath, `${lines.join('\n')}\n`);

console.log('=== Mobile scene complexity audit ===');
for (const [key, value] of Object.entries(metrics)) {
  console.log(`${key}: ${value}`);
}
for (const warning of warnings) {
  console.warn(`WARN ${warning.key}: ${warning.value} > ${warning.limit} — ${warning.message}`);
}
console.log(`Reports: ${jsonPath}, ${markdownPath}`);

if (process.env.GITHUB_STEP_SUMMARY) {
  fs.appendFileSync(process.env.GITHUB_STEP_SUMMARY, `\n${lines.join('\n')}\n`);
}
