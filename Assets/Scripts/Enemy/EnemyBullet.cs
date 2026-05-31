using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 6f;
    [SerializeField] private float despawnBehindCamera = 25f;

    private Vector2 direction = Vector2.left;
    private Transform cameraTransform;

    public void Initialize(Vector2 moveDirection, float moveSpeed)
    {
        direction = moveDirection.normalized;
        speed = moveSpeed;
    }

    public static EnemyBullet Create(Vector3 position, Vector2 moveDirection, float moveSpeed)
    {
        GameObject bulletObject = new GameObject("EnemyBullet");
        bulletObject.transform.position = position;
        bulletObject.transform.localScale = Vector3.one * 0.35f;

        SpriteRenderer spriteRenderer = bulletObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateCircleSprite(new Color(1f, 0.35f, 0.2f, 1f));
        spriteRenderer.sortingOrder = 6;

        CircleCollider2D collider = bulletObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.45f;

        EnemyBullet bullet = bulletObject.AddComponent<EnemyBullet>();
        bullet.Initialize(moveDirection, moveSpeed);
        return bullet;
    }

    void Start()
    {
        cameraTransform = Camera.main != null ? Camera.main.transform : null;
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        if (cameraTransform != null &&
            transform.position.x < cameraTransform.position.x - despawnBehindCamera)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerShield playerShield = other.GetComponent<PlayerShield>();
        if (playerShield != null && playerShield.HasShield())
        {
            playerShield.ConsumeShield();
        }
        else if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }

        Destroy(gameObject);
    }

    static Sprite CreateCircleSprite(Color color)
    {
        const int resolution = 32;
        Texture2D texture = new Texture2D(resolution, resolution);
        Color[] pixels = new Color[resolution * resolution];
        Vector2 center = new Vector2(resolution * 0.5f, resolution * 0.5f);
        float radius = resolution * 0.5f;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                pixels[y * resolution + x] = distance < radius ? color : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0f, 0f, resolution, resolution),
            new Vector2(0.5f, 0.5f),
            resolution);
    }
}
