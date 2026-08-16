import fs from 'node:fs';
import path from 'node:path';

function parseArgs(argv) {
  const args = {};
  for (let i = 0; i < argv.length; i += 1) {
    const item = argv[i];
    if (!item.startsWith('--')) continue;
    const key = item.slice(2);
    const next = argv[i + 1];
    if (next && !next.startsWith('--')) {
      args[key] = next;
      i += 1;
    } else {
      args[key] = true;
    }
  }
  return args;
}

function walk(root) {
  const files = [];
  for (const entry of fs.readdirSync(root, { withFileTypes: true })) {
    const absolute = path.join(root, entry.name);
    if (entry.isDirectory()) files.push(...walk(absolute));
    else if (entry.isFile()) files.push(absolute);
  }
  return files;
}

function bytes(value) {
  const units = ['B', 'KB', 'MB', 'GB'];
  let number = value;
  let unit = 0;
  while (number >= 1024 && unit < units.length - 1) {
    number /= 1024;
    unit += 1;
  }
  return `${number.toFixed(unit === 0 ? 0 : 2)} ${units[unit]}`;
}

function category(relative) {
  const name = relative.toLowerCase();
  if (name.includes('.data')) return 'data';
  if (name.includes('.wasm')) return 'wasm';
  if (name.includes('.js') || name.includes('.framework')) return 'javascript';
  if (/\.(png|jpg|jpeg|webp|gif|svg|ico)(\.br)?$/i.test(name)) return 'images';
  if (/\.(mp3|ogg|wav|aac|m4a)(\.br)?$/i.test(name)) return 'audio';
  return 'other';
}

const args = parseArgs(process.argv.slice(2));
const root = path.resolve(args.root || 'Builds/WebGL-Mobile');
const reportDir = path.resolve(args['report-dir'] || 'Builds/Reports');
const strict = Boolean(args.strict);

if (!fs.existsSync(root)) {
  console.error(`Mobile WebGL build not found: ${root}`);
  process.exit(1);
}

const records = walk(root).map(absolute => {
  const stat = fs.statSync(absolute);
  const relative = path.relative(root, absolute).replaceAll('\\', '/');
  return {
    path: relative,
    bytes: stat.size,
    human: bytes(stat.size),
    category: category(relative),
    brotli: relative.endsWith('.br')
  };
}).sort((a, b) => b.bytes - a.bytes);

const totalBytes = records.reduce((sum, item) => sum + item.bytes, 0);
const buildPayload = records.filter(item => item.path.startsWith('Build/'));
const buildPayloadBytes = buildPayload.reduce((sum, item) => sum + item.bytes, 0);
const categories = {};
for (const record of records) {
  categories[record.category] = (categories[record.category] || 0) + record.bytes;
}

const dataFiles = records.filter(item => item.category === 'data');
const wasmFiles = records.filter(item => item.category === 'wasm');
const largestData = dataFiles[0]?.bytes || 0;
const largestWasm = wasmFiles[0]?.bytes || 0;

const warnings = [];
const recommendations = [];

if (buildPayloadBytes > 90 * 1024 * 1024) {
  warnings.push(`Compressed/served Build payload is ${bytes(buildPayloadBytes)}; investigate startup payload before treating this as production-mobile.`);
  recommendations.push('Use Unity Build Report to identify the heaviest included assets; move non-startup content to Addressables/AssetBundles only if measurements justify it.');
}
if (largestData > 80 * 1024 * 1024) {
  warnings.push(`Largest .data payload is ${bytes(largestData)}.`);
  recommendations.push('Prioritize texture/audio residency and startup-content audit; a monolithic .data file affects both transfer and browser memory pressure.');
}
if (largestWasm > 30 * 1024 * 1024) {
  warnings.push(`Largest Wasm payload is ${bytes(largestWasm)}.`);
  recommendations.push('Verify Disk Size with LTO, managed stripping, exception support off and that development/debug symbols are not entering the release.');
}
if (!records.some(item => item.path === '_headers')) {
  warnings.push('Generated hosting _headers file is missing.');
  recommendations.push('Rebuild with the mobile build command so Brotli MIME/cache headers are emitted automatically.');
}
if (!records.some(item => item.brotli && item.category === 'wasm')) {
  warnings.push('No Brotli-compressed Wasm file detected.');
}

const report = {
  generatedAt: new Date().toISOString(),
  root,
  fileCount: records.length,
  totalBytes,
  totalHuman: bytes(totalBytes),
  buildPayloadBytes,
  buildPayloadHuman: bytes(buildPayloadBytes),
  categories: Object.fromEntries(Object.entries(categories).map(([key, value]) => [key, { bytes: value, human: bytes(value) }])),
  largestFiles: records.slice(0, 20),
  warnings,
  recommendations
};

fs.mkdirSync(reportDir, { recursive: true });
const jsonPath = path.join(reportDir, 'mobile-webgl-report.json');
const mdPath = path.join(reportDir, 'mobile-webgl-report.md');
fs.writeFileSync(jsonPath, `${JSON.stringify(report, null, 2)}\n`);

const rows = records.slice(0, 15).map(item => `| \`${item.path}\` | ${item.human} | ${item.category} |`).join('\n');
const categoryRows = Object.entries(categories)
  .sort((a, b) => b[1] - a[1])
  .map(([key, value]) => `| ${key} | ${bytes(value)} |`).join('\n');

const markdown = `# Mobile WebGL build report

Generated: ${report.generatedAt}

- Files: **${report.fileCount}**
- Entire output: **${report.totalHuman}**
- Served \`Build/\` payload: **${report.buildPayloadHuman}**
- Warnings: **${warnings.length}**

## Payload by category

| Category | Size |
|---|---:|
${categoryRows || '| n/a | 0 B |'}

## Largest files

| File | Size | Type |
|---|---:|---|
${rows || '| n/a | 0 B | n/a |'}

## Warnings

${warnings.length ? warnings.map(item => `- ${item}`).join('\n') : '- None from static payload checks.'}

## Recommendations

${recommendations.length ? [...new Set(recommendations)].map(item => `- ${item}`).join('\n') : '- Run the physical-device 10–15 minute soak test before increasing quality budgets.'}
`;

fs.writeFileSync(mdPath, markdown);
console.log(markdown);
console.log(`JSON: ${jsonPath}`);
console.log(`Markdown: ${mdPath}`);

if (process.env.GITHUB_STEP_SUMMARY) {
  fs.appendFileSync(process.env.GITHUB_STEP_SUMMARY, `${markdown}\n`);
}

if (strict && warnings.length > 0) {
  process.exitCode = 2;
}
