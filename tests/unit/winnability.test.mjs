import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
  simulateRun, calculateScore, placementFor, xpForLevel, XP_REWARDS,
  eligibleTier, mulberry32, TIER_COURSE, TIER_LADDER,
} from '../sim/engine.mjs';

const here = dirname(fileURLToPath(import.meta.url));
const data = JSON.parse(readFileSync(join(here, '..', 'data', 'gamedata.json'), 'utf8'));

const courseByFile = new Map(data.courses.map(c => [c._file.replace('.asset', ''), c]));

// ---------- Data integrity ----------

test('every course has a non-empty, fully resolved obstacle sequence', () => {
  for (const course of data.courses) {
    assert.ok(course.resolvedSequence.length >= 8,
      `${course.courseName}: only ${course.resolvedSequence.length} obstacles`);
    for (const [i, o] of course.resolvedSequence.entries()) {
      assert.ok(o, `${course.courseName}: obstacle ${i} did not resolve (broken GUID ref)`);
    }
  }
});

test('course times are coherent (0 < standard < maximum)', () => {
  for (const course of data.courses) {
    assert.ok(course.standardTime > 0, `${course.courseName}: standardTime`);
    assert.ok(course.maximumTime > course.standardTime,
      `${course.courseName}: maximumTime ${course.maximumTime} <= standardTime ${course.standardTime}`);
  }
});

test('jumpers-with-weaves courses contain no contact obstacles or table', () => {
  const banned = new Set([7, 8, 9, 10]);
  for (const course of data.courses.filter(c => c.courseType === 1)) {
    for (const o of course.resolvedSequence) {
      assert.ok(!banned.has(o.type), `${course.courseName}: illegal obstacle ${o.name}`);
    }
    assert.ok(course.resolvedSequence.some(o => o.type === 6),
      `${course.courseName}: JWW course must include weave poles`);
  }
});

test('every breed has movement stats within BreedData ranges', () => {
  for (const b of data.breeds) {
    assert.ok(b.maxSpeed >= 3 && b.maxSpeed <= 12, `${b.breedName}: maxSpeed ${b.maxSpeed}`);
    assert.ok(b.acceleration >= 2 && b.acceleration <= 15, `${b.breedName}: acceleration`);
    assert.ok(b.weaveSpeed >= 0.5 && b.weaveSpeed <= 1.5, `${b.breedName}: weaveSpeed`);
    assert.ok(b.contactSpeed >= 0.5 && b.contactSpeed <= 1.3, `${b.breedName}: contactSpeed`);
  }
});

test('career tiers all map to an existing course', () => {
  for (const [tier, file] of Object.entries(TIER_COURSE)) {
    assert.ok(courseByFile.has(file), `${tier}: missing course asset ${file}`);
  }
});

// ---------- Per-level winnability ----------

test('every course is completable under maximum time by every breed', () => {
  for (const course of data.courses) {
    for (const breed of data.breeds) {
      const run = simulateRun(course, breed);
      assert.ok(run.time < course.maximumTime,
        `${course.courseName} x ${breed.breedName}: ${run.time.toFixed(1)}s >= max ${course.maximumTime}s`);
    }
  }
});

test('every course is winnable (clean, under standard course time) by every breed', () => {
  for (const course of data.courses) {
    for (const breed of data.breeds) {
      const run = simulateRun(course, breed);
      assert.equal(run.result, 'Qualified',
        `${course.courseName} x ${breed.breedName}: ${run.result} at ${run.time.toFixed(1)}s (SCT ${course.standardTime}s)`);
    }
  }
});

test('standard course times leave at least 15% headroom for an average breed', () => {
  const avgBreed = data.breeds.find(b => b.breedName === 'Labrador') ?? data.breeds[0];
  for (const course of data.courses) {
    const run = simulateRun(course, avgBreed);
    assert.ok(run.time * 1.15 <= course.standardTime,
      `${course.courseName}: ${run.time.toFixed(1)}s x1.15 exceeds SCT ${course.standardTime}s — too tight`);
  }
});

// ---------- Show placement winnability ----------

test('a clean run wins first place more often than not at every tier', () => {
  const rng = mulberry32(42);
  for (const { tier } of TIER_LADDER) {
    const course = courseByFile.get(TIER_COURSE[tier]);
    const breed = data.breeds.find(b => b.breedName === 'BorderCollie');
    const run = simulateRun(course, breed);
    const playerScore = calculateScore(run.result, run.time, 0, course.standardTime);

    let wins = 0;
    const trials = 500;
    for (let i = 0; i < trials; i++) {
      if (placementFor(playerScore, tier, rng) === 1) wins++;
    }
    const winRate = wins / trials;
    assert.ok(winRate > 0.5,
      `${tier}: clean-run win rate ${(winRate * 100).toFixed(0)}% — tier not reliably winnable`);
  }
});

// ---------- Career progression to Westminster ----------

test('career reaches and wins Westminster within a reasonable number of shows', () => {
  const rng = mulberry32(7);
  const breed = data.breeds.find(b => b.breedName === 'BorderCollie');

  let xp = 0, level = 1, totalWins = 0, competitions = 0;
  // Effective skill after the breeding fix: ~0.45 base + 0.3 training.
  const effectiveSkill = 0.75;
  const westminsterGate = () =>
    totalWins >= 12 && level >= 25 && competitions >= 20 && effectiveSkill >= 0.65;

  let westminsterWon = false;
  let shows = 0;
  while (!westminsterWon && shows < 200) {
    shows++;
    const tier = eligibleTier(level, totalWins, westminsterGate());
    const course = courseByFile.get(TIER_COURSE[tier]);
    const run = simulateRun(course, breed);
    const score = calculateScore(run.result, run.time, 0, course.standardTime);
    const placement = placementFor(score, tier, rng);

    const showResult =
      placement === 1 ? 'FirstPlace' :
      placement === 2 ? 'SecondPlace' :
      placement === 3 ? 'ThirdPlace' : 'HonorableMention';

    competitions++;
    if (placement === 1) totalWins++;
    xp += XP_REWARDS[showResult];
    while (level < 50 && xp >= xpForLevel(level + 1)) level++;

    if (tier === 'Westminster' && placement === 1) westminsterWon = true;
  }

  assert.ok(westminsterWon, `Westminster never won in ${shows} shows (level ${level}, wins ${totalWins})`);
  assert.ok(shows <= 120, `Career took ${shows} shows — progression too grindy`);
});

test('XP curve reaches the Westminster level requirement at sane totals', () => {
  // Level 25 must be attainable from realistic per-show XP (100-500).
  const xpNeeded = xpForLevel(25);
  assert.ok(xpNeeded < 5000, `Level 25 needs ${xpNeeded} XP — unreachable`);
});
