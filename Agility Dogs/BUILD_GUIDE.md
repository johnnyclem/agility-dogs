# Agility Dogs - Build Guide

## Quick Start - Unity Editor Setup

### Step 1: Generate All Data Assets
1. Open Unity Editor
2. Go to **Window → General → AssetDatabase** (refresh if needed)
3. Go to **Agility Dogs → Generate All Data**
4. This will create:
   - 19 BreedData assets in `Assets/Data/Breeds/`
   - 8 CourseDefinition assets in `Assets/Data/Courses/`
   - 5 HandlerData assets in `Assets/Data/Handlers/`

### Step 2: Setup StartMenu Scene
1. Open `Assets/Scenes/StartMenu.unity`
2. Go to **Agility Dogs → Setup → Complete Scene Setup**
3. This will:
   - Create all menu panels (MainMenu, QuickPlay, Training, TeamSelect, Settings, Results, ModeSelect, Pause)
   - Wire up MenuManager with all button and panel references
   - Auto-populate available data references

### Step 3: Build & Run
1. Open `Assets/Scenes/SampleScene.unity`
2. Go to **Agility Dogs → Setup → Setup SampleScene**
3. Build and run

---

## Architecture Overview

### Game Flow
```
StartMenu
    ├── Quick Play → SampleScene → Results → Replay/Menu
    ├── Training → TrainingScene → Results → Menu
    └── Career → CareerScene → Breeding/Training/Shows → Westminster
```

### Core Managers
- **GameModeManager** - Routes between Quick Play, Training, Career modes
- **MenuManager** - Handles all menu interactions and transitions
- **GameManager** - Core gameplay state management
- **CareerProgressionService** - Career mode persistence and progression

### Data Assets
- **BreedData** - Dog breed configuration (speed, acceleration, handling tolerance, prefab)
- **HandlerData** - Handler stats and unlock status
- **CourseDefinition** - Course layout, timing, difficulty

---

## Scene Setup Details

### StartMenu Scene
The MenuManager requires these UI elements:

| Panel | Purpose |
|-------|---------|
| MainMenuPanel | Primary menu with Quick Play, Training, Career, Settings, Quit |
| ModeSelectPanel | Legacy mode selection (optional) |
| QuickPlayPanel | Quick play description and start button |
| TrainingPanel | Course list and training start |
| TeamSelectPanel | Handler and dog selection |
| SettingsPanel | Volume, graphics, accessibility settings |
| ResultsPanel | Post-run results display |
| PausePanel | In-game pause menu |

### Button References Required
```
MainMenuPanel/
├── ButtonsContainer/
│   ├── QuickPlayButton
│   ├── TrainingButton  
│   ├── CareerButton
│   ├── SettingsButton
│   └── QuitButton
└── VersionText

QuickPlayPanel/
├── Description
└── Buttons/
    ├── StartButton
    └── BackButton

TrainingPanel/
├── Description
├── CourseListContainer
└── Buttons/
    ├── StartButton
    └── BackButton

TeamSelectPanel/
├── Title
├── HandlerSection/
│   └── HandlerListContainer
├── DogSection/
│   └── DogListContainer
└── Buttons/
    ├── StartRunButton
    └── BackButton

SettingsPanel/
├── Title
├── SettingsContainer/
│   ├── MusicVolume (Slider)
│   ├── SFXVolume (Slider)
│   ├── VoiceVolume (Slider)
│   └── CrowdVolume (Slider)
└── Buttons/
    ├── ApplyButton
    └── CloseButton

ResultsPanel/
├── ResultTitle
├── TimeText
├── FaultsText
├── ScoreText
├── PositionText
├── PersonalBest
└── Buttons/
    ├── ReplayButton
    ├── RetryButton
    ├── NextButton
    └── MenuButton

PausePanel/
├── Title
└── Buttons/
    ├── ResumeButton
    ├── RestartButton
    ├── SettingsButton
    └── QuitButton
```

---

## Available Dog Breeds

