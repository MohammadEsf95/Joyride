using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : MonoBehaviour, ISpawner
{
    [Header("Coin Settings")]
    public GameObject coinPrefab;
    public float minSpawnInterval = 1.5f;
    public float maxSpawnInterval = 3f;
    public float despawnDistance = 20f;
    
    [Header("Pattern Settings")]
    public float minHeight = -3f;
    public float maxHeight = 4.5f;
    public float coinSpacing = 1f;
    
    private float nextSpawnCooldown;
    private List<GameObject> activeCoins = new List<GameObject>();
    private Transform cameraTransform;
    
    void Start()
    {
        cameraTransform = Camera.main.transform;
        nextSpawnCooldown = Random.Range(minSpawnInterval, maxSpawnInterval);
    }
    
    void Update()
    {
        nextSpawnCooldown -= Time.deltaTime;
        CleanupCoins();
    }
    
    public bool CanSpawn()
    {
        return nextSpawnCooldown <= 0f;
    }
    
    public void Spawn(float xPosition)
    {
        SpawnRandomPattern(xPosition);
        nextSpawnCooldown = Random.Range(minSpawnInterval, maxSpawnInterval);
    }
    
    void SpawnRandomPattern(float startX)
    {
        int patternType = Random.Range(0, 4);
        
        switch (patternType)
        {
            case 0:
                SpawnLinearHorizontal(startX);
                break;
            case 1:
                SpawnLinearVertical(startX);
                break;
            case 2:
                SpawnCurvePattern(startX);
                break;
            case 3:
                SpawnCirclePattern(startX);
                break;
        }
    }
    
    void SpawnLinearHorizontal(float startX)
    {
        float y = Random.Range(minHeight, maxHeight);
        int coinCount = Random.Range(5, 10);
        
        for (int i = 0; i < coinCount; i++)
        {
            Vector3 pos = new Vector3(startX + i * coinSpacing, y, 0f);
            SpawnCoin(pos);
        }
    }
    
    void SpawnLinearVertical(float startX)
    {
        float startY = Random.Range(minHeight, maxHeight - 3f);
        int coinCount = Random.Range(4, 7);
        
        for (int i = 0; i < coinCount; i++)
        {
            Vector3 pos = new Vector3(startX, startY + i * coinSpacing, 0f);
            SpawnCoin(pos);
        }
    }
    
    void SpawnCurvePattern(float startX)
    {
        float curveAmplitude = 1f;  // Maximum height deviation from base
    
        // Ensure baseY leaves room for the curve to go up and down
        float baseY = Random.Range(minHeight + curveAmplitude, maxHeight - curveAmplitude);
    
        for (int i = 0; i < 8; i++)
        {
            float t = i / 7f;
            float curveHeight = Mathf.Sin(t * Mathf.PI) * curveAmplitude;
            float y = baseY + curveHeight;
        
            Vector3 position = new Vector3(startX + i * 1f, y, 0f);
            SpawnCoin(position);
        }
    }
    
    void SpawnCirclePattern(float startX)
    {
        int coinCount = 8;
        float radius = 0.7f;
        float centerY = Random.Range(minHeight + radius, maxHeight - radius);
        
        for (int i = 0; i < coinCount; i++)
        {
            float angle = (i / (float)coinCount) * Mathf.PI * 2f;
            float x = startX + Mathf.Cos(angle) * radius;
            float y = centerY + Mathf.Sin(angle) * radius;
            SpawnCoin(new Vector3(x, y, 0f));
        }
    }
    
    void SpawnCoin(Vector3 position)
    {
        position.y = Mathf.Clamp(position.y, minHeight, maxHeight);
        GameObject coin = Instantiate(coinPrefab, position, Quaternion.identity);
        activeCoins.Add(coin);
    }
    
    void CleanupCoins()
    {
        for (int i = activeCoins.Count - 1; i >= 0; i--)
        {
            if (!activeCoins[i])
            {
                activeCoins.RemoveAt(i);
            }
            else if (activeCoins[i].transform.position.x < cameraTransform.position.x - despawnDistance)
            {
                Destroy(activeCoins[i]);
                activeCoins.RemoveAt(i);
            }
        }
    }
}