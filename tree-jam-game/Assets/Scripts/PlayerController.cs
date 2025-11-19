using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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
        if (isGrounded)
        {
            doubleJump = true;
            _animator.SetBool("isDoubleJumping", false);
        }

        // Jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                // First jump
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                Debug.Log("First jump executed!");
            }
            else if (doubleJump)
            {
                // Double jump
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                doubleJump = false; // Use up the double jump
                _animator.SetBool("isDoubleJumping", true);
                Debug.Log("Double jump executed!");
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
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
}