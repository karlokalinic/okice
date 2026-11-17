import fs from 'node:fs';
import path from 'node:path';

const scene = path.resolve(process.argv[2]);
const data = fs.readFileSync(scene, 'utf8');
const blocks = data.split(/^--- !u!/m);
const gameObjects = new Map();
const meshes = new Map();
const transforms = new Map();
const goToTransform = new Map();
const renderers = [];
const lights = [];
const find = (text, pattern) => pattern.exec(text)?.[1];

for (const block of blocks) {
  const type = /^([0-9]+) &/.exec(block)?.[1];
  if (!type) continue;
  const go = find(block, /m_GameObject:\s*\{fileID:\s*(-?\d+)/);
  if (type === '1') {
    const id = find(block, /^1 &(-?\d+)/);
    gameObjects.set(id, {
      name: find(block, /^\s*m_Name:\s*(.*)/m),
      active: find(block, /^\s*m_IsActive:\s*(\d+)/m) === '1',
      staticFlags: +(find(block, /^\s*m_StaticEditorFlags:\s*(\d+)/m) || 0),
    });
  } else if (type === '4' || type === '224') {
    const transformId = find(block, /^\d+ &(-?\d+)/);
    const parent = find(block, /m_Father:\s*\{fileID:\s*(-?\d+)/);
    transforms.set(transformId, { go, parent });
    goToTransform.set(go, transformId);
  } else if (type === '33') {
    const meshId = find(block, /m_Mesh:\s*\{fileID:\s*(-?\d+)/);
    const meshGuid = find(block, /m_Mesh:[^\n]*guid:\s*([0-9a-f]{32})/) || '';
    meshes.set(go, `${meshGuid}:${meshId || 'none'}`);
  } else if (type === '23') {
    const materialChunk = find(block, /m_Materials:\s*\r?\n((?:\s*-\s*[^\n]+\r?\n)*)/) || '';
    const materialList = [...materialChunk.matchAll(/\{fileID:\s*(-?\d+)(?:, guid:\s*([0-9a-f]{32}))?/g)]
      .map((m) => `${m[2] || 'builtin'}:${m[1]}`);
    renderers.push({
      go,
      enabled: find(block, /^\s*m_Enabled:\s*(\d+)/m) === '1',
      castShadows: +(find(block, /^\s*m_CastShadows:\s*(\d+)/m) || 0),
      receiveShadows: find(block, /^\s*m_ReceiveShadows:\s*(\d+)/m) === '1',
      materials: materialList.join('|'),
    });
  } else if (type === '108') {
    lights.push({
      go,
      enabled: find(block, /^\s*m_Enabled:\s*(\d+)/m) === '1',
      shadows: find(block, /^\s*m_Shadows:\s*(\d+)/m) || '',
    });
  }
}
const activeMemo = new Map();
function activeInHierarchy(go, visited = new Set()) {
  if (activeMemo.has(go)) return activeMemo.get(go);
  const own = gameObjects.get(go);
  if (!own) return true;
  if (!own.active || visited.has(go)) return false;
  visited.add(go);
  const transform = transforms.get(goToTransform.get(go));
  const parentGo = transform && transform.parent !== '0' ? transforms.get(transform.parent)?.go : null;
  const result = parentGo ? activeInHierarchy(parentGo, visited) : true;
  activeMemo.set(go, result);
  visited.delete(go);
  return result;
}
const enabled = renderers.filter((r) => r.enabled);
const enabledOwnActive = enabled.filter((r) => gameObjects.get(r.go)?.active);
const enabledInHierarchy = enabled.filter((r) => activeInHierarchy(r.go));
const hiddenRoots = new Map();
for (const r of enabled.filter((x) => !activeInHierarchy(x.go))) {
  let candidate = r.go;
  const seen = new Set();
  while (candidate && !seen.has(candidate)) {
    seen.add(candidate);
    const item = gameObjects.get(candidate);
    if (item && !item.active) {
      const key = candidate + ':' + item.name;
      hiddenRoots.set(key, (hiddenRoots.get(key) || 0) + 1);
      break;
    }
    const t = transforms.get(goToTransform.get(candidate));
    candidate = t && t.parent !== '0' ? transforms.get(t.parent)?.go : null;
  }
}
const topInactiveRoots = [...hiddenRoots.entries()].map(([root, count]) => ({root,count})).sort((a,b)=>b.count-a.count).slice(0,20);
const castShadow = enabledInHierarchy.filter((r) => r.castShadows !== 0);
const receiveShadow = enabledInHierarchy.filter((r) => r.receiveShadows);
const groups = new Map();
for (const r of enabledInHierarchy) {
  const key = `${meshes.get(r.go) || 'unresolved'}~${r.materials}`;
  const g = groups.get(key) || { instances: 0, example: gameObjects.get(r.go)?.name || '', mesh: meshes.get(r.go), materials: r.materials };
  g.instances++;
  groups.set(key, g);
}
const repeats = [...groups.values()].filter((g) => g.instances > 1).sort((a, b) => b.instances - a.instances);
const result = {
  serializedBlocks: blocks.length - 1,
  gameObjects: gameObjects.size,
  ownActiveGameObjects: [...gameObjects.values()].filter((x) => x.active).length,
  staticFlaggedGameObjects: [...gameObjects.values()].filter((x) => x.staticFlags > 0).length,
  totalMeshRenderers: renderers.length,
  enabledMeshRenderers: enabled.length,
  enabledAndOwnGameObjectActive: enabledOwnActive.length,
  enabledAndHierarchyActive: enabledInHierarchy.length,
  enabledShadowCastingRenderers: castShadow.length,
  enabledShadowReceivingRenderers: receiveShadow.length,
  meshFilters: meshes.size,
  distinctMeshMaterialPairs: groups.size,
  groupsWithRepeatedMeshMaterial: repeats.length,
  repeatedGroupRenderers: repeats.reduce((n, x) => n + x.instances, 0),
  topRepeated: repeats.slice(0, 15),
  topInactiveRoots,
  totalLights: lights.length,
  enabledLightComponents: lights.filter((l) => l.enabled).length,
  enabledAndHierarchyActiveLightComponents: lights.filter((l) => l.enabled && activeInHierarchy(l.go)).length,
};
console.log(JSON.stringify(result, null, 2));
