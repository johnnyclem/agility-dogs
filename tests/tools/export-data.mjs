// Exports Unity ScriptableObject .asset YAML (courses, breeds, obstacles, handlers)
// into JSON consumed by the data-integrity tests and the Playwright web sim.
//
// Unity asset YAML for these assets is flat key/value under "MonoBehaviour:",
// with {x,y,z} inline maps and GUID references. We parse just what we need.

import { readFileSync, readdirSync, writeFileSync, mkdirSync } from 'node:fs';
import { join, dirname, basename } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const repo = join(here, '..', '..');
const dataRoot = findDataRoot();
const outDir = join(here, '..', 'data');

function findDataRoot() {
  // Data may live at Assets/Data or Assets/Resources/Data depending on fix state.
  const a = join(repo, 'Agility Dogs', 'Assets', 'Resources', 'Data');
  const b = join(repo, 'Agility Dogs', 'Assets', 'Data');
  try { readdirSync(a); return a; } catch { return b; }
}

function parseAssetFile(path) {
  const text = readFileSync(path, 'utf8');
  const lines = text.split('\n');
  const obj = { _file: basename(path) };
  let inMono = false;
  let currentArrayKey = null;
  for (const raw of lines) {
    if (raw.startsWith('MonoBehaviour:')) { inMono = true; continue; }
    if (!inMono) continue;
    const m = raw.match(/^(\s+)([A-Za-z_][A-Za-z0-9_]*):\s?(.*)$/);
    if (m) {
      const [, indent, key, valRaw] = m;
      if (indent.length === 2) {
        currentArrayKey = null;
        const val = valRaw.trim();
        if (val === '' || val === '[]') {
          obj[key] = val === '[]' ? [] : [];
          currentArrayKey = key;
          if (val === '[]') currentArrayKey = null;
        } else {
          obj[key] = parseScalar(val);
        }
      }
      continue;
    }
    const item = raw.match(/^\s+-\s+(.*)$/);
    if (item && currentArrayKey) {
      obj[currentArrayKey].push(parseScalar(item[1].trim()));
    }
  }
  return obj;
}

function parseScalar(val) {
  // {fileID: 11500000, guid: abc, type: 3} -> reference object
  const ref = val.match(/^\{fileID:\s*(-?\d+)(?:,\s*guid:\s*([0-9a-f]+))?/);
  if (ref) return { fileID: Number(ref[1]), guid: ref[2] ?? null };
  const vec = val.match(/^\{x:\s*(-?[\d.e+]+),\s*y:\s*(-?[\d.e+]+),\s*z:\s*(-?[\d.e+]+)\}$/);
  if (vec) return { x: Number(vec[1]), y: Number(vec[2]), z: Number(vec[3]) };
  if (/^-?\d+(\.\d+)?(e[+-]?\d+)?$/i.test(val)) return Number(val);
  return val;
}

function guidOf(metaPath) {
  try {
    const m = readFileSync(metaPath, 'utf8').match(/guid:\s*([0-9a-f]+)/);
    return m ? m[1] : null;
  } catch {
    return null; // repo does not track all .meta files
  }
}

function exportDir(sub) {
  const dir = join(dataRoot, sub);
  const out = [];
  for (const entry of readdirSync(dir, { withFileTypes: true, recursive: true })) {
    if (!entry.isFile() || !entry.name.endsWith('.asset')) continue;
    const full = join(entry.parentPath ?? entry.path, entry.name);
    const obj = parseAssetFile(full);
    obj._guid = guidOf(full + '.meta');
    out.push(obj);
  }
  return out;
}

mkdirSync(outDir, { recursive: true });
const exportSet = {
  courses: exportDir('Courses'),
  breeds: exportDir('Breeds'),
  obstacles: exportDir('Obstacles'),
  handlers: exportDir('Handlers'),
};

// Resolve course obstacleSequence GUID refs -> obstacle names/types
const obstaclesByGuid = new Map(exportSet.obstacles.map(o => [o._guid, o]));
for (const course of exportSet.courses) {
  course.resolvedSequence = (course.obstacleSequence ?? [])
    .map(ref => (ref && ref.guid && obstaclesByGuid.get(ref.guid)) || null)
    .map(o => o && {
      name: o.obstacleName,
      type: o.obstacleType,
      length: o.length,
      width: o.width,
      height: o.height,
      hasContactZones: o.hasContactZones === 1,
    });
}

writeFileSync(join(outDir, 'gamedata.json'), JSON.stringify(exportSet, null, 2));
console.log(`Exported from ${dataRoot}`);
console.log(`courses=${exportSet.courses.length} breeds=${exportSet.breeds.length} obstacles=${exportSet.obstacles.length} handlers=${exportSet.handlers.length}`);
