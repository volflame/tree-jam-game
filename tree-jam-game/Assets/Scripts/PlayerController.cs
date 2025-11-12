using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public LayerMask Ground;
    public float groundCheckDistance = 0.1f; // adjustable in inspector

    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;
    [SerializeField] private Animator _animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Horizontal movement
        float move = Input.GetAxis("Horizontal");
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

        // Ground check - cast slightly smaller box to avoid self-collision
        RaycastHit2D hit = Physics2D.BoxCast(
            coll.bounds.center,
            new Vector2(coll.bounds.size.x * 0.9f, coll.bounds.size.y), // slightly narrower
            0f,
            Vector2.down,
            groundCheckDistance,
            Ground
        );

        isGrounded = hit.collider != null;

        // Debug visualization (remove after fixing)
        Debug.Log($"IsGrounded: {isGrounded}, Hit: {(hit.collider != null ? hit.collider.name : "none")}");

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            Debug.Log("Jump executed!");
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
}