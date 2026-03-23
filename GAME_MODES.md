# Game Modes - Westminster Agility Masters: Handler's Edge

## Overview

The game features three distinct game modes, each offering a different experience:

1. **Quick Play** - Jump straight into competition action
2. **Training** - Practice courses at your own pace
3. **Career** - Full career progression from puppy breeding to Westminster

---

## Quick Play Mode

### Description
Jump straight into the action with default settings. Perfect for players who want to practice competition runs without the full career progression.

### Flow
```
Main Menu → Quick Play → Gameplay → Results → Retry/Menu
```

### Features
- Random course selection
- Default handler and dog
- Full competition rules
- Quick restart option
- No progression tracking

### Implementation
- `GameModeManager.StartQuickPlay()`
- Uses default course/handler/dog or last used configuration
- Skips team selection for faster gameplay

---

## Training Mode

### Description
Practice courses at your own pace. No pressure, no competition. Perfect for learning the game mechanics and mastering your handling skills.

### Flow
```
Main Menu → Training → Select Course → Practice Run → Results → Retry/Menu
```

### Features
- Select any unlocked course
- Choose your handler and dog
- Practice without timer pressure (optional)
- No fault penalties (optional)
- Training aids visible
- Unlimited retries

### Implementation
- `GameModeManager.StartTraining(handler, dog, course)`
- `isTrainingMode = true` flag for mode-specific settings
- Optional: Training drill system for specific skills

---

## Career Mode

### Description
The full Westminster Agility Kings experience! Breed your puppy, train it, compete in local shows, and work your way up to the Agility Kings round of the Westminster Dog Show.

### Flow
```
Main Menu → Career
    ↓
┌─────────────────────────────────────────────────────────────┐
│  BREEDING PHASE                                             │
│  1. Select breed (unlocked breeds based on level)           │
│  2. Generate litter of 3 puppies                            │
│  3. Review puppy traits and base stats                      │
│  4. Select puppy (optional: rename)                         │
│  5. Advance to Training                                     │
└─────────────────────────────────────────────────────────────┘
    ↓
┌─────────────────────────────────────────────────────────────┐
│  TRAINING PHASE                                             │
│  1. View puppy stats and training progress                  │
│  2. Complete training runs                                  │
│  3. Earn XP and improve puppy skills                        │
│  4. Advance to Local Shows (after training goals met)       │
└─────────────────────────────────────────────────────────────┘
    ↓
┌─────────────────────────────────────────────────────────────┐
│  COMPETITION PHASE (Progressive Tiers)                      │
│                                                             │
│  LOCAL SHOWS → COUNTY FAIR → REGIONAL CHAMPIONSHIPS         │
│        ↓              ↓                ↓                    │
│  STATE CHAMPIONSHIPS → NATIONAL CHAMPIONSHIPS               │
│                ↓                                            │
│  ┌─────────────────────────────────────────────────────┐   │
│  │          WESTMINSTER AGILITY KINGS                  │   │
│  │          (Final Championship)                       │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

### Show Tiers

| Tier | Level Required | Wins Required | Description |
|------|----------------|---------------|-------------|
| Local | 1+ | 0 | Local park competitions |
| County | 5+ | 2 | County fair shows |
| Regional | 10+ | 4 | Regional championships |
| State | 15+ | 6 | State-level competition |
| National | 20+ | 8 | National championships |
| Westminster | 25+ | 12 | Agility Kings at Westminster! |

### Career Services

#### GameModeManager
- Central routing for all game modes
- Mode-specific configuration
- Scene loading and transitions
- Career phase management

#### DogBreedingService
- Puppy generation with random traits
- Breeding two puppies to create offspring
- Trait inheritance and mutations
- Base stat generation based on breed and traits

**Puppy Traits:**
- Energetic: +Speed, +Stamina, -Focus
- Calm: +Stamina, +Focus, -Speed
- Intelligent: +Intelligence
- Stubborn: -Intelligence, +Confidence
- Agile: +Agility
- Strong: +Jump Power, +Speed
- Sensitive: +Intelligence, +Focus, -Confidence
- Distracted: -Focus, +Speed
- Confident: +Confidence
- Nervous: -Confidence, -Focus

#### ShowManager
- Generate AI competitors
- Calculate show placements
- Track tier progression
- Westminster qualification checks

#### CareerProgressionService (Existing)
- XP system with leveling
- Currency (Wings) system
- Achievement tracking
- Handler/dog/venue unlocks

#### CareerUIManager
- Career Hub UI
- Breeding screen
- Training camp screen
- Show selection screen
- Results display
- Westminster qualification UI

### Career Phase Progression

#### 1. Breeding Phase
```
Select Breed → Generate Litter → Review Traits → Select Puppy → Rename (optional)
```

#### 2. Training Phase
```
View Puppy Stats → Select Training Drill → Practice Run → Earn XP → Level Up Skills
```

#### 3. Competition Phases (Repeated for each tier)
```
Select Show Tier → Enter Competition → Run Course → Get Results
    ↓                                    ↓
  Win = Advance                    Loss = Retry
    ↓
  Next Tier
