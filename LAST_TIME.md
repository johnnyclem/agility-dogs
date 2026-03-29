Goal
Build out the Unity project "Agility Dogs: Westminster Agility Masters - Handler's Edge" into a playable state using the Unity3D MCP server. The project has ~70 commits of code-only work (scripts, enums, data classes, services) but lacks proper Unity scene wiring — scenes exist but are mostly empty or broken (duplicate panels, missing prefabs, no NavMesh, no obstacle prefabs placed). The goal is to use the Unity MCP tools to build actual scene hierarchies, create obstacle prefabs, wire up references, and get the core gameplay loop working.
Instructions
- Use the Unity3D MCP server (unityMCP tools) to interact with the Unity Editor directly — create GameObjects, manage scenes, wire components, etc.
- Follow the RELEASE_CANDIDATE_BURNDOWN.md as the master checklist — it shows all systems as "complete" in code, but scene-level work is the real remaining gap.
- The MCP connection is configured as a remote server via opencode.json with an API key header.
- The project is a Unity 6000.4.0f1 (URP) project at /Users/johnnyclem/Desktop/Repos/agility-dogs/. IMPORTANT: The Unity project root is inside "Agility Dogs/" subdirectory — so the Assets folder is at /Users/johnnyclem/Desktop/Repos/agility-dogs/Agility Dogs/Assets/.
- Active instance must be set via set_active_instance with exact format: Agility Dogs@74b781753c4a9be8
Discoveries
Unity MCP Connection
- Remote proxy at https://mc-82a58513564647608b491c00acc383c8.ecs.us-east-2.on.aws/mcp
- API key sk-cpl-869339f66556067346960d3813553c4397d3d0543dd33e0b47e164246dade5f3 as X-API-Key header
- Config at /Users/johnnyclem/.config/opencode/opencode.json
- manage_gameobject delete by_id does NOT work for scene objects — only the first item in a batch succeeded. Delete by_name works for active objects.
- read_console has a reflection error and cannot read Unity console logs — you CANNOT read errors from the console. Must infer success from whether scene operations work.
- manage_scriptable_object tool works for creating/modifying ScriptableObjects.
- manage_asset create only supports Folder, Material, PhysicsMaterial — NOT custom ScriptableObject types. Use manage_scriptable_object instead.
- batch_execute has a limit of 25 commands per batch.
- Component types must sometimes use fully-qualified names (e.g., AgilityDogs.Presentation.Camera.AgilityCameraController), but short names often work too.
- manage_components set_property cannot set Transform references by integer instanceID — it fails with conversion errors. ScriptableObject asset references (by path string) work fine.
- execute_menu_item fails for custom MenuItem tools — always returns "Failed to execute menu item" even after compilation. Standard Unity menu items like File/Save DO work.
- Loading scenes: Use build_index parameter reliably. Using name alone sometimes fails.
- [InitializeOnLoad] auto-executing NavMesh baking in SceneCleaner caused Unity to become unresponsive — had to be reverted. Do NOT auto-call expensive editor operations on domain reload.
- Unity MCP frequently times out during compilation/domain reload. Wait 30-60 seconds and retry. manage_editor telemetry_ping works even when other tools timeout — use it as a health check.
Code Architecture (already compiled and working)
- SceneBootstrap.cs auto-finds and wires handler, dog, camera, courseRunner, scoring at runtime via FindObjectOfType. Has autoFindReferences flag.
- AgilityCameraController.cs — full camera system with Follow, Overview, SideOn, DogPOV, Cinematic, Replay modes. Has SetTarget(Transform).
- MenuManager.cs has AutoWireReferences() and CreateMissingPanels() that auto-creates panels and buttons at runtime if not wired in editor.
- All 7 core gameplay classes exist and compile: CourseRunner, HandlerController, DogAgentController, AgilityScoringService, CommandBuffer, ObstacleBase, ConcreteObstacles (15+ obstacle types).
- Dog prefabs exist from Red_Deer asset pack (19+ breeds in Assets/Red_Deer/Dogs/*/Dog/Prefabs/).
- Many data assets already exist: 8+ CourseDefinition assets, 19 BreedData assets, 5 HandlerData assets, skill trees, etc.
- Project uses com.unity.ai.navigation 2.0.4 (NavMeshSurface available), Input System 1.19.0, URP.
- Input actions at Assets/InputSystem_Actions.inputactions has Player map with Move, Look, Sprint, Jump, Attack, Interact, Crouch, Previous, Next.
Accomplished
Prior Session (from LAST_TIME.md)
1. ✅ MCP Connection Established, all key gameplay scripts read
2. ✅ SampleScene gameplay hierarchy built with all core objects (Main Camera, GameManager, CourseRunner, Ground with NavMeshSurface, Directional Light, Handler with CharacterController + PlayerInput, Dog with NavMeshAgent + Animator + DogAgentController + CommandBuffer, ScoringService, 5 obstacles with child EntryPoint/CommitPoint/ExitPoint, HUDCanvas with GameHUD, EventSystem, SceneBootstrap)
3. ✅ QuickPlayCourse CourseDefinition ScriptableObject created
4. ✅ Duplicate GameHUD removed from HUDCanvas
5. ✅ Global Light 2D deleted from SampleScene
6. ✅ WeavePolesObstacle component added to WeavePoles GameObject
7. ✅ 5 ObstacleData ScriptableObjects created and configured (BarJump, Tunnel, WeavePoles, AFrame, PauseTable)
8. ✅ ObstacleData references wired to all 5 obstacle components
9. ✅ BorderCollie BreedData wired to Dog's DogAgentController
10. ✅ QuickPlayCourse wired to CourseRunner's currentCourse
11. ✅ ObstacleBase.Awake() modified to auto-find EntryPoint/CommitPoint/ExitPoint children by name at runtime
12. ✅ Build settings updated — Replaced StartMenu.unity with MainMenu.unity at index 0
13. ✅ SceneCleaner.cs enhanced with Wire Scene References menu item
This Session
14. ✅ Fixed duplicate OnDestroy in CareerUIManager.cs — Merged two OnDestroy methods (event cleanup + singleton null) into one at line 107, removed duplicate at line 864
15. ✅ Fixed duplicate OnDestroy in TournamentUI.cs — Same fix, merged into one at line 70, removed duplicate at line 382
16. ✅ Removed broken runtime NavMesh bake from SceneBootstrap.cs — NavMeshSurface.BuildNavMesh() is Editor-only API, would fail at runtime. Removed BakeNavMesh() method entirely and removed using UnityEngine.AI;
17. ✅ Added AutoFindPolePositions() to WeavePolesObstacle — In ConcreteObstacles.cs, WeavePolesObstacle.Awake() now auto-discovers children named "Pole01", "Pole02", etc., sorts them numerically, and populates the polePositions array
18. ✅ Cleaned up SceneCleaner.cs — Removed NavMesh bake code (which couldn't compile because Editor assembly doesn't reference NavMesh package). Removed using UnityEngine.AI;. Kept Cleanup Duplicate Panels and Wire Scene References menu items.
19. ✅ Attempted InitializeOnLoad auto-bake — Caused Unity to become completely unresponsive. Reverted.
20. ✅ Compilation verified — All scripts compile, SampleScene loads successfully with 16 root objects
Known Issues Still Remaining
- NavMesh NOT baked — NavMeshSurface exists on Ground plane but has no baked data. Must be baked manually via Unity's Window > AI > Navigation panel (cannot be done via MCP). Without this, the dog's NavMeshAgent won't navigate.
- Custom menu items can't be executed via MCP — execute_menu_item doesn't work for custom MenuItem entries.
- StartMenu.unity still exists with 28 duplicate panels — no longer in build settings but still in project.
- read_console is broken — Cannot read Unity console logs due to reflection errors.
Next Steps (Priority Order)
1. NavMesh baking — Must be done manually in Unity Editor (Window > AI > Navigation > Bake). This is the #1 blocker for gameplay — dog movement requires NavMesh data.
2. Test play mode — Verify core loop: handler moves with input, dog follows via NavMesh, obstacles register entry/exit, scoring tracks faults.
3. Check MainMenu.unity — Verify it works as build index 0 (has proper Canvas, MenuManager, buttons wired).
4. Wire remaining CourseRunner references — handler, dog, scoringService SerializeFields (SceneBootstrap auto-wires at runtime, but editor-time wiring would help inspection).
5. Test full flow — Menu → SampleScene → countdown → gameplay → scoring → results.
Relevant files / directories
Project Root
- /Users/johnnyclem/Desktop/Repos/agility-dogs/ — Git repo root
- /Users/johnnyclem/Desktop/Repos/agility-dogs/Agility Dogs/ — Unity project root
- /Users/johnnyclem/Desktop/Repos/agility-dogs/Agility Dogs/Assets/ — Unity Assets
Scenes
- Assets/Scenes/SampleScene.unity — Main gameplay scene (BUILT OUT, 16 root objects, saved)
- Assets/Scenes/MainMenu.unity — Clean menu scene (build index 0)
- Assets/Scenes/StartMenu.unity — OLD menu scene (removed from build, has 28 duplicate panels)
- Assets/Scenes/CompetitionScene.unity, TrainingScene.unity, CareerScene.unity, Demo.unity — Other game scenes
- Build settings: MainMenu(0), SampleScene(1), Demo(2), CompetitionScene(3), TrainingScene(4), CareerScene(5)
Key Gameplay Scripts (modified this session)
- Assets/Scripts/UI/CareerUIManager.cs — Fixed duplicate OnDestroy (merged cleanup + singleton reset)
- Assets/Scripts/UI/TournamentUI.cs — Fixed duplicate OnDestroy (merged cleanup + singleton reset)
- Assets/Scripts/Services/SceneBootstrap.cs — Removed broken runtime NavMesh bake, removed using UnityEngine.AI;
- Assets/Scripts/Gameplay/Obstacles/ConcreteObstacles.cs — Added AutoFindPolePositions() to WeavePolesObstacle.Awake()
- Assets/Scripts/Editor/SceneCleaner.cs — Removed NavMesh bake code (couldn't compile), removed using UnityEngine.AI;, kept wiring utilities
Key Gameplay Scripts (modified prior session, not this session)
- Assets/Scripts/Gameplay/Obstacles/ObstacleBase.cs — Modified Awake() to auto-find EntryPoint/CommitPoint/ExitPoint children by name
Key Gameplay Scripts (read, not modified)
- Assets/Scripts/Gameplay/CourseRunner.cs — Orchestrates runs, wires to handler/dog/scoring
- Assets/Scripts/Gameplay/Handler/HandlerController.cs — Player handler, requires CharacterController + PlayerInput
- Assets/Scripts/Gameplay/Dog/DogAgentController.cs — Dog AI, requires NavMeshAgent + Animator + breed data
- Assets/Scripts/Gameplay/Scoring/AgilityScoringService.cs — Fault/timer tracking
- Assets/Scripts/Gameplay/Commands/CommandBuffer.cs — Command queue for dog
- Assets/Scripts/Presentation/Camera/AgilityCameraController.cs — Camera follow system
- Assets/Scripts/Services/GameManager.cs — Singleton game state, scene loading
- Assets/Scripts/UI/GameHUD.cs — In-game HUD
- Assets/Scripts/Events/GameEvents.cs — Static event system
- Assets/Scripts/Core/AgilityEnums.cs — All enums (ObstacleType: None=0, BarJump=1, TireJump=2, BroadJump=3, WallJump=4, Tunnel=5, WeavePoles=6, AFrame=7, DogWalk=8, Teeter=9, PauseTable=10, ...)
- Assets/Scripts/Data/CourseDefinition.cs — Course SO
- Assets/Scripts/Data/BreedData.cs — Breed SO
- Assets/Scripts/Data/ObstacleData.cs — Obstacle config SO
Data Assets (created prior session)
- Assets/Data/Obstacles/BarJumpData.asset — BarJump=enum 1
- Assets/Data/Obstacles/TunnelData.asset — Tunnel=enum 5
- Assets/Data/Obstacles/WeavePolesData.asset — WeavePoles=enum 6
- Assets/Data/Obstacles/AFrameData.asset — AFrame=enum 7, hasContactZones=true
- Assets/Data/Obstacles/PauseTableData.asset — PauseTable=enum 10
- Assets/Data/Courses/QuickPlayCourse.asset — Quick play course definition
Data Assets (existing)
- Assets/Data/Breeds/ — 19 BreedData assets
- Assets/Data/Handlers/ — 5 HandlerData assets
- Assets/Data/Courses/ — 8+ CourseDefinition assets
Config
- /Users/johnnyclem/.config/opencode/opencode.json — MCP connection config
- ProjectSettings/EditorBuildSettings.asset — MainMenu(0), SampleScene(1), Demo(2), CompetitionScene(3), TrainingScene(4), CareerScene(5)
- Packages/manifest.json — Unity packages (NavMesh 2.0.4, InputSystem 1.19.0, URP 17.4.0)
Dog Prefabs (Red_Deer)
- Assets/Red_Deer/Dogs/*/Dog/Prefabs/ — 19 breeds
Spec Documents
- RELEASE_CANDIDATE_BURNDOWN.md — Master checklist (all shows "complete" in code)
- LAST_TIME.md — Prior session notes (now partially outdated after this session's changes)
