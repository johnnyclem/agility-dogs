    Goal
    Build out the Unity project "Agility Dogs: Westminster Agility Masters - Handler's Edge" into a playable state using the Unity3D MCP server. The project has ~70 commits of code-only work (scripts, enums, data classes, services) but lacks proper Unity scene wiring — scenes exist but are mostly empty or broken (duplicate panels, missing prefabs, no NavMesh, no obstacle prefabs placed). The goal is to use the Unity MCP tools to build actual scene hierarchies, create obstacle prefabs, wire up references, and get the core gameplay loop working.
    Instructions
    - Use the Unity3D MCP server (unityMCP tools) to interact with the Unity Editor directly — create GameObjects, manage scenes, wire components, etc.
    - Follow the RELEASE_CANDIDATE_BURNDOWN.md as the master checklist — it shows all systems as "complete" in code, but scene-level work is the real remaining gap.
    - The MCP connection is configured as a remote server via opencode.json with an API key header.
    - The project is a Unity 6000.4.0f1 (URP) project at /Users/johnnyclem/Desktop/Repos/agility-dogs/. IMPORTANT: The Unity project root is inside Agility Dogs/ subdirectory — so the Assets folder is at /Users/johnnyclem/Desktop/Repos/agility-dogs/Agility Dogs/Assets/.
    - Active instance must be set via set_active_instance with exact format: Agility Dogs@74b781753c4a9be8
    Discoveries
    Unity MCP Connection
    - Remote proxy at https://mc-82a58513564647608b491c00acc383c8.ecs.us-east-2.on.aws/mcp
    - API key sk-cpl-869339f66556067346960d3813553c4397d3d0543dd33e0b47e164246dade5f3 as X-API-Key header
    - Config at /Users/johnnyclem/.config/opencode/opencode.json
    - manage_gameobject delete by_id does NOT work for scene objects — only the first item in a batch succeeded. Delete by_name works for active objects.
    - read_console has a reflection error and cannot read Unity console logs.
    - manage_scriptable_object tool works for creating/modifying ScriptableObjects.
    - manage_asset create only supports Folder, Material, PhysicsMaterial — NOT custom ScriptableObject types. Use manage_scriptable_object instead.
    - batch_execute has a limit of 25 commands per batch.
    - Component types must sometimes use fully-qualified names (e.g., AgilityDogs.Presentation.Camera.AgilityCameraController), but short names often work too.
    - manage_components set_property cannot set Transform references by integer instanceID — it fails with "Failed to convert value for serialized field 'entryPoint' to type 'Transform'." ScriptableObject asset references (by path string) work fine.
    - execute_menu_item fails for custom MenuItem tools — always returns "Failed to execute menu item. It might be invalid, disabled, or context-dependent." even after compilation and domain reload. Standard Unity menu items like File/Save DO work.
    - Loading scenes: Use build_index parameter reliably. Using name alone sometimes fails. Use manage_scene action: load, build_index: 1 for SampleScene.
    - CameraFollow script created in a prior session could NOT be compiled by Unity — likely due to existing compilation errors elsewhere blocking new script compilation. It was deleted; existing AgilityCameraController is used instead.
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
    Prior Session
    1. ✅ MCP Connection Established, all key gameplay scripts read
    2. ✅ SampleScene gameplay hierarchy built with all core objects (Main Camera, GameManager, CourseRunner, Ground with NavMeshSurface, Directional Light, Handler with CharacterController + PlayerInput, Dog with NavMeshAgent + Animator + DogAgentController + CommandBuffer, ScoringService, 5 obstacles (BarJump, Tunnel, WeavePoles, AFrame, PauseTable) with child EntryPoint/CommitPoint/ExitPoint, HUDCanvas with GameHUD, EventSystem, SceneBootstrap)
    3. ✅ QuickPlayCourse CourseDefinition ScriptableObject created
    This Session
    4. ✅ Duplicate GameHUD removed from HUDCanvas (was added twice by accident)
    5. ✅ Global Light 2D deleted from SampleScene (was a 2D light in a 3D scene)
    6. ✅ WeavePolesObstacle component added to WeavePoles GameObject (was missing — only had Transform)   7. ✅ 5 ObstacleData ScriptableObjects created and configured:
       - Assets/Data/Obstacles/BarJumpData.asset (BarJump=enum 1)
       - Assets/Data/Obstacles/TunnelData.asset (Tunnel=enum 5)
       - Assets/Data/Obstacles/WeavePolesData.asset (WeavePoles=enum 6)
       - Assets/Data/Obstacles/AFrameData.asset (AFrame=enum 7, hasContactZones=true, contactZoneLength=0.36)
       - Assets/Data/Obstacles/PauseTableData.asset (PauseTable=enum 10)
    8. ✅ ObstacleData references wired to all 5 obstacle components' obstacleData field via MCP set_property
    9. ✅ BorderCollie BreedData wired to Dog's DogAgentController breedData field
    10. ✅ QuickPlayCourse wired to CourseRunner's currentCourse field
    11. ✅ ObstacleBase.Awake() modified to auto-find child transforms by name ("EntryPoint", "CommitPoint", "ExitPoint") at runtime — solves the MCP inability to wire Transform references directly
    12. ✅ Build settings updated — Replaced StartMenu.unity (index 0, had 28 duplicate panels) with MainMenu.unity (clean menu scene) in EditorBuildSettings.asset
    13. ✅ SceneCleaner.cs enhanced — Added Tools/Bake NavMesh Surfaces and Tools/Wire Scene References menu items (though they can't be executed via MCP)
    14. ✅ SceneBootstrap.cs modified — Added BakeNavMesh() method that auto-bakes all NavMeshSurface components at runtime during Awake
    15. ✅ SampleScene saved with all changes
    Known Issues Still Remaining
    - NavMesh baking — Added runtime baking in SceneBootstrap, but NavMeshSurface.BuildNavMesh() is an Editor-only API. This WILL fail at runtime. Need to either: (a) bake NavMesh data as an asset manually, or (b) find an alternative runtime approach, or (c) use a different NavMesh bake strategy.
    - WeavePoles polePositions array not wired — The WeavePolesObstacle has a polePositions Transform[] that references Pole01 through Pole06 children. Can't be set via MCP (Transform ref issue). The editor script SceneCleaner.WireWeavePolePositions() was written but can't be executed via MCP. Best fix: add auto-find logic in WeavePolesObstacle.Awake() similar to ObstacleBase.
    - Transform references for entry/commit/exit points — Now handled at runtime by the modified ObstacleBase.Awake(), but only if ObstacleBase runs its own Awake. The concrete obstacles call base.Awake() so this should work. NEED TO VERIFY that WeavePolesObstacle calls base.Awake() — looking at ConcreteObstacles.cs, line 109: base.Awake() is called ✅.
    - Custom menu items can't be executed via MCP — The execute_menu_item tool doesn't work for custom MenuItem entries, even after recompilation and domain reload. This is a known MCP limitation.
    - StartMenu.unity still exists with 28 duplicate panels — but it's no longer in build settings (replaced by MainMenu.unity at index 0). Still in the project but not loaded at runtime.
    Relevant files / directories
    Project Root
    - /Users/johnnyclem/Desktop/Repos/agility-dogs/ — Git repo root
    - /Users/johnnyclem/Desktop/Repos/agility-dogs/Agility Dogs/ — Unity project root
    - /Users/johnnyclem/Desktop/Repos/agility-dogs/Agility Dogs/Assets/ — Unity Assets
    Scenes
    - Assets/Scenes/SampleScene.unity — Main gameplay scene (BUILT OUT, saved this session)
    - Assets/Scenes/StartMenu.unity — OLD menu scene (removed from build index 0, still in project, has 28 duplicate panels)
    - Assets/Scenes/MainMenu.unity — Clean menu scene (NOW at build index 0)
    - Assets/Scenes/CompetitionScene.unity, TrainingScene.unity, CareerScene.unity, Demo.unity — Other game scenes
    - Build settings: MainMenu(0), SampleScene(1), Demo(2), CompetitionScene(3), TrainingScene(4), CareerScene(5)
    Key Gameplay Scripts (modified this session)
    - Assets/Scripts/Gameplay/Obstacles/ObstacleBase.cs — Modified Awake() to auto-find EntryPoint/CommitPoint/ExitPoint children by name at runtime
    - Assets/Scripts/Services/SceneBootstrap.cs — Added NavMesh baking in Awake (WARNING: NavMeshSurface.BuildNavMesh() is Editor-only, will fail at runtime!)
    - Assets/Scripts/Editor/SceneCleaner.cs — Added BakeNavMesh and WireSceneReferences menu items (can't be executed via MCP)
    Key Gameplay Scripts (read, not modified)
    - Assets/Scripts/Gameplay/CourseRunner.cs — Orchestrates runs, wires to handler/dog/scoring
    - Assets/Scripts/Gameplay/Handler/HandlerController.cs — Player handler, requires CharacterController + PlayerInput
    - Assets/Scripts/Gameplay/Dog/DogAgentController.cs — Dog AI, requires NavMeshAgent + Animator + breed data
    - Assets/Scripts/Gameplay/Obstacles/ConcreteObstacles.cs — 15+ obstacle implementations
    - Assets/Scripts/Gameplay/Scoring/AgilityScoringService.cs — Fault/timer tracking
    - Assets/Scripts/Gameplay/Commands/CommandBuffer.cs — Command queue for dog
    - Assets/Scripts/Presentation/Camera/AgilityCameraController.cs — Camera follow system
    - Assets/Scripts/Services/GameManager.cs — Singleton game state, scene loading
    - Assets/Scripts/UI/GameHUD.cs — In-game HUD
    - Assets/Scripts/Events/GameEvents.cs — Static event system
    - Assets/Scripts/Core/AgilityEnums.cs — All enums (ObstacleType: None=0, BarJump=1, TireJump=2, BroadJump=3, WallJump=4, Tunnel=5, WeavePoles=6, AFrame=7, DogWalk=8, Teeter=9, PauseT:
able=10, ...)
    - Assets/Scripts/Data/CourseDefinition.cs — Course SO
    - Assets/Scripts/Data/BreedData.cs — Breed SO
    - Assets/Scripts/Data/ObstacleData.cs — Obstacle config SO
    Data Assets (created this session)
    - Assets/Data/Obstacles/BarJumpData.asset — guid: 1186caf01131d4a20aeb81e0afa1c6da
    - Assets/Data/Obstacles/TunnelData.asset — guid: 4f5766568f46e4f809af781e0e3e0a58
    - Assets/Data/Obstacles/WeavePolesData.asset — guid: d8b9cb2b0465f417abffbfc16e46e73b
    - Assets/Data/Obstacles/AFrameData.asset — guid: e566426b7717c4986b3ecb57c2286a59
    - Assets/Data/Obstacles/PauseTableData.asset — guid: 5b0b2d64419c849ab8394ff79dd76ecb
    - Assets/Data/Courses/QuickPlayCourse.asset — Created prior session
    Data Assets (existing)
    - Assets/Data/Breeds/ — 19 BreedData assets (BorderCollie guid: a1b2c3d4e5f6789012345678abcdef01)
    - Assets/Data/Handlers/ — 5 HandlerData assets
    - Assets/Data/Courses/ — 8+ CourseDefinition assets
    Config
    - /Users/johnnyclem/.config/opencode/opencode.json — MCP connection config
    - ProjectSettings/EditorBuildSettings.asset — Modified to use MainMenu.unity at index 0 instead of StartMenu.unity
    - Packages/manifest.json — Unity packages (NavMesh 2.0.4, InputSystem 1.19.0, URP 17.4.0)
    Dog Prefabs (Red_Deer)
    - Assets/Red_Deer/Dogs/*/Dog/Prefabs/ — 19 breeds
    Spec Documents
    - RELEASE_CANDIDATE_BURNDOWN.md — Master checklist (all shows "complete" in code)
    Next Steps (Priority Order)
    1. FIX CRITICAL: NavMesh baking is Editor-only — NavMeshSurface.BuildNavMesh() does NOT work at runtime. Remove the runtime bake from SceneBootstrap and instead: (a) bake NavMesh data as an asset in the editor via a script, or (b) use NavMeshSurface with pre-baked data. The SceneCleaner.cs already has BakeNavMeshSurfaces() — but it can't be executed via MCP. Alternative: manually call it by having Unity auto-execute on load, or use [InitializeOnLoad] / [DidReloadScripts] to trigger it.
    2. Add auto-wire for WeavePoles polePositions — Modify WeavePolesObstacle.Awake() (in ConcreteObstacles.cs) to auto-find children named "Pole01", "Pole02", etc. and populate the polePositions array, similar to how ObstacleBase auto-finds entry/commit/exit.
    3. Use [InitializeOnLoad] or [DidReloadScripts] in SceneCleaner.cs to auto-execute NavMesh baking and reference wiring when Unity enters play mode or after domain reload — this bypasses the execute_menu_item limitation.
    4. Save scene and test play mode — Verify the core loop: handler moves with input, dog follows via NavMesh, obstacles register entry/exit, scoring tracks faults.
    5. Check MainMenu.unity scene — Verify it actually works as build index 0 (has proper Canvas, MenuManager, buttons wired).
    6. Wire remaining CourseRunner references — handler, dog, scoringService SerializeFields (SceneBootstrap auto-wires these at runtime, but editor-time wiring would be nice for inspection).
    7. Test the full flow — Menu → SampleScene → countdown → gameplay → scoring → results.


