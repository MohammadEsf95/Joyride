# Joyride — Development Backlog

Backlog derived from codebase analysis: implemented features, partial work, TODOs, placeholders, and architectural gaps.

**Sources scanned:** all 16 scripts under `Assets/Scripts/`, prefabs, `SampleScene.unity`, git history (2 commits), TODO/FIXME grep.

---

## High Priority

### 1. Assign ShooterEnemy prefab to EnemySpawner

**Rationale:** `EnemySpawner` is in `SampleScene` with ground/ceiling references, but `enemyPrefab` is unassigned. Enemies currently spawn via `ShooterEnemy.CreateSample()` (procedural fallback). Assigning the prefab is the intended path and exposes the broken prefab config (item 2).

**Tasks:**
- Assign `ShooterEnemy` prefab on the scene `EnemySpawner`.
- Fix prefab move values (item 2) before or immediately after assignment.
- Playtest spawn timing relative to existing spawners.

---

### 2. Fix ShooterEnemy prefab configuration

**Rationale:** `ShooterEnemy.prefab` serializes `moveDistance: 0` and `moveDuration: 0`. When spawned via prefab (the intended path in `EnemySpawner.Spawn`), enemies will not move vertically. Procedural `CreateSample()` uses code defaults and works — prefab path is broken.

**Tasks:**
- Set valid `moveDistance`, `moveDuration`, and `moveCurve` on prefab.
- Add/configure `bulletPrefab` reference or confirm procedural bullet fallback is acceptable.
- Add SpriteRenderer/collider to prefab or document reliance on `EnsureVisual()`.

---

### 3. Add game-over UI and feedback

**Rationale:** `GameManager.GameOver()` only sets `Time.timeScale = 0`. No visual tells the player the run ended or that any key restarts. Core UX gap for any playtest or demo.

**Tasks:**
- Game-over panel with final score.
- "Press any key to restart" prompt.
- Optional: brief delay before accepting restart input.

---

### 4. Remove ShieldSpawner debug logging

**Rationale:** `ShieldSpawner.CanSpawn()` emits `Debug.Log` on every evaluation. The coordinator polls spawners every 0.5s — this spams the console and has minor perf cost.

**Tasks:**
- Remove or gate logs behind `#if UNITY_EDITOR` / debug flag.

---

### 5. Resolve spawner priority and spawn fairness

**Rationale:** `SpawnCoordinator` spawns at most one entity per tick and uses first-match from non-deterministic `FindObjectsOfType` order. As content types grow, some spawners may starve when multiple are ready simultaneously.

**Tasks:**
- Replace discovery order with explicit prioritized list on coordinator.
- Or: weighted random selection among eligible spawners.
- Document intended spawn rates after change.

---

## Medium Priority

### 6. Introduce object pooling for high-churn entities

**Rationale:** Coins (5–10 per pattern), enemy bullets (3 per burst, multiple bursts), and obstacles all use Instantiate/Destroy. At max camera speed with enemies active, GC spikes are likely on mobile/target platforms.

**Tasks:**
- Pool coins and enemy bullets first (highest churn).
- Adapt spawner cleanup to return objects to pool instead of Destroy.
- Consider UnityEngine.Pool or lightweight custom pool.

---

### 7. Centralize vertical bounds (play area)

**Rationale:** Y limits are duplicated across spawners and differ between code defaults and scene overrides — e.g. `CoinSpawner` code `-3`/`4.5` vs scene `1`/`4`; `ObstacleSpawner` scene `3.5` max; `ShieldSpawner` scene `-2`/`3`; `EnemySpawner` reads ground/ceiling transforms (`y: -5` / `5`). MapGenerator recycles segments but bounds are not read from a single source.

**Tasks:**
- Create `PlayAreaBounds` component or ScriptableObject referencing ground/ceiling.
- Inject into spawners and enemies.

---

### 8. Scale spawn difficulty with camera speed

**Rationale:** `PlayerCamera` accelerates to `maxSpeed = 100` but spawn intervals are fixed in world time. Effective spacing shrinks as speed increases — late game may become overcrowded or empty depending on spawner cooldowns.

**Tasks:**
- Tie spawn cooldowns or `spawnDistance` to `PlayerCamera.currentSpeed`.
- Playtest obstacle/coin density at min vs max speed.

---

### 9. Decouple damage/death handling from singletons

**Rationale:** `Player`, `Coin`, and `EnemyBullet` directly call `GameManager.Instance`. Hard to test, extend (lives, invincibility frames), or add analytics.

**Tasks:**
- Introduce `IDamageHandler` / `IScoreHandler` or simple C# events on GameManager.
- Null-safe access patterns everywhere.

---

### 10. Gameplay polish: shield stacking policy

**Rationale:** Picking up a shield while one is active resets the timer (`ActivateShield`) but behavior is implicit. Obstacle hit destroys obstacle — may feel inconsistent vs bullet hit.

**Tasks:**
- Define design: refresh duration, ignore pickup, or stack charges.
- Implement consistently; add VFX/audio on activate/consume.

---

### 11. Replace procedural placeholder art

