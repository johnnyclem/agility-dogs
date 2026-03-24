# Agility Dogs - Build Guide

## What's Been Completed

### 1. Data Setup
- **BreedData Assets** - Updated all 5 breeds with Red_Deer prefab references:
  - `BorderCollie.asset` → `BorderCollie_anim_IP.prefab`
  - `Corgi.asset` → `Corgi_anim_IP.prefab`
  - `GoldenRetriever.asset` → `Retriever_anim_IP.prefab`
  - `JackRussellTerrier.asset` → `JRTerrier_anim_IP.prefab`
  - `ShibaInu.asset` → `Spitz_anim_IP.prefab` (ShibaInu uses Spitz model)

### 2. Core Scripts Enhanced
- **MenuManager.cs** - Auto-loads data from Resources if not assigned
- **GameModeManager.cs** - Auto-loads defaults, routes to correct scenes:
  - QuickPlay → `SampleScene`
  - Training → `TrainingScene`
  - Career → `CareerScene`

### 3. New Runtime Scripts Created
- **GameBootstrapper.cs** - Ensures required managers exist in any scene
- **CompetitionSceneConfigurator.cs** - Configures gameplay scenes with:
  - Handler spawning
  - Dog spawning (from selected breed)
  - Course setup
  - Camera setup
  - Reference wiring

### 4. Scenes Created
- `CompetitionScene.unity` (copy of SampleScene)
- `TrainingScene.unity` (copy of SampleScene)
- `CareerScene.unity` (copy of SampleScene)

### 5. EditorBuildSettings Updated
All scenes now registered:
- StartMenu
- SampleScene
- Demo
- CompetitionScene
- TrainingScene
- CareerScene

---

## Remaining Setup Steps (Requires Unity Editor)

### 1. StartMenu.unity Wiring
The MenuManager needs UI references assigned:

```
MenuManager Component:
├── mainMenuPanel: [Assign MainMenu Panel GameObject]
├── modeSelectPanel: [Assign ModeSelect Panel GameObject] 
├── quickPlayPanel: [Assign QuickPlay Panel GameObject]
├── trainingPanel: [Assign Training Panel GameObject]
├── teamSelectPanel: [Assign TeamSelect Panel GameObject]
├── settingsPanel: [Assign Settings Panel GameObject]
├── resultsPanel: [Assign Results Panel GameObject]
├── pausePanel: [Assign Pause Panel GameObject]
│
├── [Main Menu Buttons]
├── quickPlayButton: [Assign Button]
├── trainingButton: [Assign Button]
├── careerButton: [Assign Button]
├── settingsButton: [Assign Button]
├── quitButton: [Assign Button]
│
├── [Mode Select Buttons]
├── startQuickPlayButton: [Assign Button]
├── startTrainingButton: [Assign Button]
│
├── [Team Select]
├── handlerListContainer: [Assign Container Transform]
├── dogListContainer: [Assign Container Transform]
├── handlerEntryPrefab: [Assign Handler Entry Prefab]
├── dogEntryPrefab: [Assign Dog Entry Prefab]
│
└── [Data References]
    ├── availableHandlers: [Assign HandlerData assets]
    └── availableDogs: [Assign BreedData assets]
```

### 2. Create UI Prefabs

**HandlerEntryPrefab** - Should contain:
- Portrait Image
- Name Text (TMP)
- Selection highlight

**DogEntryPrefab** - Should contain:
- Portrait Image  
- Name Text (TMP)
- Breed text (TMP)
- Stats display

### 3. Gameplay Scenes Setup

For **CompetitionScene**, **TrainingScene**, **CareerScene**:

1. Add `CompetitionSceneConfigurator` component to a GameObject

2. Assign references:
```
CompetitionSceneConfigurator:
├── handlerSpawnPoint: [Create empty at start position]
├── dogSpawnPoint: [Create empty at dog start position]
├── handlerPrefab: [Create or assign handler prefab]
├── dogPrefabs: [Assign Red_Deer dog prefabs]
├── competitionCourse: [Assign CourseDefinition]
└── hudPrefab: [Create HUD prefab if needed]
```

