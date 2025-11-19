using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 15f;
    public float jumpForce = 2f;
    public LayerMask Ground;
    public float groundCheckDistance = 0.1f; // adjustable in inspector

    [SerializeField] private GameObject clonePrefab;
    [SerializeField] private float cloneMass = 5f;
    [SerializeField] private float cloneLinearDrag = 1f;
    [SerializeField] private Color cloneTint = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private string cloneGroundLayerName = "Ground";

    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;
    private float move;
    private int cloneGroundLayer = -1;

    [SerializeField] private Animator _animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (!string.IsNullOrEmpty(cloneGroundLayerName))
        {
            int resolvedLayer = LayerMask.NameToLayer(cloneGroundLayerName);
            if (resolvedLayer != -1)
            {
                cloneGroundLayer = resolvedLayer;
            }
        }
    }

    void Update()
    {
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

        // Reset double jump when landing
        // Jump (single jump only)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            SpawnClone();
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

    private void SpawnClone()
    {
        GameObject source = clonePrefab != null ? clonePrefab : gameObject;
        GameObject clone = Instantiate(source, transform.position, transform.rotation);
        clone.name = $"{gameObject.name}_Clone";

        // Remove Player tag so clones don't trigger player-specific interactions (like portals)
        clone.tag = "Untagged";

        // Remove player-specific behaviour so the clone stays static
        PlayerController cloneController = clone.GetComponent<PlayerController>();
        if (cloneController != null)
        {
            Destroy(cloneController);
        }

        TrailRenderer cloneTrail = clone.GetComponent<TrailRenderer>();
        if (cloneTrail != null)
        {
            Destroy(cloneTrail);
        }

        Animator cloneAnimator = clone.GetComponent<Animator>();
        if (cloneAnimator != null)
        {
            cloneAnimator.enabled = false;
        }

        Rigidbody2D cloneRb = clone.GetComponent<Rigidbody2D>();
        if (cloneRb != null)
        {
            cloneRb.velocity = Vector2.zero;
            cloneRb.angularVelocity = 0f;
            cloneRb.gravityScale = rb != null ? rb.gravityScale : 1f;
            cloneRb.mass = cloneMass;
            cloneRb.drag = cloneLinearDrag;
            cloneRb.bodyType = RigidbodyType2D.Dynamic;
            cloneRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        Collider2D cloneCollider = clone.GetComponent<Collider2D>();
        if (cloneCollider != null)
        {
            cloneCollider.enabled = true;
            cloneCollider.isTrigger = false;
        }

        SpriteRenderer cloneSprite = clone.GetComponent<SpriteRenderer>();
        if (cloneSprite != null)
        {
            cloneSprite.color = cloneTint;
        }

        if (cloneGroundLayer >= 0)
        {
            SetLayerRecursively(clone.transform, cloneGroundLayer);
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
}