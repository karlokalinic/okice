import fs from "node:fs";
import path from "node:path";

// Read-only serialized-reference preflight. Does not move or delete objects.
// Counts internal Unity YAML fileID references crossing candidate scene roots.
const scenePath = path.resolve(process.argv[2] ?? "Assets/Scenes/SampleScene.unity");
const outputPath = process.argv[3] ? path.resolve(process.argv[3]) : null;
const candidates = process.argv.slice(4).length
  ? process.argv.slice(4) : ["Outside", "Bridge", "MaddieRoom", "Side2", "Bunker"];
const yaml = fs.readFileSync(scenePath, "utf8");
const chunks = yaml.split(/^--- !u!/m);
const records = [];
const recordById = new Map();
const names = new Map();
const transforms = new Map();
const transformByGo = new Map();
const get = (s, r) => r.exec(s)?.[1] ?? null;
for (let i = 1; i < chunks.length; i++) {
  const text = chunks[i];
  const match = /^(\d+)\s+&(-?\d+)/.exec(text);
  if (!match) continue;
  const type = +match[1], id = match[2];
  const owner = type === 1 ? id : get(text, /m_GameObject:\s*\{fileID:\s*(-?\d+)/);
  const rec = { id, type, owner, text };
  records.push(rec);
  recordById.set(id, rec);
  if (type === 1) names.set(id, get(text, /^\s*m_Name:\s*(.*)\r?$/m));
  if (type === 4 || type === 224) {
    const parent = get(text, /m_Father:\s*\{fileID:\s*(-?\d+)/);
    transforms.set(id, { id, owner, parent });
    transformByGo.set(owner, id);
  }
}
const children = new Map();
for (const t of transforms.values()) {
  if (!t.parent || t.parent === "0") continue;
  const a = children.get(t.parent) || [];
  a.push(t.id);
  children.set(t.parent, a);
}
const matchedRoots = [...names.entries()].filter(([id, name]) => candidates.includes(name) &&
  (!transforms.get(transformByGo.get(id))?.parent || transforms.get(transformByGo.get(id)).parent === "0"));
const alternativeMatches = [...names.entries()].filter(([id, name]) => candidates.includes(name));
function subtree(rootGoId) {
  const out = new Set(), seen = new Set(), pending = [transformByGo.get(rootGoId)];
  while (pending.length) {
    const trId = pending.pop();
    if (!trId || seen.has(trId)) continue;
    seen.add(trId);
    const t = transforms.get(trId);
    if (t?.owner) out.add(t.owner);
    for (const child of children.get(trId) || []) pending.push(child);
  }
  return out;
}
const fileId = /\{fileID:\s*(-?\d+)(?:,\s*guid:\s*([a-fA-F0-9]{32}))?/g;
const scans = [];
for (const [rootGoId, rootName] of (matchedRoots.length ? matchedRoots : alternativeMatches)) {
  const gos = subtree(rootGoId);
  const movedBlockIds = new Set(records.filter(r => gos.has(r.owner)).map(r => r.id));
  const incoming = [], outgoing = [];
  const incomingByType = {}, outgoingByType = {};
  for (const rec of records) {
    const inSubtree = movedBlockIds.has(rec.id);
    fileId.lastIndex = 0;
    let hit;
    while ((hit = fileId.exec(rec.text))) {
      const target = hit[1], externalGuid = hit[2];
      if (!recordById.has(target) || (externalGuid && !/^0{32}$/.test(externalGuid))) continue;
      const targetMoved = movedBlockIds.has(target);
      if (inSubtree === targetMoved) continue;
      const context = rec.text.slice(Math.max(0, hit.index - 85), Math.min(rec.text.length, hit.index + 110))
        .replace(/\s+/g, " ").trim();
      const example = {
        fromId: rec.id, fromType: rec.type, fromObject: names.get(rec.owner) || null,
        targetId: target, targetType: recordById.get(target)?.type,
        targetObject: names.get(recordById.get(target)?.owner) || null, context,
      };
      const list = inSubtree ? outgoing : incoming;
      const byType = inSubtree ? outgoingByType : incomingByType;
      byType[rec.type] = (byType[rec.type] || 0) + 1;
      if (list.length < 30) list.push(example);
    }
  }
  scans.push({
    root: rootName, rootId: rootGoId, gameObjects: gos.size, serializedComponents: movedBlockIds.size,
    incomingReferences: Object.values(incomingByType).reduce((a,b)=>a+b,0),
    outgoingReferences: Object.values(outgoingByType).reduce((a,b)=>a+b,0),
    incomingByComponentType: incomingByType, outgoingByComponentType: outgoingByType,
    incomingExamples: incoming, outgoingExamples: outgoing,
    eligibleForAutomaticSplit: !Object.keys(incomingByType).length && !Object.keys(outgoingByType).length,
  });
}
const result = {
  scene: scenePath, blockCount: records.length, sceneObjectCount: names.size,
  exactRootCandidates: matchedRoots.map(([id,name]) => ({ id, name })),
  candidateCount: scans.length, scans,
  warning: "This is a serialized YAML preflight, not a PlayMaker/runtime global-reference or scene-event audit. Zero cross-references alone does NOT authorize moving a chapter root.",
};
if (outputPath) fs.writeFileSync(outputPath, JSON.stringify(result, null, 2));
console.log(JSON.stringify({
  sceneObjectCount: result.sceneObjectCount, matched: matchedRoots.map(([id,name])=>({id,name})),
  audit: scans.map(s=>({root:s.root, gameObjects:s.gameObjects, components:s.serializedComponents,
    incoming:s.incomingReferences, outgoing:s.outgoingReferences,
    incomingExamples:s.incomingExamples.slice(0,6), outgoingExamples:s.outgoingExamples.slice(0,6)})),
}, null, 2));
