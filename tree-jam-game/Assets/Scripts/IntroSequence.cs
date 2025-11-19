using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroSequence : MonoBehaviour
{
    [Header("References")]
    public GameObject player;
    public Camera mainCamera;
    public CanvasGroup titleCanvasGroup;
    public Button startButton;
    public Button audioButton;
    public Image audioButtonIcon;
    public Sprite audioOnSprite;
    public Sprite audioOffSprite;

    [Header("Ghost Spawning")]
    public GameObject ghostPrefab;
    public Transform ghostSpawnArea;
    public float ghostSpawnInterval = 0.15f; 
    public float ghostSpawnRangeX = 10f;
    public float ghostSpawnHeight = 10f;
    public int maxGhosts = 20;

    [Header("Animation Settings")]
    public float playerFallDuration = 1.5f;
    public float playerStartHeight = 10f;
    public float cameraShakeDuration = 0.3f;
    public float cameraShakeIntensity = 0.2f;
    public float titleFadeInDelay = 0.5f;
    public float titleFadeInDuration = 1f;
    public float buttonsFadeInDelay = 0.5f;
    public float buttonsFadeInDuration = 0.8f;

    [Header("Audio")]
    public AudioSource musicSource;
    public AudioClip backgroundMusic;
    public AudioClip startGameSound;

    [Header("Scene Transition")]
    public string nextSceneName = "Level1";

    private Vector3 playerLandingPosition;
    private Vector3 originalCameraPosition;
    private bool isTransitioning = false;
    private bool isMusicOn = true;

    void Start()
    {
        SetupPlayer();
        SetupCamera();
        SetupUI();
        SetupAudio();

        StartCoroutine(PlayIntroSequence());
    }

    void SetupPlayer()
    {
        if (player != null)
        {
            playerLandingPosition = player.transform.position;

            player.transform.position = new Vector3(
                playerLandingPosition.x,
                playerStartHeight,
                playerLandingPosition.z
            );

            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.gravityScale = 3f;
            }
        }
    }

    void SetupCamera()
    {
        if (mainCamera != null)
        {
            originalCameraPosition = mainCamera.transform.position;
        }
    }

    void SetupUI()
    {
        if (titleCanvasGroup != null)
        {
            titleCanvasGroup.alpha = 0;
        }

        if (startButton != null)
        {
            CanvasGroup startCG = startButton.GetComponent<CanvasGroup>();
            if (startCG == null)
            {
                startCG = startButton.gameObject.AddComponent<CanvasGroup>();
            }
            startCG.alpha = 0;
            startButton.onClick.AddListener(OnStartButtonClicked);
        }

        if (audioButton != null)
        {
            CanvasGroup audioCG = audioButton.GetComponent<CanvasGroup>();
            if (audioCG == null)
            {
                audioCG = audioButton.gameObject.AddComponent<CanvasGroup>();
            }
            audioCG.alpha = 0;
            audioButton.onClick.AddListener(ToggleAudio);
            UpdateAudioButtonIcon();
        }
    }

    void SetupAudio()
    {
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    IEnumerator ShakeUI(RectTransform uiElement, float duration, float intensity)
    {
        if (uiElement == null) yield break;

        Vector3 originalPos = uiElement.anchoredPosition;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float offsetX = Random.Range(-intensity, intensity);
            float offsetY = Random.Range(-intensity, intensity);

            uiElement.anchoredPosition = originalPos + new Vector3(offsetX, offsetY);

            yield return null;
        }

        uiElement.anchoredPosition = originalPos;
    }

    IEnumerator PlayIntroSequence()
    {
        // Wait for player to fall
        yield return StartCoroutine(PlayerFall());

        // Camera shake on landing
        StartCoroutine(CameraShake());

        // Shake UI elements (with null checks)
        if (titleCanvasGroup != null)
        {
            StartCoroutine(ShakeUI(titleCanvasGroup.GetComponent<RectTransform>(), 0.6f, 20f));
        }
        if (startButton != null)
        {
            StartCoroutine(ShakeUI(startButton.GetComponent<RectTransform>(), 0.6f, 20f));
        }
        if (audioButton != null)
        {
            StartCoroutine(ShakeUI(audioButton.GetComponent<RectTransform>(), 0.6f, 20f));
        }

        // Fade in title
        yield return new WaitForSeconds(titleFadeInDelay);
        StartCoroutine(FadeInCanvasGroup(titleCanvasGroup, titleFadeInDuration));

        // Start spawning ghosts
        StartCoroutine(SpawnGhosts());

        // Fade in buttons
        yield return new WaitForSeconds(buttonsFadeInDelay);
        if (startButton != null)
        {
            CanvasGroup startCG = startButton.GetComponent<CanvasGroup>();
            if (startCG != null)
            {
                StartCoroutine(FadeInCanvasGroup(startCG, buttonsFadeInDuration));
            }
        }
        if (audioButton != null)
        {
            CanvasGroup audioCG = audioButton.GetComponent<CanvasGroup>();
            if (audioCG != null)
            {
                StartCoroutine(FadeInCanvasGroup(audioCG, buttonsFadeInDuration));
            }
        }
    }

    IEnumerator PlayerFall()
    {
        if (player == null) yield break;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.gravityScale = 3f;
        }

        // Wait for player to land
        yield return new WaitForSeconds(playerFallDuration);
    }

    IEnumerator CameraShake()
    {
        if (mainCamera == null) yield break;

        float elapsed = 0f;

        while (elapsed < cameraShakeDuration)
        {
            elapsed += Time.deltaTime;
            float intensity = cameraShakeIntensity * (1 - elapsed / cameraShakeDuration);

            float offsetX = Random.Range(-intensity, intensity);
            float offsetY = Random.Range(-intensity, intensity);

            mainCamera.transform.position = originalCameraPosition + new Vector3(offsetX, offsetY, 0);
            yield return null;
        }

        mainCamera.transform.position = originalCameraPosition;
    }

    IEnumerator FadeInCanvasGroup(CanvasGroup group, float duration)
    {
        if (group == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        group.alpha = 1f;
    }

    IEnumerator SpawnGhosts()
    {
        int ghostCount = 0;

        while (!isTransitioning && ghostCount < maxGhosts)
        {
            SpawnSingleGhost();
            ghostCount++;
            yield return new WaitForSeconds(ghostSpawnInterval);
        }
    }
    void SpawnSingleGhost()
    {
        if (ghostPrefab == null)
        {
            Debug.LogWarning("IntroSequence: Ghost Prefab is not assigned!");
            return;
        }

        // Spawn relative to camera position, just above visible area
        Vector3 cameraPos = mainCamera != null ? mainCamera.transform.position : Vector3.zero;

        float randomX = Random.Range(-ghostSpawnRangeX, ghostSpawnRangeX);
        float spawnHeight = Random.Range(ghostSpawnHeight * 0.5f, ghostSpawnHeight); // Spawn at varying heights above

        // Spawn above the screen
        Vector3 spawnPosition = new Vector3(cameraPos.x + randomX, cameraPos.y + spawnHeight, 0);

        Debug.Log("Spawning ghost at: " + spawnPosition);

        GameObject ghost = Instantiate(ghostPrefab, spawnPosition, Quaternion.identity);

        // Set up Rigidbody2D for falling
        Rigidbody2D ghostRb = ghost.GetComponent<Rigidbody2D>();
        if (ghostRb == null)
        {
            ghostRb = ghost.AddComponent<Rigidbody2D>();
        }

        ghostRb.isKinematic = false;
        ghostRb.gravityScale = 0.3f; // CHANGED: Even gentler fall
        ghostRb.drag = 1f;
        ghostRb.velocity = Vector2.zero;
    }

    void OnStartButtonClicked()
    {
        if (isTransitioning) return;

        if (startGameSound != null && musicSource != null)
        {
            musicSource.PlayOneShot(startGameSound);
        }

        StartCoroutine(TransitionToGame());
    }

    void ToggleAudio()
    {
        isMusicOn = !isMusicOn;

        if (musicSource != null)
        {
            if (isMusicOn)
            {
                musicSource.UnPause();
            }
            else
            {
                musicSource.Pause();
            }
        }

        UpdateAudioButtonIcon();
    }

    void UpdateAudioButtonIcon()
    {
        if (audioButtonIcon != null && audioOnSprite != null && audioOffSprite != null)
        {
            audioButtonIcon.sprite = isMusicOn ? audioOnSprite : audioOffSprite;
        }
    }

    IEnumerator TransitionToGame()
    {
        isTransitioning = true;

        float fadeDuration = 0.5f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            if (titleCanvasGroup != null)
            {
                titleCanvasGroup.alpha = 1f - t;
            }
            if (startButton != null)
            {
                CanvasGroup startCG = startButton.GetComponent<CanvasGroup>();
                if (startCG != null)
                {
                    startCG.alpha = 1f - t;
                }
            }
            if (audioButton != null)
            {
                CanvasGroup audioCG = audioButton.GetComponent<CanvasGroup>();
                if (audioCG != null)
                {
                    audioCG.alpha = 1f - t;
                }
            }

            yield return null;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}