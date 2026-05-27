using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour, ISpawner
{
    [Header("Prefabs")]
    public GameObject obstaclePrefab;
    
    [Header("Spawn Settings")]
    public float minSpawnInterval = 3f;
    public float maxSpawnInterval = 6f;
    public float despawnDistance = 20f;
    
    [Header("Height Limits")]
    public float minHeight = -3f;
    public float maxHeight = 4.5f;
    
    private List<GameObject> activeObstacles = new List<GameObject>();
    private Transform cameraTransform;
    private float nextSpawnCooldown;
    
    void Start()
    {
        cameraTransform = Camera.main.transform;
        nextSpawnCooldown = Random.Range(minSpawnInterval, maxSpawnInterval);
    }
    
    void Update()
    {
        nextSpawnCooldown -= Time.deltaTime;
        CleanupObstacles();
    }
    
    public bool CanSpawn()
    {
        return nextSpawnCooldown <= 0f;
    }
    
    public void Spawn(float xPosition)
    {
        float spawnY = Random.Range(minHeight, maxHeight);
        Vector3 pos = new Vector3(xPosition, spawnY, 0f);
        
        GameObject obstacle = Instantiate(obstaclePrefab, pos, Quaternion.identity);
        activeObstacles.Add(obstacle);
        
        nextSpawnCooldown = Random.Range(minSpawnInterval, maxSpawnInterval);
    }
    
    void CleanupObstacles()
    {
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            if (!activeObstacles[i])
            {
                activeObstacles.RemoveAt(i);
            }
            else if (activeObstacles[i].transform.position.x < cameraTransform.position.x - despawnDistance)
            {
                Destroy(activeObstacles[i]);
                activeObstacles.RemoveAt(i);
            }
        }
    }
}