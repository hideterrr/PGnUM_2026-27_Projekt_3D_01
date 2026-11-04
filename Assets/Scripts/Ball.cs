using System.Collections;
using UnityEngine;

public enum BallType
{
    Normal,     // +1 point, standard ball
    Bonus,      // +3 points, gold, faster / smaller
    Hazard      // -2 points, red, penalty!
}

public class Ball : MonoBehaviour
{
    [Header("Ball Configuration")]
    public BallType ballType = BallType.Normal;
    public int scoreValue = 1;
    public float lifeTime = 15f;
    public float deathY = -4f;

    [Header("Visual & Effects")]
    public Material normalMaterial;
    public Material bonusMaterial;
    public Material hazardMaterial;

    private Rigidbody rb;
    private MeshRenderer meshRenderer;
    private bool isHit = false;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }

    protected virtual void Start()
    {
        // Safety self-destruction after lifetime
        Destroy(gameObject, lifeTime);
    }

    protected virtual void Update()
    {
        // Safety containment cleanup if fallen below or escaped outside arena
        Vector3 pos = transform.position;
        if (pos.y < deathY || Mathf.Abs(pos.x) > 4.2f || Mathf.Abs(pos.z) > 3.2f)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initializes ball properties dynamically when spawned.
    /// </summary>
    public void Init(BallType type, Material mat, float scale, Vector3 initialVelocity)
    {
        ballType = type;

        switch (ballType)
        {
            case BallType.Normal:
                scoreValue = 1;
                break;
            case BallType.Bonus:
                scoreValue = 3;
                break;
            case BallType.Hazard:
                scoreValue = -2;
                break;
        }

        if (mat != null && meshRenderer != null)
        {
            meshRenderer.material = mat;
        }

        transform.localScale = Vector3.one * scale;

        if (rb != null)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = initialVelocity;
#else
            rb.velocity = initialVelocity;
#endif
            rb.angularVelocity = Random.insideUnitSphere * 5f;
        }
    }

    /// <summary>
    /// Called when the player shoots/clicks this ball.
    /// </summary>
    public virtual void Hit()
    {
        if (isHit) return;
        isHit = true;

        if (GameManager.Instance != null && GameManager.Instance.IsGameActive)
        {
            GameManager.Instance.OnBallHit(this, transform.position);
        }

        StartCoroutine(HitFeedbackAndDestroy());
    }

    // Direct Unity mouse/touch click fallback
    protected virtual void OnMouseDown()
    {
        Hit();
    }

    private IEnumerator HitFeedbackAndDestroy()
    {
        // Disable collider immediately to prevent double clicks
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Quick pop-and-shrink animation
        Vector3 origScale = transform.localScale;
        float elapsed = 0f;
        float duration = 0.12f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Pop slightly larger, then shrink to zero
            float s = Mathf.Lerp(1.25f, 0f, t);
            transform.localScale = origScale * s;
            yield return null;
        }

        Destroy(gameObject);
    }
}
