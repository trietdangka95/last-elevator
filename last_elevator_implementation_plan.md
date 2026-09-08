# LAST ELEVATOR — Implementation Plan

> **Project codename:** Last Elevator  
> **Engine:** Unity + C#  
> **Target MVP platform:** Android  
> **Orientation:** Portrait 9:16, one-hand UI  
> **Team:** Solo developer  
> **Save:** Local only  
> **Primary goal:** Ship a playable, testable Android MVP in 30 days without overbuilding systems that do not prove the core loop.

---

# 0. How to Use This Document

This is not a high-level GDD. It is an **implementation-oriented execution plan** intended to be usable by a coding agent or solo developer directly.

The project is split into:

- **MVP1 / Core Validation:** prove that the run loop is fun and replayable.
- **MVP1 / Content + Polish:** make the validated loop feel like a real game.
- **MVP2:** survivor depth, infection, relationships, meta progression.
- **Monetization:** full game is free; rewarded ads only, never forced; no IAP/paywall for the current product plan. Provider integration happens last.

## Hard rule

Do **not** implement an MVP2 feature while an MVP1 acceptance criterion is still failing.

If a feature is not explicitly listed in MVP1 scope, treat it as out of scope.

---

# 1. Locked Product Decisions

| Area | Decision |
|---|---|
| Engine | Unity, C# |
| Platform | Android first |
| Screen | Portrait 9:16 |
| Control | One hand, primarily lower 40% of screen |
| Art direction | 2D dark-cute / chibi vector-like sprites |
| Core genre | Roguelite survival + decision management |
| Combat | No real-time action combat |
| Run length | Target 8–15 minutes |
| Floors | 30 floors |
| Resources | Energy, Integrity, Capacity, Scrap |
| Save | Local JSON |
| Backend | None for MVP |
| Cloud save | No |
| Meta progression | Not in core validation; planned for MVP2 |
| Monetization | Full game free; rewarded ads only; no forced ads, banners, interstitials, or IAP; implement last |
| Low-end Android support | Not a launch constraint |
| Team | Solo dev, all assets self-produced |

---

# 2. Product Vision

## Player fantasy

The player operates the last functioning elevator in a collapsing building.

The player cannot save everyone.

The elevator is a moving shelter with limited:

- energy,
- structural integrity,
- seats,
- information,
- time.

Every few seconds the player should face a meaningful question:

> “Is this floor worth the risk?”

and periodically:

> “I only have one seat left. Who matters more?”

The elevator itself is not the true hook.

The hook is:

> **Limited capacity + uncertain information + irreversible decisions.**

## Marketing sentence

> **6 seats. 7 survivors. Who do you leave behind?**

---

# 3. Design Pillars

Every feature must support at least one pillar.

## P1 — Fast Decisions

A normal decision should take roughly 2–8 seconds.

Avoid:

- large inventories,
- complex skill trees inside a run,
- long dialogue,
- tactical grids,
- real-time movement.

## P2 — Incomplete Information

The player should rarely know everything before choosing.

Examples:

- a floor says `Distress Signal`, not `Doctor waiting here`,
- an enemy threat shows a range or warning level,
- some rewards are known, some are uncertain.

## P3 — Scarcity Creates Stories

The player should regularly lack at least one thing:

- energy,
- integrity,
- capacity,
- scrap.

The best moments happen when the player is forced to sacrifice one goal for another.

## P4 — Runs Are Short Enough to Retry

Failure should trigger:

> “One more run.”

not:

> “I lost 45 minutes.”

Target:

- first loss: 5–10 min,
- competent run: 8–15 min,
- maximum normal run: < 18 min.

## P5 — Content Is Data-Driven

Events, enemies, survivors, upgrades, and floor clues must be authored without changing gameplay code.

Use **ScriptableObjects for static content** and serializable plain C# runtime state for saves/runs.

---

# 4. Scope

# 4.1 MVP1 — Core Validation Scope

The smallest version that proves the game works.

Must contain:

- main menu,
- start run,
- 30-floor run structure,
- route/floor choice,
- Energy,
- Integrity,
- Capacity,
- Scrap,
- survivor pickup,
- survivor replacement when full,
- encounter cards,
- basic enemies,
- Fight / Escape / Seal Door style outcomes,
- in-run elevator upgrades,
- death,
- win,
- restart,
- seeded random generation,
- local save of settings + best result,
- basic sound/haptics,
- Android build.

Content target by end of MVP1:

- 20 encounters,
- 5 enemy types,
- 10 survivor definitions,
- 5 elevator upgrade definitions,
- 1 final boss encounter.

## MVP1 survivor depth

Survivors intentionally remain shallow.

Each survivor has:

- id,
- display name,
- portrait,
- role label,
- combat power,
- one optional simple passive modifier.

Do **not** build:

- relationship graphs,
- infection spread,
- individual HP,
- morale,
- hunger,
- large trait systems.

Those belong to MVP2.

---

# 4.2 MVP2 Scope

Only begin after MVP1 playtests show the core loop is fun.

Add:

- survivor class abilities,
- traits,
- infection state,
- hidden infection possibility,
- relationship links,
- event consequences based on roster,
- meta currency,
- permanent tech tree,
- class unlocks,
- codex/collection,
- more event chains,
- alternate endings.

Detailed MVP2 plan is in section 23.

---

# 4.3 Explicitly Out of Scope for First 30 Days

Do not implement:

- multiplayer,
- PvP,
- guilds,
- cloud save,
- accounts,
- backend,
- battle pass,
- daily quests,
- live events,
- procedural dialogue generation,
- real-time combat,
- pathfinding,
- controllable character movement,
- large inventory,
- crafting tree,
- equipment items,
- localization pipeline beyond string centralization,
- elaborate tutorial cutscenes,
- multiple game modes.

---

# 5. Core Gameplay Loop

```text
Start Run
   ↓
Preview 2–3 reachable floor signals
   ↓
Choose next stop
   ↓
Spend Energy based on distance
   ↓
Arrive at floor
   ↓
Resolve Encounter
   ↓
Gain/Lose Energy / Integrity / Scrap / Survivor
   ↓
If roster full, optionally replace survivor
   ↓
Optional upgrade decision
   ↓
Choose next floor
   ↓
Checkpoint at 10 / 20
   ↓
Final encounter at 30
   ↓
Win or Die
   ↓
Run Summary
   ↓
Retry
```

## Core loop timing target

| Action | Target duration |
|---|---:|
| Floor selection | 2–5 sec |
| Elevator transition | 0.5–1.5 sec |
| Encounter reading | 3–8 sec |
| Outcome animation | 0.5–1.5 sec |
| Roster replacement | 3–8 sec |
| Upgrade choice | 3–6 sec |

The average floor should not require more than ~15–20 seconds of attention.

---

# 6. Floor Navigation

## Design

At most non-milestone stops, show the player the next **three reachable floor candidates**:

- `current + 1`
- `current + 2`
- `current + 3`

Each candidate shows:

- floor number,
- signal/clue,
- energy cost,
- danger indicator if known.

Example:

```text
Floor 13
📻 Distress Signal
Energy: -2

Floor 14
⚡ Power Surge
Energy: -4

Floor 15
⚠ Scratching Sounds
Energy: -6
```

Choosing floor 15 permanently skips floors 13 and 14 for that run.

This creates a meaningful route decision without building a map.

## Mandatory floors

- Floor 10: checkpoint encounter
- Floor 20: checkpoint encounter
- Floor 30: final encounter

When a checkpoint is within the candidate range, it must appear.

The final encounter cannot be skipped.

## Energy cost

Initial rule:

```text
energyCost = distance * 2
```

Examples:

- +1 floor = 2 Energy
- +2 floors = 4 Energy
- +3 floors = 6 Energy

Upgrades may modify this later.

## No backtracking in MVP1

Backtracking/falling elevator events are promising, but do not belong in core MVP implementation.

Add only after the normal route system is stable.

---

# 7. Core Resources

## 7.1 Energy

Purpose:

- forces routing decisions,
- prevents visiting every floor,
- creates urgency.

Initial values:

```text
MaxEnergy = 100
StartEnergy = 75
```

Lose condition:

