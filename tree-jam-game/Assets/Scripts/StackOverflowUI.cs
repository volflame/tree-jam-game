using UnityEngine;
using UnityEngine.UI;

public class StackOverflowUI : MonoBehaviour
{
    [Header("UI References")]
    public Slider stackSlider;
    public Image fillImage;

    [Header("Colors")]
    public Color normalColor = Color.green;
    public Color warningColor = Color.yellow;
    public Color dangerColor = Color.red;

    [Header("Thresholds")]
    [Range(0f, 1f)] public float warningThreshold = 0.5f;
    [Range(0f, 1f)] public float dangerThreshold = 0.8f;

    void Start()
    {
        if (stackSlider != null)
        {
            stackSlider.minValue = 0f;
            stackSlider.maxValue = 1f;
            stackSlider.value = 0f;
        }

        if (StackOverflowManager.Instance != null)
        {
            StackOverflowManager.Instance.OnStackChanged += HandleStackChanged;
            HandleStackChanged(StackOverflowManager.Instance.currentRatio);
        }
    }

    void OnDestroy()
    {
        if (StackOverflowManager.Instance != null)
        {
            StackOverflowManager.Instance.OnStackChanged -= HandleStackChanged;
        }
    }

    void HandleStackChanged(float ratio)
    {
        if (stackSlider != null)
        {
            stackSlider.value = ratio;
        }

        if (fillImage != null)
        {
            if (ratio >= dangerThreshold)
                fillImage.color = dangerColor;
            else if (ratio >= warningThreshold)
                fillImage.color = warningColor;
            else
                fillImage.color = normalColor;
        }
    }
}