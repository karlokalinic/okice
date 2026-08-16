#!/usr/bin/env node

import fs from 'node:fs';
import path from 'node:path';

const args = new Set(process.argv.slice(2));
const strict = args.has('--strict');
const rootArg = process.argv.find((value, index, list) => list[index - 1] === '--root') || '.';
const root = path.resolve(rootArg);

const POINTER_HEADER = 'version https://git-lfs.github.com/spec/v1';
const KNOWN_MOBILE_OPTIONAL = new Set([
  'Assets/bootanim.mp4',
]);
const SKIP_DIRS = new Set(['.git', 'Library', 'Temp', 'Obj', 'Logs', 'Builds', 'UserSettings', 'MemoryCaptures']);
const TEXT_EXTENSIONS = new Set([
  '.anim', '.asmdef', '.asmref', '.asset', '.compute', '.controller', '.cs', '.hlsl', '.html',
  '.inputactions', '.json', '.mat', '.md', '.meta', '.overridecontroller', '.playable', '.prefab',
  '.shader', '.shadergraph', '.shadersubgraph', '.signal', '.spriteatlas', '.txt', '.unity', '.uss',
  '.uxml', '.xml', '.yaml', '.yml'
]);

function rel(file) {
  return path.relative(root, file).split(path.sep).join('/');
}

function walk(dir, output = []) {
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    if (entry.isDirectory() && SKIP_DIRS.has(entry.name)) continue;
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) walk(full, output);
    else if (entry.isFile()) output.push(full);
  }
  return output;
}

function readHead(file, max = 512) {
  const fd = fs.openSync(file, 'r');
  try {
    const buffer = Buffer.alloc(max);
    const count = fs.readSync(fd, buffer, 0, max, 0);
    return buffer.subarray(0, count).toString('utf8');
  } finally {
    fs.closeSync(fd);
  }
}

function parsePointer(file) {
  let head;
  try { head = readHead(file); } catch { return null; }
  if (!head.startsWith(POINTER_HEADER)) return null;
  const oid = head.match(/^oid sha256:([0-9a-f]{64})$/m)?.[1] || null;
  const size = Number(head.match(/^size (\d+)$/m)?.[1] || 0);
  return { file, path: rel(file), oid, size };
}

function parseMetaGuid(assetPath) {
  const metaPath = `${assetPath}.meta`;
  if (!fs.existsSync(metaPath)) return { guid: null, metaPath: null };
  try {
    const text = fs.readFileSync(metaPath, 'utf8');
    return { guid: text.match(/^guid:\s*([0-9a-f]{32})\s*$/mi)?.[1] || null, metaPath };
  } catch {
    return { guid: null, metaPath };
  }
}

function isDynamicLoadRisk(assetPath) {
  const lower = assetPath.toLowerCase();
  return lower.includes('/resources/') ||
    lower.startsWith('assets/resources/') ||
    lower.includes('/streamingassets/') ||
    lower.startsWith('assets/streamingassets/') ||
    lower.includes('/addressableassetsdata/') ||
    lower.startsWith('assets/addressableassetsdata/');
}

function formatBytes(value) {
  const units = ['B', 'KiB', 'MiB', 'GiB'];
  let amount = value;
  let unit = 0;
  while (amount >= 1024 && unit < units.length - 1) {
    amount /= 1024;
    unit++;
  }
  return `${amount.toFixed(unit === 0 ? 0 : 2)} ${units[unit]}`;
}

if (!fs.existsSync(root)) {
  console.error(`LFS audit: project root does not exist: ${root}`);
  process.exit(2);
}

const files = walk(root);
const pointers = files.map(parsePointer).filter(Boolean);
const pointerByGuid = new Map();

for (const pointer of pointers) {
  const meta = parseMetaGuid(pointer.file);
  pointer.guid = meta.guid;
  pointer.metaPath = meta.metaPath ? rel(meta.metaPath) : null;
  pointer.references = 0;
  pointer.referenceExamples = [];
  pointer.dynamicRisk = isDynamicLoadRisk(pointer.path);
  pointer.knownMobileOptional = KNOWN_MOBILE_OPTIONAL.has(pointer.path);
  if (pointer.guid) pointerByGuid.set(pointer.guid, pointer);
}