- If Energy reaches 0 and the player cannot reach another valid stop, run ends.

Events may restore Energy.

## 7.2 Integrity

Elevator health.

Initial:

```text
MaxIntegrity = 100
StartIntegrity = 100
```

If Integrity <= 0:

- immediate run failure.

## 7.3 Capacity

Initial:

```text
Capacity = 4
```

Upgrade ceiling in MVP1:

```text
MaxCapacity = 6
```

Starting with 4 instead of 6 creates the capacity conflict earlier.

## 7.4 Scrap

In-run upgrade currency.

Initial:

```text
StartScrap = 0
```

Scrap disappears at end of run in MVP1.

MVP2 may convert part of end-run score into meta currency.

---

# 8. Survivor System — MVP1

## Runtime role

Survivors serve three purposes:

1. emotional ownership,
2. capacity pressure,
3. combat power / simple utility.

## Minimal survivor definition

```csharp
public enum SurvivorRole
{
    Civilian,
    Medic,
    Engineer,
    Guard,
    Technician
}

[CreateAssetMenu(menuName = "LastElevator/Survivor")]
public sealed class SurvivorDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite portrait;
    public SurvivorRole role;
    [Range(0, 5)] public int combatPower;
    public SimplePassive passive;
}
```

## MVP1 passive types

Keep these extremely small.

```csharp
public enum SimplePassive
{
    None,
    ReduceTravelEnergyOne,
    AddCombatPowerOne,
    RepairOnCheckpointFive,
    RevealDangerSometimes
}
```

Do not build a generalized buff framework for MVP1.

Use direct calculation functions.

## Roster replacement

When adding a survivor at full capacity:

1. show new survivor,
2. show current roster,
3. player chooses one current survivor to leave,
4. or refuses the new survivor.

Leaving a survivor is permanent for the run.

No confirmation popup after the first tutorial occurrence. The replacement screen itself is the confirmation.

---

# 9. Combat Design

No real-time combat.

Combat is a **risk-resolution decision**.

## Enemy data

Each enemy has:

- threat power,
- integrity damage on loss,
- scrap reward,
- tags,
- clue text.

Example:

```csharp
[CreateAssetMenu(menuName = "LastElevator/Enemy")]
public sealed class EnemyDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite portrait;
    public int threatPower;
    public int lossIntegrityDamage;
    public Vector2Int scrapRewardRange;
}
```

## Team power

Initial formula:

```text
TeamPower =
    sum(survivor.combatPower)
    + temporaryRunModifiers
```

## Fight success chance

MVP1 formula:

```text
delta = TeamPower - ThreatPower
successChance = clamp(0.20 + 0.10 * delta + 0.50, 0.15, 0.90)
```

Simplified equivalent:

```text
successChance = clamp(0.70 + 0.10 * (TeamPower - ThreatPower), 0.15, 0.90)
```

Examples:

| Team - Enemy | Win chance |
|---:|---:|
| -5 | 20% |
| -3 | 40% |
| -1 | 60% |
| 0 | 70% |
| +1 | 80% |
| +2 or more | 90% cap |

This is an initial balancing formula, not sacred.

## Encounter choices

Typical enemy encounter:

### Fight

- rolls success,
- win: gain Scrap,
- lose: lose Integrity,
- occasional survivor-loss outcome only in later tuning if needed.

### Escape

- guaranteed,
- lose Energy.

### Seal Door

- guaranteed,
- lose Integrity,
- smaller loss than failed fight.

The player should usually have at least one non-RNG option.

---

# 10. Encounter System

Encounters are the primary content format.

## Event categories

```csharp
public enum EncounterCategory
{
    Survivor,
    Enemy,
    Resource,
    Hazard,
    Mystery,
    Checkpoint,
    Boss
}
```

## Data strategy

Use a single `EncounterDefinition` ScriptableObject with a flat list of choices and effects.

Avoid polymorphic `SerializeReference` hierarchies in MVP1.

```csharp
[CreateAssetMenu(menuName = "LastElevator/Encounter")]
public sealed class EncounterDefinition : ScriptableObject
{
    public string id;
    public string title;
    [TextArea] public string body;
    public Sprite illustration;
    public EncounterCategory category;
    public string clueText;
    public int weight = 10;
    public int minFloor = 1;
    public int maxFloor = 29;
    public List<EncounterChoiceData> choices;
}
```

## Choice data

```csharp
[Serializable]
public sealed class EncounterChoiceData
{
    public string label;
    public List<ConditionData> conditions;
    public List<EffectData> successEffects;
    public List<EffectData> failureEffects;

    [Range(0f, 1f)]
    public float successChance = 1f;
}
```

## Condition types

```csharp
public enum ConditionType
{
    None,
    MinEnergy,
    MinIntegrity,
    MinScrap,
    HasFreeCapacity,
    MinTeamPower,
    HasRole
}
```

## Effect types

```csharp
public enum EffectType
{
    None,
    AddEnergy,
    AddIntegrity,
    AddScrap,
    DamageIntegrity,
    AddSurvivor,
    StartCombat,
    SetRunFlag
}
```

## Effect data

```csharp
[Serializable]
public sealed class EffectData
{
    public EffectType type;
    public int intValue;
    public string stringValue;
    public SurvivorDefinition survivor;
    public EnemyDefinition enemy;
}
```

## Resolver rule

`EncounterResolver` must be the only place that mutates run state from encounter effects.

UI must never directly modify `RunState`.

---

# 11. Floor Generation

## Requirements

The generator must:

- be seeded,
- avoid impossible states,
- avoid excessive repeats,
- scale danger over the run,
- guarantee checkpoints,
- produce clues before the encounter is chosen.

## Seeded RNG

Create an abstraction.

```csharp
public interface IRandomService
{
    int Range(int minInclusive, int maxExclusive);
    float Value();
    T PickWeighted<T>(IReadOnlyList<T> items, Func<T, int> getWeight);
}
```

Use `System.Random` internally.

Store the seed in `RunState`.

This allows bug reports such as:

```text
Seed: 18492301
Run failed at Floor 17
```

to be replayed exactly.

## Difficulty bands

```text
Floors 1–9    = Early
Floors 11–19  = Mid
Floors 21–29  = Late
Floor 10      = Checkpoint A
Floor 20      = Checkpoint B
Floor 30      = Boss
```

Suggested content weights:

### Early

- Survivor: 30%
- Resource: 30%
- Enemy: 15%
- Hazard: 10%
- Mystery: 15%

### Mid

- Survivor: 20%
- Resource: 20%
- Enemy: 30%
- Hazard: 15%
- Mystery: 15%

### Late

- Survivor: 15%
- Resource: 15%
- Enemy: 40%
- Hazard: 20%
- Mystery: 10%

Do not hardcode these percentages into the generator.

Expose as balancing config.

---

# 12. In-Run Upgrades

Upgrades are purchased with Scrap.

## MVP1 upgrade set

### Battery Pack

```text
Cost: 12 Scrap
Max Energy +15
Immediate Energy +10
```

### Reinforced Walls

```text
Cost: 12 Scrap
Max Integrity +20
Immediate Integrity +10
```

### Extra Seat

```text
Cost: 18 Scrap
Capacity +1
Maximum 2 purchases
```

### Efficient Motor

```text
Cost: 16 Scrap
Travel cost -1 Energy per move
Minimum travel cost = 1
```

### Door Brace

```text
Cost: 14 Scrap
Seal Door Integrity cost -30%
```

## Upgrade presentation

Do not build a full shop scene.

After specific resource/checkpoint events, show:

```text
Choose one upgrade:
[Upgrade A]
[Upgrade B]
[Leave]
```

This keeps the run moving.

---

# 13. Addictive Loop Without Overcomplication

The intended replayability comes from five systems.

## 13.1 Variable route information

The player sees signals, not exact rewards.

## 13.2 Capacity conflict

Roster fills before the end of the run.

The player should face at least 2 meaningful replace/refuse decisions in an average run.

## 13.3 Resource near-misses

Target balance should produce moments like:

```text
Energy 8/100
Floor 27
Need 6 energy to reach a likely battery floor
```

Near-misses are desirable.

Unavoidable deaths are not.

## 13.4 Short irreversible outcomes

