Goal
Build out the Unity project "Agility Dogs: Westminster Agility Masters - Handler's Edge" into a playable state using the Unity3D MCP server. The project has ~70 commits of code-only work (scripts, enums, data classes, services) but lacks proper Unity scene wiring — scenes exist but are mostly empty or broken (duplicate panels, missing prefabs, no NavMesh, no obstacle prefabs placed). The goal is to use the Unity MCP tools to build actual scene hierarchies, create obstacle prefabs, wire up references, and get the core gameplay loop working.
Instructions
- Use the Unity3D MCP server (unityMCP tools) to interact with the Unity Editor directly — create GameObjects, manage scenes, wire components, etc.
- Follow the RELEASE_CANDIDATE_BURNDOWN.md as the master checklist — it shows all systems as "complete" in code, but scene-level work is the real remaining gap.
- The MCP connection is configured as a remote server via opencode.json with an API key header.
- The project is a Unity 6000.4.0f1 (URP) project at /Users/johnnyclem/Desktop/Repos/agility-dogs/.
Discoveries
Unity MCP Connection
- The Unity MCP server is a remote proxy at https://mc-82a58513564647608b491c00acc383c8.ecs.us-east-2.on.aws/mcp
- API key sk-cpl-869339f66556067346960d3813553c4397d3d0543dd33e0b47e164246dade5f3 must be passed as X-API-Key header
- Config is at /Users/johnnyclem/.config/opencode/opencode.json — the headers field was added manually
- Active instance must be set via set_active_instance with exact Name@hash format. The instance is: Agility Dogs@74b781753c4a9be8
- manage_gameobject delete action with search_method: "by_id" does NOT work for scene objects — only by_name works for find, and deletion via batch by ID failed consistently
- execute_menu_item tool failed for custom menu items (might need exact formatting)
- manage_gameobject get_components is not a valid action — only create, modify, delete, duplicate, move_relative
- read_console has a reflection error and cannot read Unity console logs
Scene State
- StartMenu.unity (build index 0): Had 29 children under Canvas — 4 duplicate copies of each panel type (MainMenuPanel, QuickPlayPanel, TrainingPanel, TeamSelectPanel, SettingsPanel, ResultsPanel, ModeSelectPanel, PausePanel). Deletion attempts via MCP failed.
- SampleScene.unity (build index 1): Minimal — just Main Camera (with URP + AudioListener), Global Light 2D, GameManager (empty), CourseRunner (empty).
- MainMenu.unity was created fresh as a clean replacement — has Main Camera, Directional Light, GameManager, Canvas (Screen Space Overlay, UI layer, CanvasScaler 1920x1080), EventSystem, MenuManager.
- Build settings has 6 scenes: StartMenu(0), SampleScene(1), Demo(2), CompetitionScene(3), TrainingScene(4), CareerScene(5).
Code Architecture
- MenuManager.cs has AutoWireReferences() and CreateMissingPanels() that auto-creates panels and buttons at runtime if not wired in editor. So the MainMenu scene just needs basic structure.
- CourseRunner.cs needs [SerializeField] references to: HandlerController, DogAgentController, AgilityScoringService, and CourseDefinition. It finds obstacles via FindObjectsOfType<ObstacleBase>().
- HandlerController.cs requires CharacterController component, uses PlayerInput from Input System.
- DogAgentController.cs requires NavMeshAgent and Animator components, needs BreedData, CommandBuffer, and handler transform references.
- ObstacleBase.cs is abstract with entry/commit/exit point transforms, contact zones, and trigger zone patterns.
- ConcreteObstacles.cs has 15+ obstacle types (BarJump, Tunnel, WeavePoles, AFrame, etc.) all extending ObstacleBase.
- Dog prefabs exist from Red_Deer asset pack (17+ breeds: Boxer, Beagle, Border Collie, Pitbull, Husky, Shiba Inu, etc.)
- UI prefabs exist in Assets/Prefabs/UI/: CourseEntryPrefab, DogEntryPrefab, HandlerEntryPrefab
Accomplished
1. ✅ MCP Connection Established — After 3 restart cycles, successfully connected to Unity MCP, set active instance to Agility Dogs@74b781753c4a9be8
2. ✅ Project Assessment — Cataloged all scenes (7+ game scenes + asset demo scenes), scripts (~100+ .cs files), prefabs (dog breeds, UI entries), and build settings
3. ✅ Clean MainMenu Scene Created — Assets/Scenes/MainMenu.unity with: Main Camera + AudioListener, Directional Light, GameManager, Canvas (Screen Space Overlay, 1920x1080 reference, UI layer), EventSystem + InputSystemUIInputModule, MenuManager component
4. ✅ Editor Utility Script — Created Assets/Scripts/Editor/SceneCleaner.cs with [MenuItem("Tools/Cleanup Duplicate Panels")] for cleaning the old StartMenu scene (couldn't execute it via MCP though)
5. 🔄 Gameplay Scene (SampleScene) — Assessment complete, NOT YET BUILT. Needs: ground plane, NavMesh, handler (capsule + CharacterController + PlayerInput), dog (from Red_Deer prefabs + NavMeshAgent + Animator), obstacle prefabs (primitives with ObstacleBase components), CourseRunner wiring, AgilityScoringService, GameHUD canvas, camera system
Relevant files / directories
Scenes
- Assets/Scenes/MainMenu.unity — New clean scene (just created, saved)
- Assets/Scenes/StartMenu.unity — Old messy scene with 29 duplicate panels (build index 0, needs cleanup or replacement)
- Assets/Scenes/SampleScene.unity — Main gameplay scene (build index 1, minimal content)
- Assets/Scenes/CompetitionScene.unity, TrainingScene.unity, CareerScene.unity — Other game scenes
Key Scripts (gameplay scene needs these wired)
- Assets/Scripts/Gameplay/CourseRunner.cs — Orchestrates runs, needs handler/dog/scoring refs
- Assets/Scripts/Gameplay/Handler/HandlerController.cs — Player handler, needs CharacterController + PlayerInput
- Assets/Scripts/Gameplay/Dog/DogAgentController.cs — Dog AI, needs NavMeshAgent + Animator + breed data
- Assets/Scripts/Gameplay/Obstacles/ObstacleBase.cs — Abstract base for all obstacles
- Assets/Scripts/Gameplay/Obstacles/ConcreteObstacles.cs — 15+ obstacle implementations
- Assets/Scripts/Gameplay/Scoring/AgilityScoringService.cs — Fault/timer tracking
- Assets/Scripts/Gameplay/Commands/CommandBuffer.cs — Command queue for dog
- Assets/Scripts/Services/GameManager.cs — Singleton game state, scene loading, DontDestroyOnLoad
- Assets/Scripts/Services/GameModeManager.cs — Mode routing (QuickPlay, Training, Career)
- Assets/Scripts/UI/MenuManager.cs — Menu system with auto-wiring
- Assets/Scripts/UI/GameHUD.cs — In-game HUD
- Assets/Scripts/Core/AgilityEnums.cs — All enums (ObstacleType, DogState, GameMode, etc.)
- Assets/Scripts/Data/CourseDefinition.cs — Course data structure
- Assets/Scripts/Data/BreedData.cs — Dog breed data
- Assets/Scripts/Data/ObstacleData.cs — Obstacle configuration data
Editor Utilities
- Assets/Scripts/Editor/SceneCleaner.cs — Cleanup duplicate panels tool
- Assets/Scripts/Editor/SceneSetupUtility.cs — Scene setup utility (existing, not yet used)
Dog Prefabs (Red_Deer asset pack)
- Assets/Red_Deer/Dogs/*/Dog/Prefabs/ — Breed prefabs (Boxer, Beagle, Border_Collie, Corgi, Dalmatian, Doberman, FrenchBulldog, GoldenRetriever, Husky, JackRussellTerrier, Labrador, Pitbull, Pug, Rottweiler, Shepherd, ShibaInu, Spitz, ToyTerrier, BullTerrier)
UI Prefabs
- Assets/Prefabs/UI/CourseEntryPrefab.prefab
- Assets/Prefabs/UI/DogEntryPrefab.prefab
- Assets/Prefabs/UI/HandlerEntryPrefab.prefab
Config
- /Users/johnnyclem/.config/opencode/opencode.json — MCP connection config with API key header
Spec Documents
- RELEASE_CANDIDATE_BURNDOWN.md — Master checklist (all marked complete in code)
- CAMPAIGN_MODE.md, GAME_MODES.md — Mode specifications
- PRD_AUDIT_REPORT.md — Original requirements audit
Next Steps (Priority Order)
1. Build SampleScene gameplay hierarchy — Ground plane, NavMesh surface, Handler (capsule + CharacterController + PlayerInput), Dog (Red_Deer prefab + NavMeshAgent + DogAgentController), Camera follow system, CourseRunner + AgilityScoringService, obstacle prefabs (primitive shapes with ConcreteObstacles components + trigger zones), GameHUD Canvas
2. Clean up StartMenu scene — Either run the SceneCleaner menu item manually in Unity, or replace build index 0 with the new MainMenu.unity
3. Wire Build Settings — Ensure scene flow: MainMenu(0) → SampleScene(1) works
4. Fix compilation errors — Check console after scene setup
5. Test play mode — Verify core loop: menu → course → run → score → results
