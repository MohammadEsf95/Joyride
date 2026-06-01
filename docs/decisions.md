# Joyride — Architectural Decisions

Inferred from the codebase. Each entry documents what was decided (by implementation), supporting evidence, benefits, and tradeoffs.

---

## AD-01: Singleton GameManager for score and game state

**Decision:** Use a single `GameManager` MonoBehaviour with static `Instance` for score tracking, game-over, and scene restart.

**Evidence:**
- `GameManager.Awake()` — classic singleton with duplicate destroy.
- `Coin`, `Player`, `EnemyBullet` call `GameManager.Instance` directly.

**Benefits:**
- Minimal boilerplate; easy for beginners to trace.
- Single place for score UI binding and restart logic.
- No setup wiring in Inspector for score events.

**Tradeoffs:**
- Hidden dependencies; hard to unit test without scene setup.
- Null reference risk if GameManager missing from scene (`Player` does not null-check).
- Grows into a "god object" if more systems attach (audio, ads, analytics).

---

## AD-02: Camera-driven scrolling with player as child

**Decision:** Horizontal movement is owned by `PlayerCamera` on the MainCamera object. The player is parented to the camera and only moves vertically via physics.

**Evidence:**
- `PlayerCamera.Update()` — increments `transform.position.x` with accelerating speed.
- SampleScene hierarchy: `Player` child of `PlayerCamera`.
- Spawners use `Camera.main.transform.position.x` for spawn/despawn calculations.

**Benefits:**
- Player stays in frame without camera follow scripts.
- Spawn distance is naturally tied to visible world edge.
- Simplifies player X logic to zero.

**Tradeoffs:**
- Couples player transform to camera; camera shake or lag would affect player.
- SpawnCoordinator TODO questions camera vs player — if they diverge later, spawn math breaks.
- Unconventional vs separate camera follow target.

---

## AD-03: ISpawner interface with central SpawnCoordinator

**Decision:** All world content spawners implement `ISpawner` (`CanSpawn`, `Spawn(float x)`). A coordinator polls on an interval and delegates to the first eligible spawner.

**Evidence:**
- `ISpawner.cs` — two-method interface.
- `SpawnCoordinator` — registers via `FindObjectsOfType`, enforces gap, one spawn per tick.
- Implementors: `ObstacleSpawner`, `CoinSpawner`, `ShieldSpawner`, `EnemySpawner`.

**Benefits:**
- Adding new content types does not modify coordinator logic.
- Global spawn gap prevents overlapping X clusters.
- Each spawner owns its own cooldown/rules.

**Tradeoffs:**
- First-match wins — order matters but is non-deterministic (`FindObjectsOfType`).
- Only one spawn per check limits density.
- No cross-spawner awareness (e.g. "don't spawn enemy during coin pattern").

---

## AD-04: Instantiate/Destroy lifecycle (no object pooling)

**Decision:** All spawned entities are created with `Instantiate` and removed with `Destroy`. Spawners track active lists only for off-screen cleanup.

**Evidence:**
- `CoinSpawner.SpawnCoin`, `ObstacleSpawner.Spawn`, `EnemySpawner.Spawn`, `ShooterEnemy.FireBullet` — all Instantiate.
- Cleanup loops call `Destroy` when behind camera or on pickup.

**Benefits:**
- Simplest mental model; fastest to prototype.
- No pool bookkeeping, prewarm, or reset-on-reuse logic.
- Works correctly for current entity counts.

**Tradeoffs:**
- GC pressure from coin patterns (5–10 coins) and bullet bursts.
- Instantiate cost at high camera speeds.
- Will need refactor when targeting mobile or long sessions.

---

## AD-05: Per-spawner cooldown and eligibility logic

**Decision:** Each spawner independently decides `CanSpawn()` using timers, random chance, and type-specific gates (e.g. `enemyAlive`).

**Evidence:**
- `ObstacleSpawner` / `CoinSpawner` — `nextSpawnCooldown` countdown.
- `ShieldSpawner` — cooldown + `spawnChance`.
- `EnemySpawner` — `firstSpawnDelay`, `minTimeBetweenSpawns`, `spawnChance`, `enemyAlive`.

**Benefits:**
- Localized tuning per content type in Inspector.
- Enemy spawner can enforce "only one shooter at a time" without coordinator changes.
- Random chance adds variety without complex wave scripts.

**Tradeoffs:**
- Difficulty tuning scattered across four components.
- No global "intensity" knob; hard to orchestrate authored sequences.
- Shield/enemy low chance + coordinator first-match can make rare spawns rarer.