Once a choice resolves, do not add undo.

## 13.5 One-more-run restart

From Run End to new Floor 1:

- maximum 2 taps,
- target < 3 seconds on a normal device.

---

# 14. Game State Architecture

Separate:

- static definitions,
- runtime run state,
- persistent player state.

## 14.1 Static definitions

ScriptableObjects:

```text
EncounterDefinition
SurvivorDefinition
EnemyDefinition
UpgradeDefinition
BalanceConfig
AudioLibrary
```

## 14.2 Runtime run state

Plain serializable C# classes.

```csharp
[Serializable]
public sealed class RunState
{
    public int seed;
    public int currentFloor;
    public int energy;
    public int maxEnergy;
    public int integrity;
    public int maxIntegrity;
    public int capacity;
    public int scrap;
    public int travelEnergyReduction;

    public List<string> survivorIds = new();
    public List<string> ownedUpgradeIds = new();
    public List<string> resolvedEncounterIds = new();
    public List<string> flags = new();

    public bool isRunOver;
    public bool isVictory;
    public bool rewardedReviveUsed;
}
```

## 14.3 Persistent save

```csharp
[Serializable]
public sealed class PlayerSaveData
{
    public int schemaVersion = 1;
    public int bestFloor;
    public int runsStarted;
    public int runsWon;
    public bool tutorialCompleted;

    public float musicVolume = 1f;
    public float sfxVolume = 1f;
    public bool hapticsEnabled = true;

    // MVP2 fields can be added through schema migration.
}
```

Do not serialize ScriptableObject references.

Persist IDs only.

---

# 15. Save System

Path:

```csharp
Application.persistentDataPath + "/player_save.json"
```

## Requirements

- load on boot,
- if no save exists, create defaults,
- if malformed, back up corrupt file and create defaults,
- atomic save:
  1. write `.tmp`,
  2. replace main file,
- save after:
  - run start,
  - run end,
  - settings change,
  - tutorial completion.

For MVP1, active run resume is optional until Day 21.

By release candidate, support active run resume after app background/kill if feasible.

## Interface

```csharp
public interface ISaveService
{
    PlayerSaveData LoadPlayerSave();
    void SavePlayerSave(PlayerSaveData data);

    RunState LoadActiveRun();
    void SaveActiveRun(RunState run);
    void ClearActiveRun();
}
```

---

# 16. Scene Structure

Keep scene count low.

```text
00_Bootstrap
01_MainMenu
02_Game
```

## 00_Bootstrap

Responsibilities:

- initialize services,
- load save,
- register content,
- set target frame rate,
- transition to MainMenu.

## 01_MainMenu

Contains:

- Play,
- Best Floor,
- Settings,
- Credits,
- Continue Run if active run exists.

## 02_Game

One gameplay scene.

Panels are UI states, not new scenes.

Panels:

```text
RunHUD
FloorChoicePanel
EncounterPanel
OutcomePanel
RosterReplacePanel
UpgradePanel
PausePanel
RunEndPanel
```

---

# 17. Core Runtime Systems

Recommended dependency direction:

```text
UI
 ↓
RunController
 ↓
Domain Systems
 ↓
RunState
```

UI sends commands to `RunController`.

UI does not call multiple systems itself.

## 17.1 RunController

Responsibilities:

- initialize run,
- hold current `RunState`,
- coordinate state transitions,
- expose view data,
- determine run end,
- save active run.

Main methods:

```csharp
StartNewRun(int? seed = null)
ResumeRun()
ChooseFloor(int floor)
ChooseEncounterOption(int optionIndex)
ChooseSurvivorReplacement(string survivorIdToRemove)
ChooseUpgrade(string upgradeId)
RestartRun()
```

## 17.2 FloorGenerator

Responsibilities:

- create candidate floor stops,
- choose encounter definitions,
- guarantee checkpoints,
- enforce content constraints.

Must not mutate UI.

## 17.3 EncounterResolver

Responsibilities:

- validate conditions,
- roll success,
- apply effects,
- return an `EncounterResolution`.

## 17.4 SurvivorRoster

Responsibilities:

- calculate total power,
- add/remove survivor IDs,
- validate capacity,
- calculate simple passives.

## 17.5 UpgradeSystem

Responsibilities:

- validate cost,
- apply upgrade,
- calculate derived modifiers.

## 17.6 RunRules

Pure functions where possible:

```text
GetTravelEnergyCost()
GetFightChance()
CanReachFloor()
IsRunDead()
GetTeamPower()
GetAvailableCapacity()
```

Keep balancing math here instead of spreading constants across MonoBehaviours.

---

# 18. Run Flow State Machine

Use an explicit enum.

```csharp
public enum RunPhase
{
    None,
    ChoosingFloor,
    Travelling,
    Encounter,
    Resolving,
    ReplacingSurvivor,
    ChoosingUpgrade,
    RunEnded
}
```

`RunController` owns phase transitions.

Allowed flow:

```text
ChoosingFloor
  -> Travelling
  -> Encounter
  -> Resolving
  -> ReplacingSurvivor? 
  -> ChoosingUpgrade?
  -> ChoosingFloor
```

Any invalid UI input during the wrong phase should be ignored and logged in development builds.

This prevents double-tap bugs.

---

# 19. Event Bus

Avoid a giant global event bus.

Use C# events on the controller/service that owns the state.

Example:

```csharp
public event Action<RunViewModel> RunStateChanged;
public event Action<EncounterViewModel> EncounterStarted;
public event Action<EncounterResolution> EncounterResolved;
public event Action<RunEndViewModel> RunEnded;
```

UI subscribes on `OnEnable`, unsubscribes on `OnDisable`.

---

# 20. UI / UX Specification

## 20.1 Visual hierarchy

Top 20%:

- floor,
- Energy,
- Integrity,
- Capacity,
- Scrap.

Middle 40%:

- elevator scene,
- door,
- current occupants,
- event illustration.

Bottom 40%:

- current decision,
- primary buttons.

All normal gameplay decisions must be reachable by one thumb.

## 20.2 HUD

Persistent:

```text
FLOOR 17

⚡ 42/100
♥ 78/100
👥 4/5
🔩 11
```

Avoid tiny text.

## 20.3 Floor choice

Three vertical cards or a horizontal stack reachable from lower screen.

Each card:

```text
FLOOR 18
📻 Distress Signal
-2 ⚡

[GO]
```

## 20.4 Encounter panel

Contains:

- illustration,
- title,
- 1–3 sentences,
- 2–3 large action buttons,
- known cost/reward text.

No scroll for normal encounter text.

## 20.5 Roster replace

New survivor is visually emphasized.

Current roster shows:

- portrait,
- name,
- role,
- combat power,
- passive icon.

Actions:

- tap current survivor to replace,
- `Leave New Survivor`.

## 20.6 Feedback

When a resource changes:

```text
Energy -6
Integrity -12
Scrap +8
```

Animate the corresponding HUD value.

Do not use modal popups for every resource update.

---

# 21. Art Direction

## Style

**Dark-cute chibi 2D.**

Think:

- rounded silhouettes,
- large heads / expressive faces,
- simple 2–3-value shading,
- slightly desaturated environment,
- bright readable UI symbols,
- horror through lighting/sound rather than gore.

Why:

- easier for a solo developer to draw consistently,
- survivor decisions feel more emotional when characters are cute,
- clearer on small mobile screens,
- strong contrast between charming characters and dangerous setting,
- easier to produce marketing images later.

## Asset constraints

### Survivor portraits

- square source,
- one bust/portrait each,
- strong silhouette,
- 2–3 facial expressions only if time allows.

### Enemy art

- one portrait/half-body image,
- no full animation requirement.

### Elevator

MVP can use:

- 1 closed-door state,
- 1 open-door state,
- flicker overlay,
- shake animation,
- damage cracks overlay.

Avoid skeletal animation in first 30 days.

## Production target

By Day 26:

- 10 survivor portraits,
- 5 enemy portraits,
- 1 boss illustration,
- 20 encounter illustration/icons,
- elevator interior,
- door open/closed,
- ~25 UI icons/sprites.

Reuse backgrounds aggressively.

---

# 22. Audio / Haptics

Minimum audio set:

