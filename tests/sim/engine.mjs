// Gameplay simulation engine — a direct port of the Unity game's course and
// scoring rules, used to validate that every level is winnable.
//
// Source-of-truth mapping (keep in sync):
//   layout            -> Assets/Scripts/Gameplay/CourseLayoutBuilder.cs
//   movement speeds   -> Assets/Scripts/Gameplay/Dog/DogAgentController.cs
//   speed multipliers -> Assets/Scripts/Gameplay/Obstacles/ConcreteObstacles.cs
//   run results       -> Assets/Scripts/Gameplay/Scoring/AgilityScoringService.cs
//   show scoring      -> Assets/Scripts/Services/ShowManager.cs

// ---- Layout (CourseLayoutBuilder.cs) ----
export const OBSTACLES_PER_ROW = 4;
export const COLUMN_SPACING = 8;
export const ROW_SPACING = 9;
export const START_OFFSET = 6;

export function obstaclePosition(index) {
  const row = Math.floor(index / OBSTACLES_PER_ROW);
  let col = index % OBSTACLES_PER_ROW;
  if (row % 2 === 1) col = OBSTACLES_PER_ROW - 1 - col;
  return {
    x: (col - (OBSTACLES_PER_ROW - 1) * 0.5) * COLUMN_SPACING,
    z: START_OFFSET + row * ROW_SPACING,
  };
}

export function obstacleDirection(index) {
  const row = Math.floor(index / OBSTACLES_PER_ROW);
  const posInRow = index % OBSTACLES_PER_ROW;
  if (posInRow === OBSTACLES_PER_ROW - 1) return { x: 0, z: 1 };
  return row % 2 === 0 ? { x: 1, z: 0 } : { x: -1, z: 0 };
}

// ---- Obstacle behavior (ConcreteObstacles.cs / DogAgentController.cs) ----
const TYPE_NAMES = {
  1: 'BarJump', 2: 'TireJump', 3: 'BroadJump', 4: 'WallJump', 5: 'Tunnel',
  6: 'WeavePoles', 7: 'AFrame', 8: 'DogWalk', 9: 'Teeter', 10: 'PauseTable',
  11: 'DoubleJump', 12: 'TripleJump', 13: 'PanelJump', 14: 'LongJump', 15: 'SpreadJump',
};
export const typeName = (t) => TYPE_NAMES[t] ?? `Type${t}`;

// GetSpeedMultiplier overrides in ConcreteObstacles.cs
const SPEED_MULTIPLIERS = { 5: 1.1, 7: 0.75, 8: 0.65, 9: 0.7 };
const CONTACT_TYPES = new Set([7, 8, 9]); // AFrame, DogWalk, Teeter

export const PAUSE_TABLE_WAIT = 5.5; // UpdateWaitingAtTable: required 5s + 0.5s margin
export const WEAVE_POLES = 12;
export const WEAVE_SPACING = 0.6;
export const WEAVE_SPEED_FACTOR = 0.45; // UpdateCommittingToObstacle weave entry

// Build runtime obstacle descriptors for a course's resolved sequence.
export function buildCourseLayout(course) {
  return course.resolvedSequence.map((o, i) => {
    const pos = obstaclePosition(i);
    const dir = obstacleDirection(i);
    const halfLength = Math.max(0.5, (o.length ?? 1) * 0.5);
    const point = (d) => ({ x: pos.x + dir.x * d, z: pos.z + dir.z * d });
    return {
      index: i,
      type: o.type,
      name: o.name,
      halfLength,
      position: pos,
      direction: dir,
      commit: point(-halfLength - 1.5),
      entry: point(-halfLength - 0.5),
      exit: point(halfLength + 0.5),
    };
  });
}

const dist = (a, b) => Math.hypot(b.x - a.x, b.z - a.z);

// Time to cover a straight segment with acceleration-limited trapezoidal
// profile, starting from standstill-ish transitions at obstacles.
function segmentTime(distance, topSpeed, accel) {
  const rampDist = (topSpeed * topSpeed) / (2 * accel);
  if (distance <= rampDist) return Math.sqrt((2 * distance) / accel);
  return topSpeed / accel + (distance - rampDist) / topSpeed;
}

// NavMesh paths are not straight lines; apply a curvature inefficiency.
const PATH_INEFFICIENCY = 1.2;

/**
 * Simulate a full run of a course by the given breed.
 * Returns { time, segments: [{name, type, time}], result, timeFaults }.
 */