if (pointerByGuid.size > 0) {
  const guidRegex = /guid:\s*([0-9a-f]{32})/g;
  for (const file of files) {
    const extension = path.extname(file).toLowerCase();
    if (!TEXT_EXTENSIONS.has(extension)) continue;

    let stat;
    try { stat = fs.statSync(file); } catch { continue; }
    if (stat.size > 32 * 1024 * 1024) continue;

    let text;
    try { text = fs.readFileSync(file, 'utf8'); } catch { continue; }
    guidRegex.lastIndex = 0;
    let match;
    while ((match = guidRegex.exec(text)) !== null) {
      const pointer = pointerByGuid.get(match[1]);
      if (!pointer) continue;
      const source = rel(file);
      if (source === pointer.metaPath) continue;
      pointer.references++;
      if (pointer.referenceExamples.length < 4 && !pointer.referenceExamples.includes(source)) {
        pointer.referenceExamples.push(source);
      }
    }
  }
}

const required = pointers.filter(p => !p.knownMobileOptional && (p.references > 0 || p.dynamicRisk));
const optional = pointers.filter(p => p.knownMobileOptional);
const unreferenced = pointers.filter(p => !p.knownMobileOptional && p.references === 0 && !p.dynamicRisk);
const declaredBytes = pointers.reduce((sum, p) => sum + (Number.isFinite(p.size) ? p.size : 0), 0);

console.log('=== Mobile WebGL Git LFS audit ===');
console.log(`Pointers present: ${pointers.length}`);
console.log(`Declared LFS payload: ${formatBytes(declaredBytes)}`);
console.log(`Required/dynamic unresolved: ${required.length}`);
console.log(`Known mobile-optional unresolved: ${optional.length}`);
console.log(`No GUID reference found: ${unreferenced.length}`);

function printGroup(title, group) {
  if (!group.length) return;
  console.log(`\n${title}`);
  for (const item of group) {
    const flags = [];
    if (item.knownMobileOptional) flags.push('mobile-optional');
    if (item.dynamicRisk) flags.push('dynamic-load-risk');
    if (item.references) flags.push(`${item.references} GUID ref(s)`);
    console.log(`- ${item.path} | ${formatBytes(item.size)}${flags.length ? ` | ${flags.join(', ')}` : ''}`);
    if (item.referenceExamples.length) console.log(`  refs: ${item.referenceExamples.join(', ')}`);
  }
}

printGroup('BLOCKING unresolved LFS objects', required);
printGroup('Known mobile-optional LFS objects', optional);
printGroup('Potentially unused/unreferenced LFS objects', unreferenced);

const summaryPath = process.env.GITHUB_STEP_SUMMARY;
if (summaryPath) {
  const lines = [
    '## Git LFS preflight',
    '',
    `- Pointer files present: **${pointers.length}**`,
    `- Declared unresolved LFS payload: **${formatBytes(declaredBytes)}**`,
    `- Required/dynamic unresolved: **${required.length}**`,
    `- Known mobile-optional unresolved: **${optional.length}**`,
    `- No GUID reference found: **${unreferenced.length}**`,
    ''
  ];
  if (required.length) {
    lines.push('### Blocking files');
    for (const item of required.slice(0, 30)) lines.push(`- \`${item.path}\` (${formatBytes(item.size)}, refs=${item.references}${item.dynamicRisk ? ', dynamic-load-risk' : ''})`);
    lines.push('');
  }
  fs.appendFileSync(summaryPath, `${lines.join('\n')}\n`);
}

if (strict && required.length > 0) {
  console.error(`\nLFS audit failed: ${required.length} unresolved LFS object(s) are referenced by the Unity project or live in dynamic-load paths.`);
  console.error('The build is intentionally blocked rather than compiling with pointer stubs. Restore those objects or remove/replace their references.');
  process.exit(1);
}

console.log('\nLFS audit passed for the current strictness level.');
