import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';
import os from 'node:os';

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

function contentType(file) {
  const lower = file.toLowerCase().replace(/\.br$/, '');
  if (lower.endsWith('.html')) return 'text/html; charset=utf-8';
  if (lower.endsWith('.js')) return 'application/javascript; charset=utf-8';
  if (lower.endsWith('.wasm')) return 'application/wasm';
  if (lower.endsWith('.data')) return 'application/octet-stream';
  if (lower.endsWith('.json')) return 'application/json; charset=utf-8';
  if (lower.endsWith('.css')) return 'text/css; charset=utf-8';
  if (lower.endsWith('.png')) return 'image/png';
  if (lower.endsWith('.jpg') || lower.endsWith('.jpeg')) return 'image/jpeg';
  if (lower.endsWith('.webp')) return 'image/webp';
  if (lower.endsWith('.svg')) return 'image/svg+xml';
  if (lower.endsWith('.txt')) return 'text/plain; charset=utf-8';
  return 'application/octet-stream';
}

const args = parseArgs(process.argv.slice(2));
const root = path.resolve(args.root || 'Builds/WebGL-Mobile');
const host = String(args.host || '0.0.0.0');
const port = Number(args.port || 8080);

if (!fs.existsSync(path.join(root, 'index.html'))) {
  console.error(`No index.html under ${root}`);
  process.exit(1);
}

const server = http.createServer((request, response) => {
  try {
    const requestUrl = new URL(request.url || '/', `http://${request.headers.host || 'localhost'}`);
    let pathname = decodeURIComponent(requestUrl.pathname);
    if (pathname === '/') pathname = '/index.html';

    const resolved = path.resolve(root, `.${pathname}`);
    if (resolved !== root && !resolved.startsWith(`${root}${path.sep}`)) {
      response.writeHead(403).end('Forbidden');
      return;
    }

    if (!fs.existsSync(resolved) || !fs.statSync(resolved).isFile()) {
      response.writeHead(404, { 'Content-Type': 'text/plain; charset=utf-8' }).end('Not found');
      return;
    }

    const stat = fs.statSync(resolved);
    const relative = path.relative(root, resolved).replaceAll('\\', '/');
    const headers = {
      'Content-Type': contentType(resolved),
      'Content-Length': stat.size,
      'X-Content-Type-Options': 'nosniff',
      'Cross-Origin-Resource-Policy': 'same-origin'
    };

    if (resolved.endsWith('.br')) headers['Content-Encoding'] = 'br';
    headers['Cache-Control'] = relative === 'index.html'
      ? 'no-cache, no-store, must-revalidate'
      : relative.startsWith('Build/')
        ? 'public, max-age=31536000, immutable'
        : 'no-cache';

    response.writeHead(200, headers);
    if (request.method === 'HEAD') {
      response.end();
      return;
    }

    fs.createReadStream(resolved).pipe(response);
  } catch (error) {
    response.writeHead(500, { 'Content-Type': 'text/plain; charset=utf-8' }).end(String(error));
  }
});

server.listen(port, host, () => {
  console.log(`\nGRADOMRAZ Mobile WebGL\nRoot: ${root}`);
  console.log(`Desktop: http://localhost:${port}/`);

  const interfaces = os.networkInterfaces();
  const addresses = [];
  for (const entries of Object.values(interfaces)) {
    for (const entry of entries || []) {
      if (entry.family === 'IPv4' && !entry.internal) addresses.push(entry.address);
    }
  }

  for (const address of [...new Set(addresses)]) {
    console.log(`Phone/LAN: http://${address}:${port}/`);
  }

  console.log('\nPress Ctrl+C to stop. For production and iOS cache validation, deploy over HTTPS.\n');
});

process.on('SIGINT', () => server.close(() => process.exit(0)));
process.on('SIGTERM', () => server.close(() => process.exit(0)));
