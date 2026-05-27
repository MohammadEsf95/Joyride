using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Visual")]
    public float rotationSpeed = 180f;
    
    [Header("Collection")]
    public int coinValue = 1;
    public AudioClip collectSound;
    
    void Update()
    {
        // Rotate coin for visual effect
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CollectCoin(other.gameObject);
        }
    }
    
    void CollectCoin(GameObject player)
    {
        GameManager.Instance?.AddCoins(coinValue);
        
        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        
        Destroy(gameObject);
    }
}