export function simulateRun(course, breed) {
  const layout = buildCourseLayout(course);
  const maxSpeed = breed.maxSpeed;
  const accel = breed.acceleration;

  let time = 0;
  let pos = { x: 0, z: 0 }; // course start position
  const segments = [];

  for (const obs of layout) {
    const segStart = time;

    // Approach: run to commit point, then entry (DogState.Running/Seeking/Committing)
    const approach = (dist(pos, obs.commit) + dist(obs.commit, obs.entry)) * PATH_INEFFICIENCY;
    time += segmentTime(approach, maxSpeed, accel);

    // Traverse entry -> exit at obstacle speed (UpdateOnObstacle)
    if (obs.type === 6) {
      // Weave: pole-to-pole at weave speed (UpdateWeaving)
      const weaveSpeed = maxSpeed * (breed.weaveSpeed ?? 1) * WEAVE_SPEED_FACTOR;
      const weaveDist = (WEAVE_POLES - 1) * WEAVE_SPACING + 2; // poles + entry/exit margin
      time += weaveDist / weaveSpeed;
    } else if (obs.type === 10) {
      // Pause table: mandatory wait
      time += dist(obs.entry, obs.exit) / Math.max(1, maxSpeed * 0.5);
      time += PAUSE_TABLE_WAIT;
    } else {
      const mult = SPEED_MULTIPLIERS[obs.type] ?? 1;
      const contact = CONTACT_TYPES.has(obs.type) ? (breed.contactSpeed ?? 0.85) : 1;
      const speed = Math.max(0.5, maxSpeed * mult * contact);
      time += dist(obs.entry, obs.exit) / speed;
    }

    pos = obs.exit;
    segments.push({ name: obs.name, type: obs.type, time: time - segStart });
  }

  // Result mapping (AgilityScoringService.EvaluateRunResult, zero faults)
  let result;
  if (time >= course.maximumTime) result = 'NonQualified';
  else if (time <= course.standardTime) result = 'Qualified';
  else result = 'TimeFaultOnly';

  const timeFaults = Math.max(0, Math.ceil(Math.max(0, time - course.standardTime) / 5));
  return { time, segments, result, timeFaults };
}

// ---- Show scoring (ShowManager.cs) ----
export function calculateScore(runResult, time, faults, coursePar) {
  let score = { Qualified: 100, TimeFaultOnly: 60, NonQualified: 30, Elimination: 0 }[runResult] ?? 0;
  score -= faults * 10;
  const par = coursePar > 0 ? coursePar : 60;
  score += Math.max(0, 1 - time / par) * 50;
  return Math.max(0, score);
}

export const TIER_BASE_SKILL = {
  Local: 0.3, County: 0.4, Regional: 0.5, State: 0.6, National: 0.7, Westminster: 0.85,
};
export const OPPONENT_VARIANCE = 0.2;
export const COMPETITORS_PER_SHOW = 8;

export function simulateCompetitorScore(skill, rng) {
  const variance = (rng() * 2 - 1) * 20;
  return Math.max(0, skill * 100 + variance);
}

export function placementFor(playerScore, tier, rng) {
  const base = TIER_BASE_SKILL[tier];
  let placement = 1;
  for (let i = 0; i < COMPETITORS_PER_SHOW - 1; i++) {
    const skill = Math.min(1, Math.max(0, base + (rng() * 2 - 1) * OPPONENT_VARIANCE));
    if (simulateCompetitorScore(skill, rng) > playerScore) placement++;
  }
  return Math.min(placement, COMPETITORS_PER_SHOW);
}

// XP ladder (CareerProgressionService.cs): cumulative XP for a level
export function xpForLevel(level) {
  if (level <= 1) return 0;
  return Math.round(100 * Math.pow(1.15, level - 2));
}

export const XP_REWARDS = {
  BestInShow: 500, FirstPlace: 300, SecondPlace: 200, ThirdPlace: 150,
  HonorableMention: 100, DidNotPlace: 50,
};

// Tier ladder (ShowManager.cs after fix): level gate + cumulative wins gate
export const TIER_LADDER = [
  { tier: 'Local', level: 1, wins: 0 },
  { tier: 'County', level: 5, wins: 2 },
  { tier: 'Regional', level: 10, wins: 4 },
  { tier: 'State', level: 15, wins: 6 },
  { tier: 'National', level: 20, wins: 8 },
  { tier: 'Westminster', level: 25, wins: 12 },
];

export function eligibleTier(level, totalWins, westminsterOk) {
  for (let i = TIER_LADDER.length - 1; i >= 0; i--) {
    const t = TIER_LADDER[i];
    if (t.tier === 'Westminster' && !westminsterOk) continue;
    if (level >= t.level && totalWins >= t.wins) return t.tier;
  }
  return 'Local';
}

// Deterministic RNG for reproducible tests
export function mulberry32(seed) {
  let a = seed >>> 0;
  return function () {
    a |= 0; a = (a + 0x6d2b79f5) | 0;
    let t = Math.imul(a ^ (a >>> 15), 1 | a);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

export const TIER_COURSE = {
  Local: 'LocalPark', County: 'CountyFair', Regional: 'RegionalChamp',
  State: 'StateChamp', National: 'NationalChamp', Westminster: 'Westminster',
};
