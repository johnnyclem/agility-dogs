# Campaign/Story Mode - Westminster Agility Masters

## Overview

The Campaign/Story Mode provides a narrative-driven experience that follows the player's journey from their first dog to becoming Westminster Agility Kings. Story beats unlock at key career milestones, introducing new challenges, mentors, and plot points.

## Story Structure

### Chapters
```
Chapter 1: "A New Beginning"
  - Starting career, first puppy selection
  - Mentor introduction: Coach Sarah
  - Tutorial: Learn the basics

Chapter 2: "Local Legends"
  - First local competition
  - Rival introduction: Marcus Chen
  - Learn handling techniques

Chapter 3: "Rising Stars"
  - County fair competition
  - Coach Sarah's backstory revealed
  - New rival: Emily Rodriguez

Chapter 4: "The Regionals"
  - Regional championship
  - Flashback: Original Westminster 2020
  - Facing the champion

Chapter 5: "State of Mind"
  - State championship
  - Personal challenge: balancing work/training
  - Bonding with dog

Chapter 6: "National Dreams"
  - National championship
  - Previous rival becomes ally
  - Coach's final lesson

Chapter 7: "Westminster Calling"
  - Invitation to Agility Kings
  - Travel and pre-show nerves
  - Support from all rivals

Chapter 8: "Agility Kings"
  - The final championship
  - Culmination of journey
  - Becoming champion
```

## Architecture

### CampaignService
- Manages story progression state
- Unlocks chapters based on career milestones
- Triggers story events at appropriate times

### Cutscene System
- Dialogue sequences with character portraits
- Camera animations
- Triggered by story progress

### Character System
- Named characters with portraits
- Relationship tracking
- Dialogue trees

## Integration Points

### Story Triggers
- `CareerPhase` changes
- Show tier completions
- Specific achievements
- Time-based (first play, return play)

### Story Events
- `OnChapterUnlocked`
- `OnStoryEventTriggered`
- `OnCutsceneStarted`
- `OnCutsceneEnded`

## Data Structures

### StoryChapter
- Chapter number
- Title and subtitle
- Unlock conditions
- Associated cutscenes
- Character appearances

### Cutscene
- Sequence of dialogue lines
- Background scene
- Camera instructions
- Duration

### DialogueLine
- Speaker name
- Portrait sprite
- Dialogue text
- Animation trigger

### Character
- Name
- Title/Role
- Portrait sprite
- Appears in chapters

## File Structure
```
Assets/Scripts/
├── Services/
│   └── CampaignService.cs      # Story progression management
├── Data/
│   ├── Story/
│   │   ├── StoryChapter.asset  # Chapter definitions
│   │   ├── CutsceneData.asset  # Cutscene definitions
│   │   └── CharacterData.asset # Character definitions
└── UI/
    └── CutsceneUI.cs           # Cutscene playback UI
```

## Implementation Priority

1. **CampaignService** - Core story state machine
2. **CutsceneData** - ScriptableObject for cutscene data
3. **CutsceneUI** - Basic cutscene playback
4. **Story integration** - Hook into career milestones
5. **Polish** - Camera work, animations, character portraits