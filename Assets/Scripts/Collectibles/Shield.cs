using UnityEngine;

public class Shield : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;
    
    private Vector3 startPosition;
    
    void Start()
    {
        startPosition = transform.position;
    }
    
    void Update()
    {
        // Rotate the shield
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        // Bob up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CollectShield(other.gameObject);
        }
    }

    private void CollectShield(GameObject player)
    {
        PlayerShield playerShield = player.GetComponent<PlayerShield>();
        if (playerShield != null)
        {
            playerShield.ActivateShield();
        }
        else
        {
            Debug.LogWarning("Player missing PlayerShield component!");
        }
    
        Destroy(gameObject);
    }

}