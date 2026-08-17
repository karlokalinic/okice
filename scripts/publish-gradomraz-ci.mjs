import { brotliDecompressSync } from "node:zlib";
import { execFileSync } from "node:child_process";
import { existsSync, readFileSync, readdirSync, statSync } from "node:fs";
import { join, relative } from "node:path";

const bucket = process.env.GRADOMRAZ_R2_BUCKET || "karlo-webgl-builds";
const webDir = "Builds/WebGL";
const windowsZip = "Builds/GRADOMRAZ-Windows.zip";
const sha = process.env.GITHUB_SHA || "local";
const wrangler = process.platform === "win32" ? "npx.cmd" : "npx";

for (const path of [webDir, windowsZip]) if (!existsSync(path)) throw new Error(`Missing production artifact: ${path}`);

function walk(dir) {
  const out = [];
  for (const entry of readdirSync(dir)) {
    const p = join(dir, entry);
    statSync(p).isDirectory() ? out.push(...walk(p)) : out.push(p);
  }
  return out;
}
function mime(path) {
  const raw = path.replace(/\.(br|gz)$/i, "");
  const ext = raw.split(".").pop().toLowerCase();
  return ({html:"text/html; charset=utf-8",js:"text/javascript; charset=utf-8",css:"text/css; charset=utf-8",json:"application/json; charset=utf-8",wasm:"application/wasm",data:"application/octet-stream",png:"image/png",jpg:"image/jpeg",jpeg:"image/jpeg",svg:"image/svg+xml",ico:"image/x-icon",txt:"text/plain; charset=utf-8",zip:"application/zip"})[ext] || "application/octet-stream";
}
function put(key, file, extra = []) {
  const immutable = key.includes("/Build/") || key.startsWith("downloads/");
  const args = ["--yes","wrangler@4","r2","object","put",`${bucket}/${key}`,"--file",file,"--remote","--force","--content-type",mime(file),"--cache-control",immutable?"public,max-age=31536000,immutable":"public,max-age=0,must-revalidate",...extra];
  let last;
  for (let attempt=1; attempt<=7; attempt++) {
    try { console.log(`R2 PUT ${attempt}/7 ${key}`); execFileSync(wrangler,args,{stdio:"inherit",env:process.env}); return; }
    catch (e) { last=e; if(attempt<7) Atomics.wait(new Int32Array(new SharedArrayBuffer(4)),0,0,Math.min(30,2**attempt)*1000); }
  }
  throw last;
}
function readVaruint(bytes, at) { let value=0,shift=0,i=at; for(;;){const b=bytes[i++];value|=(b&0x7f)<<shift;if((b&0x80)===0)return[value>>>0,i];shift+=7;if(shift>35)throw new Error("Invalid WASM varuint");} }
function wasmMemory(file) {
  let bytes=readFileSync(file); if(file.endsWith(".br"))bytes=brotliDecompressSync(bytes);
  if(bytes[0]!==0||bytes[1]!==97||bytes[2]!==115||bytes[3]!==109)throw new Error(`Not WASM: ${file}`);
  let p=8; while(p<bytes.length){const id=bytes[p++];let size;[size,p]=readVaruint(bytes,p);const end=p+size;if(id===5){let count;[count,p]=readVaruint(bytes,p);if(count<1)throw new Error("No WASM memory");let flags;[flags,p]=readVaruint(bytes,p);let min;[min,p]=readVaruint(bytes,p);let max=null;if(flags&1)[max,p]=readVaruint(bytes,p);return{initialMb:min*65536/1048576,maximumMb:max==null?null:max*65536/1048576};}p=end;}throw new Error("WASM memory section missing");
}

const files = walk(webDir);
const required = {
  loader: files.find(x=>x.endsWith(".loader.js")),
  data: files.find(x=>/\.data(\.br|\.gz)?$/.test(x)),
  framework: files.find(x=>/\.framework\.js(\.br|\.gz)?$/.test(x)),
  wasm: files.find(x=>/\.wasm(\.br|\.gz)?$/.test(x)),
};
for (const [kind,file] of Object.entries(required)) if(!file) throw new Error(`Missing Unity ${kind} output`);
const memory=wasmMemory(required.wasm);
console.log("GRADOMRAZ WASM memory",memory);
if(memory.initialMb>320)throw new Error(`Initial WASM memory ${memory.initialMb} MB exceeds mobile-safe production gate 320 MB`);
if(memory.maximumMb!=null&&memory.maximumMb>1024)throw new Error(`Maximum WASM memory ${memory.maximumMb} MB exceeds production gate 1024 MB`);

// Hash-named payloads and template assets first. The existing index continues referencing the old
// immutable payload until the final PUT, which makes the promotion safe without deleting rollback files.
const index = join(webDir,"index.html");
for (const file of files.filter(x=>x!==index)) {
  const rel=relative(webDir,file).replaceAll("\\","/");
  const extra=file.endsWith(".br")?["--content-encoding","br"]:file.endsWith(".gz")?["--content-encoding","gzip"]:[];
  put(`game-01/${rel}`,file,extra);
}
put("game-01/index.html",index);
put("downloads/gradomraz/windows.zip",windowsZip,["--content-disposition",`attachment; filename=\"GRADOMRAZ-Windows-x64-${sha.slice(0,8)}.zip\"`]);

let verified=false;
for(let i=0;i<20;i++){
  try{
    const [statusRes,manifestRes]=await Promise.all([
      fetch(`https://gradomraz.karlolegend.workers.dev/api/build-status?ci=${Date.now()}`,{cache:"no-store"}),
      fetch(`https://gradomraz.karlolegend.workers.dev/unity/game-01/GRADOMRAZ-BUILD-MANIFEST.txt?ci=${Date.now()}`,{cache:"no-store"})
    ]);
    const status=await statusRes.json(); const manifest=await manifestRes.text();
    if(status.webgl&&status.windows&&manifest.includes(`sourceCommit=${sha}`)){console.log("LIVE GRADOMRAZ VERIFIED",status);verified=true;break;}
    console.log("Waiting for GRADOMRAZ promotion",status,manifest.slice(0,120));
  }catch(e){console.warn("verification retry",String(e));}
  await new Promise(r=>setTimeout(r,2000));
}
if(!verified)throw new Error(`GRADOMRAZ production did not converge to source ${sha}`);