**Rationale:** `ShooterEnemy`, `EnemyBullet`, and `PlayerShield` generate runtime textures/sprites. Fine for prototyping; blocks art pipeline and causes allocations on fallback paths.

**Tasks:**
- Assign proper sprites/materials to prefabs.
- Remove or `#if DEVELOPMENT_BUILD` guard procedural creators.

---

### 12. Address SpawnCoordinator camera vs player TODO

**Rationale:** Comment in `SpawnCoordinator.Start()`: `// todo why camera transform instead of player?` Player is camera child, so positions align today — but the design intent is undocumented. Future camera effects (shake, offset) could desync spawn position.

**Tasks:**
- Document decision or switch to player transform if decoupling camera from player.
- Use consistent reference across all spawners (some cache `Camera.main` independently).

---

## Low Priority

### 13. Main menu and scene flow

**Rationale:** Game loads directly into gameplay; restart reloads same scene. No title screen, settings, or quit flow.

**Tasks:**
- Menu scene with Start / Quit.
- Optional: high score display.

---

### 14. Persist high scores

**Rationale:** `_totalCoins` resets every run; no `PlayerPrefs` or save system.

**Tasks:**
- Track best run score.
- Show on game-over and menu.

---

### 15. Audio system

**Rationale:** `Coin.collectSound` exists but is unassigned. No music, obstacle hit, shield, or enemy SFX.

**Tasks:**
- AudioSource pool or simple AudioManager.
- Hook collect, game over, shield, shoot events.

---

### 16. Extract shared sprite utility

**Rationale:** Three copies of procedural circle/square sprite generation across `PlayerShield`, `ShooterEnemy`, `EnemyBullet`.

**Tasks:**
- Static utility class `ProceduralSprites` or move to editor-only tooling.

---

### 17. Namespace and assembly organization

**Rationale:** All types are global namespace; single Assembly-CSharp. Fine for tiny project; friction grows with team/size.

**Tasks:**
- Namespaces: `Joyride.Core`, `Joyride.Player`, etc.
- Optional asmdef splits.

---

### 18. Automated tests

**Rationale:** `com.unity.test-framework` is installed; zero tests exist.

**Tasks:**
- EditMode tests for spawn cooldown logic, shield timer, score accumulation.
- PlayMode smoke test for scene load.

---

### 19. Input System migration

**Rationale:** Uses legacy `Input.GetButton("Jump")`. New Input System is project-agnostic but better for rebinds and multi-platform.

**Tasks:**
- Migrate to Input System package when input complexity grows.

---

### 20. Additional enemy types and patterns

**Rationale:** `ISpawner` + coroutine enemy pattern supports extension. Only one enemy type exists.

**Tasks:**
- New enemy implementing similar lifecycle (notify spawner on finish).
- Register with coordinator via new spawner or combined enemy spawner with type table.

### 21. Despawn uncollected shield pickups

**Rationale:** `ShieldSpawner` instantiates shields but does not track or destroy them when they scroll off-screen (unlike `CoinSpawner` / `ObstacleSpawner`). Missed shields accumulate in the scene during long runs.

**Tasks:**
- Add active-list + distance cleanup to `ShieldSpawner`, or share a reusable despawn helper.

---

## Partially Implemented Features

| Feature | What exists | What's missing |
|---------|-------------|----------------|
| Shooter enemy | Full behavior, bullet, spawner in scene | Prefab unassigned (procedural fallback active); prefab tuning, bullet prefab |
| Coin audio | `AudioClip` field + PlayClipAtPoint | Clip assignment |
| MapGenerator | Segment recycling in Update | Empty Start(); hardcoded 60 offset and -10 threshold |
| Game over | Time freeze + restart | UI, score summary, intentional restart key |
| Shield power-up | Full pickup + timed visual | Off-screen despawn, stacking policy, art, spawn balance tuning |
| Tag system | Used in code/prefabs | TagManager.asset shows empty tags array — verify project settings |

---

## Placeholder / Prototype Code Inventory

| Location | Placeholder behavior |
|----------|---------------------|
| `ShooterEnemy.CreatePlaceholderSprite` | Red circle-ish procedural sprite |
| `ShooterEnemy.CreateSample` | Full runtime enemy assembly |
| `EnemyBullet.Create` | Runtime bullet with procedural sprite |
| `PlayerShield.CreateShieldVisual` | Runtime blue circle shield |
| `ShooterEnemy.prefab` | Incomplete inspector values (zero movement) |
| `MapGenerator.Start()` | Empty method body |

---

## TODO Comments (in repo)

| File | Comment |
|------|---------|
| `SpawnCoordinator.cs:33` | `// todo why camera transform instead of player?` |

---

## Suggested Development Sequence

1. Assign + fix enemy prefab (items 1–2) — completes latest feature vertically.
2. Game-over UI (item 3) — makes prototype presentable.
3. Spawner priority + bounds centralization (items 5, 7) — stabilizes content pipeline.
4. Pooling + difficulty scaling (items 6, 8) — prepares for longer sessions and higher speeds.
5. Polish layer (audio, art, persistence) — items 11–15.
