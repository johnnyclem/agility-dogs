// Course simulator UI. Uses the shared simulation engine (a port of the
// Unity game's rules) and the exported game data to play each level visibly.
import {
  simulateRun, buildCourseLayout, typeName, WEAVE_POLES, WEAVE_SPACING,
} from '../sim/engine.mjs';

const $ = (id) => document.getElementById(id);
const canvas = $('arena');
const ctx = canvas.getContext('2d');

const data = await (await fetch('../data/gamedata.json')).json();
data.courses.sort((a, b) => a.difficultyRating - b.difficultyRating);

const params = new URLSearchParams(location.search);

for (const [i, c] of data.courses.entries()) {
  const opt = document.createElement('option');
  opt.value = i;
  opt.textContent = `${c.courseName} (difficulty ${c.difficultyRating})`;
  opt.dataset.file = c._file.replace('.asset', '');
  $('course').appendChild(opt);
}
for (const [i, b] of data.breeds.entries()) {
  const opt = document.createElement('option');
  opt.value = i;
  opt.textContent = b.displayName || b.breedName;
  opt.dataset.name = b.breedName;
  $('breed').appendChild(opt);
}

// Query-parameter driving (used by Playwright): ?course=Westminster&breed=Pug&speed=50&autorun=1
if (params.get('course')) {
  const idx = [...$('course').options].findIndex(o => o.dataset.file === params.get('course'));
  if (idx >= 0) $('course').value = idx;
}
if (params.get('breed')) {
  const idx = [...$('breed').options].findIndex(o => o.dataset.name === params.get('breed'));
  if (idx >= 0) $('breed').value = idx;
}
if (params.get('speed')) {
  const opt = document.createElement('option');
  opt.value = params.get('speed');
  opt.textContent = `${params.get('speed')}x`;
  $('speed').appendChild(opt);
  $('speed').value = params.get('speed');
}

// World-to-canvas transform sized for the serpentine layout
const SCALE = 16;
const wx = (x) => canvas.width / 2 + x * SCALE;
const wz = (z) => canvas.height - 60 - z * SCALE;

let animation = null;

function drawCourse(layout, dogPos, completedCount) {
  ctx.clearRect(0, 0, canvas.width, canvas.height);

  // start line
  ctx.strokeStyle = '#fff';
  ctx.setLineDash([6, 6]);
  ctx.beginPath();
  ctx.moveTo(wx(-8), wz(0));
  ctx.lineTo(wx(8), wz(0));
  ctx.stroke();
  ctx.setLineDash([]);

  for (const obs of layout) {
    const done = obs.index < completedCount;
    ctx.save();
    ctx.translate(wx(obs.position.x), wz(obs.position.z));
    const angle = Math.atan2(obs.direction.x, obs.direction.z);
    ctx.rotate(angle);

    if (obs.type === 6) { // weave poles
      ctx.fillStyle = done ? '#9ccc65' : '#e3f2fd';
      const span = (WEAVE_POLES - 1) * WEAVE_SPACING * SCALE;
      for (let i = 0; i < WEAVE_POLES; i++) {
        ctx.beginPath();
        ctx.arc(0, span / 2 - i * WEAVE_SPACING * SCALE, 2.5, 0, Math.PI * 2);
        ctx.fill();
      }
    } else if (obs.type === 5) { // tunnel
      ctx.fillStyle = done ? '#9ccc65' : '#fbc02d';
      ctx.fillRect(-8, -obs.halfLength * SCALE, 16, obs.halfLength * 2 * SCALE);
    } else if (obs.type === 10) { // pause table
      ctx.fillStyle = done ? '#9ccc65' : '#42a5f5';
      ctx.fillRect(-12, -12, 24, 24);
    } else if ([7, 8, 9].includes(obs.type)) { // contacts
      ctx.fillStyle = done ? '#9ccc65' : '#ef6c00';
      ctx.fillRect(-7, -obs.halfLength * SCALE, 14, obs.halfLength * 2 * SCALE);
      ctx.fillStyle = '#fdd835';
      ctx.fillRect(-7, -obs.halfLength * SCALE, 14, 6);
      ctx.fillRect(-7, obs.halfLength * SCALE - 6, 14, 6);
    } else { // jumps
      ctx.strokeStyle = done ? '#9ccc65' : '#fff';
      ctx.lineWidth = 3;
      ctx.beginPath();
      ctx.moveTo(-10, 0);
      ctx.lineTo(10, 0);
      ctx.stroke();
      ctx.lineWidth = 1;
    }
    ctx.restore();

    ctx.fillStyle = '#fff';
    ctx.font = '11px sans-serif';
    ctx.fillText(`${obs.index + 1} ${typeName(obs.type)}`,
      wx(obs.position.x) - 20, wz(obs.position.z) - 18);
  }

  if (dogPos) {
    ctx.fillStyle = '#3e2723';
    ctx.beginPath();
    ctx.arc(wx(dogPos.x), wz(dogPos.z), 7, 0, Math.PI * 2);
    ctx.fill();
    ctx.fillStyle = '#d7ccc8';
    ctx.beginPath();
    ctx.arc(wx(dogPos.x), wz(dogPos.z), 3.5, 0, Math.PI * 2);
    ctx.fill();
  }
}