```text
elevator_start
elevator_stop
door_open
door_close
button_press
combat_hit_01
combat_hit_02
integrity_damage
energy_gain
scrap_gain
survivor_join
warning
run_fail
run_win
```

Music:

- one low-intensity loop,
- one tension/boss loop.

Haptics:

- light: button/arrival,
- medium: damage,
- strong: critical hit/boss/failure.

Expose haptic toggle.

---

# 23. MVP2 — Detailed Feature Plan

Do not implement until MVP1 is validated.

# 23.1 Survivor Classes

Upgrade role system into real classes:

```text
Doctor
Engineer
Guard
Hacker
Scout
Civilian
```

Abilities:

### Doctor

Every 3 resolved floors:

```text
Integrity +3
```

Narratively represented as maintaining passengers and emergency equipment.

### Engineer

```text
Travel Energy Cost -1
```

Only one stack allowed unless balancing proves otherwise.

### Guard

```text
Team Combat Power +2
```

### Hacker

Chance to reveal exact encounter category/reward.

### Scout

Chance to reveal one extra future floor candidate.

### Civilian

No stat advantage, but can participate in emotional event chains and endings.

# 23.2 Traits

Each survivor may get one trait.

Examples:

```text
Brave
Cowardly
Lucky
Selfish
Resourceful
Injured
```

Do not generate arbitrary combinatorial effects.

Start with 6 trait types.

# 23.3 Infection

State:

```csharp
public enum InfectionState
{
    Clean,
    Suspicious,
    Infected,
    HiddenInfected
}
```

MVP2 infection goals:

- create uncertainty,
- make medical/check events valuable,
- create delayed consequences.

Avoid a complex disease simulation.

# 23.4 Relationships

Use simple directional tags, not a generic social graph engine.

Example:

```text
survivorA.relatedSurvivorId = survivorB
relationshipType = Parent
```

Initial relationship types:

```text
Parent
Sibling
Partner
Friend
```

Only authored survivor pairs have relationships.

Effects should be event-driven, not constantly simulated.

Example:

- leave child behind,
- parent gains `Distressed` flag,
- later event changes.

# 23.5 Meta Currency

Add:

```text
TechParts
```

Earned from:

- floor reached,
- boss kill,
- first-time event discoveries,
- challenge bonuses.

## Rule

Permanent upgrades must improve consistency, not remove decisions.

Recommended total permanent power increase cap:

```text
~25%
```

# 23.6 Tech Tree

Branches:

```text
Battery
Armor
Scanner
Roster
Utility
```

Initial 10 nodes.

Examples:

```text
Start Energy +5
Max Integrity +5
First unknown floor each run is revealed
Start with +2 Scrap
First survivor replacement refunds 2 Energy
```

Avoid:

```text
+100% damage
double resources
auto-win early floors
```

# 23.7 Collection

Track:

```text
Survivors discovered
Enemies encountered
Events discovered
Endings
```

No need for achievements backend initially.

---

# 24. Monetization Plan — LOCKED — Implement Last

## 24.0 Product monetization decision

The base game is **100% free to play from Floor 1 through the ending**.

Current product plan:

```text
FULL GAME FREE
+
OPTIONAL REWARDED ADS ONLY
+
NO IAP
+
NO PAYWALL
+
NO FORCED ADS
```

Explicitly prohibited for the current plan:

```text
banner ads
interstitial ads after floors/runs
app-open ads
forced ads after death
ads triggered by normal navigation
fake close buttons
ad buttons that visually impersonate normal gameplay buttons
pay-to-win purchases
"remove ads" / supporter purchase
```

Core rule:

> A player must be able to install the game, finish the entire game, and enjoy the intended progression while watching exactly **zero ads**.

Ads are a voluntary recovery/bonus tool, not part of baseline game balance.

Gameplay must remain viable with monetization code completely disabled.

---

## 24.1 Why rewarded ads fit this game

The emotional peak of a roguelite run occurs when the player loses something they have invested in.

The ad offer should therefore feel like:

```text
"I almost made it. I want one more chance."
```

Never:

```text
"The game intentionally killed me so I would watch an ad."
```

The design goal is **light frustration + regret + agency**.

Failure should normally be attributable to:

- a risky floor choice,
- resource mismanagement,
- taking one survivor too many,
- a combat gamble,
- choosing short-term reward over long-term safety,
- normal RNG that the player understood before accepting the risk.

Do not secretly modify RNG, enemy stats, energy costs, or boss difficulty based on ad availability.

---

## 24.2 Ads abstraction

Keep SDK-specific code outside gameplay systems.

```csharp
public interface IAdsService
{
    bool IsRewardedReady(string placementId);
    void ShowRewarded(string placementId, Action<RewardedAdResult> onComplete);
}

public enum RewardedAdResult
{
    Completed,
    Skipped,
    Failed,
    Unavailable
}
```

Provide:

```text
NullAdsService
RealAdsService
```

Development builds default to `NullAdsService`.

Gameplay systems must request a reward through an application-level monetization controller; they must not call an ad SDK directly.

Suggested wrapper:

```csharp
public sealed class RewardedAdsController
{
    public bool CanOfferRevive(RunState run);
    public void RequestRevive(RunState run, Action<bool> result);
    public bool CanOfferRunBonus(RunResult result);
    public void RequestRunBonus(RunResult result, Action<bool> result);
}
```

No reward is granted until the SDK reports successful rewarded-ad completion.

If an ad fails or is unavailable, return the player safely to the same game-over/result screen with no lost progress and no soft lock.

---

## 24.3 Placement A — Rewarded Revive

Placement ID:

```text
run_revive
```

This is the primary and only rewarded-ad placement required for the first public MVP.

### Eligibility

Initial tuning:

```text
Floor 1–9   -> do not offer revive
Floor 10–14 -> normally do not offer in MVP1
Floor 15–20 -> revive may be offered with neutral copy
Floor 21–26 -> offer revive with "you have come far" copy
Floor 27–29 -> strongest "almost there" copy
Floor 30    -> allow only if boss state can be restored safely
```

Additional rules:

- maximum **1 rewarded revive per run**,
- only after a genuine run-ending failure,
- never interrupt the player before the death/result screen has fully resolved,
- never auto-open an ad,
- if no rewarded ad is ready, hide/disable the ad CTA gracefully,
- pressing the normal end-run CTA must immediately end the run without an ad.

### Revive reward

On successful completion:

```text
current floor remains the same
run seed remains the same
survivor roster remains the same
previous choices remain committed
rewardedReviveUsed = true
Integrity = max(30% of max Integrity, minimum safe resume value)
Energy = max(current Energy, energy required for one valid next decision + small buffer)
```

Do **not** rewind several floors or erase the consequence history of the run.

The reward means:

> "Keep this run alive from here."

not:

> "Undo every mistake."

### Revive UI layout

Example:

```text
┌─────────────────────────────┐
│          RUN OVER           │
│                             │
│          FLOOR 27           │
│      Only 3 floors left     │
│                             │
│  [ END RUN ]                │
│                             │
│  🎬 CONTINUE FROM FLOOR 27  │
│     Watch 1 rewarded ad     │
└─────────────────────────────┘
```

Both choices must be visually clear.

Do not make `END RUN` tiny, hidden, low-contrast, or intentionally difficult to tap.

---

## 24.4 Rewarded Revive Copy System

The emotional copy should be **dynamic and truthful**.

Never show "You're almost there" when the player is still far from Floor 30.

Recommended copy bands:

### Floor 15–20 — mild regret

```text
Title: Run này vẫn còn cứu được
Body: Bạn đã đi khá xa. Muốn tiếp tục ngay tại tầng {floor}?
CTA: 🎬 Cứu run này
Secondary: Kết thúc run
```

### Floor 21–26 — investment reminder

```text
Title: Bạn đã đi rất xa rồi
Body: Giữ nguyên đội hình và tiếp tục từ tầng {floor} bằng 1 lượt hồi sinh.
CTA: 🎬 Tiếp tục từ tầng {floor}
Secondary: Kết thúc run
```

### Floor 27–29 — near-finish tension

