# Release Candidate Burndown List
## Agility Dogs: Westminster Agility Masters - Handler's Edge
### Evaluated: March 22, 2026

---

## Executive Summary

**Current State:** Proof-of-Concept (Phase A) - Core loop functional
**Target:** Release Candidate (Phase B production-quality build)

The project has a solid foundation with the core gameplay loop operational. However, reaching release candidate status requires completing multiple systems and adding significant polish.

---

## Implementation Status Overview

| System | Status | Completion |
|--------|--------|------------|
| Handler Control | Partial | 40% |
| Dog AI | Substantial | 65% |
| Obstacles | Partial | 60% |
| Scoring | Substantial | 75% |
| Camera | Partial | 50% |
| Commentary | Substantial | 70% |
| Commands | Partial | 45% |
| Replay | Substantial | 55% |
| UI/HUD | Missing | 0% |
| Crowd | Missing | 0% |
| Progression/Career | Missing | 0% |
| Online Features | Missing | 0% |
| Audio System | Missing | 0% |
| Accessibility | Missing | 0% |

---

## Priority 1: Critical Core Systems (Must Complete)

### 1.1 Handler Control Completion
- [ ] Implement gesture inputs system
- [ ] Add directional lean/body-language influence
- [ ] Implement contextual command inputs (based on facing/velocity)
- [ ] Add voice command input via microphone (optional)
- [ ] Implement handler position influence on dog's line

### 1.2 Dog AI Enhancement
- [ ] Implement momentum physics system
- [ ] Add advanced commitment logic with timing sensitivity
- [ ] Implement contextual reaction to handler position/motion
- [ ] Add recovery/fallback logic after imperfect input
- [ ] Implement breed-specific tendencies (beyond basic NavMesh params)
- [ ] Add obstacle reading beyond nearest-of-type
- [ ] Implement command interpretation with timing windows

### 1.3 Complete Obstacle Set
- [ ] Implement Tire Jump obstacle
- [ ] Implement Broad Jump obstacle
- [ ] Implement Wall Jump obstacle
- [ ] Add Jumpers-With-Weaves course type validation
- [ ] Implement refusal and run-out fault detection

### 1.4 Fault System Completion
- [ ] Complete WrongCourse fault detection
- [ ] Implement run-out fault detection
- [ ] Add flow-sensitive run evaluation
- [ ] Integrate all fault types with scoring

---

## Priority 2: Presentation Layer (High Impact)

### 2.1 Camera System Enhancement
- [ ] Implement cinematic chase camera mode
- [ ] Add replay camera network with multiple angles
- [ ] Implement freeze-frame highlight moments
- [ ] Add broadcast-style cutaways and hero shots
- [ ] Implement dog POV camera with proper framing

### 2.2 Replay System Polish
- [ ] Add super slow-motion on every fault
- [ ] Implement replay on personal-best split
- [ ] Add contextual replay framing
- [ ] Implement post-run replay review/editor
- [ ] Add highlight selection system

### 2.3 Commentary System Completion
- [ ] Implement weighted/random line selection
- [ ] Add anti-repetition logic
- [ ] Implement breed callouts
- [ ] Add split-time callouts
- [ ] Implement near-miss recognition
- [ ] Add championship pressure escalation
- [ ] Integrate PA announcer role
- [ ] Add authored/templated line assembly pipeline

### 2.4 Crowd System (New)
- [ ] Implement procedural population system (200+ target)
- [ ] Add reactive cheering/ovation logic
- [ ] Implement fault/tension reactions
- [ ] Add broadcast-style cutaways
- [ ] Implement crowd audio layers tied to performance

---

## Priority 3: UI/UX Systems

### 3.1 HUD Implementation (New)
- [ ] Design and implement gameplay HUD
- [ ] Add timer display with split-time indicators
- [ ] Implement fault counter display
- [ ] Add score/position overlay
- [ ] Implement course map/progress indicator
- [ ] Add command feedback indicators

### 3.2 Menu System (New)
- [ ] Implement Main Menu
- [ ] Create Mode Select screen (Training, Exhibition, Career)
- [ ] Build Team Select screen (handler/dog selection)
- [ ] Design and implement Results screen
- [ ] Add Settings menu

### 3.3 In-Game UI (New)
- [ ] Implement countdown overlay
- [ ] Add run completion celebration UI
- [ ] Create fault notification system
- [ ] Build split-time highlight UI

---

## Priority 4: Audio System (New)

### 4.1 Spatial Audio
- [ ] Implement spatial dog movement and footfalls
- [ ] Add obstacle impact sounds
- [ ] Implement environmental ambience
- [ ] Add subtle music bed

### 4.2 Audio Mix
- [ ] Implement crowd audio layers tied to performance
- [ ] Add swelling victory theme
- [ ] Implement dynamic mix priorities (replay/commentary)
- [ ] Add PA announcer audio integration

