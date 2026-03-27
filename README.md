# Dungeon Run Prototype

A Unity action-roguelite prototype built around run-based progression, scalable enemy spawning, weapon-based auto attacks, boss reset flow, and persistent player upgrades.

This project includes:
- Run-based XP and local level progression
- Persistent account-style upgrades
- Auto-attacking weapons
- Enemy health and XP rewards
- Boss and player death reset flow
- Weapon spawning and attachment
- Enemy pooling and scalable spawning
- Centralized gameplay value tuning

---

## Overview

This prototype is designed around repeated dungeon runs where the player gains temporary run progress and permanent account progress at the same time.

The gameplay loop works like this:
- Enter a dungeon run
- Fight enemies and gain XP
- Increase local run level
- Convert local level gains into long-term player progression
- Spend earned permanent level points on stat upgrades
- Fight until death or boss completion
- Reset the run and start again

Weapons attack automatically depending on the selected weapon, while enemies are spawned dynamically around the player and scale with local run level.

---

## Main Systems

## 1. Persistent Game Progression
`GameManager.cs`

Controls permanent progression and upgrade allocation.

Handles:
- Persistent player level storage
- Free stat point calculation
- HP upgrades
- Speed upgrades
- Parry upgrades
- Selected weapon storage
- Save/load using `PlayerPrefs`

Tracked progression:
- `currentLevel`
- `hpLevel`
- `speedLevel`
- `parryLevel`
- `localLevelProgress`

Derived stats:
- `CurrentMaxHP`
- `CurrentMoveSpeed`
- `CurrentParry`

Weapon options:
- None
- Sword
- Spear
- Bow
- Amulet

This is the main long-term progression system for the project.

---

## 2. Run XP and Local Level System
`XPHandler.cs`

Controls temporary run progression.

Handles:
- XP gain from enemy deaths
- Local level increases during a run
- XP requirement scaling
- UI updates for XP and local level
- Run reset after death or boss completion

Progression behavior:
- Killing enemies grants XP
- XP increases local run level
- Local level thresholds scale with:
  - current local level
  - permanent player level

When local levels are gained:
- `OnLocalLevelChanged` is broadcast
- `GameManager.OnLocalLevelGained()` is called

This connects run progress to permanent progress.

---

## 3. Central Gameplay Values
`ValueHandler.cs`

Central tuning singleton for gameplay variables.

Handles:
- Enemy XP value
- XP curve values
- Parry settings
- Bow timing and damage
- Sword timing, range, and damage
- Spear timing, range, and damage
- Amulet timing, radius, and damage
- Enemy projectile speed
- Boss projectile damage and speed

This acts like a lightweight balancing/configuration hub for combat and progression values.

---

## 4. Player Health and Parry
`PlayerHP.cs`

Controls player survivability and parry logic.

Handles:
- Current and max health
- HP UI
- Parry charge count
- Parry activation window
- Parry recharge
- Applying permanent stat upgrades from `GameManager`
- Death handling

Features:
- Reads max HP from permanent progression
- Updates health slider and text
- Supports multiple parry charges
- Uses timed active and recharge windows
- Calls `RunSceneManager` on death

Parry behavior:
- Triggered with Space
- Consumes one charge
- Activates a parry object temporarily
- Recharges charges over time

---

## 5. Enemy Health and XP Rewards
`EnemyHealthXP.cs`

Controls enemy health, death, XP reward, and culling.

Handles:
- Enemy HP
- XP reward value
- Damage and instant kill
- Death event broadcasting
- Distance-based culling
- Player lookup for cull distance checks

Key behavior:
- On death, broadcasts `OnEnemyDied`
- On death, grants XP indirectly through subscribed systems
- If too far from the player, the enemy is disabled without granting XP

This is the core enemy lifecycle script.

---

## 6. Run Flow / Scene Reset
`RunSceneManager.cs`

Controls what happens when the player dies or the boss is defeated.

Handles:
- Resetting run XP
- Unlocking cursor on death
- Reloading scenes
- Detecting boss death through enemy death events

Flow:
- Player death:
  - reset run XP
  - restore time scale
  - unlock cursor
  - load scene 0
- Boss death:
  - reset run XP
  - restore time scale
  - reload current scene

This script controls run reset and completion behavior.

---

## 7. Dungeon Spawning System
`Spawner.cs`

Handles spawning for player, boss, enemies, and equipped weapons.

Handles:
- Delayed startup until dungeon generation is ready
- Player spawn in smallest room
- Boss spawn in largest room
- Weapon attachment to player
- Enemy object pooling
- Active enemy count scaling with local level
- Random enemy spawning around the player

Features:
- Separate pools for two enemy types
- Weapon prefab selection from `GameManager`
- Local-level-based enemy count scaling
- Random spawn position validation using raycasts to ground

This is the central runtime spawning system for the dungeon.

---

## 8. Sword / Spear Auto Attack
`SwordAttack.cs`

Controls melee auto attacks for sword and spear weapons.

Handles:
- Swing animation between start and end transforms
- Repeating attack timing
- Forward raycast hit detection
- Per-swing hit tracking to avoid duplicate hits
- Debug rays and gizmos

Weapon modes:
- Sword
- Spear

Each mode reads values from `ValueHandler`:
- swing interval
- damage
- range

This is the auto melee combat system.

---

## 9. Amulet Auto Pulse Attack
`AmuletAttack.cs`

Controls area-of-effect pulse damage around the player.

