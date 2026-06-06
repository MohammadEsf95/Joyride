using UnityEngine;

public class EnemySpawner : MonoBehaviour, ISpawner
{
    public static EnemySpawner Instance;

    [Header("Prefabs")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Bounds")]
    public Transform ground;
    public Transform ceiling;

    [Header("Spawn Frequency")]
    [SerializeField] private float firstSpawnDelay = 10f;
    [SerializeField] private float spawnChance = 0.18f;
    [SerializeField] private float minTimeBetweenSpawns = 18f;

    private bool enemyAlive;
    private float lastSpawnTime = -999f;

    void Awake()
    {
        Instance = this;
    }

    public bool CanSpawn()
    {
        if (enemyAlive) return false;
        if (Time.time < firstSpawnDelay) return false;
        if (Time.time - lastSpawnTime < minTimeBetweenSpawns) return false;
        return Random.value <= spawnChance;
    }

    public SpawnOccupancy Spawn(float spawnX)
    {
        Player player = FindObjectOfType<Player>();
        float spawnY = player != null ? player.transform.position.y : 0f;
        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);

        if (enemyPrefab != null)
        {
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            ShooterEnemy.CreateSample(spawnPosition);
        }

        enemyAlive = true;
        lastSpawnTime = Time.time;
        return SpawnOccupancy.Point(spawnX);
    }

    public void OnEnemyFinished()
    {
        enemyAlive = false;
    }
}