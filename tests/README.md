# Agility Dogs — Test Harness

Automated verification for the Unity game, runnable without the Unity Editor.

## Layout

| Path | Purpose |
|------|---------|
| `tools/export-data.mjs` | Parses the Unity `.asset` YAML (courses, breeds, obstacles, handlers) into `data/gamedata.json` |
| `sim/engine.mjs` | JS port of the game's course/scoring rules (layout, movement, run results, show scoring). Source-of-truth mapping is documented at the top of the file |
| `unit/` | `node:test` suites: data integrity, per-level winnability for every breed, show win rates, career progression to Westminster |
| `websim/` | Playable canvas simulator of every level, driven by the exported data |
| `e2e/` | Playwright specs that play every level in the websim and assert a qualifying run |
| `compile-check/` | .NET project that compiles all runtime game scripts (and tests) against stub Unity APIs — catches compile errors without the editor |

## Running

```bash
cd tests
npm install
npm run export   # regenerate data/gamedata.json after editing assets
npm test         # unit suites
npm run e2e      # playwright (needs: npx playwright install chromium)
dotnet build compile-check   # compile gate
```

The websim can be explored manually: `node tools/serve.mjs` then open
http://127.0.0.1:8787/websim/index.html

## Keep in sync

If you change any of these in the Unity code, mirror the change in `sim/engine.mjs`:
- `CourseLayoutBuilder` layout constants
- obstacle speed multipliers in `ConcreteObstacles.cs`
- result thresholds in `AgilityScoringService.cs`
- show scoring in `ShowManager.cs`

Unity-side EditMode tests live in `Agility Dogs/Assets/Tests/Editor/` (run via
Unity Test Runner) — `GameLogicTests.cs` covers the same scoring semantics in-engine.
