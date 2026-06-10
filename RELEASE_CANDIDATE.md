# Release Candidate — Beta

**Date:** June 10, 2026
**Branch:** `claude/game-review-release-candidate-4mv90f`
**Status:** All automated verification green (compile gate, 11 unit suites, 13 Playwright e2e tests)

This document supersedes the optimistic status in `RELEASE_CANDIDATE_BURNDOWN.md` /
`SUMMARY_CURRENT_STATE.md`. A full review of the code, gameplay, and mechanics found
the project had drifted significantly from those claims. This RC fixes the core game
loop end to end and verifies every level is actually winnable.

---

## What was actually broken (and is now fixed)

### Game-breaking

| # | Bug | Fix |
|---|-----|-----|
| 1 | **All 11 course assets had empty obstacle sequences.** Scoring treated `0 >= 0` obstacles as course-complete, so every run ended "Qualified" the moment the dog touched any obstacle | All 11 courses now have real 8–15 obstacle sequences with a difficulty ramp (15 new `ObstacleData` assets under `Resources/Data/Obstacles/Course/`) |
| 2 | **No `Resources/` folder existed**, yet 30+ call sites load courses/breeds/handlers via `Resources.LoadAll` — every one silently returned nothing | `Assets/Data` moved to `Assets/Resources/Data` (paths match all existing call sites) |
| 3 | **Courses had no physical layout** — every course reused the same 5 hand-placed scene obstacles | New `CourseLayoutBuilder` spawns each course's obstacle sequence at runtime on a deterministic serpentine layout |
| 4 | **The dog was never given its first obstacle target** (`AdvanceToNextObstacle` only ran *after* a completion) — runs could only progress by accident | `CourseRunner.StartRun` now targets obstacle 1 immediately |
| 5 | **Weave poles could never complete**: `AdvanceWeave()` was never called, `currentWeaveIndex` stayed 0 forever, and the dog hung in the `Weaving` state with no timeout | Dog now weaves pole-to-pole, advancing per pole, with a refusal-fault timeout failsafe |
| 6 | **Pause table always recorded a Refusal** — the dog exited instantly; the 5s wait state was only reachable via a manual command | Commit flow routes by obstacle type: `PauseTable → WaitingAtTable`, `WeavePoles → Weaving` |
| 7 | **Obstacle traversal speed was ~0.6–0.85 m/s** (absolute, instead of a fraction of breed max speed) — courses ballooned past max time | `navAgent.speed = breed.maxSpeed × obstacle multiplier × contact factor` |
| 8 | **Career tier progression deadlock**: unlocking County required wins *at County* (impossible before unlocking it). `OnTierUnlocked` could never fire | Ladder now uses cumulative career wins (2/4/6/8/12), matching the documented design |
| 9 | **Run completion fired multiple times** (scoring service and GameManager both raised `OnRunCompleted`; GameManager *and* ShowManager both processed career results) — double-counted wins and XP | Single authority: CourseRunner → ScoringService → GameManager raises the event once; ShowManager is the only career processor |
| 10 | **High tiers were statistically unwinnable**: show scoring used a fixed 60s time-bonus par, so on National/Westminster courses (65–70s SCT) a perfect run scored ~100 vs opponents up to ~115 | Time bonus is now relative to the course's standard time |
| 11 | **Westminster skill gate was unreachable** once stats were in honest units (raw base stats max ~0.5 vs gate 0.8) — compounded by puppy stats mixing m/s into a 0–1 rating | Stats normalized to 0–1, gate uses base + training (`GetEffectiveSkill()`), threshold 0.65 |

### Secondary fixes

- `EvaluateRunResult` rewritten: over max time → NonQualified; faults/over-SCT under max → TimeFaultOnly; clean+under SCT → Qualified (was: unreachable branches, over-max reported as "time fault only")
- Negative time-fault credit eliminated (`CeilToInt` of a negative over-time)
- Course personal best is now actually recorded on qualifying runs
- Wrong-course no longer skips the expected obstacle — the dog is re-targeted
- `GameModeManager` pointed at a scene (`StartMenu`) that isn't in the build → `MainMenu`
- Build settings referenced a nonexistent `Demo.unity` (fake GUID) — removed
- Selected course now propagates GameModeManager → GameManager → SceneBootstrap (was: gameplay always started `courses[0]`)
- Dog falls back to mode-selected/first breed when scene wiring is missing
- `GetLevelProgress` clamped at max level; split-delta guards against the unset sentinel best time
- Best In Show commentary system (568 lines of dialogue) was fully built but never instantiated — now wired via SceneBootstrap with a Resources fallback for its dialogue asset

## Dead code removed (~6,400 lines)

Verified unreferenced before deletion (class-name cross-reference + compile gate):

- **Abandoned commentary stacks (2 of 3):** `CommentaryManager`, `CommentaryDialogueManager`, `CommentaryDirector`, `EastworldClient`, `EastworldTestWindow` (the live system is `BestInShowDialogueManager` + the narrator suite)
- **Unwired stubs:** `VoiceCommandService` (v2 stub), `AddressableManager` (package not installed), `PlatformAbstractionLayer` + `PlatformManager` (duplicate, both dead), `SaveManager`, `ScreenReaderService`, `AccessibilitySettings`, `VOAssetManager`, `VOLocalizationService`
- **Dev cruft:** `DevTestRunner`, `SimpleTest`, `MCPPackageFixer` (disabled), `CharacterPortraitGenerator` (stub), legacy `StartMenu.unity` (28 duplicate panels)
- `AnnouncerType` enum extracted to its own file (was buried in a deleted file but used by live code — caught by the compile gate)

All recoverable from git history.

## Verification

| Gate | Scope | Result |
|------|-------|--------|
| **Compile check** (`tests/compile-check`) | All runtime scripts + tests compiled against stub Unity APIs | ✅ Build succeeded |
| **Unit suites** (`tests/unit`, 11 tests) | Data integrity, course/time coherence, JWW legality, **every course winnable by every one of the 19 breeds**, ≥15% SCT headroom, >50% first-place rate at every tier, full career sim reaches & wins Westminster in <120 shows | ✅ 11/11 |
| **Playwright e2e** (`tests/e2e`, 13 tests) | Every level played start-to-finish in the web simulator: qualifies under SCT, all obstacles completed; slowest breed finishes hardest course under max time | ✅ 13/13 |
| **Unity EditMode tests** (`Assets/Tests/Editor/GameLogicTests.cs`) | Same scoring semantics in-engine, layout determinism, asset integrity, stat normalization | Run in Unity Test Runner |

The e2e harness is a faithful JS port of the game's rules (mapping documented in
`tests/sim/engine.mjs`) since the Unity Editor can't run in this environment. The
in-editor `GameLogicTests` cover the same semantics against the real C#.

## Known limitations / follow-ups for beta

1. **Meta files are not tracked** in this repo, so the `Assets/Data → Assets/Resources/Data` move will regenerate GUIDs locally. Scene-serialized references to data assets (e.g. a course wired on `CourseRunner`) will need re-picking once; runtime fallbacks cover the gameplay path. Recommend committing `.meta` files going forward (fix the `.gitignore` un-ignore path — the project lives under `Agility Dogs/`, the rule assumes `Assets/` at repo root).
2. A final in-editor QA pass (Unity Test Runner + a manual run of Quick Play / Training / Career) is still recommended before shipping the beta — this environment cannot launch the Unity runtime.
3. Tournament/Campaign modes compile and their progression math is sound, but their UI flows received only static review.
4. ElevenLabs TTS requires an API key at runtime; commentary degrades gracefully without it.