```text
Title: Chỉ còn {floorsRemaining} tầng nữa
Body: Tiếc run này lắm. Xem 1 quảng cáo để ở lại tầng {floor} và tiếp tục.
CTA: 🎬 Ở lại tầng {floor}
Secondary: Kết thúc run
```

Optional transparent support line beneath the CTA:

```text
Quảng cáo này cũng giúp hỗ trợ game.
```

This line may be used, but never imply the player is hurting the developer by declining.

Avoid guilt/shame copy such as:

```text
"Không xem là bỏ phí công sức của mình."
"Dev cần bạn xem quảng cáo."
"Bạn chắc chắn muốn bỏ cuộc chứ?"
```

The target emotion is **"tiếc run"**, not **"có lỗi với dev"**.

### Copy implementation

Do not hard-code the final strings into `RunEndView`.

Suggested model:

```csharp
public readonly struct ReviveOfferCopy
{
    public string title;
    public string body;
    public string rewardedCta;
    public string declineCta;
}

public interface IReviveOfferCopyProvider
{
    ReviveOfferCopy Get(int currentFloor, int finalFloor);
}
```

This makes tone changes/A-B tests possible without changing run logic.

---

## 24.5 Placement B — End-of-Run Bonus

Placement ID:

```text
run_reward_bonus
```

This placement is **planned, not required for MVP1**.

Reason: MVP1 intentionally does not rely on meta currency. Do not invent persistent currency solely to create an ad placement.

Enable this only after MVP2 introduces a legitimate persistent reward such as `TechParts`.

Example after MVP2:

```text
RUN COMPLETE

Tech Parts earned: 80

[ COLLECT 80 ]

🎬 [ COLLECT 120 ]
    +50% rewarded bonus
```

Rules:

- normal reward is always immediately claimable,
- rewarded bonus is optional,
- recommended bonus: +25% to +50%, tune from telemetry,
- never reduce the normal reward because the bonus placement exists,
- do not show a second rewarded offer if the run already produced excessive monetization fatigue during testing.

Initial hard cap after this placement ships:

```text
maximum rewarded ad opportunities completed per run = 2
```

One revive + one end-of-run bonus is the absolute planned maximum.

---

## 24.6 UX rules for every ad placement

These are product requirements, not suggestions.

### Ads may only start after an explicit tap

The CTA must clearly communicate that an ad will play.

Good:

```text
🎬 Xem quảng cáo để tiếp tục
🎬 Nhận +50% Tech Parts
```

Bad:

```text
CONTINUE
FREE
CLAIM
```

when those buttons unexpectedly open an ad.

### Never cover gameplay with persistent advertising UI

No:

- banner at top/bottom,
- floating ad button over decision choices,
- ad icon obstructing elevator controls,
- countdown forcing a decision,
- auto-redirect after a normal tap.

### Respect decline

If the player selects `END RUN` / `COLLECT`, perform that action immediately.

Do not show:

```text
"Are you sure? Watch an ad instead!"
```

Do not ask twice.

### Reward integrity

Grant the exact reward described before the ad.

If the ad fails:

```text
"Không tải được quảng cáo. Bạn có thể thử lại hoặc kết thúc run."
```

Never consume the revive opportunity on a provider/network failure.

---

## 24.7 Monetization balancing guardrails

Never design failure states specifically to force an ad.

If a normal run becomes statistically unrealistic to finish without rewarded revive, balancing is wrong.

Target behavioral model:

```text
skilled player
-> can finish with 0 ads

average player
-> often loses in late game
-> sometimes voluntarily saves a valuable run

new player
-> learns through early losses
-> is not repeatedly shown ads during the tutorial/early floors
```

The game should create **fair frustration**:

```text
"I made the wrong call."
"I got greedy."
"I should have skipped that floor."
"I was three floors away... one revive is tempting."
```

Not:

```text
"The game cheated."
"I have to watch ads to progress."
```

---

## 24.8 Local monetization telemetry

Even without a backend, log locally during development/playtests:

```text
revive_offer_shown
revive_offer_floor
revive_offer_accepted
revive_ad_completed
revive_ad_failed
revive_after_ad_final_floor
revive_after_ad_victory
run_bonus_offer_shown          // MVP2
run_bonus_offer_accepted       // MVP2
```

Useful derived metrics:

```text
Revive Opt-In Rate
= accepted / shown

Revive Completion Rate
= completed / accepted

Revive-to-Win Rate
= victories after rewarded revive / completed revive ads

Late-Run Revive Opt-In
= accepted on Floor 27–29 / offers on Floor 27–29
```

Do not optimize only for ad views.

If ad opt-in rises while run completion, replay intent, or player sentiment falls, monetization is damaging the game.

---

## 24.9 Monetization release order

```text
1. Make core loop fun with all ads disabled.
2. Pass MVP1 Core Fun Gate.
3. Add Ads abstraction + NullAdsService.
4. Add rewarded revive UI and simulated completion.
5. Test revive copy + resume behavior locally.
6. Integrate a real rewarded-ad provider.
7. Release with rewarded revive only.
8. Observe player feedback/retention.
9. Add end-of-run bonus only after MVP2 has real meta currency and only if UX remains clean.
```

Do not add another ad format simply because revenue is lower than expected.


# 25. Local Analytics for Development

No backend needed.

Create a development run log saved locally.

Record:

```csharp
[Serializable]
public sealed class RunTelemetry
{
    public int seed;
    public float durationSeconds;
    public int finalFloor;
    public bool victory;
    public int survivorsCollected;
    public int survivorsRejected;
    public int fights;
    public int fightsWon;
    public int endingEnergy;
    public int endingIntegrity;
    public List<string> chosenEncounterOptions;
}
```

Export as JSON.

Use this to balance 20–30 test runs.

Key metrics:

```text
Average run duration
Median death floor
Win rate
Energy at death
Integrity at death
Average survivor replacements
Fight choice rate
Escape choice rate
Upgrade pick frequency
Encounter choice frequency
```

---

# 26. Initial Balance Targets

These are starting values only.

## Run

```text
Start Energy: 75
Max Energy: 100
Start Integrity: 100
Max Integrity: 100
Start Capacity: 4
Max Capacity: 6
```

## Target player outcomes

After first balance pass:

```text
New player median death floor: 16–22
Experienced player win rate: 20–35%
Average run length: 8–15 min
Average roster replacements: 2–4
Average upgrades/run: 2–4
```

## Resource tuning principle

At any point from Floor 15 onward, ideally at least one resource is under pressure.

Bad:

```text
Energy 95
Integrity 98
Capacity 3/6
Scrap 50
```

Good:

```text
Energy 22
Integrity 61
Capacity 5/5
Scrap 14
```

---

# 27. Initial Content List

Use IDs from day one.

## 27.1 Survivors

```text
survivor_maya_medic
survivor_ken_engineer
survivor_rin_guard
survivor_tom_civilian
survivor_ivy_technician
survivor_noah_civilian
survivor_lia_medic
survivor_max_guard
survivor_eli_engineer
survivor_zoe_civilian
```

MVP1 can assign only simple roles/power.

## 27.2 Enemies

```text
enemy_crawler
enemy_brute
enemy_infected_worker
enemy_stalker
enemy_nest
boss_elevator_mimic
```

Suggested threat powers:

```text
Crawler          2
Infected Worker  4
Stalker          6
Brute            8
Nest             9
Boss            special resolution
```

## 27.3 Encounters

Minimum 20:

### Survivor

```text
event_knocking_door
event_locked_office
event_child_signal
event_injured_worker
event_security_room_survivor
```

### Resource

```text
event_battery_room
event_abandoned_cart
event_maintenance_cache
event_emergency_generator
event_broken_vending_area
```

### Enemy

```text
event_crawler_attack
event_worker_attack
event_stalker_hallway
event_brute_door
event_nest_blockage
```

### Mystery/Hazard

```text
event_strange_radio
event_flickering_floor
event_bloodless_room
event_power_overload
event_jammed_door
```

### Mandatory

```text
checkpoint_floor_10
checkpoint_floor_20
boss_floor_30
```

---

# 28. Recommended Project Structure

