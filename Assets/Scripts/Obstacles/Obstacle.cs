using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [Header("Height Variation")]
    public float minHeight = 0.5f;
    public float maxHeight = 3f;
    
    void Start()
    {
        float randomHeight = Random.Range(minHeight, maxHeight);
        transform.localScale = new Vector3(transform.localScale.x, randomHeight, transform.localScale.z);
    }
}