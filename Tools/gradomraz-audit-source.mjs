import fs from 'node:fs';
import path from 'node:path';

const root = path.resolve(process.argv[2] || process.cwd());
const output = path.resolve(process.argv[3] || 'gradomraz-source-audit.json');
const all = [];
function walk(dir) {
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    if (entry.name === '.git') continue;
    const filename = path.join(dir, entry.name);
    if (entry.isDirectory()) walk(filename);
    else if (entry.isFile()) {
      const stat = fs.statSync(filename);
      all.push({ path: path.relative(root, filename).replaceAll('\\', '/'), absolute: filename, bytes: stat.size });
    }
  }
}
walk(root);

const sum = (arr) => arr.reduce((acc, x) => acc + x.bytes, 0);
const countByExtension = {};
for (const f of all) {
  const ext = path.extname(f.path).toLowerCase() || '(no extension)';
  const record = countByExtension[ext] ||= { count: 0, bytes: 0 };
  record.count++;
  record.bytes += f.bytes;
}
const pngs = [];
const textureImports = { total: 0, streamingEnabled: 0, streamingDisabled: 0, mipmapsEnabled: 0, mipmapsDisabled: 0, crunchedEnabled: 0 };
const audioImports = { total: 0, preloadEnabled: 0, loadTypes: {} };
const guids = new Map();
const duplicateGuids = [];
const materials = [];
const scenes = [];
const importedGuids = new Map();
const countType = (src, id) => (src.match(new RegExp('^--- !u!' + id + ' &', 'gm')) || []).length;
const readText = (f) => fs.readFileSync(f.absolute, 'utf8');

for (const f of all) {
  if (f.path.toLowerCase().endsWith('.png')) {
    const fd = fs.openSync(f.absolute, 'r');
    const header = Buffer.alloc(24);
    fs.readSync(fd, header, 0, 24, 0);
    fs.closeSync(fd);
    if (header.subarray(0, 8).toString('hex') === '89504e470d0a1a0a') {
      pngs.push({ path: f.path, bytes: f.bytes, width: header.readUInt32BE(16), height: header.readUInt32BE(20) });
    }
  }
  if (f.path.endsWith('.meta')) {
    const src = readText(f);
    const guid = /^guid:\s*([0-9a-f]{32})/m.exec(src)?.[1];
    if (guid) {
      if (guids.has(guid)) duplicateGuids.push({ guid, paths: [guids.get(guid), f.path] });
      else guids.set(guid, f.path);
    }
    if (src.includes('TextureImporter:')) {
      textureImports.total++;
      if (/^\s*streamingMipmaps:\s*1\s*$/m.test(src)) textureImports.streamingEnabled++;
      else textureImports.streamingDisabled++;
      if (/^\s*enableMipMap:\s*1\s*$/m.test(src)) textureImports.mipmapsEnabled++;
      else textureImports.mipmapsDisabled++;
      if (/^\s*crunchedCompression:\s*1\s*$/m.test(src)) textureImports.crunchedEnabled++;
    }
    if (src.includes('AudioImporter:')) {
      audioImports.total++;
      if (/^\s*preloadAudioData:\s*1\s*$/m.test(src)) audioImports.preloadEnabled++;
      const m = /^\s*loadType:\s*(\d+)/m.exec(src);
      if (m) audioImports.loadTypes[m[1]] = (audioImports.loadTypes[m[1]] || 0) + 1;
    }
  }
  if (f.path.endsWith('.mat')) {
    const src = readText(f);
    const guid = /m_Shader:\s*\{[^\n]*guid:\s*([0-9a-f]{32})/.exec(src)?.[1];
    materials.push({ path: f.path, shaderGuid: guid || null });
  }
  if (f.path.startsWith('Assets/Scenes/') && f.path.endsWith('.unity')) {
    const src = readText(f);
    scenes.push({ path: f.path, bytes: f.bytes, gameObjects: countType(src, 1), meshRenderers: countType(src, 23), skinnedMeshRenderers: countType(src, 137), lights: countType(src, 108), cameras: countType(src, 20), audioSources: countType(src, 82), monoBehaviours: countType(src, 114), colliders: countType(src, 65) + countType(src, 64) + countType(src, 136) });
  }
}
const shaderUses = new Map();
for (const m of materials) {
  const key = m.shaderGuid || '(no serialized shader GUID)';
  const value = shaderUses.get(key) || { materials: 0, example: m.path };
  value.materials++;
  shaderUses.set(key, value);
}
const topShaderUses = [...shaderUses.entries()].map(([guid, val]) => ({ shaderGuid: guid, localMeta: guids.get(guid) || null, ...val })).sort((a, b) => b.materials - a.materials).slice(0, 20);
const report = {
  root, scannedFiles: all.length, sourceBytes: sum(all), duplicateGuids,
  byExtension: Object.entries(countByExtension).map(([extension, v]) => ({ extension, ...v })).sort((a, b) => b.bytes - a.bytes),
  textures: { ...textureImports, totalPngs: pngs.length, largestPngs: pngs.sort((a, b) => b.bytes - a.bytes).slice(0, 30), largestByPixels: [...pngs].sort((a, b) => b.width * b.height - a.width * a.height).slice(0, 20) },
  audio: audioImports, scenes, materialCount: materials.length, shaderUses: topShaderUses,
};
fs.writeFileSync(output, JSON.stringify(report, null, 2));
console.log(JSON.stringify({ scannedFiles: report.scannedFiles, sourceMB: +(report.sourceBytes / 1e6).toFixed(2), duplicateGuids: duplicateGuids.length, textures: { ...textureImports, totalPngs: pngs.length, largestByPixels: report.textures.largestByPixels.slice(0, 8) }, audio: audioImports, scenes, materialCount: materials.length, shaderUses: topShaderUses }, null, 2));