---

## AD-06: Active list + distance-based despawn

**Decision:** Spawners that emit multiple objects maintain a `List<GameObject>` and destroy entries when X position falls behind camera minus `despawnDistance`.

**Evidence:**
- `CoinSpawner.activeCoins` + `CleanupCoins()`.
- `ObstacleSpawner.activeObstacles` + `CleanupObstacles()`.
- `EnemyBullet` self-manages despawn via `despawnBehindCamera`.

**Benefits:**
- Prevents unbounded scene object growth during long runs.
- Handles objects destroyed early (null check in list cleanup).
- Despawn distance configurable per spawner type.

**Tradeoffs:**
- O(n) cleanup scan every frame per spawner.
- Lists can briefly hold destroyed obstacle references until next cleanup.
- Duplicated cleanup pattern — not extracted to shared utility.

---

## AD-07: Tag-based collision discrimination

**Decision:** Use Unity tags (`"Player"`, `"Obstacle"`) for gameplay collision filtering instead of layers or interfaces.

**Evidence:**
- `Player.OnCollisionEnter2D` — `CompareTag("Obstacle")`.
- `Coin`, `Shield`, `EnemyBullet` — `CompareTag("Player")` on triggers.
- Prefabs: `StaticObstacle` tagged Obstacle; Player tagged in scene.

**Benefits:**
- Readable in Inspector and code.
- Minimal setup for prototype.

**Tradeoffs:**
- Tag string typos fail silently at runtime.
- No physics layer matrix optimization.
- `"Coin"` tag on CoinSpawner GameObject appears unused/confusing.

---

## AD-08: Shield as timed MonoBehaviour on player

**Decision:** Shield power-up is not a separate game system — it's `PlayerShield` on the player with bool timer, procedural visual, and consume-on-hit API.

**Evidence:**
- `Shield` collectible calls `playerShield.ActivateShield()`.
- `Player` and `EnemyBullet` call `HasShield()` / `ConsumeShield()`.
- Timer, rotation, alpha flash in `Update()`.

**Benefits:**
- Fast to implement; no buff framework needed.
- Clear API: activate, consume, query.
- Visual generated at runtime if not assigned.

**Tradeoffs:**
- Does not generalize to other power-ups without copy-paste.
- Pickup while active implicitly refreshes — undocumented design.
- `material` access creates instance copy (memory).

---

## AD-09: Obstacle collision destroys obstacle when shield absorbs hit

**Decision:** When player with shield collides with obstacle, shield is consumed and obstacle GameObject is destroyed — player continues.

**Evidence:**
- `Player.OnCollisionEnter2D` — `ConsumeShield()` then `Destroy(collision.gameObject)`.

**Benefits:**
- Immediate feedback; clears path after blocked hit.
- Single-hit shield clearly spent.

**Tradeoffs:**
- ObstacleSpawner list stale until cleanup.
- Destroying world objects from Player violates single-responsibility.
- No VFX/score for blocked hit.

---

## AD-10: Enemy architecture — coroutine behavior + camera-locked X

**Decision:** Shooter enemies run an `IEnumerator` routine (move vertically, shoot bursts, exit). X position is forced each `LateUpdate` to stay at fixed screen offset from camera.

**Evidence:**
- `ShooterEnemy.EnemyRoutine()` — move loop, `ShootBurst()`, notify `EnemySpawner.OnEnemyFinished()`, `Destroy`.
- `LateUpdate()` — `position.x = camera.x + screenHoldOffset`.
- `EnemySpawner.enemyAlive` flag until routine completes.

**Benefits:**
- Enemy behaves like a "set piece" at fixed screen depth — readable telegraph for player.
- Coroutines readable for designers vs state machine code.
- Spawner knows when to allow next enemy.

**Tradeoffs:**
- Not a physically moving world enemy — differs from obstacles/coins that scroll with world.
- Depends on `EnemySpawner.Instance` for bounds and lifecycle callback.
- If spawner missing, enemy never clears `enemyAlive` (when spawner absent, callback skipped but enemy still destroys self). In SampleScene the spawner is present; `enemyPrefab` is null so `CreateSample()` is used instead of the prefab.

---

## AD-11: Dual prefab / procedural creation fallback

**Decision:** Enemies and bullets support Inspector prefabs with runtime procedural fallback when prefabs are null.

**Evidence:**
- `EnemySpawner.Spawn` — `Instantiate(enemyPrefab)` OR `ShooterEnemy.CreateSample()`.
- `ShooterEnemy.FireBullet` — prefab OR `EnemyBullet.Create()`.
- `ShooterEnemy.EnsureVisual()` / `PlayerShield.CreateShieldVisual()` — runtime components.

