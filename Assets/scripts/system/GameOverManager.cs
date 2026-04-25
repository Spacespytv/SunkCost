using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    [Header("UI References")]
    [SerializeField] private Image hitFlashImage;
    [SerializeField] private RectTransform chalkboard;
    [SerializeField] private GameObject[] uiToHide;
    [SerializeField] private TextMeshProUGUI chalkboardLayerText;

    [Header("Settings")]
    [SerializeField] private float targetDimAlpha = 0.8f;
    [SerializeField] private float fadeDuration = 1.0f; // Added this back!
    [SerializeField] private Vector2 chalkboardStartPos = new Vector2(0, 1200);
    [SerializeField] private Vector2 chalkboardEndPos = new Vector2(0, 0);
    [SerializeField] private float slideDuration = 0.8f;

    private bool canRestart = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        if (chalkboard != null) chalkboard.anchoredPosition = chalkboardStartPos;
    }

    public void TriggerGameOver()
    {
        // Stop any competing hit flash routines
        HitEffects hitFX = FindFirstObjectByType<HitEffects>();
        if (hitFX != null) hitFX.StopAllCoroutines();

        int finalLayer = (GameplayManager.Instance != null) ? GameplayManager.Instance.currentLayer : 1;
        StartCoroutine(GameOverRoutine(finalLayer));
    }

    private IEnumerator GameOverRoutine(int layerReached)
    {
        // 1. FADE THE HITFLASH TO DIM BLACK
        if (hitFlashImage != null)
        {
            hitFlashImage.gameObject.SetActive(true);

            float elapsed = 0;
            float startAlpha = hitFlashImage.color.a;

            while (elapsed < fadeDuration)
            {
                // Use unscaledDeltaTime because Time.timeScale is likely 0
                elapsed += Time.unscaledDeltaTime;
                float currentAlpha = Mathf.Lerp(startAlpha, targetDimAlpha, elapsed / fadeDuration);

                hitFlashImage.color = new Color(0, 0, 0, currentAlpha);
                yield return null;
            }
            hitFlashImage.color = new Color(0, 0, 0, targetDimAlpha);
        }

        // 2. HIDE THE UI
        foreach (GameObject ui in uiToHide)
        {
            if (ui != null) ui.SetActive(false);
        }

        // 3. SLIDE THE CHALKBOARD
        if (chalkboardLayerText != null) chalkboardLayerText.text = "LAYER: " + layerReached;

        float slideElapsed = 0;
        while (slideElapsed < slideDuration)
        {
            slideElapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(slideElapsed / slideDuration);
            t = t * t * (3f - 2f * t); // SmoothStep

            chalkboard.anchoredPosition = Vector2.Lerp(chalkboardStartPos, chalkboardEndPos, t);
            yield return null;
        }

        canRestart = true;
    }

    public void RestartLevel()
    {
        if (canRestart)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}