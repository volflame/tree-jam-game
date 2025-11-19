using UnityEngine;

public class SpikeHazard : MonoBehaviour
{
    [Header("Effect on Stack")]
    [Tooltip("How much of the bar to add (0.1 = +10%).")]
    public float ratioIncrease = 0.15f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (StackOverflowManager.Instance != null)
        {
            StackOverflowManager.Instance.IncreaseStack(ratioIncrease);
        }
    }
}