```

#### 4. Westminster Phase
```
Qualify → Enter Championship → Compete → Crown Agility Kings!
```

### Westminster Qualification Requirements

To enter the Westminster Agility Kings:
- **Total Wins:** 12+
- **Career Level:** 25+
- **Dog Skill Rating:** 0.8+ (trained puppy)
- **Total Competitions:** 20+

### XP Rewards by Show Result

| Result | XP Awarded |
|--------|------------|
| Best in Show | 500 |
| First Place | 300 |
| Second Place | 200 |
| Third Place | 150 |
| Honorable Mention | 100 |
| Did Not Place | 50 |

---

## File Structure

### New Service Files
```
Assets/Scripts/Services/
├── GameModeManager.cs          # Central mode routing
├── DogBreedingService.cs       # Puppy creation & traits
├── ShowManager.cs              # Competition management
└── GameManager.cs              # Updated with mode integration
```

### New UI Files
```
Assets/Scripts/UI/
└── CareerUIManager.cs          # Career mode UI screens
```

### Updated Files
```
Assets/Scripts/
├── Core/AgilityEnums.cs        # Added GameMode, CareerPhase, ShowTier, etc.
├── UI/MenuManager.cs           # Updated for three-mode system
└── Services/GameManager.cs     # Integrated with GameModeManager
```

---

## UI Setup Required

### Main Menu (StartMenu Scene)

Add three main buttons:
1. **Quick Play Button** - Start quick competition
2. **Training Button** - Go to training mode
3. **Career Button** - Start career mode

### Career Hub Panels

Create UI panels for:
1. **CareerHubPanel** - Main career hub with stats and puppy info
2. **BreedingPanel** - Puppy generation and selection
3. **TrainingCampPanel** - Training drills and skill progress
4. **ShowSelectionPanel** - Tier selection and competition entry
5. **ShowResultsPanel** - Results display with progression
6. **WestminsterPanel** - Final championship qualification

### Prefabs Needed

1. **PuppyCardPrefab** - Card for puppy selection in breeding
2. **ShowTierCardPrefab** - Card for show tier selection
3. **CourseEntryPrefab** - Course list item for training

---

## Next Steps

1. **Scene Setup:**
   - Add CareerHub panel to StartMenu scene or create new CareerHub scene
   - Configure UI references in CareerUIManager

2. **Data Setup:**
   - Ensure BreedData assets exist for all dog breeds
   - Create CourseDefinition assets for each show tier
   - Configure HandlerData for player selection

3. **Testing:**
   - Test Quick Play flow (fastest to implement)
   - Test Training mode with course selection
   - Test Career mode breeding → training → shows progression
   - Test Westminster qualification requirements

4. **Polish:**
   - Add animations/transitions between career phases
   - Add sound effects for career events
   - Add achievement notifications
   - Balance XP rewards and progression timing

---

## Example Career Session

1. **Start Career** - Player clicks Career button
2. **Breeding Screen** - Generate 3 puppies, pick one named "Dash"
3. **Training Camp** - Complete 5 training runs, improve Jump technique
4. **Local Shows** - Win 2 local shows, unlock County
5. **County Fair** - Win 2 county shows, unlock Regional
6. **Regional Championships** - Win 2 regional shows, unlock State
7. **State Championships** - Win 2 state shows, unlock National
8. **National Championships** - Win 2 national shows, unlock Westminster
9. **Westminster Agility Kings** - Compete for the ultimate title!

**Total estimated playtime for career:** 15-25 hours