```text
Assets/
  _Game/
    Art/
      Characters/
        Survivors/
        Enemies/
      Environment/
      UI/
      FX/
    Audio/
      Music/
      SFX/
    Data/
      Balance/
      Encounters/
      Enemies/
      Survivors/
      Upgrades/
    Prefabs/
      Gameplay/
      UI/
    Scenes/
      00_Bootstrap.unity
      01_MainMenu.unity
      02_Game.unity
    Scripts/
      Core/
        Bootstrap/
        State/
        Random/
        Save/
      Data/
        Definitions/
        Runtime/
      Gameplay/
        Run/
        Floor/
        Encounters/
        Survivors/
        Combat/
        Upgrades/
      UI/
        Common/
        MainMenu/
        HUD/
        FloorChoice/
        Encounter/
        Roster/
        Upgrade/
        RunEnd/
      Audio/
      Haptics/
      Monetization/
      Debug/
    Tests/
      EditMode/
      PlayMode/
```

---

# 29. Naming Conventions

## C#

```text
PascalCase: classes, methods, properties
camelCase: private fields/local variables
_ prefix: private serialized fields
IThing: interface
ThingDefinition: static ScriptableObject content
ThingState: runtime mutable data
ThingView: MonoBehaviour/UI component
ThingViewModel: readonly presentation data
ThingSystem: domain logic
```

## ScriptableObject IDs

Lower snake case:

```text
event_knocking_door
upgrade_extra_seat
enemy_crawler
```

IDs must never change after release without save migration.

---

# 30. Code Quality Rules for Agent

1. Prefer plain C# for gameplay rules.
2. MonoBehaviours should mostly bridge Unity lifecycle/UI.
3. No static mutable global gameplay state.
4. No `FindObjectOfType` in normal runtime flow.
5. No magic gameplay constants inside UI scripts.
6. No direct SO mutation during a run.
7. All randomness goes through `IRandomService`.
8. All encounter effects go through `EncounterResolver`.
9. All run phase changes go through `RunController`.
10. Add tests for pure rules before adding animation polish.
11. Avoid generic frameworks unless a second concrete use case exists.
12. Do not introduce dependency injection frameworks for MVP.
13. Do not use Addressables unless asset volume proves it necessary.
14. Prefer serialized inspector references over runtime scene searches.
15. Never block the main loop on unnecessary disk I/O.

---

# 31. Testing Strategy

## Edit Mode tests

Must cover:

### Travel

```text
distance 1 costs correct Energy
distance 3 costs correct Energy
Efficient Motor modifier applies
cost never below minimum
```

### Combat

```text
success chance clamps to min/max
team power sums correctly
seed gives deterministic result
```

### Roster

```text
cannot exceed capacity
replacement removes correct survivor
passive modifies rules correctly
```

### Encounter

```text
condition blocks invalid choice
effects mutate expected fields
invalid effect logs/fails safely
```

### Floor generation

```text
candidate floors increase
checkpoint cannot be skipped
floor 30 always boss
encounters respect min/max floor
same seed creates same sequence
```

### Save

```text
default created if missing
valid save round-trip
corrupt save recovery
schema version field preserved
```

## Play Mode tests

Minimum:

```text
Start new run
Choose floor
Open encounter
Resolve encounter
HUD updates
Run can die
Run can win
Restart works
Pause/resume works
```

---

# 32. Debug Tools

Create a development-only debug panel.

Buttons:

```text
+10 Energy
-10 Energy
+10 Integrity
-10 Integrity
+10 Scrap
Add Random Survivor
Jump to Floor 9
Jump to Floor 19
Jump to Floor 29
Force Enemy Encounter
Force Survivor Encounter
Force Boss
Win Run
Kill Run
Copy Seed
```

This will save hours.

Compile behind:

```csharp
#if UNITY_EDITOR || DEVELOPMENT_BUILD
```

---

# 33. 30-Day Execution Plan

The plan assumes one focused solo developer.

A day is considered complete only when its Definition of Done passes.

---

## Day 1 — Project Skeleton

Tasks:

- create Unity project,
- configure portrait Android orientation,
- create three scenes,
- establish folder structure,
- create `Bootstrap`,
- create `RunState`,
- create `PlayerSaveData`,
- create `BalanceConfig`,
- add basic Git ignore/settings,
- create empty `RunController`.

Definition of Done:

- Android target selected,
- app boots to MainMenu,
- Play loads Game scene,
- no console errors.

Agent task ID:

```text
M1-D01
```

---

## Day 2 — Run State + Rules

Implement:

- `RunState`,
- `RunRules`,
- Energy,
- Integrity,
- Capacity,
- Scrap,
- travel cost,
- run death rules,
- Edit Mode tests.

Definition of Done:

- a pure C# test can create a run and simulate resource loss,
- death rules pass tests,
- no gameplay rule depends on UI.

Task:

```text
M1-D02
```

---

## Day 3 — Seeded RNG + Floor Candidates

Implement:

- `IRandomService`,
- seeded implementation,
- `FloorCandidate`,
- `FloorGenerator`,
- +1/+2/+3 candidate generation,
- mandatory floors,
- early/mid/late bands.

Definition of Done:

- same seed = same floor/encounter sequence,
- floor 10/20/30 cannot be skipped,
- tests pass.

Task:

```text
M1-D03
```

---

## Day 4 — Minimal Gameplay UI

Implement:

- HUD,
- floor choice cards,
- choose floor command,
- resource display,
- simple travel transition.

Use placeholder rectangles/text.

Definition of Done:

- from Floor 1 player can tap through to Floor 30,
- Energy decreases correctly,
- no event system yet required.

Task:

```text
M1-D04
```

---

## Day 5 — Encounter Data + Resolver

Implement:

- `EncounterDefinition`,
- conditions,
- effects,
- `EncounterResolver`,
- encounter UI,
- 4 placeholder encounters.

Definition of Done:

- floor choice leads to encounter,
- choice resolves,
- resources change,
- player returns to floor choice.

Task:

```text
M1-D05
```

---

## Day 6 — Survivors + Capacity Conflict

Implement:

- `SurvivorDefinition`,
- runtime roster IDs,
- roster calculation,
- survivor encounter effect,
- full-capacity replacement UI.

Create 4 placeholder survivor definitions.

Definition of Done:

- player can recruit survivor,
- full roster forces replace/refuse screen,
- no roster overflow possible.

Task:

```text
M1-D06
```

---

## Day 7 — Combat

Implement:

- `EnemyDefinition`,
- team power,
- combat chance,
- Fight,
- Escape,
- Seal Door,
- deterministic combat roll.

Create 2 placeholder enemies.

Definition of Done:

- enemy encounter can be resolved three ways,
- costs/results are readable before choosing,
- combat tests pass.

Task:

```text
M1-D07
```

---

## Day 8 — Run End + Restart

Implement:

- Energy dead-end failure,
- Integrity death,
- Floor 30 temporary win,
- run end panel,
- stats summary,
- restart.

Definition of Done:

- full run can start and end,
- restart requires <=2 taps,
- no scene-state contamination between runs.

Task:

```text
M1-D08
```

### MILESTONE A

At end of Day 8:

> A full run must be playable with placeholder UI.

If not, stop and fix before any art.

---

## Day 9 — Upgrade System

Implement:

- `UpgradeDefinition`,
- purchase validation,
- 5 MVP upgrades,
- upgrade selection panel.

Definition of Done:

- Scrap is spendable,
- each upgrade has visible gameplay impact,
- capacity upgrade works.

Task:

```text
M1-D09
```

---

## Day 10 — Encounter Authoring Pipeline

Create 10 total encounters.

Ensure:

- clue text differs from exact event text,
- choices have known costs,
- no impossible choice-only events,
- event weights work.

Add validation tool or editor checks for:

```text
duplicate IDs
empty titles
no choices
invalid floor ranges
```

Task:

```text
M1-D10
```

---

## Day 11 — Checkpoints

Implement Floor 10 and Floor 20 encounters.

Checkpoint goals:

- pause normal rhythm,
- offer repair/upgrade/resource decision,
- increase perceived run structure.

Definition of Done:

- both checkpoints always occur,
- checkpoint cannot repeat,
- checkpoint outcome affects run.

Task:

```text
M1-D11
```

---

## Day 12 — Boss

Implement Floor 30 boss as a multi-choice scripted encounter.

Do not build a new battle engine.

Example boss phases:

