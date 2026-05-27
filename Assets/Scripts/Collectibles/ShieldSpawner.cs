using UnityEngine;

public class ShieldSpawner : MonoBehaviour, ISpawner
{
    [SerializeField] private GameObject shieldPrefab;
    [SerializeField] private float spawnChance = 0.15f;
    [SerializeField] private float minTimeBetweenSpawns = 10f;
    [SerializeField] private float minY = -3f;
    [SerializeField] private float maxY = 3f;

    private float lastSpawnTime = -999f;

    public bool CanSpawn()
    {
        bool cooldownReady = Time.time - lastSpawnTime >= minTimeBetweenSpawns;
        bool chanceSuccess = Random.value <= spawnChance;
        
        Debug.Log($"ShieldSpawner CanSpawn: cooldown={cooldownReady}, chance={chanceSuccess}, timeSinceLastSpawn={Time.time - lastSpawnTime}");
        
        return cooldownReady && chanceSuccess;
    }

    public void Spawn(float spawnX)
    {
        if (shieldPrefab == null)
        {
            Debug.LogError("ShieldSpawner: shieldPrefab is not assigned!");
            return;
        }

        float spawnY = Random.Range(minY, maxY);
        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);
        
        GameObject shield = Instantiate(shieldPrefab, spawnPosition, Quaternion.identity);
        lastSpawnTime = Time.time;
        
        Debug.Log($"ShieldSpawner: Spawned shield at {spawnPosition}");
    }
}