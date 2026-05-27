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
    private float lastSpawnX = -999f;
    
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
        
        // Enforce minimum gap between any spawns
        if (spawnX - lastSpawnX < minSpawnGap)
        {
            return;
        }
        
        // Check each spawner
        foreach (ISpawner spawner in spawners)
        {
            if (spawner.CanSpawn())
            {
                spawner.Spawn(spawnX);
                lastSpawnX = spawnX;
                break; // Only spawn one thing per check
            }
        }
    }
}