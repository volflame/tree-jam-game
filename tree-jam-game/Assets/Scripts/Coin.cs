using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Stack Effect")]
    [Tooltip("How much of the bar to remove (0.05 = 5%).")]
    public float ratioReduction = 0.05f;

    [Header("Movement")]
    public Vector2 moveDirection = Vector2.right;
    public float moveDistance = 2f;
    public float moveSpeed = 1f;

    private Vector3 _startPos;

    void Start()
    {
        _startPos = transform.position;
        moveDirection = moveDirection.normalized;
    }

    void Update()
    {
        // Simple back-and-forth motion
        float t = Mathf.Sin(Time.time * moveSpeed);
        transform.position = _startPos + (Vector3)(moveDirection * moveDistance * t);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (StackOverflowManager.Instance != null)
        {
            StackOverflowManager.Instance.DecreaseStack(ratioReduction);
        }

        Destroy(gameObject);
    }
}