### 4.3 Voice Over Pipeline
- [ ] Complete ElevenLabs integration testing
- [ ] Add voice ownership workflow
- [ ] Implement audio asset management
- [ ] Add localization support for VO

---

## Priority 5: Progression & Career Systems (New)

### 5.1 Career Mode
- [ ] Implement career progression structure
- [ ] Add unlockable handlers system
- [ ] Add unlockable dogs system
- [ ] Add unlockable venues system
- [ ] Implement persistent performance records

### 5.2 Progression Systems
- [ ] Design and implement XP/leveling system
- [ ] Add achievement/trophy system
- [ ] Implement skill trees for handlers/dogs
- [ ] Add training mode with drills

---

## Priority 6: Online Features (New - Optional for RC)

### 6.1 Save System
- [ ] Implement cloud save abstraction
- [ ] Add local save fallback
- [ ] Implement save slot management

### 6.2 Leaderboards
- [ ] Design leaderboard data schema
- [ ] Implement leaderboard submission
- [ ] Add leaderboard display UI
- [ ] Implement ghost run storage/playback

---

## Priority 7: Accessibility (New)

### 7.1 Settings System
- [ ] Implement settings menu
- [ ] Add remappable controls
- [ ] Implement audio settings (volume, mix)
- [ ] Add visual accessibility options

### 7.2 Visual Aids
- [ ] Implement colorblind modes
- [ ] Add high contrast UI option
- [ ] Implement screen reader compatibility
- [ ] Add subtitle/caption system

---

## Priority 8: Content & Tooling

### 8.1 Course Authoring
- [ ] Build course editor tool
- [ ] Implement obstacle validation tool
- [ ] Add sequence legality checking
- [ ] Create command timing visualization

### 8.2 Debug Tools
- [ ] Implement dog path debug visualization
- [ ] Add replay diagnostics
- [ ] Create AI state visualization
- [ ] Add performance profiler hooks

### 8.3 Data Pipeline
- [ ] Create additional breed data
- [ ] Design and create venue data
- [ ] Add handler profile data
- [ ] Implement course library

---

## Priority 9: Platform & Performance

### 9.1 WebGL Considerations
- [ ] Evaluate WebGL prototype constraints
- [ ] Implement performance budgets
- [ ] Add scalable fallbacks (crowd, LOD)
- [ ] Test browser compatibility

### 9.2 Production Architecture
- [ ] Review PoC to Production migration path
- [ ] Implement Addressables for content streaming
- [ ] Add platform abstraction layer
- [ ] Implement telemetry hooks

---

## Open Questions Requiring Decision

| Question | Impact | Recommended Default |
|----------|--------|---------------------|
| Eastworld content scope? | Commentary pipeline | Limited to gameplay commentary |
| Approval workflow for generated scripts? | Content pipeline | Author review before deployment |
| ElevenLabs licensing/ownership? | Legal/VO | Standard commercial license |
| Recorded vs synthesized VO split? | Production cost | Synthesized for gameplay, recorded for key moments |
| Mocap scope? | Animation quality | Key animations only (handler, dog states) |
| Breed count at launch? | Content volume | 4-6 breeds |
| Number of launch venues? | Content volume | 3-5 venues |
| Ghost run scope in v1? | Online features | Local ghost only, no sharing |
| Replay storage format? | Data management | Custom binary format |
| WebGL vs Production code sharing? | Architecture | Separate branches |

---

## Estimated Timeline to RC

| Phase | Duration | Key Deliverables |
|-------|----------|------------------|
| Phase 1: Core Completion | 4-6 weeks | Handler, Dog AI, Obstacles, Faults complete |
| Phase 2: Presentation | 3-4 weeks | Camera, Replay, Commentary, Crowd polish |
| Phase 3: UI/UX | 3-4 weeks | HUD, Menus, In-game UI |
| Phase 4: Audio & Progression | 3-4 weeks | Audio system, Career mode |
| Phase 5: Polish & Accessibility | 2-3 weeks | Settings, Accessibility, Bug fixes |
| Phase 6: Platform & Testing | 2-3 weeks | WebGL optimization, QA pass |

**Total Estimated: 17-24 weeks (4-6 months)**

---

## Risk Register

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Dog AI complexity exceeds timeline | High | High | Prioritize core states, defer advanced behaviors |
| Commentary quality inconsistent | Medium | Medium | Implement fallback authored lines |
| Performance issues on WebGL | High | High | Early profiling, scalable settings |
| ElevenLabs API reliability | Low | High | Implement local TTS fallback |
| Scope creep in progression systems | Medium | Medium | Strict v1 feature gating |

---

*Generated by evaluating current codebase against agility-dogs.md PRD requirements*