```text
1. Door is trapped
2. Decide spend Energy / Integrity / Team Power
3. Final risk check
```

Definition of Done:

- boss has at least 2 viable strategic paths,
- boss can win/lose run,
- floor 30 always resolves to final state.

Task:

```text
M1-D12
```

---

## Day 13 — Core Content Complete

Reach:

- 20 encounters,
- 10 survivors,
- 5 enemies,
- 5 upgrades,
- 1 boss.

No final art required.

Definition of Done:

- no obvious duplicate encounter two floors in a row,
- at least 3 distinct run stories occur across 5 seeds.

Task:

```text
M1-D13
```

---

## Day 14 — First Balance Pass

Play at least 10 seeded runs.

Record:

- death floor,
- duration,
- Energy,
- Integrity,
- roster size,
- choices.

Adjust only:

- start resources,
- event weights,
- enemy power,
- costs,
- rewards.

Do not add features to solve balance problems.

Definition of Done:

- median test death floor is roughly 15–23,
- at least one run reaches boss,
- no common deterministic unwinnable pattern.

Task:

```text
M1-D14
```

### MILESTONE B — CORE FUN GATE

Ask:

1. Do floor choices feel meaningful?
2. Does capacity ever create hesitation?
3. Do players understand risk before choosing?
4. Is retry immediate?
5. Did tester voluntarily start another run?

If answers 1–4 are not mostly yes, do not proceed to final art.

---

## Day 15 — Save System

Implement:

- player save,
- settings,
- best floor,
- run stats,
- active run save,
- corrupt save fallback.

Definition of Done:

- force-kill app mid-run,
- reopen,
- Continue restores valid state.

Task:

```text
M1-D15
```

---

## Day 16 — UI Pass 1

Replace debug layout with final structure.

Focus:

- thumb reach,
- readability,
- no scroll,
- consistent resource icons,
- button states.

Definition of Done:

- full run playable one-handed,
- no essential tap target near top corners,
- choices fit common portrait aspect ratios.

Task:

```text
M1-D16
```

---

## Day 17 — Elevator Juice

Add:

- door open/close,
- travel shake,
- floor number transition,
- lights flicker,
- damage overlay.

Definition of Done:

- travel feels responsive,
- animations do not block input longer than intended,
- skip/debug animation option available in development.

Task:

```text
M1-D17
```

---

## Day 18 — Audio + Haptics

Implement audio service.

Add:

- button,
- travel,
- door,
- damage,
- reward,
- survivor,
- fail/win.

Definition of Done:

- settings control volume,
- haptics toggle works,
- no overlapping audio spam from fast taps.

Task:

```text
M1-D18
```

---

## Day 19 — Art: Elevator + UI

Produce:

- elevator interior,
- closed/open doors,
- UI frame,
- primary icons,
- floor cards,
- encounter panel frame.

Definition of Done:

- no gameplay screen depends on placeholder rectangles.

Task:

```text
M1-D19
```

---

## Day 20 — Art: Survivors

Produce 10 survivor portraits in consistent dark-cute style.

Prioritize:

- silhouette,
- expression,
- role readability.

Definition of Done:

- roster can be understood at glance,
- portraits remain readable at actual phone size.

Task:

```text
M1-D20
```

---

## Day 21 — Art: Enemies + Boss

Produce:

- 5 enemy images,
- boss image,
- damage/hazard overlays.

Definition of Done:

- enemy threat visually escalates,
- boss feels distinct without requiring a new animation system.

Task:

```text
M1-D21
```

---

## Day 22 — Encounter Art + Copy Polish

Add art/icon treatment for 20 encounters.

Rewrite encounter text so each event is:

- 1 short title,
- max ~3 short sentences,
- choices understandable without rereading.

Definition of Done:

- no encounter requires scrolling on target device.

Task:

```text
M1-D22
```

---

## Day 23 — Tutorial

Build tutorial through first run, not a separate tutorial scene.

Teach only:

1. choose floor,
2. resources,
3. encounter choice,
4. survivor capacity,
5. fight risk.

Definition of Done:

- tutorial can be completed in <3 min,
- returning player can skip,
- no 10-screen slideshow.

Task:

```text
M1-D23
```

---

## Day 24 — Debug + Telemetry

Implement:

- dev debug panel,
- local run telemetry,
- JSON export,
- seed display/copy.

Definition of Done:

- a balancing run can be reproduced from seed,
- telemetry file records required metrics.

Task:

```text
M1-D24
```

---

## Day 25 — Balance Pass 2

Run 20+ games, mix manual and accelerated debug testing.

Tune:

- event weights,
- energy curve,
- integrity damage,
- survivor power,
- Scrap economy,
- upgrade cost,
- boss difficulty.

Definition of Done target:

```text
Run 8–15 min
Median death 16–22
Experienced win 20–35%
Roster replacements 2–4/run
```

Task:

```text
M1-D25
```

---

## Day 26 — Android Device QA

Test on at least 2 physical Android devices if available.

Check:

- safe area,
- navigation bar,
- background/resume,
- incoming call/app switch,
- audio focus,
- portrait lock,
- different resolutions,
- touch latency.

Definition of Done:

- no blocker device issue.

Task:

```text
M1-D26
```

---

## Day 27 — Performance + Build Hygiene

Even though low-end is not a target, remove obvious waste.

Check:

- sprite sizes,
- texture compression,
- GC allocations during repeated floor loop,
- duplicated audio,
- unnecessary Update methods,
- console warnings,
- development-only code.

Definition of Done:

- no recurring GC spikes from normal UI decisions,
- no console errors,
- stable frame pacing on target device.

Task:

```text
M1-D27
```

---

## Day 28 — Rewarded Ads Integration

Only now add monetization plumbing.

Implement:

- `IAdsService`,
- `RewardedAdResult`,
- `NullAdsService`,
- `RewardedAdsController`,
- `IReviveOfferCopyProvider`,
- dynamic revive copy by floor band,
- rewarded revive UI,
- once-per-run revive state,
- local monetization telemetry,
- real rewarded-ad provider adapter if release stability allows it.

MVP1 monetization scope is **rewarded revive only**.

Do not implement:

```text
banner ads
interstitial ads
app-open ads
end-of-run reward multiplier
IAP
remove-ads purchase
```

The end-of-run rewarded bonus stays disabled until MVP2 has legitimate meta currency.

Definition of Done:

- player can finish the whole game with ads disabled,
- no ad starts without an explicit rewarded-ad tap,
- Floors 1–14 do not aggressively surface revive offers,
- Floor 27–29 copy truthfully states the exact floors remaining,
- completed rewarded revive resumes the same run safely,
- skipped/failed/unavailable ads never consume the revive opportunity,
- normal `END RUN` works immediately without additional prompts,
- gameplay works with ads service unavailable,
- no soft lock,
- no forced, banner, or interstitial ad exists anywhere in the build.

Task:

```text
M1-D28
```

---

## Day 29 — Release Candidate QA

Run checklist:

```text
Fresh install
Start run
Kill app mid-run
Resume run
Lose by Integrity
Lose by Energy
Revive success
Revive unavailable
Boss win
Boss loss
Restart
Settings save
Audio toggle
Haptics toggle
Airplane mode
Repeated fast taps
Back button
Pause/resume
Corrupt save fallback
```

Fix only blockers/high severity issues.

Task:

```text
M1-D29
```

---

## Day 30 — Android Release Build

Tasks:

- final production build,
- versioning,
- signing,
- final smoke test,
- capture screenshots,
- record short gameplay clip,
- archive build + seed test list + balance config.

Definition of Done:

- installable signed build,
- no development menu,
- no test ad behavior unless intentionally configured,
- full run verified from fresh install.

Task:

```text
M1-D30
```

---

# 34. Agent Execution Order

An agent should execute tasks strictly in dependency order:

```text
D01
 ↓
D02
 ↓
D03
 ↓
D04
 ↓
D05
 ↓
D06
 ↓
D07
 ↓
D08
 ↓
D09–D13
 ↓
D14 Core Fun Gate
 ↓
D15–D24
 ↓
D25 Balance Gate
 ↓
D26–D30
```

Do not parallelize foundational systems before interfaces/data ownership are stable.

