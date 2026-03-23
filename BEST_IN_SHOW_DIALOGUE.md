# Best In Show Dialogue System

## Overview

A AAA-quality dialogue system for Arthur Pendelton (Play-by-Play) and Buck Hastings (Color Commentator) using the **"bag of clips"** anti-repetition pattern.

## Architecture

### Core Pattern: DialoguePool (Bag of Clips)

```csharp
// AAA anti-repetition pattern
public class DialoguePool
{
    private List<DialogueLineEntry> availableLines;  // Current bag
    private List<DialogueLineEntry> sourceLines;     // Master list
    private DialogueLineEntry lastPlayedLine;
    
    public DialogueLineEntry GetNextLine()
    {
        // Refill bag if empty
        if (availableLines.Count == 0) RefillPool();
        
        // Weighted random selection
        int index = GetWeightedRandomIndex();
        var line = availableLines[index];
        
        // Avoid immediate repeats
        if (line == lastPlayedLine && sourceLines.Count > 1)
            line = availableLines[(index + 1) % availableLines.Count];
        
        // Remove from bag
        availableLines.RemoveAt(index);
        lastPlayedLine = line;
        
        return line;
    }
}
```

### State-Based Organization

| State | Arthur (10 lines) | Buck (5-7 lines) |
|-------|-------------------|------------------|
| Match Intro | Welcome, SCT, pedigree | Taxes, calves, beef jerky |
| Weave Poles | Entry, cadence, popped out | Pub weaving, broom tail |
| Contact Obstacles | A-frame, yellow zone, fly-off | Fire department, nap, firework |
| Tunnel | Blind entry, wrong tunnel | Sleeping bag, toothpaste |
| Teeter-Totter | Drop, fly-off, refusal | Seesaw trauma, pirate hat |
| Jumps | Tire, dropped bar, oxer | Tire eating, Mets shortstop |
| Mistakes | Off course, sniffing, elimination | Oil discovery, Spanish dog |
| Finish Line | Clean run, masterclass | Giant check, spay/neuter |

### Total: 80 Dialogue Lines (45 Buck, 35 Arthur)

## Files Created

### Data Layer
- **`DialoguePool.cs`** - Generic pool with anti-repetition
- **`CommentaryStatePool.cs`** - State-specific pools for both announcers
- **`CommentaryDialogueData.cs`** - Legacy data structure (kept for compatibility)
- **`BestInShowDialogue.cs`** - ScriptableObject containing all dialogue

### Service Layer
- **`BestInShowDialogueManager.cs`** - Main manager with queue processing
- **`CommentaryDialogueManager.cs`** - Alternative implementation (legacy)

### Editor
- **`BestInShowDialoguePopulator.cs`** - Auto-populates ScriptableObject

## Usage

### 1. Create the Dialogue Asset

In Unity Editor:
1. Go to **Agility Dogs > Populate Best In Show Dialogue**
2. This creates `Assets/Data/BestInShowDialogue.asset` with all 80 lines

### 2. Setup the Manager

1. Add `BestInShowDialogueManager` to a GameObject
2. Assign the `BestInShowDialogue` asset
3. Assign audio sources for Arthur and Buck
4. Assign `ElevenLabsService` reference

### 3. Automatic Triggers

The system automatically triggers dialogue based on game events:

| Event | Triggered State |
|-------|-----------------|
| Run Started | MatchIntro |
| Obstacle: WeavePoles | WeavePoles |
| Obstacle: A-Frame/DogWalk | ContactObstacles |
| Obstacle: Tunnel | Tunnel |
| Obstacle: Teeter | TeeterTotter |
| Obstacle: Any Jump | Jumps |
| Fault Committed | Mistakes |
| Run Completed | FinishLine |

### 4. Manual Triggering

```csharp
// Force specific state
dialogueManager.TriggerState(CommentaryState.WeavePoles);

// Force specific line
dialogueManager.ForcePlayLine(AnnouncerType.Main, "Custom line here");

// Reset all pools (refill bags)
dialogueManager.ResetAllPools();
```

## Anti-Repetition Features

1. **Bag Depletion** - Lines are removed from pool when played
2. **Pool Refill** - Bag refills when empty (all lines heard)
3. **Consecutive Prevention** - Same line never plays twice in a row
4. **Weighted Selection** - Some lines more/less likely
5. **State Isolation** - Each state has its own pool

## Integration Points

### With Existing Systems

```csharp
// GameEvents automatically triggers dialogue
GameEvents.OnObstacleCompleted += (type, clean) => {
    // Maps to commentary state
};

// CommentaryManager can trigger dialogue
CommentaryManager.OnCommentaryTrigger += (category, text) => {
    // Handle custom triggers
};
```

### With AudioDuckingService

The dialogue system works with the existing `AudioDuckingService`:
- Commentary plays → Other audio ducks (-4dB)
- Commentary ends → Audio returns to normal

## Configuration

### Timing Settings

| Setting | Default | Description |
|---------|---------|-------------|
| `minDialogueInterval` | 3s | Minimum between lines |
| `maxDialogueInterval` | 12s | Maximum wait time |
| `arthurDelay` | 0s | Delay before Arthur speaks |
| `buckDelay` | 0.5s | Delay after Arthur (if Buck speaks) |
| `buckChance` | 0.4 | Chance Buck comments |

### Voice Settings

```csharp
[SerializeField] private string arthurVoiceId = "ErXwobaYiN019PkySvjV";
[SerializeField] private string buckVoiceId = "VR6AewLTigWG4xSOukaG";
```

## Character Profiles

### Arthur Pendelton (Play-by-Play)
- **Personality**: Deadpan, knowledgeable, professional
- **Terms Used**: Front crosses, contact zones, refusals, oxer, stanchion
- **Role**: Technical analysis, play-by-play narration

### Buck Hastings (Color Commentary)
- **Personality**: Ignorant, confident, off-topic, hilarious
- **Topics**: Taxes, hot dogs, margaritas, ex-wives, pirates
- **Role**: Comic relief, audience surrogate

## Testing

### In Editor

Right-click `BestInShowDialogueManager` component:
- Test Arthur Line
- Test Buck Line
- Test Match Intro
- Test Weave Poles
- Test Mistakes

### Console Commands

```csharp
// Force Arthur line
dialogueManager.ForcePlayLine(AnnouncerType.Main, "Test line");

// Force Buck line  
dialogueManager.ForcePlayLine(AnnouncerType.Color, "Test line");

// Trigger banter
dialogueManager.TriggerState(CommentaryState.Mistakes);
```

## Performance Considerations

1. **Audio Generation** - ElevenLabs TTS takes ~1-2s per line
2. **Queue Processing** - Lines play sequentially, not in parallel
3. **Memory** - Dialogue data is lightweight (text only)
4. **Pooling** - Audio clips managed by ElevenLabsService

## Future Enhancements

- [ ] Add more states (championship pressure, tie-breaker)
- [ ] Dynamic weight adjustment based on run quality
- [ ] User-configurable buckChance
- [ ] Localization support
- [ ] Skip dialogue option in settings

---

*"Arthur, tell the truth: Are any of these dogs wearing a wire? It feels rigged."*
— Buck Hastings, Color Commentator