| Breed | Speed | Acceleration | Responsiveness | Jump Power |
|-------|-------|--------------|----------------|------------|
| Border Collie | 9.0 | 8.0 | 0.85 | 1.1 |
| Jack Russell Terrier | 8.0 | 8.5 | 0.90 | 1.0 |
| Shiba Inu | 7.5 | 7.0 | 0.65 | 0.95 |
| Golden Retriever | 7.0 | 6.5 | 0.70 | 1.0 |
| Corgi | 6.5 | 7.0 | 0.80 | 0.9 |
| Beagle | 6.0 | 6.0 | 0.70 | 0.85 |
| Boxer | 7.0 | 6.0 | 0.75 | 1.05 |
| Husky | 7.5 | 7.0 | 0.70 | 1.0 |
| Labrador | 6.5 | 6.0 | 0.70 | 1.0 |
| Rottweiler | 6.0 | 5.5 | 0.65 | 1.1 |
| Pitbull | 7.0 | 6.5 | 0.70 | 1.05 |
| Dalmatian | 7.5 | 7.0 | 0.75 | 1.0 |
| Doberman | 8.0 | 7.5 | 0.75 | 1.05 |
| French Bulldog | 5.5 | 5.0 | 0.70 | 0.8 |
| Pug | 5.0 | 4.5 | 0.65 | 0.75 |
| Shepherd | 7.5 | 7.0 | 0.80 | 1.0 |
| Bull Terrier | 6.5 | 6.0 | 0.70 | 1.05 |
| Toy Terrier | 6.0 | 6.5 | 0.85 | 0.8 |
| Spitz | 7.0 | 6.5 | 0.70 | 0.9 |

---

## Career Mode Flow

```
[Breeding Phase]
    └── Select puppy traits/breed from available dogs
        ↓
[Training Phase]
    └── Practice courses, build skills via SkillTree
        ↓
[Local Shows]
    └── Compete at Local Park venue (easy courses)
        ↓ (advance with 1st place)
[Regional Shows]
    └── County Fair & Regional Championship venues
        ↓ (advance with 1st place)
[National Shows]
    └── State & National Championship venues
        ↓ (advance with 1st place)
[Westminster]
    └── Final: Westminster Agility Kings championship
```

---

## Editor Utilities

### Available via Window Menu

1. **Agility Dogs/Generate All Data**
   - Creates all BreedData, CourseDefinition, HandlerData assets

2. **Agility Dogs/Setup/Complete Scene Setup**
   - Sets up StartMenu scene with all UI panels
   - Wires MenuManager references
   - Creates UI prefabs

3. **Agility Dogs/Setup/Setup SampleScene**
   - Adds GameManager and CourseRunner to SampleScene

### Editor Windows

1. **SceneSetupUtility** (`Assets/Scripts/Editor/SceneSetupUtility.cs`)
   - Full scene setup for StartMenu and gameplay scenes

2. **DataGenerator** (`Assets/Scripts/Editor/DataGenerator.cs`)
   - Generates breed, course, and handler data assets

---

## Common Issues

### Missing References (999+ errors)
**Cause:** Scene references not wired in Unity Editor

**Solution:**
1. Run **Agility Dogs → Setup → Complete Scene Setup** in StartMenu scene
2. If errors persist, manually assign references in Inspector

### BreedData Assets Not Found
**Cause:** BreedData assets don't exist or prefab references are null

**Solution:**
1. Run **Agility Dogs → Generate All Data**
2. Verify `Assets/Data/Breeds/` contains .asset files

### Scene Won't Load
**Cause:** GameModeManager references missing scene names

**Solution:**
1. Select GameModeManager in scene
2. Verify scene names: `StartMenu`, `SampleScene`, `TrainingScene`, `CareerScene`
3. Ensure scenes are added to Build Settings (File → Build Settings)

---

## Testing Checklist

After setup, verify:

- [ ] StartMenu loads without console errors
- [ ] Opening sequence plays (or skip works)
- [ ] Main menu buttons are visible and clickable
- [ ] Quick Play starts SampleScene
- [ ] Training mode shows TrainingScene  
- [ ] Career mode shows CareerScene
- [ ] Settings panel opens and sliders work
- [ ] Results screen displays after run completion
- [ ] Pause menu accessible during gameplay
- [ ] Return to menu works from all states

---

## Command Keys

| Key | Action |
|-----|--------|
| W/↑ | Move forward |
| S/↓ | Move backward |
| A/← | Move left |
| D/→ | Move right |
| Space | Sprint |
| 1 | Come Bye (turn right) |
| 2 | Away (turn left) |
| 3 | Jump |
| 4 | Tunnel |
| 5 | Weave poles |
| 6 | Pause table |
| ESC | Pause game |

---

## Next Implementation Steps

1. **Obstacle Course Layout** - Create actual course geometry in scenes
2. **Handler Controller** - Complete handler movement with input system
3. **Dog AI** - Finish command interpretation and pathfinding
4. **Stadium Environment** - Add arena/crowd visuals
5. **HUD** - Timer, faults, score display during gameplay
6. **Commentary** - Wire CommentaryManager for play-by-play
7. **Crowd** - Add CrowdManager with reactions
8. **Sound** - Footsteps, bar knocks, whistle, ambient audio