### 4. Handler Prefab Requirements

The handler prefab needs:
- `HandlerController` component
- Rigidbody or CharacterController
- Input references
- Visual representation (can be simple capsule)

### 5. Dog Setup

The dog prefabs (Red_Deer) need:
- `DogAgentController` component
- BreedData reference
- Animation setup

### 6. Obstacle Course Creation

**For Competition/Championship Mode:**
Create obstacle course with these obstacle types:
- Bar Jumps (5-6)
- Tunnel (1-2)
- Weave Poles (1 set of 6-12)
- A-Frame (1)
- Dog Walk (1)
- Pause Table (1)

**For Training Mode:**
Practice course with fewer obstacles focused on specific skills.

---

## Dog Breeds Available

| Breed | Prefab | Speed | Agility | Focus | Best For |
|-------|--------|-------|---------|-------|----------|
| Border Collie | BorderCollie_anim_IP | 9 | High | 85% | Advanced |
| Jack Russell Terrier | JRTerrier_anim_IP | 8 | High | 90% | Weave Poles |
| Corgi | Corgi_anim_IP | 6.5 | High | 80% | Contacts |
| Golden Retriever | Retriever_anim_IP | 7 | Medium | 70% | Beginners |
| Shiba Inu | Spitz_anim_IP | 7.5 | Medium | 65% | Experienced |

---

## Game Flow

### Main Menu Flow
```
StartMenu.unity
    │
    ├── [Quick Play] → SampleScene → Competition
    │
    ├── [Training] → TrainingScene → Practice
    │
    ├── [Career] → CareerScene → Breeding/Training/Shows
    │
    └── [Settings] → Settings Panel
```

### Career Mode Flow
```
CareerScene (Breeding Phase)
    └── Select puppy traits/breed
        ↓
    Training Phase
    └── Practice courses, skill building
        ↓
    Local Shows
    └── Entry-level competitions
        ↓
    Regional Shows
        ↓
    National Shows
        ↓
    Westminster Agility Kings (Final)
```

---

## Testing Checklist

1. [ ] StartMenu loads without errors
2. [ ] Main menu buttons are visible and clickable
3. [ ] Quick Play starts competition scene
4. [ ] Training mode shows training scene
5. [ ] Career mode shows career scene
6. [ ] Dog selection shows available breeds
7. [ ] Handler spawns at correct position
8. [ ] Dog spawns at correct position
9. [ ] Camera follows handler
10. [ ] Handler movement works (WASD/arrows)
11. [ ] Dog follows handler commands
12. [ ] Obstacles can be navigated
13. [ ] Scoring tracks faults
14. [ ] Timer functions correctly
15. [ ] Results screen shows after run
16. [ ] Return to menu works

---

## Command Keys (When Implemented)

| Key | Command | Description |
|-----|---------|-------------|
| W/↑ | Move Forward | Handler moves forward |
| S/↓ | Move Back | Handler moves backward |
| A/← | Move Left | Handler moves left |
| D/→ | Move Right | Handler moves right |
| Space | Sprint | Handler runs |
| 1 | Come Bye | Dog turns right |
| 2 | Away | Dog turns left |
| 3 | Jump | Dog jumps next obstacle |
| 4 | Tunnel | Dog enters tunnel |
| 5 | Weave | Dog does weave poles |
| 6 | Table | Dog goes to pause table |
| ESC | Pause | Opens pause menu |

---

## Next Steps for Full Implementation

1. **Obstacle Course Layout** - Design actual course geometry using primitives or imported assets
2. **Handler Controller** - Complete the handler movement system with input
3. **Dog AI** - Complete the command interpretation system
4. **Stadium Environment** - Create or import stadium/arena assets
5. **HUD Design** - Create the in-game UI for timer, faults, etc.
6. **Commentary System** - Wire up the CommentaryManager
7. **Crowd System** - Add CrowdManager to gameplay scenes
8. **Sound Design** - Add audio for footsteps, bar knocks, whistle, etc.
