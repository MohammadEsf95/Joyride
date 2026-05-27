using UnityEngine;

public class PlayerShield : MonoBehaviour
{
    [SerializeField] private GameObject shieldVisual;
    [SerializeField] private float shieldDuration = 10f;
    
    private bool hasShield = false;
    private float shieldTimer = 0f;
    
    private Material shieldMaterial;

    void Start()
    {
        if (shieldVisual == null)
        {
            shieldVisual = CreateShieldVisual();
        }
        shieldMaterial = shieldVisual.GetComponent<Renderer>().material;
        shieldVisual.SetActive(false);
    }

    private void SetShieldAlpha(float alpha)
    {
        if (shieldMaterial)
        {
            Color color = shieldMaterial.color;
            color.a = alpha;
            shieldMaterial.color = color;
        }
    }
    
    void Update()
    {
        if (hasShield)
        {
            shieldTimer -= Time.deltaTime;
        
            // Rotate shield visual (2D rotation)
            shieldVisual.transform.Rotate(Vector3.forward, 150f * Time.deltaTime);
        
            // Flash when about to expire
            if (shieldTimer < 2f)
            {
                float alpha = Mathf.PingPong(Time.time * 5f, 0.5f) + 0.2f;
                SetShieldAlpha(alpha);
            }
        
            if (shieldTimer <= 0f)
            {
                DeactivateShield();
            }
        }
    }
    
    public void ActivateShield()
    {
        hasShield = true;
        shieldTimer = shieldDuration;
        shieldVisual.SetActive(true);
        SetShieldAlpha(0.5f);
    }
    
    public void DeactivateShield()
    {
        hasShield = false;
        shieldVisual.SetActive(false);
    }
    
    public bool HasShield()
    {
        return hasShield;
    }
    
    public void ConsumeShield()
    {
        if (hasShield)
        {
            DeactivateShield();
        }
    }
    
    private GameObject CreateShieldVisual()
    {
        GameObject shield = new GameObject("ShieldVisual");
        shield.transform.SetParent(transform);
        shield.transform.localPosition = Vector3.zero;
        shield.transform.localScale = Vector3.one * 2.5f;
    
        SpriteRenderer sr = shield.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = new Color(0.3f, 0.7f, 1f, 0.5f);
        sr.sortingOrder = 10; // Render above player
    
        return shield;
    }

    private Sprite CreateCircleSprite()
    {
        int resolution = 64;
        Texture2D texture = new Texture2D(resolution, resolution);
        Color[] pixels = new Color[resolution * resolution];
    
        Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
        float radius = resolution / 2f;
    
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = distance < radius ? 1f - (distance / radius) * 0.5f : 0f;
                pixels[y * resolution + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
    
        texture.SetPixels(pixels);
        texture.Apply();
    
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f));
    }
}
