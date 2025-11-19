using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class StackOverflowManager : MonoBehaviour
{
    public static StackOverflowManager Instance { get; private set; }

    [Header("Stack Usage (0–1)")]
    [Range(0f, 1f)]
    public float currentRatio = 0f;

    [Tooltip("Slow automatic growth while playing (0.01 ~ 100s to fill).")]
    public float passiveIncreasePerSecond = 0.01f;

    [Header("Game Over")]
    public string gameOverSceneName = "GameOver";

    // Event sends currentRatio (0–1) to the UI
    public event Action<float> OnStackChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Notify();
    }

    void Update()
    {
        if (passiveIncreasePerSecond > 0f)
        {
            IncreaseStack(passiveIncreasePerSecond * Time.deltaTime);
        }
    }

    public void IncreaseStack(float amount)
    {
        if (amount <= 0f) return;

        currentRatio += amount;
        ClampAndCheck();
    }

    public void DecreaseStack(float amount)
    {
        if (amount <= 0f) return;

        currentRatio -= amount;
        ClampAndCheck();
    }

    void ClampAndCheck()
    {
        currentRatio = Mathf.Clamp01(currentRatio);
        Notify();

        if (currentRatio >= 1f)
        {
            HandleOverflow();
        }
    }

    void Notify()
    {
        if (OnStackChanged != null)
        {
            OnStackChanged(currentRatio);
        }
    }

    void HandleOverflow()
    {
        if (!string.IsNullOrEmpty(gameOverSceneName))
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}