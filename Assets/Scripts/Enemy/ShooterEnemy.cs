using System.Collections;
using UnityEngine;

public class ShooterEnemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private int minMoves = 2;
    [SerializeField] private int maxMoves = 4;
    [SerializeField] private float moveDuration = 0.45f;
    [SerializeField] private AnimationCurve moveCurve;
    [SerializeField] private float screenHoldOffset = 9f;

    [Header("Shooting")]
    [SerializeField] private int bulletsPerBurst = 3;
    [SerializeField] private float shootInterval = 0.5f;
    [SerializeField] private float bulletSpeed = 11f;
    [SerializeField] private GameObject bulletPrefab;

    private Camera mainCamera;
    private Transform playerTransform;

    void Awake()
    {
        if (moveCurve == null || moveCurve.length == 0)
        {
            moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }
    }

    void Start()
    {
        mainCamera = Camera.main;
        Player player = FindObjectOfType<Player>();
        playerTransform = player != null ? player.transform : null;
        EnsureVisual();
        StartCoroutine(EnemyRoutine());
    }

    void LateUpdate()
    {
        if (mainCamera == null) return;

        Vector3 position = transform.position;
        position.x = mainCamera.transform.position.x + screenHoldOffset;
        transform.position = position;
    }

    public static GameObject CreateSample(Vector3 position)
    {
        GameObject enemyObject = new GameObject("ShooterEnemy");
        enemyObject.transform.position = position;

        SpriteRenderer spriteRenderer = enemyObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreatePlaceholderSprite(new Color(0.85f, 0.15f, 0.2f, 1f));
        spriteRenderer.sortingOrder = 5;

        BoxCollider2D collider = enemyObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(1.2f, 1.2f);

        enemyObject.AddComponent<ShooterEnemy>();
        return enemyObject;
    }

    IEnumerator EnemyRoutine()
    {
        int moveCount = Random.Range(minMoves, maxMoves + 1);

        for (int i = 0; i < moveCount; i++)
        {
            float targetY = playerTransform != null
                ? GetClampedTargetY(playerTransform.position.y)
                : GetClampedTargetY(transform.position.y);
            Vector3 targetPos = new Vector3(transform.position.x, targetY, 0f);

            yield return MoveStep(targetPos);
            yield return ShootBurst();
        }

        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.OnEnemyFinished();
        }

        Destroy(gameObject);
    }

    IEnumerator MoveStep(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            float curvedT = moveCurve.Evaluate(t);
            transform.position = Vector3.Lerp(startPos, targetPos, curvedT);
            yield return null;
        }

        transform.position = targetPos;
    }

    IEnumerator ShootBurst()
    {
        for (int i = 0; i < bulletsPerBurst; i++)
        {
            FireBullet();

            if (i < bulletsPerBurst - 1)
            {
                yield return new WaitForSeconds(shootInterval);
            }
        }
    }

    void FireBullet()
    {
        Vector3 spawnPos = transform.position + Vector3.left * 0.6f;

        if (bulletPrefab != null)
        {
            GameObject bulletObject = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
            EnemyBullet bullet = bulletObject.GetComponent<EnemyBullet>();
            if (bullet != null)
            {
                bullet.Initialize(Vector2.left, bulletSpeed);
            }
            return;
        }

        EnemyBullet.Create(spawnPos, Vector2.left, bulletSpeed);
    }

    float GetClampedTargetY(float desiredY)
    {
        if (EnemySpawner.Instance != null &&
            EnemySpawner.Instance.ground != null &&
            EnemySpawner.Instance.ceiling != null)
        {
            float minY = EnemySpawner.Instance.ground.position.y;
            float maxY = EnemySpawner.Instance.ceiling.position.y;
            return Mathf.Clamp(desiredY, minY, maxY);
        }

        return Mathf.Clamp(desiredY, -3f, 4.5f);
    }

    void EnsureVisual()
    {
        if (GetComponent<SpriteRenderer>() != null) return;

        SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreatePlaceholderSprite(new Color(0.85f, 0.15f, 0.2f, 1f));
        spriteRenderer.sortingOrder = 5;

        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(1.2f, 1.2f);
        }
    }

    static Sprite CreatePlaceholderSprite(Color color)
    {
        const int resolution = 48;
        Texture2D texture = new Texture2D(resolution, resolution);
        Color[] pixels = new Color[resolution * resolution];
        Vector2 center = new Vector2(resolution * 0.5f, resolution * 0.5f);
        float outerRadius = resolution * 0.5f;
        float innerRadius = outerRadius * 0.35f;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                Vector2 point = new Vector2(x, y);
                float distance = Vector2.Distance(point, center);

                if (distance > outerRadius)
                    pixels[y * resolution + x] = Color.clear;
                else if (distance > innerRadius)
                    pixels[y * resolution + x] = color;
                else
                    pixels[y * resolution + x] = Color.Lerp(color, Color.white, 0.35f);
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