Handles:
- Repeating pulse timing
- Pulse active window
- Overlap sphere enemy detection
- Per-pulse hit tracking
- Radius gizmo drawing

Reads values from `ValueHandler`:
- pulse interval
- pulse damage
- pulse radius

During each pulse:
- nearby enemies inside the radius are found
- each valid enemy is damaged once per pulse

This is the auto AoE weapon system.

---

## 10. Upgrade UI
`UpradeHandler.cs`

Controls the permanent upgrade menu UI.

Handles:
- Displaying total permanent level
- Displaying free level points
- Updating upgrade visuals
- Calling `GameManager` upgrade methods
- Resetting all upgrades

Upgrade categories:
- HP
- Speed
- Parry

Visual behavior:
- Activates level image objects up to the current upgrade level

This is the main front-end for permanent stat upgrades.

---

## Core Gameplay Loop

1. Player enters a dungeon run
2. `Spawner` creates player, boss, and enemy pools
3. Equipped weapon is attached to the player
4. Weapon attacks run automatically
5. Enemy deaths trigger XP rewards
6. `XPHandler` increases local level
7. Local level increases can contribute to permanent progression through `GameManager`
8. Permanent points can be spent on HP, speed, and parry
9. If the player dies, the run resets
10. If the boss dies, the run also resets

---

## System Relationships

### Progression flow
- `EnemyHealthXP` broadcasts enemy death
- `XPHandler` listens and grants XP
- `XPHandler` increases local run level
- `GameManager` receives local level gains
- Permanent player level increases over time
- `UpradeHandler` spends permanent level points on stats

### Combat flow
- Selected weapon is stored in `GameManager`
- `Spawner` attaches the matching weapon prefab to the player
- `SwordAttack` or `AmuletAttack` handles automatic damage
- `EnemyHealthXP` receives damage and dies when HP reaches zero

### Run reset flow
- `PlayerHP` calls `RunSceneManager.OnPlayerDied()`
- `RunSceneManager` resets run XP and reloads scenes
- Boss death is also detected through `EnemyHealthXP.OnEnemyDied`

---

## Weapon Systems

## Sword
- Auto swing weapon
- Uses raycast hit detection
- Uses sword damage/range/interval from `ValueHandler`

## Spear
- Uses the same `SwordAttack` system with different values
- Longer or different reach depending on configuration

## Amulet
- Pulses damage around the player
- Uses overlap sphere checks
- Damages each enemy once per pulse window

## Bow
- Weapon type exists in progression/spawning
- Value support exists in `ValueHandler`
- Actual bow attack script was not included here

---

## Progression Design

This project uses two progression layers.

### 1. Local run progression
Temporary for the current run:
- XP
- local level

Resets on:
- player death
- boss defeat

### 2. Permanent player progression
Persistent across runs:
- total player level
- HP upgrades
- speed upgrades
- parry upgrades

Saved with:
- `PlayerPrefs`

This creates a roguelite structure where short-term and long-term progression both matter.

---

## Enemy Spawning Design

Enemies are managed through object pools.

Spawner behavior:
- keeps a minimum number of each enemy type active
- increases target count as local level increases
- spawns around the player between minimum and maximum distances
- validates spawn points with downward raycasts

Scaling rule:
- every configured number of local levels adds more enemies of both types

This supports increasing pressure as the run continues.

---

## UI Systems

Included UI support:
- HP slider
- HP text
- parry count text
- XP progress text
- local level text
- permanent level text
- free level points text
- upgrade visual images

Most UI refreshes happen automatically through:
- `GameManager.OnStatsChanged`
- `XPHandler` UI refresh methods

---

## Included Files

### Core progression
- `GameManager.cs`
- `XPHandler.cs`
- `ValueHandler.cs`
- `UpradeHandler.cs`

### Player / run systems
- `PlayerHP.cs`
- `RunSceneManager.cs`
- `Spawner.cs`

### Combat
- `SwordAttack.cs`
- `AmuletAttack.cs`

### Enemy
- `EnemyHealthXP.cs`

---

## Example Gameplay Scenario

1. The player selects a weapon
2. The dungeon run starts
3. `Spawner` creates the player in the smallest room and the boss in the largest room
4. Enemies spawn around the player
5. The selected weapon attacks automatically
6. Enemies die and give XP
7. XP increases local level
8. Local level progression contributes toward permanent player levels
9. The player spends permanent points on:
   - more HP
   - more movement speed
   - more parry charges
10. The run ends when the player dies or defeats the boss

---

## Notes

- `Spawner` depends on a `DungeonCreator` that provides room center positions.
- Boss detection in `RunSceneManager` currently checks for `BossBulletHell` in the enemy's parent hierarchy.
- Weapon attachment exists for Sword, Spear, Bow, and Amulet, but only Sword/Spear/Amulet attack scripts were included here.
- `UpradeHandler` appears to be the upgrade UI script despite the typo in the class name.
- Some values are stored in `ValueHandler`, but not every consumer script shown here reads all of them yet.

---

## Future Expansion Ideas

- Add the missing bow attack implementation
- Add enemy AI and movement systems
- Add room-specific spawning rules
- Add loot or weapon rarity
- Add procedural boss scaling
- Add run modifiers or blessings
- Add save/load for selected weapon
- Add UI for permanent stat summaries
- Add animation and VFX feedback for attacks and parries

---

## Project Status

Prototype / gameplay systems implementation.

The current project focuses on the core loop of:
run start -> combat -> XP gain -> local level scaling -> permanent upgrades -> reset on death or boss completion.