Art tasks may be parallelized later only if another agent exists.

---

# 35. Agent Task Template

Use this prompt structure when handing a task to an implementation agent.

```text
PROJECT:
Last Elevator, Unity/C#, Android portrait mobile game.

TASK:
<M1-DXX task name>

GOAL:
<single concrete result>

IN SCOPE:
- ...
- ...

OUT OF SCOPE:
- any MVP2 system
- unrelated refactors
- package changes unless required

ARCHITECTURE CONSTRAINTS:
- gameplay state mutated only through RunController/domain systems
- randomness through IRandomService
- ScriptableObjects are static definitions only
- no global mutable state
- add tests for pure gameplay rules

ACCEPTANCE CRITERIA:
1. ...
2. ...
3. ...

DELIVERABLES:
- source files changed
- tests added/updated
- short implementation note
- known limitations

DO NOT:
- add speculative abstractions
- redesign unrelated UI
- add monetization early
- add features not required by acceptance criteria
```

---

# 36. Pull Request / Commit Checklist

Before accepting agent work:

```text
[ ] Project compiles
[ ] No new console errors
[ ] Acceptance criteria pass
[ ] Tests pass
[ ] No ScriptableObject runtime mutation
[ ] No hardcoded asset paths unless intentional
[ ] No unrelated feature additions
[ ] No duplicated gameplay constants
[ ] UI does not own gameplay state
[ ] Seeded randomness remains deterministic
[ ] Save schema changes documented
```

---

# 37. Core Fun Test Script

Use this with testers after Day 14.

Do not explain strategy.

Ask tester to play one run.

Observe:

```text
Where do they hesitate?
Do they understand floor signals?
Do they notice energy cost?
Do they care who joins the elevator?
Do they replace anyone?
Do they understand fight odds?
Do they feel cheated by RNG?
Do they restart after failure?
```

After play:

1. What was the hardest decision?
2. Did you ever feel a floor choice was obvious?
3. Which passenger did you care about?
4. Did any loss feel unfair?
5. Would you immediately play another run?
6. What do you think the game is “about”?

Success signal:

The answer to #6 should resemble:

> “Choosing who/what to risk with limited resources.”

If they answer only:

> “Going up an elevator and fighting monsters,”

the decision layer is not strong enough.

---

# 38. Balance Failure Diagnosis

## Players always choose nearest floor

Possible causes:

- distance energy cost too punishing,
- clues not valuable enough,
- far floor reward expectation unclear.

Do not solve by random bonuses first.

## Players never fight

Possible causes:

- fight reward too low,
- failure punishment too high,
- win chance unreadable.

## Players always fight

Possible causes:

- escape/seal costs too high,
- team power scaling too strong,
- combat reward too generous.

## Capacity never matters

Possible causes:

- too few survivor encounters,
- starting capacity too high,
- survivor differences too weak.

First fixes:

```text
Start capacity 4
Increase survivor encounter rate early
Make roles/power readable
```

## Runs end only from Energy

Integrity is irrelevant.

Increase:

- hazards,
- enemy pressure,
- meaningful Seal Door use.

## Runs end only from Integrity

Route resource game is irrelevant.

Reduce unavoidable damage and increase Energy tension.

---

# 39. Risk Register

## Risk 1 — Core becomes a text menu game

Mitigation:

- strong elevator travel feedback,
- door animation,
- art/audio changes,
- short encounter copy,
- frequent visual roster presence.

## Risk 2 — RNG feels unfair

Mitigation:

- show fight percentage,
- always offer a deterministic alternative where practical,
- seeded runs,
- cap chances,
- avoid hidden instant death.

## Risk 3 — Solo art production explodes

Mitigation:

- portraits, not full-body animation,
- reuse backgrounds,
- 2D dark-cute style,
- icon-driven encounters,
- no skeletal animation requirement.

## Risk 4 — Too many systems before fun

Mitigation:

- Day 14 Core Fun Gate,
- no MVP2 before pass.

## Risk 5 — Agent over-engineers architecture

Mitigation:

- interfaces only for actual boundaries,
- no DI framework,
- no generic buff engine,
- no custom ECS,
- no Addressables by default.

---

# 40. First 90 Minutes — Start Here

If implementation starts immediately, do this in order.

## 0–15 min

Create project and Git repository.

Set:

```text
Portrait orientation
Android target
60 FPS target
```

## 15–35 min

Create directories and scenes.

Add:

```text
00_Bootstrap
01_MainMenu
02_Game
```

## 35–60 min

Implement:

```text
RunState
BalanceConfig
RunRules
```

Add first tests:

```text
Travel cost
Integrity death
Capacity
```

## 60–90 min

Create a temporary Game scene with:

```text
Floor label
Energy label
Integrity label
Three floor buttons
```

Wire buttons to a temporary `RunController`.

Goal at 90 minutes:

> Tap a floor button → current floor changes → Energy decreases.

No art.

---

# 41. MVP1 Definition of Done

MVP1 is complete only when all are true.

## Core

```text
[ ] Start-to-end run works
[ ] 30 floors
[ ] Floor choice matters
[ ] Energy works
[ ] Integrity works
[ ] Capacity works
[ ] Scrap works
[ ] Survivors work
[ ] Replacement works
[ ] Enemy choices work
[ ] Upgrades work
[ ] Boss works
[ ] Win/loss works
```

## Content

```text
[ ] 20 encounters
[ ] 10 survivors
[ ] 5 enemies
[ ] 5 upgrades
[ ] 1 boss
```

## Technical

```text
[ ] Seeded RNG
[ ] Local save
[ ] Active run resume
[ ] Core EditMode tests
[ ] No console errors
[ ] Android signed build
```

## UX

```text
[ ] One-hand playable
[ ] Encounter text does not scroll normally
[ ] Restart <=2 taps
[ ] Resource changes readable
[ ] Fight risk readable
[ ] Full roster decision readable
```

## Validation

```text
[ ] At least 20 balance runs logged
[ ] Run target ~8–15 min
[ ] No common unwinnable generation pattern
[ ] Tester can explain core decision loop
[ ] At least some testers voluntarily retry
```

---

# 42. MVP2 Entry Criteria

Only start MVP2 if:

```text
Core loop is understandable
Route choice creates real tradeoffs
Capacity creates real tradeoffs
Combat alternatives feel fair
Runs are short enough to replay
No major save/state bugs
```

Then implement MVP2 in this order:

```text
1. Survivor classes
2. Traits
3. Infection
4. Authored relationships
5. Meta currency
6. Tech tree
7. Collection
8. Event chains
9. Alternate endings
10. More content
```

Do not start with the tech tree.

The character decisions should deepen before permanent stat progression.

---

# 43. Suggested MVP2 Milestones

## M2-A — Survivor Identity

- 6 classes,
- 6 traits,
- updated roster UI,
- class-dependent encounter options.

## M2-B — Infection

- Suspicious/Clean/Infected,
- medical checks,
- delayed consequences,
- 8–10 infection events.

## M2-C — Relationships

- 4 relationship types,
- authored pairs only,
- leave-behind consequences.

## M2-D — Meta

- Tech Parts,
- 10 tech nodes,
- unlocks,
- collection screen.

## M2-E — Content Expansion

Target:

```text
40–60 encounters
20–30 survivors
8–10 enemy types
3 bosses/final variants
3 endings
```

---

# 44. Product Principle for Future Features

Before adding any feature, ask:

> Does this create a harder decision during the run?

Strong additions:

- hidden infection,
- relationships,
- survivor-specific event options,
- conflicting clues,
- risky shortcuts.

Weak additions:

- giant inventory,
- cosmetic crafting,
- complicated equipment stats,
- passive idle rewards,
- large menus that do not affect run decisions.

The game should grow **deeper**, not merely wider.

---

# 45. Final Build Philosophy

The first version succeeds if players say:

> “I knew saving that person was a bad idea, but I did it anyway.”

It does not need:

- huge content volume,
- complex animations,
- dozens of systems.

The core product is the sequence:

```text
Information
→ Scarcity
→ Decision
→ Consequence
→ Regret / Relief
→ Next Decision
```

Everything in the implementation should make that sequence faster, clearer, and more emotionally meaningful.