function setBanner(state, text) {
  const banner = $('banner');
  banner.className = state;
  banner.dataset.state = state;
  banner.textContent = text;
}

function runLevel() {
  if (animation) cancelAnimationFrame(animation);

  const course = data.courses[$('course').value];
  const breed = data.breeds[$('breed').value];
  const playback = Number($('speed').value);

  const layout = buildCourseLayout(course);
  const run = simulateRun(course, breed);

  $('sct').textContent = course.standardTime;
  $('max').textContent = course.maximumTime;
  setBanner('running', `Running: ${course.courseName} with ${breed.displayName || breed.breedName}…`);

  // Build a waypoint timeline from the run segments: each obstacle contributes
  // approach (prev exit -> commit -> entry) and traversal (entry -> exit).
  const waypoints = [{ t: 0, p: { x: 0, z: 0 } }];
  let t = 0;
  let prev = { x: 0, z: 0 };
  for (const [i, obs] of layout.entries()) {
    const segTime = run.segments[i].time;
    // Split segment time proportionally across approach and traversal legs.
    const legs = [obs.commit, obs.entry, obs.exit];
    const lens = legs.map((p, j) => {
      const from = j === 0 ? prev : legs[j - 1];
      return Math.hypot(p.x - from.x, p.z - from.z);
    });
    const total = lens.reduce((a, b) => a + b, 0) || 1;
    for (let j = 0; j < legs.length; j++) {
      t += segTime * (lens[j] / total);
      waypoints.push({ t, p: legs[j], completes: j === 2 ? i + 1 : undefined });
    }
    prev = obs.exit;
  }

  const startReal = performance.now();
  const tick = () => {
    const simTime = Math.min(((performance.now() - startReal) / 1000) * playback, run.time);
    let pos = waypoints[0].p;
    let completed = 0;
    for (let i = 1; i < waypoints.length; i++) {
      const a = waypoints[i - 1], b = waypoints[i];
      if (b.completes !== undefined && simTime >= b.t) completed = b.completes;
      if (simTime <= b.t) {
        const f = (simTime - a.t) / (b.t - a.t || 1);
        pos = { x: a.p.x + (b.p.x - a.p.x) * f, z: a.p.z + (b.p.z - a.p.z) * f };
        break;
      }
      pos = b.p;
    }

    $('timer').textContent = simTime.toFixed(1);
    $('progress').textContent = `${completed}/${layout.length}`;
    drawCourse(layout, pos, completed);

    if (simTime < run.time) {
      animation = requestAnimationFrame(tick);
    } else {
      $('progress').textContent = `${layout.length}/${layout.length}`;
      const cls = run.result === 'Qualified' ? 'qualified'
        : run.result === 'TimeFaultOnly' ? 'timefault' : 'nq';
      const label = run.result === 'Qualified' ? 'QUALIFIED'
        : run.result === 'TimeFaultOnly' ? 'TIME FAULTS' : 'NON-QUALIFYING';
      setBanner(cls, `${label} — ${run.time.toFixed(1)}s (SCT ${course.standardTime}s)`);
    }
  };
  tick();
}

$('run').addEventListener('click', runLevel);
drawCourse(buildCourseLayout(data.courses[$('course').value]), { x: 0, z: 0 }, 0);

if (params.get('autorun') === '1') runLevel();
