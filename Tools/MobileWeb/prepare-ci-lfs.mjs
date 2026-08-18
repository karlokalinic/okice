#!/usr/bin/env node

import fs from 'node:fs';
import path from 'node:path';

const rootArg = process.argv.find((value, index, list) => list[index - 1] === '--root') || '.';
const root = path.resolve(rootArg);
const POINTER_HEADER = 'version https://git-lfs.github.com/spec/v1';
const TEXT_EXTENSIONS = new Set([
  '.anim', '.asset', '.controller', '.mat', '.overridecontroller', '.playable', '.prefab',
  '.signal', '.spriteatlas', '.unity'
]);
const SKIP_DIRS = new Set(['.git', 'Library', 'Temp', 'Obj', 'Logs', 'Builds', 'UserSettings']);

function target(rel) { return path.join(root, ...rel.split('/')); }
function normalize(file) { return path.relative(root, file).split(path.sep).join('/'); }
function read(rel) { return fs.readFileSync(target(rel), 'utf8'); }
function isPointer(rel) {
  const file = target(rel);
  if (!fs.existsSync(file)) return false;
  const fd = fs.openSync(file, 'r');
  try {
    const buffer = Buffer.alloc(256);
    const count = fs.readSync(fd, buffer, 0, buffer.length, 0);
    return buffer.subarray(0, count).toString('utf8').startsWith(POINTER_HEADER);
  } finally { fs.closeSync(fd); }
}
function metaGuid(rel) {
  const match = read(`${rel}.meta`).match(/^guid:\s*([0-9a-f]{32})\s*$/mi);
  if (!match) throw new Error(`Cannot read Unity GUID from ${rel}.meta`);
  return match[1];
}
function walk(dir, output = []) {
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    if (entry.isDirectory() && SKIP_DIRS.has(entry.name)) continue;
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) walk(full, output);
    else if (entry.isFile() && TEXT_EXTENSIONS.has(path.extname(entry.name).toLowerCase())) output.push(full);
  }
  return output;
}
function removeFileAndMeta(rel) {
  for (const candidate of [rel, `${rel}.meta`]) {
    const file = target(candidate);
    if (fs.existsSync(file)) fs.rmSync(file, { force: true });
  }
}
function writeIfChanged(file, before, after) {
  if (after === before) return false;
  fs.writeFileSync(file, after, 'utf8');
  return true;
}

function replaceFontPointer() {
  const source = 'Assets/Font/VT323-Regular SDF.asset';
  const fallback = 'Assets/MonoBehaviour/PublicPixel SDF.asset';
  if (!isPointer(source)) return { handled: false, replacements: 0, note: 'VT323 SDF is already materialized.' };

  const sourceGuid = metaGuid(source);
  const fallbackGuid = metaGuid(fallback);
  let replacements = 0;

  for (const file of walk(target('Assets'))) {
    const rel = normalize(file);
    if (rel === source || rel === `${source}.meta`) continue;
    let before;
    try { before = fs.readFileSync(file, 'utf8'); } catch { continue; }
    if (!before.includes(sourceGuid)) continue;

    let after = before;
    if (rel === fallback) {
      // PublicPixel currently uses VT323 only as a fallback. During quota-blocked CI
      // it must not become a self-referencing fallback after the substitution.
      const fallbackRow = new RegExp(`\\n\\s*- \\{fileID:\\s*11400000,\\s*guid:\\s*${sourceGuid},\\s*type:\\s*2\\}`, 'g');
      after = after.replace(fallbackRow, '');
      if (after.includes(sourceGuid)) after = after.split(sourceGuid).join(fallbackGuid);
    } else {
      after = after.split(sourceGuid).join(fallbackGuid);
    }

    if (writeIfChanged(file, before, after)) {
      const beforeCount = before.split(sourceGuid).length - 1;
      const afterCount = after.split(sourceGuid).length - 1;
      replacements += Math.max(0, beforeCount - afterCount);
    }
  }

  if (replacements === 0) throw new Error(`Expected Unity references to ${sourceGuid}, but none were rewritten.`);
  removeFileAndMeta(source);
  return { handled: true, replacements, note: `Rewired ${replacements} VT323 SDF reference(s) to PublicPixel for CI-only validation.` };
}

function stripMobileBootVideoPointer() {
  const source = 'Assets/Sprite/logo_4K_60FPS_MAX.mp4';
  if (!isPointer(source)) return { handled: false, replacements: 0, note: 'Boot video is already materialized.' };

  const sourceGuid = metaGuid(source);
  let replacements = 0;
  const referencePattern = new RegExp(`\\{fileID:\\s*[^,}]+,\\s*guid:\\s*${sourceGuid},\\s*type:\\s*\\d+\\}`, 'g');

  for (const file of walk(target('Assets'))) {
    let before;
    try { before = fs.readFileSync(file, 'utf8'); } catch { continue; }
    if (!before.includes(sourceGuid)) continue;
    const matches = before.match(referencePattern)?.length || 0;
    let after = before.replace(referencePattern, '{fileID: 0}');
    // Defensive fallback for unusual serialized shapes: no stale video GUID may remain.
    if (after.includes(sourceGuid)) {
      after = after.split(sourceGuid).join('00000000000000000000000000000000');
    }
    if (writeIfChanged(file, before, after)) replacements += Math.max(1, matches);
  }

  if (replacements === 0) throw new Error(`Expected a Boot scene reference to video GUID ${sourceGuid}, but none was rewritten.`);
  removeFileAndMeta(source);
  return { handled: true, replacements, note: `Removed ${replacements} desktop boot-video reference(s) from the CI workspace.` };
}

try {
  console.log('=== Prepare quota-safe mobile CI workspace ===');
  const font = replaceFontPointer();
  const video = stripMobileBootVideoPointer();
  console.log(`Font: ${font.note}`);
  console.log(`Video: ${video.note}`);
  console.log('The repository branch is not mutated by this script; substitutions exist only inside the CI checkout.');

  if (process.env.GITHUB_STEP_SUMMARY) {
    fs.appendFileSync(process.env.GITHUB_STEP_SUMMARY,
      `## LFS CI workspace repair\n\n- ${font.note}\n- ${video.note}\n- Source branch remains unchanged; these are CI-workspace substitutions only.\n`);
  }
} catch (error) {
  console.error(`CI LFS preparation failed: ${error.stack || error.message}`);
  process.exit(1);
}
