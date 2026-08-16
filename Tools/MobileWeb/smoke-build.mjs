#!/usr/bin/env node

import fs from 'node:fs';
import path from 'node:path';

const rootArg = process.argv.find((value, index, list) => list[index - 1] === '--root') || 'Builds/WebGL-Mobile';
const root = path.resolve(rootArg);
const failures = [];
const warnings = [];

function exists(rel) { return fs.existsSync(path.join(root, ...rel.split('/'))); }
function read(rel) { return fs.readFileSync(path.join(root, ...rel.split('/')), 'utf8'); }
function walk(dir, output = []) {
  if (!fs.existsSync(dir)) return output;
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) walk(full, output);
    else if (entry.isFile()) output.push(full);
  }
  return output;
}
function formatBytes(value) {
  const units = ['B', 'KiB', 'MiB', 'GiB'];
  let amount = value;
  let unit = 0;
  while (amount >= 1024 && unit < units.length - 1) { amount /= 1024; unit++; }
  return `${amount.toFixed(unit === 0 ? 0 : 2)} ${units[unit]}`;
}

if (!fs.existsSync(root)) {
  console.error(`Build smoke test: build root does not exist: ${root}`);
  process.exit(2);
}

for (const rel of ['index.html', '_headers', 'DEPLOYMENT.txt']) {
  if (!exists(rel)) failures.push(`Missing ${rel}`);
}

const buildDir = path.join(root, 'Build');
if (!fs.existsSync(buildDir)) failures.push('Missing Build/ directory.');
const buildFiles = walk(buildDir);

function matching(pattern) {
  return buildFiles.filter(file => pattern.test(path.basename(file)));
}

const loader = matching(/\.loader\.js$/i);
const wasm = matching(/\.wasm(?:\.br)?$/i);
const data = matching(/\.data(?:\.br)?$/i);
const framework = matching(/\.framework\.js(?:\.br)?$/i);

if (!loader.length) failures.push('No Unity *.loader.js file found.');
if (!wasm.length) failures.push('No Unity *.wasm or *.wasm.br file found.');
if (!data.length) failures.push('No Unity *.data or *.data.br file found.');
if (!framework.length) failures.push('No Unity *.framework.js or *.framework.js.br file found.');

for (const file of buildFiles) {
  const size = fs.statSync(file).size;
  if (size === 0) failures.push(`Zero-byte build file: ${path.relative(root, file)}`);
}

if (exists('index.html')) {
  const html = read('index.html');
  if (html.includes('{{{')) failures.push('Generated index.html still contains unresolved Unity template placeholders.');
  if (!html.includes('createUnityInstance')) failures.push('Generated index.html does not contain createUnityInstance bootstrap.');
  if (loader.length) {
    const loaderName = path.basename(loader[0]);
    if (!html.includes(loaderName)) warnings.push(`index.html does not literally contain loader filename ${loaderName}; verify generated loader URL logic.`);
  }
}

if (exists('_headers')) {
  const headers = read('_headers');
  if (!headers.includes('Content-Encoding: br')) failures.push('_headers does not declare Brotli Content-Encoding.');
  if (!headers.includes('Content-Type: application/wasm')) failures.push('_headers does not declare application/wasm.');
  if (!headers.includes('immutable')) failures.push('_headers does not declare immutable caching for hashed build files.');
}

const compressedExpected = [...wasm, ...data, ...framework];
const nonBrotli = compressedExpected.filter(file => !file.endsWith('.br'));
if (nonBrotli.length) warnings.push(`${nonBrotli.length} core build file(s) are not .br-suffixed; verify Unity compression output and hosting mode.`);

const allFiles = walk(root);
const totalBytes = allFiles.reduce((sum, file) => sum + fs.statSync(file).size, 0);
const coreBytes = buildFiles.reduce((sum, file) => sum + fs.statSync(file).size, 0);

console.log('=== Mobile WebGL build smoke test ===');
console.log(`Files: ${allFiles.length}`);
console.log(`Total build directory: ${formatBytes(totalBytes)}`);
console.log(`Build/ payload: ${formatBytes(coreBytes)}`);
console.log(`Loader: ${loader.map(file => path.basename(file)).join(', ') || 'MISSING'}`);
console.log(`Wasm: ${wasm.map(file => `${path.basename(file)} (${formatBytes(fs.statSync(file).size)})`).join(', ') || 'MISSING'}`);
console.log(`Data: ${data.map(file => `${path.basename(file)} (${formatBytes(fs.statSync(file).size)})`).join(', ') || 'MISSING'}`);
console.log(`Framework: ${framework.map(file => `${path.basename(file)} (${formatBytes(fs.statSync(file).size)})`).join(', ') || 'MISSING'}`);
for (const warning of warnings) console.warn(`WARN: ${warning}`);
for (const failure of failures) console.error(`FAIL: ${failure}`);

if (process.env.GITHUB_STEP_SUMMARY) {
  const lines = [
    '## Mobile WebGL build smoke test',
    '',
    `- Total output: **${formatBytes(totalBytes)}**`,
    `- Build payload: **${formatBytes(coreBytes)}**`,
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
console.log('Build smoke test passed.');
