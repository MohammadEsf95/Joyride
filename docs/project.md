# Joyride — Project Overview

## Game Overview

**Joyride** is a 2D side-scrolling endless runner inspired by games like *Jetpack Joyride*. The player controls a character that flies vertically using thrust while the world scrolls horizontally at an ever-increasing speed. The goal is to survive as long as possible, collect coins for score, and avoid obstacles and enemy fire.

The game is built as a single-scene prototype (`SampleScene`) with procedurally spawned content ahead of the camera and recycled ground/ceiling tiles behind the player.

## Gameplay Loop

```
Start run
    │
    ▼
Camera auto-scrolls forward (SampleScene: startSpeed 10 → maxSpeed 100)
    │
    ▼
Player holds Jump to thrust upward; releases to fall (enhanced gravity)
    │
    ├── Collect coins → score increases (GameManager)
    ├── Collect shield → timed invulnerability (PlayerShield)
    ├── Hit obstacle (no shield) → Game Over (time frozen)
    ├── Hit obstacle (with shield) → shield consumed, obstacle destroyed
    └── Hit enemy bullet (no shield) → Game Over
    │
    ▼
Game Over → press any key → scene reload
```

**Movement model:** The player is parented to `PlayerCamera`, so the player stays roughly fixed on screen while the camera moves. World objects spawn ahead of the camera and despawn behind it. Difficulty increases implicitly as `PlayerCamera` acceleration raises scroll speed over time.

## Current Implemented Features

| Feature | Status | Notes |
|---------|--------|-------|
| Player thrust / gravity flight | ✅ Complete | `Player.cs` — Rigidbody2D forces, velocity clamping |
| Auto-accelerating camera scroll | ✅ Complete | `PlayerCamera.cs` |
| Infinite ground/ceiling recycling | ✅ Complete | `MapGenerator.cs` — two-segment swap at 60-unit intervals |
| Coordinated world spawning | ✅ Complete | `SpawnCoordinator` + `ISpawner` implementors |
| Obstacle spawning & collision | ✅ Complete | Random height pillars; collision ends run or consumes shield |
| Coin spawning (4 patterns) | ✅ Complete | Horizontal, vertical, curve, circle patterns |
| Coin collection & score UI | ✅ Complete | TMP score display via `GameManager` |
| Shield power-up | ✅ Complete | Timed shield with visual, flash-before-expire |
| Shield vs obstacles & bullets | ✅ Complete | Consumes shield instead of game over |
| Shooter enemy | ⚠️ Partial | `EnemySpawner` in scene; `enemyPrefab` unassigned so `CreateSample()` runs. Prefab has broken move values. |
| Game over & restart | ⚠️ Minimal | Freezes time; no game-over UI; any key reloads scene |
| Audio on coin collect | ⚠️ Hook only | `AudioClip` field exists; not assigned in prefab |

## Technology Stack

| Layer | Choice |
|-------|--------|
| Engine | Unity **2022.1.24f1** |
| Language | C# (MonoBehaviour scripts, no assemblies/namespaces) |
| Rendering | 2D (SpriteRenderer, orthographic camera) |
| Physics | Unity Physics2D (Rigidbody2D, BoxCollider2D, CircleCollider2D) |
| UI | TextMeshPro (`TMP_Text`) + uGUI Canvas |
| Input | Legacy Input Manager (`Input.GetButton("Jump")`) |
| Packages | `com.unity.feature.2d`, TextMeshPro 3.0.6, Timeline, Visual Scripting (unused in scripts), Test Framework (no tests written) |
| Version control | Git (2 commits on `main`) |

**Project structure (scripts):**

```
Assets/Scripts/
├── Core/           GameManager
├── Player/         Player, PlayerShield, PlayerCamera
├── World/          SpawnCoordinator, MapGenerator, ISpawner
├── Obstacles/      Obstacle, ObstacleSpawner
├── Collectibles/   Coin, CoinSpawner, Shield, ShieldSpawner
└── Enemy/          ShooterEnemy, EnemyBullet, EnemySpawner
```

**Prefabs:** `Coin`, `StaticObstacle`, `ShieldCollectible`, `ShooterEnemy`

## Current Progress

The project is an **early playable prototype** (~16 gameplay scripts, single scene):

- **Core loop works:** fly, dodge obstacles, collect coins/shields, die, restart.
- **Spawning pipeline is established:** coordinator pattern with four spawner types (all wired in scene).
- **Enemy system is partially integrated** (commit `8243a5c`) — spawner in scene, procedural enemy fallback active; prefab not assigned.
- **No polish layer:** no menus, no persistence, no VFX pipeline, no audio mix, no difficulty curves beyond camera acceleration.
- **No automated tests** despite Test Framework being installed.

Estimated maturity: **vertical slice / proof-of-concept** — enough to play and iterate on systems, not yet a shippable product.

## Future Extension Points

The codebase naturally supports extension at these boundaries:

1. **`ISpawner` interface** — add new spawnable content (power-ups, enemy types, environmental hazards) by implementing `CanSpawn()` / `Spawn(float x)`.
2. **`GameManager`** — central hook for score, lives, high scores, pause, and game-state transitions.
3. **`PlayerShield`** — template for timed buffs; could generalize to a buff/effect component.
4. **`ShooterEnemy` coroutine pattern** — reusable for scripted enemy behaviors (move → attack → exit).
5. **`MapGenerator` segment swap** — can evolve into chunk-based level data or biome switching.
6. **`PlayerCamera` speed curve** — difficulty tuning without touching spawn logic.
7. **Prefab + `CreateSample()` dual paths** — enemies and bullets support runtime creation for rapid prototyping before art assets exist.

Recommended integration order for growth: assign/fix enemy prefab → add game-over UI → object pooling → event decoupling → difficulty/progression systems.