**Benefits:**
- Playable without art pipeline.
- Supports rapid iteration before assets exist.

**Tradeoffs:**
- Two code paths to maintain and test.
- Prefab misconfiguration (zero move values) silently broken vs fallback defaults.
- Runtime texture allocation.

---

## AD-12: MapGenerator two-segment ground/ceiling swap

**Decision:** Infinite terrain uses two ground and two ceiling segments swapped when player passes a threshold, repositioning the trailing segment 60 units ahead.

**Evidence:**
- `MapGenerator.Update()` — compares `player.position.x` to `ground.position.x - 10`.
- Swaps references and moves `prevGround`/`prevCeiling` by +60 on X.

**Benefits:**
- O(1) streaming; no growing scene hierarchy.
- Works with static colliders for floor/ceiling.

**Tradeoffs:**
- Magic numbers (60, 10) not tied to segment sprite size in code.
- Empty `Start()` — no validation of references.
- Separate from spawn system — visual/collision bounds not shared with spawners.

---

## AD-13: Direct static references over event system

**Decision:** No gameplay event bus. Systems communicate via singletons and `GetComponent` on collision.

**Evidence:**
- Grep shows no `Action`, `event`, ScriptableObject events, or UnityEvent usage in scripts.
- All cross-system calls are direct method invocation.

**Benefits:**
- Zero indirection; easy to debug in small project.
- No subscriber lifecycle management.

**Tradeoffs:**
- Tight coupling everywhere.
- Adding listeners (UI, achievements, sound) requires editing source classes.
- Difficult to test in isolation.

---

## AD-14: Coin spawn patterns as inline switch

**Decision:** Coin variety is implemented as four pattern methods called from a random switch in `CoinSpawner` — not data-driven.

**Evidence:**
- `SpawnRandomPattern` — `Random.Range(0, 4)` switch.
- Methods: horizontal, vertical, curve, circle.

**Benefits:**
- Easy to read and modify patterns in code.
- No ScriptableObject or JSON authoring needed yet.

**Tradeoffs:**
- Adding patterns requires code changes.
- Pattern weights equal (25% each) — not configurable.
- Pattern spawns multiple coins per coordinator tick (burst) while coordinator thought it spawned "one thing".

---

## AD-15: Game over via time scale freeze

**Decision:** Death pauses simulation with `Time.timeScale = 0`; restart restores scale and reloads scene.

**Evidence:**
- `GameManager.GameOver()` / `Restart()`.

**Benefits:**
- One-line pause of physics, movement, spawners.
- Clean reset via full scene reload — no manual state cleanup.

**Tradeoffs:**
- UI animations/coroutines also frozen unless unscaled time used.
- No pause menu distinction (pause vs game over same mechanism potential).
- Reload is heavy vs soft reset.

---

## AD-16: Legacy Input Manager for flight control

**Decision:** Player thrust uses `Input.GetButton("Jump")` in `Update`, applied in `FixedUpdate`.

**Evidence:**
- `Player.Update` / `Player.FixedUpdate`.

**Benefits:**
- Works out of box with default Unity input axes.
- Simple hold-to-fly feel.

**Tradeoffs:**
- Not multi-platform friendly without axis mapping.
- Input polled in Update, physics in FixedUpdate — minor timing mismatch possible.
- New Input System package not adopted.

---

## AD-17: EnemySpawner singleton without duplicate guard

**Decision:** `EnemySpawner` exposes public static `Instance` assigned in `Awake()` without destroying duplicates (unlike GameManager/SpawnCoordinator).

**Evidence:**
- `EnemySpawner.Awake()` — `Instance = this` only.

**Benefits:**
- Simpler Awake for single-scene prototype.

**Tradeoffs:**
- Inconsistent singleton pattern across project.
- Duplicate spawners would silently overwrite Instance.
- Last Awake wins — undefined if multiple exist.

---

## Decision Summary Matrix

| Area | Choice | Maturity |
|------|--------|----------|
| State management | Singletons | Prototype |
| Spawning | Coordinator + ISpawner | Solid pattern, tuning issues |
| Pooling | None | Not started |
| Events | Direct calls | Not started |
| Power-ups | Player-bound component | Single type only |
| Enemies | Coroutine + screen-lock | Spawner in scene; prefab path unused |
| World streaming | Two-segment swap | Minimal |
| Input | Legacy | Adequate for prototype |
| UI | TMP score only | Minimal |
