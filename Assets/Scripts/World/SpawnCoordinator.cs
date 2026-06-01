using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnCoordinator : MonoBehaviour
{
    public static SpawnCoordinator Instance { get; private set; }
    
    [Header("Spawn Settings")]
    public float spawnCheckInterval = 0.5f;
    public float spawnDistance = 15f;
    public float minSpawnGap = 3f;
    
    private List<ISpawner> spawners = new List<ISpawner>();
    private Transform cameraTransform;
    private float checkTimer;
    private float lastOccupiedMaxX = -999f;

    // Coin circle patterns can extend slightly behind the spawn anchor.
    const float MaxSpawnBackExtent = 0.7f;
    
    void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // todo why camera transform instead of player?
        cameraTransform = Camera.main.transform;
        
        // Register all spawners in the scene
        spawners.AddRange(FindObjectsOfType<MonoBehaviour>().OfType<ISpawner>());
    }
    
    void Update()
    {
        checkTimer += Time.deltaTime;
        
        if (checkTimer >= spawnCheckInterval)
        {
            TrySpawn();
            checkTimer = 0f;
        }
    }
    
    void TrySpawn()
    {
        float spawnX = cameraTransform.position.x + spawnDistance;

        // Enforce minimum gap using the full occupied X range of the previous spawn.
        if (spawnX < lastOccupiedMaxX + minSpawnGap + MaxSpawnBackExtent)
            return;

        foreach (ISpawner spawner in spawners)
        {
            if (!spawner.CanSpawn())
                continue;

            SpawnOccupancy occupancy = spawner.Spawn(spawnX);
            if (!occupancy.IsValid)
                continue;

            lastOccupiedMaxX = occupancy.MaxX;
            break;
        }
    }
}