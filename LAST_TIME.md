# Agility Dogs - Project Status (Last Session)

## Goal

Build out the Unity project "Agility Dogs: Westminster Agility Masters - Handler's Edge" into a playable state. The project has ~70 commits of code-only work but lacks proper Unity scene wiring. Work has shifted from the SampleScene (gameplay scene with NavMesh agent dog) to the TrainingDemo scene (physics-based dog movement with keyboard controls).

## Instructions

- Use the Unity3D MCP server (unityMCP tools) to interact with the Unity Editor directly
- Project root: `/Users/johnnyclem/Desktop/Repos/agility-dogs/Agility Dogs/` (note the space)
- Active instance format: `Agility Dogs@74b781753c4a9be8`
- MCPPackageFixer.cs has `[InitializeOnLoad]` disabled to prevent infinite reload loops
- When Unity gets stuck in compilation loops, force-quit and reopen
- The user wants a working training scene where the dog can jump over hurdles

## Discoveries

### MCP Connection
- Remote proxy at `https://mc-82a58513564647608b491c00acc383c8.ecs.us-east-2.on.aws/mcp`
- `manage_gameobject delete by_id` does NOT work for scene objects — use `by_name`
- `read_console` is broken — cannot read Unity console logs, must infer from user feedback
- `execute_menu_item` fails for custom MenuItem entries
- `manage_components set_property` cannot set references by instanceID — use asset path strings for ScriptableObjects
- Unity frequently times out during compilation/domain reload — `telemetry_ping` works as health check
- `batch_execute` tool name in batch is NOT `unityMCP_manage_components` — use correct tool names

### Scene Architecture
- **SampleScene** (build index 1): Main gameplay with NavMesh agent dog, HandlerController, CourseRunner, 5 obstacles, SceneBootstrap, HUDCanvas
- **TrainingDemo** (build index 5): Physics-based training scene created at runtime by `TrainingDemoScene.cs` — dog uses Rigidbody + WASD/Space controls, no NavMesh
- **MainMenu** (build index 0): Menu scene with MenuManager that auto-creates UI panels at runtime
- URP pipeline configured with Renderer2D (index 0) and UniversalRenderer (index 1, 3D)

### TrainingDemo Scene Runtime Structure
- `TrainingDemoScene.cs` creates ALL obstacles, dog, lights, fences at runtime
- Dog: Corgi prefab, scaled to 1.5, uses Rigidbody (mass=10, drag=5) + CapsuleCollider
- Jumps: Cylinder poles + Cube crossbar, base collider removed (was blocking dog), crossbar now solid
- Tunnel: Cylinder rotated 90°, collider set as trigger
- Weave poles: 6 thin cylinders with trigger area collider
- Pause table: Cube top + legs, child colliders destroyed, only parent trigger remains
- Dog movement: `rb.MovePosition` at 2.5 units/s, jump force 12 (ForceMode.Impulse)

## Accomplished

### Completed
1. ✅ **MCP Connection** — configured and working with remote Unity instance
2. ✅ **SampleScene built out** — 16 root objects, obstacles wired, data assets configured
3. ✅ **Script fixes** — HandlerController auto-finds PlayerInput, DogAgentController auto-finds CommandBuffer, CourseRunner got SetScoringService()
4. ✅ **SceneBootstrap enhanced** — auto-wires scoringService, auto-starts course + countdown
5. ✅ **NavMesh baked** — user baked manually via NavMeshSurface component
6. ✅ **Ground enlarged** — scale from (5,1,5) to (50,1,50), obstacles spread in course layout
7. ✅ **MainMenu scene fixed** — Camera component added, has Canvas, MenuManager, EventSystem
8. ✅ **GameManager fixed** — mainMenuScene changed from "StartMenu" to "MainMenu"
9. ✅ **TrainingDemo jump fixes** — removed solid base collider, made crossbar solid, fixed ground detection bug, scaled dog to 1.5, increased jump force to 12
10. ✅ **TrainingDemo tunnel fix** — entrance collider set as trigger
11. ✅ **TrainingDemo pause table fix** — child colliders destroyed, only parent trigger remains
12. ✅ **Ground plane solidified** — replaced MeshCollider with BoxCollider for stable physics

