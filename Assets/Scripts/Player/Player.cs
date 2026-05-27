using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerShield playerShield;

    [SerializeField] private float thrust = 18f;
    [SerializeField] private float maxUpSpeed = 7f;
    [SerializeField] private float maxDownSpeed = -12f;
    [SerializeField] private float releaseGravityMultiplier = 2f;

    private bool thrustHeld;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerShield = GetComponent<PlayerShield>();
    }

    void Update()
    {
        thrustHeld = Input.GetButton("Jump");
    }

    void FixedUpdate()
    {
        if (thrustHeld)
        {
            rb.AddForce(Vector2.up * thrust, ForceMode2D.Force);
        }
        else
        {
            if (rb.velocity.y > 0)
            {
                rb.velocity += Vector2.up * (Physics2D.gravity.y * (releaseGravityMultiplier - 1f) * Time.fixedDeltaTime);
            }
        }

        float y = Mathf.Clamp(rb.velocity.y, maxDownSpeed, maxUpSpeed);
        rb.velocity = new Vector2(rb.velocity.x, y);
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            if (playerShield && playerShield.HasShield())
            {
                playerShield.ConsumeShield();
                Destroy(collision.gameObject);
            }
            else
            {
                GameManager.Instance.GameOver();
            }
        }
    }

}