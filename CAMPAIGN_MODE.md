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

## Characters

### Main Characters

| Character ID | Name | Role | Description |
|--------------|------|------|-------------|
| `coach_sarah` | Coach Sarah Chen | Mentor | Former champion, retired due to injury, guides the player |
| `marcus` | Marcus Chen | Friendly Rival | Enthusiastic, competitive, grows throughout journey |
| `emily` | Emily Rodriguez | Elite Handler | Skilled, respectful, represents top-tier competition |
| `victoria` | Victoria Price | Former Champion | Authoritative, wise, won Westminster 15 years ago |
| `narrator` | Narrator | Storyteller | Sets scenes, builds atmosphere |
| `announcer` | Announcer | Voice of Competition | Introduces competitors at major events |

## Character Portrait System

### CharacterPortraitData ScriptableObject

Located: `Assets/Data/Characters/Portraits/`

Each character can have a `CharacterPortraitData` ScriptableObject with:
- Emotion-specific portraits (happy, sad, excited, competitive, etc.)
- Default portrait fallback
- Emotion mapping for dynamic portrait selection

### Emotion Types

| Emotion | Usage |
|---------|-------|
| `happy` | Proud, gracious, approving moments |
| `sad` | Melancholy, nostalgic moments |
| `excited` | Ecstatic, thrilling moments |
| `competitive` | Fierce, determined moments |
| `thoughtful` | Reflective, wise moments |
| `emotional` | Tears, overwhelmed moments |
| `authoritative` | Stern, serious moments |
| `friendly` | Warm, amused, brotherly moments |
| `triumphant` | Victory, awe moments |

### Portrait Generation Tool

Editor utility: `Agility Dogs/Generate Character Portraits`

This creates placeholder circular portraits with character-specific colors. To use:
1. Open Unity Editor
2. Go to `Agility Dogs/Generate Character Portraits`
3. Click "Generate All Placeholder Portraits"
4. Replace placeholder PNGs with actual character art

## Story Events

### Automatic Story Events

These trigger automatically during gameplay:

| Event ID | Trigger Condition | Cutscene |
|----------|-------------------|----------|
| `first_victory` | Win first show | Celebration with Coach Sarah and Marcus |
| `first_defeat` | DNF after 1+ shows | Encouragement from Coach Sarah and Emily |
| `tier_complete` | Win at tier thresholds | Coach Sarah emotional, Victoria warns |
| `westminster_qualify` | Qualify for finals | Official announcement, celebration |

### Manual Story Events

These can be triggered programmatically:

| Event ID | Trigger | Cutscene |
|----------|---------|----------|
| `first_jump` | Complete first jump obstacle | - |
| `first_tunnel` | Complete first tunnel | - |
| `first_weave` | Complete first weave poles | - |
| `marcus_rivalry` | Call PlayMarcusRivalry() | Marcus admits defeat, friendship |
| `emily_rivalry` | Call PlayEmilyRivalry() | Emily expresses respect |

## Cutscene System

### Cutscene Playback

Cutscenes display dialogue with:
- Typewriter text effect (30 chars/sec)
- Character portrait (emotion-aware)
- Speaker name display
- Auto-advance (skippable)
- Input: click/space/enter

### Default Cutscenes

| Cutscene ID | Description |
|-------------|-------------|
| `intro` | Campaign intro, meet Coach Sarah |
| `chapter2_intro` | Marcus rivalry begins |
| `chapter3_intro` | Emily rivalry begins |
| `chapter4_intro` | Victoria appears |
| `chapter5_intro` | Elite competition |
| `chapter6_intro` | Coach Sarah's past revealed |
| `chapter7_intro` | Rivals become allies |
| `finale` | Westminster intro |
| `first_victory` | First win celebration |
| `first_defeat` | First loss encouragement |
| `marcus_rivalry` | Marcus friendship |
| `emily_rivalry` | Emily respect |
| `tier_complete` | Tier victory |
| `westminster_qualify` | Qualification moment |
| `agility_kings_victory` | Final victory celebration |

## Character Relationships

Relationships affect dialogue and unlock special interactions:

| Character | Starting Level | Max Level | How to Improve |
|-----------|--------------|-----------|----------------|
| Coach Sarah | 0 | 10 | Complete shows, win championships |
| Marcus Chen | 0 | 10 | Beat him in competitions |
| Emily Rodriguez | 0 | 10 | Place top 3 in high-tier shows |
| Victoria Price | 0 | 10 | Show discipline and respect |

## Campaign Service API

```csharp
// Starting
CampaignService.Instance.StartCampaign();
CampaignService.Instance.ResumeCampaign();

// Cutscenes
CampaignService.Instance.PlayCutscene("intro");
CampaignService.Instance.PlayMarcusRivalry();
CampaignService.Instance.PlayEmilyRivalry();

// Progress
CampaignService.Instance.CheckChapterUnlocks();
CampaignService.Instance.TriggerStoryEvent("event_id");

// Queries
CampaignService.Instance.GetCurrentChapter();
CampaignService.Instance.IsChapterUnlocked(3);
CampaignService.Instance.GetCharacter("coach_sarah");
CampaignService.Instance.GetRelationshipLevel("marcus");
CampaignService.Instance.GetUnlockedChapters();

// Events
OnChapterUnlocked += (chapter) => { };
OnStoryEventTriggered += (eventId) => { };
OnCutsceneStarted += (cutscene) => { };
OnCutsceneEnded += () => { };
```

## Integration with Career Mode

Campaign mode shares progression with Career mode:
- Same breeding, training, show progression
- Same career level, XP, achievements
- Campaign adds narrative layer on top

## File Structure

```
Assets/Scripts/
├── Services/
│   └── CampaignService.cs           # Story progression (1350+ lines)
├── Data/
│   └── CharacterPortraitData.cs      # Portrait ScriptableObject
├── UI/
│   └── CutsceneUI.cs                # Cutscene playback (375 lines)
└── Editor/
    └── CharacterPortraitGenerator.cs # Portrait placeholder generator

Assets/Data/Characters/Portraits/
├── coach_sarah_portrait.png
├── marcus_portrait.png
├── emily_portrait.png
├── victoria_portrait.png
├── narrator_portrait.png
└── announcer_portrait.png
```

## TODO

- [ ] Replace placeholder portraits with actual character art
- [ ] Add voice acting integration
- [ ] Create background images for each chapter
- [ ] Add camera animation presets for cutscenes
- [ ] Implement character-specific dialogue variations
- [ ] Add achievement for watching all cutscenes
