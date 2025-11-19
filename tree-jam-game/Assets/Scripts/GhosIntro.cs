using UnityEngine;

public class GhostIntro : MonoBehaviour
{
    [Header("Fall Settings")]
    public float fallGravity = 0.3f;
    public float fallDistance = 3f; // CHANGED: How far to fall before floating (in units)
    public float maxFallSpeed = 2f;

    [Header("Float Settings")]
    public float floatSpeed = 1f;
    public float floatAmplitude = 0.3f;
    public float horizontalDrift = 0.5f;

    [Header("Rotation")]
    public float rotationSpeed = 30f;

    [Header("Fade")]
    public float fadeInDuration = 0.5f;

    [Header("Lifetime")]
    public float maxLifetime = 30f;

    [Header("Ground Protection")]
    public float groundCollisionY = -5f; // Stop falling if reaching this Y position

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private float startTime;
    private Vector3 floatStartPosition;
    private Vector3 spawnPosition;
    private float randomOffset;
    private bool hasBeenVisible = false;
    private bool isFloating = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
        {
            Debug.LogError("GhostIntro: No SpriteRenderer found on " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        startTime = Time.time;
        spawnPosition = transform.position;
        randomOffset = Random.Range(0f, 100f);

        // Set initial alpha to 0
        Color c = spriteRenderer.color;
        c.a = 0;
        spriteRenderer.color = c;

        // Setup Rigidbody2D for falling
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.isKinematic = false;
        rb.gravityScale = fallGravity;
        rb.drag = 1.5f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Destroy after max lifetime
        Destroy(gameObject, maxLifetime);

        Debug.Log("Ghost spawned at: " + transform.position);
    }

    void Update()
    {
        if (spriteRenderer == null) return;

        FadeIn();
        Rotate();
        CheckFallDistance();
        CheckGroundCollision();

        if (isFloating)
        {
            FloatMovement();
        }
        else
        {
            // Limit fall speed
            if (rb != null && rb.velocity.y < -maxFallSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -maxFallSpeed);
            }
        }
    }

    void FadeIn()
    {
        float elapsed = Time.time - startTime;
        if (elapsed < fadeInDuration)
        {
            Color color = spriteRenderer.color;
            color.a = Mathf.Clamp01(elapsed / fadeInDuration);
            spriteRenderer.color = color;
        }
        else
        {
            Color color = spriteRenderer.color;
            if (color.a < 1f)
            {
                color.a = 1f;
                spriteRenderer.color = color;
            }
        }
    }

    void CheckFallDistance()
    {
        // Switch to floating after falling a certain distance
        if (!isFloating)
        {
            float distanceFallen = spawnPosition.y - transform.position.y;

            if (distanceFallen >= fallDistance)
            {
                StartFloating();
            }
        }
    }

    void CheckGroundCollision()
    {
        // Prevent falling through ground
        if (!isFloating && transform.position.y <= groundCollisionY)
        {
            Vector3 pos = transform.position;
            pos.y = groundCollisionY;
            transform.position = pos;

            StartFloating();
        }
    }

    void StartFloating()
    {
        isFloating = true;
        floatStartPosition = transform.position;

        // Stop falling
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
            rb.gravityScale = 0;
        }

        Debug.Log("Ghost started floating at: " + transform.position);
    }

    void FloatMovement()
    {
        // Gentle sine wave floating
        float t = Time.time + randomOffset;
        float vertical = Mathf.Sin(t * floatSpeed) * floatAmplitude;
        float horizontal = Mathf.Sin(t * 0.5f) * horizontalDrift;

        transform.position = floatStartPosition + new Vector3(horizontal, vertical, 0);
    }

    void Rotate()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    void OnBecameVisible()
    {
        hasBeenVisible = true;
    }

    void OnBecameInvisible()
    {
        if (hasBeenVisible)
        {
            Destroy(gameObject, 1f);
        }
    }
}