### In Progress
- **TrainingDemo jump testing** — user was about to test the fixed jumps after ground collider replacement

### Remaining
- Test jump mechanics in TrainingDemo
- Fix any remaining TrainingDemo issues (collision, jumping feel, obstacles)
- Add visual polish to SampleScene (materials, lighting)
- Wire up GameHUD/UI for gameplay feedback
- Test full flow: MainMenu → Training/SampleScene → gameplay → scoring
- StartMenu.unity still exists with 28 duplicate panels (not in build settings)

## Key Files

### Scripts Modified This Session
- `Assets/Scripts/Demo/TrainingDemoScene.cs` — Creates TrainingDemo scene at runtime. Fixed jump colliders, ground detection, dog scale, jump force, tunnel trigger, pause table colliders, ground BoxCollider
- `Assets/Scripts/Services/SceneBootstrap.cs` — Removed NavMesh bake code, added auto-start course/countdown
- `Assets/Scripts/Gameplay/CourseRunner.cs` — Added SetScoringService() method
- `Assets/Scripts/Gameplay/Handler/HandlerController.cs` — Added GetComponent<PlayerInput>() in Awake()
- `Assets/Scripts/Gameplay/Dog/DogAgentController.cs` — Added GetComponent<CommandBuffer>() in Awake()
- `Assets/Scripts/Services/GameManager.cs` — Changed mainMenuScene to "MainMenu"
- `Assets/Scripts/Editor/SceneCleaner.cs` — Removed NavMesh bake code
- `Assets/Scripts/Editor/MCPPackageFixer.cs` — [InitializeOnLoad] commented out

### Scripts Read But Not Modified
- `Assets/Scripts/Gameplay/Obstacles/ObstacleBase.cs` — Auto-finds child transforms
- `Assets/Scripts/Gameplay/Obstacles/ConcreteObstacles.cs` — 15+ obstacle types
- `Assets/Scripts/Presentation/Camera/AgilityCameraController.cs` — Camera follow system
- `Assets/Scripts/UI/MenuManager.cs` — Auto-creates MainMenuPanel at runtime
- `Assets/Scripts/Services/GameModeManager.cs` — Alternative mode manager

### Data Assets
- `Assets/Data/Obstacles/` — BarJumpData, TunnelData, WeavePolesData, AFrameData, PauseTableData
- `Assets/Data/Courses/QuickPlayCourse.asset` — Wired to CourseRunner
- `Assets/Data/Breeds/BorderCollie.asset` — Wired to DogAgentController
- `Assets/Data/Breeds/` — 19 breed assets
- `Assets/Data/Handlers/` — 5 handler assets
- `Assets/Data/Courses/` — 8+ course assets

### Settings/Config
- `Assets/Settings/UniversalRP.asset` — URP pipeline
- `Assets/Settings/UniversalRenderer.asset` — 3D renderer (index 1)
- `Assets/Settings/Renderer2D.asset` — 2D renderer (index 0)
- `ProjectSettings/EditorBuildSettings.asset` — MainMenu(0), SampleScene(1), Demo(2), CompetitionScene(3), TrainingScene(4), CareerScene(5)

### Scenes
- `Assets/Scenes/TrainingDemo.unity` — Currently working scene
- `Assets/Scenes/SampleScene.unity` — Main gameplay scene
- `Assets/Scenes/MainMenu.unity` — Menu scene
- `Assets/Scenes/StartMenu.unity` — Old menu (removed from build)

### Dog Prefabs (Red_Deer)
- `Assets/Red_Deer/Dogs/Corgi/Dog/Prefabs/Corgi_anim_RM.prefab` — Used in TrainingDemo
- `Assets/Red_Deer/Dogs/*/Dog/Prefabs/` — 19 breeds available
