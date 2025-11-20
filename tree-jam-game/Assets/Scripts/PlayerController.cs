using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 15f;
    public float jumpForce = 2f;
    public LayerMask Ground;
    public float groundCheckDistance = 0.1f; // adjustable in inspector

    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;
    private bool doubleJump = true;

    private bool canDash = true;
    private bool isDashing;
    public float dashingPower = 24f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 1f;
    private float move;

    [SerializeField] private Animator _animator;
    [SerializeField] private TrailRenderer _trailRenderer;

    // -------------------- CLONE FIELDS (added) --------------------
    [SerializeField] private GameObject clonePrefab;
    [SerializeField] private float cloneMass = 5f;
    [SerializeField] private float cloneLinearDrag = 1f;
    [SerializeField] private Color cloneTint = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private string cloneGroundLayerName = "Ground";
    [SerializeField] private float cloneLifetime = 15f;
    [SerializeField] private int maxClones = 40;

    private int cloneGroundLayer = -1;
    // ---------------------------------------------------------------
    private Vector3 startingPosition;
    private Queue<GameObject> cloneQueue = new Queue<GameObject>();

    public float cameraShakeDuration = 0.3f;
    public float cameraShakeIntensity = 0.2f;
    public Camera mainCamera;
    private Vector3 originalCameraPosition;
    [SerializeField] private CinemachineImpulseSource impulseSource;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startingPosition = transform.position;

        // clone layer setup
        if (!string.IsNullOrEmpty(cloneGroundLayerName))
        {
            int resolvedLayer = LayerMask.NameToLayer(cloneGroundLayerName);
            if (resolvedLayer != -1)
            {
                cloneGroundLayer = resolvedLayer;
            }
        }

        if (mainCamera != null)
        {
            originalCameraPosition = mainCamera.transform.position;
        }
    }

    void Update()
    {
        if (isDashing)
        {
            return;
        }

        // Horizontal movement
        move = Input.GetAxis("Horizontal");

        // running animation
        if (move != 0)
        {
            spriteRenderer.flipX = move < 0f;
            _animator.SetBool("isRunning", true);
        }
        else
        {
            _animator.SetBool("isRunning", false);
        }

        // double jump reset
        if (isGrounded && !Input.GetButton("Jump"))
        {
            doubleJump = false;
        }

        rb.velocity = new Vector2(move * moveSpeed, rb.velocity.y);

        // Ground check
        RaycastHit2D hit = Physics2D.BoxCast(
            coll.bounds.center,
            new Vector2(coll.bounds.size.x * 0.9f, coll.bounds.size.y),
            0f,
            Vector2.down,
            groundCheckDistance,
            Ground
        );

        isGrounded = hit.collider != null;

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && (isGrounded || doubleJump))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);

            if (!isGrounded && doubleJump)
            {
                _animator.SetBool("isDoubleJumping", true);
            }
            else
            {
                _animator.SetBool("isDoubleJumping", false);
            }

            doubleJump = !doubleJump;
        }

        // Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }

        // -------------------- CLONE SPAWN KEY --------------------
        if (Input.GetKeyDown(KeyCode.C))
        {
            // StartCoroutine(CameraShake());
            SpawnClone();
        }
        // ----------------------------------------------------------

        // Restart button - reset player position but keep clones
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetPlayerPosition();
        }
    }

    // Visual debugging in Scene view
    void OnDrawGizmos()
    {
        if (coll == null) return;

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 boxSize = new Vector3(coll.bounds.size.x * 0.9f, groundCheckDistance, coll.bounds.size.z);
        Vector3 boxCenter = coll.bounds.center - new Vector3(0, coll.bounds.extents.y + groundCheckDistance / 2, 0);
        Gizmos.DrawWireCube(boxCenter, boxSize);
    }

    private IEnumerator Dash()
    {
        int dashDirection = 0;

        if (move < 0f)
        {
            dashDirection = -1;
        }
        else
        {
            dashDirection = 1;
        }

        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero; // cancel ANY existing speed
        rb.AddForce(new Vector2(dashDirection * dashingPower, 0f), ForceMode2D.Impulse);
        _trailRenderer.emitting = true;

        yield return new WaitForSeconds(dashingTime);
        _trailRenderer.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    // ----------------------------- CLONE FUNCTIONALITY -----------------------------
    private void SpawnClone()
    {
        // Camera shake when spawning a clone
        ShakeCamera();

        // Remove oldest clone if we've reached the maximum
        if (cloneQueue.Count >= maxClones)
        {
            GameObject oldestClone = cloneQueue.Dequeue();
            if (oldestClone != null)
            {
                Destroy(oldestClone);
            }
        }

        GameObject source = clonePrefab != null ? clonePrefab : gameObject;
        GameObject clone = Instantiate(source, transform.position, transform.rotation);
        clone.name = $"{gameObject.name}_Clone";

        clone.tag = "Untagged";

        // remove player script so clone is static
        PlayerController cloneController = clone.GetComponent<PlayerController>();
        if (cloneController != null)
        {
            Destroy(cloneController);
        }

        // remove trail
        TrailRenderer cloneTrail = clone.GetComponent<TrailRenderer>();
        if (cloneTrail != null)
        {
            Destroy(cloneTrail);
        }

        // adjust physics
        Rigidbody2D cloneRb = clone.GetComponent<Rigidbody2D>();
        if (cloneRb != null)
        {
            cloneRb.velocity = Vector2.zero;
            cloneRb.angularVelocity = 0f;
            cloneRb.gravityScale = rb.gravityScale;
            cloneRb.mass = cloneMass;
            cloneRb.drag = cloneLinearDrag;
            cloneRb.bodyType = RigidbodyType2D.Dynamic;
            cloneRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // collider on
        Collider2D cloneCollider = clone.GetComponent<Collider2D>();
        if (cloneCollider != null)
        {
            cloneCollider.enabled = true;
            cloneCollider.isTrigger = false;
        }

        // tint
        SpriteRenderer cloneSprite = clone.GetComponent<SpriteRenderer>();
        if (cloneSprite != null)
        {
            cloneSprite.color = cloneTint;
        }

        // set layer
        if (cloneGroundLayer >= 0)
        {
            SetLayerRecursively(clone.transform, cloneGroundLayer);
        }

        // Add clone to queue and start despawn coroutine
        cloneQueue.Enqueue(clone);
        StartCoroutine(DespawnCloneAfterDelay(clone, cloneLifetime));
    }

    private IEnumerator DespawnCloneAfterDelay(GameObject clone, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (clone != null)
        {
            // Rebuild queue without this clone
            Queue<GameObject> newQueue = new Queue<GameObject>();
            while (cloneQueue.Count > 0)
            {
                GameObject queuedClone = cloneQueue.Dequeue();
                if (queuedClone != null && queuedClone != clone)
                {
                    newQueue.Enqueue(queuedClone);
                }
            }
            cloneQueue = newQueue;

            Destroy(clone);
        }
    }

    private void ResetPlayerPosition()
    {
        // Reset player position to starting position
        transform.position = startingPosition;

        // Reset velocity
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private void SetLayerRecursively(Transform target, int layer)
    {
        target.gameObject.layer = layer;
        for (int i = 0; i < target.childCount; i++)
        {
            SetLayerRecursively(target.GetChild(i), layer);
        }
    }

    private void ShakeCamera()
    {
        Debug.Log("ShakeCamera called!");

        if (impulseSource != null)
        {
            Debug.Log("Generating impulse with source: " + impulseSource.name);

            // Check if any listeners exist
            var listeners1 = FindObjectsOfType<CinemachineImpulseListener>();
            var listeners2 = FindObjectsOfType<CinemachineIndependentImpulseListener>();
            Debug.Log("Found " + listeners1.Length + " CinemachineImpulseListener");
            Debug.Log("Found " + listeners2.Length + " CinemachineIndependentImpulseListener");

            impulseSource.GenerateImpulseWithForce(1f);
        }
        // else
        // {
        //     Debug.LogError("Impulse Source is NULL!");
        // }
    }
    // ------------------------------------------------------------------------------
}
