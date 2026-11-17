import fs from 'node:fs';
import path from 'node:path';

// Read-only ranking of visual-only branches that could become streamed scenes.
// Serialized references are checked, but dynamic Find(), PlayMaker variables,
// animation and scene-transition behavior still require in-Editor verification.
const scenePath = path.resolve(process.argv[2] || 'Assets/Scenes/SampleScene.unity');
const rootName = process.argv[3] || 'Outside';
const source = fs.readFileSync(scenePath, 'utf8');
const chunks = source.split(/^--- !u!/m);
const recs = [], byId = new Map(), gos = new Map(), goTransform = new Map(), transforms = new Map();
const field = (s, rx) => rx.exec(s)?.[1] || null;
for (let i = 1; i < chunks.length; i++) {
  const text = chunks[i], m = /^(\d+) &(-?\d+)/.exec(text);
  if (!m) continue;
  const type = +m[1], id = m[2];
  const owner = type === 1 ? id : field(text, /m_GameObject:\s*\{fileID:\s*(-?\d+)/);
  const r = { type, id, owner, text };
  recs.push(r); byId.set(id,r);
  if (type === 1) gos.set(id, { id, name:field(text,/^\s*m_Name:\s*(.*)$/m), ownRenderers:0, ownNonvisual:0,
    renderers:0, nonvisual:0, subtreeSize:1, children:[], parent:null });
  if (type === 4 || type === 224) {
    transforms.set(id,{owner,parent:field(text,/m_Father:\s*\{fileID:\s*(-?\d+)/)});
    goTransform.set(owner,id);
  }
}
for(const r of recs) {
  if (!r.owner || !gos.has(r.owner) || r.type === 1) continue;
  if (r.type === 23) gos.get(r.owner).ownRenderers++;
  else if (![4,224,33].includes(r.type)) gos.get(r.owner).ownNonvisual++;
}
const roots=[];
for(const [id,go] of gos){
 const t=transforms.get(goTransform.get(id));
 const parentGo=t?.parent && t.parent!=='0'?transforms.get(t.parent)?.owner:null;
 if(parentGo&&gos.has(parentGo)){go.parent=parentGo;gos.get(parentGo).children.push(id)}
 else roots.push(id);
}
const intervals=new Map();
let clock=0;
function visit(id){
  const go=gos.get(id);if(!go)return;
  const tin=clock++;
  go.renderers=go.ownRenderers;go.nonvisual=go.ownNonvisual;
  for(const child of go.children){
    visit(child);
    const c=gos.get(child);
    go.renderers+=c.renderers;go.nonvisual+=c.nonvisual;go.subtreeSize+=c.subtreeSize;
  }
  intervals.set(id,[tin,clock++]);
}
for(const r of roots)visit(r);
const descendant=(a,b)=>{
 const i=intervals.get(a),j=intervals.get(b);
 return i&&j&&i[0]<=j[0]&&i[1]>=j[1];
};
const rootIds=[...gos.values()].filter(x=>x.name===rootName).map(x=>x.id);
const candidates=[...gos.values()].filter(go=>
 go.renderers >= 10 && go.nonvisual===0 &&
 rootIds.some(root=>descendant(root,go.id)));
const edges=[];
const refPattern=/\{fileID:\s*(-?\d+)(?:,\s*guid:\s*([0-9a-fA-F]{32}))?/g;
for(const r of recs){
 if (!r.owner || !gos.has(r.owner) || [4,224].includes(r.type))continue;
 refPattern.lastIndex=0;
 let m;
 while((m=refPattern.exec(r.text))){
   if (m[2] && !/^0{32}$/.test(m[2]))continue;
   const dest=byId.get(m[1])?.owner;
   if(!dest || !gos.has(dest) || dest===r.owner)continue;
   edges.push([r.owner,dest,r.type]);
 }
}
const scored=[];
for(const go of candidates){
 let inbound=0,outbound=0;
 for(const [src,dst] of edges){
   const srcIn=descendant(go.id,src),dstIn=descendant(go.id,dst);
   if (!srcIn&&dstIn)inbound++;
   else if(srcIn&&!dstIn)outbound++;
 }
 if(inbound||outbound)continue;
 scored.push({
   name:go.name,id:go.id,parent:gos.get(go.parent)?.name||null,
   objects:go.subtreeSize,renderers:go.renderers,nonvisual:go.nonvisual,
   blockers:0,
 });
}
scored.sort((a,b)=>b.renderers-a.renderers);
console.log(JSON.stringify({
 scene:scenePath,rootName,matchedRootIds:rootIds,
 totalSceneObjects:gos.size,serializedNonTransformCrossReferences:edges.length,
 visualOnlyBranchesWithNoSerializedCrossReferences:scored.length,
 top:scored.slice(0,40),
 warning:'Candidates are NOT automatically safe for extraction. Dynamic PlayMaker/Find, transformation, visibility and asset ownership must be checked in Unity.',
},null,2));
