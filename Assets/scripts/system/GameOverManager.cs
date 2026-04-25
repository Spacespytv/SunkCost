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
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private Vector2 chalkboardStartPos = new Vector2(0, 1200);
    [SerializeField] private Vector2 chalkboardEndPos = new Vector2(0, 0);
    [SerializeField] private float slideDuration = 1.0f;

    private bool canRestart = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        if (chalkboard != null) chalkboard.anchoredPosition = chalkboardStartPos;
    }

    void Update()
    {
        if (canRestart && Input.GetButtonDown("Jump"))
        {
            RestartLevel();
        }
    }

    public void TriggerGameOver()
    {
        HitEffects hitFX = FindFirstObjectByType<HitEffects>();
        if (hitFX != null) hitFX.StopAllCoroutines();

        int finalLayer = 1;
        if (GameplayManager.Instance != null)
        {
            finalLayer = GameplayManager.Instance.currentLayer;
        }

        StartCoroutine(GameOverRoutine(finalLayer));
    }

    private IEnumerator GameOverRoutine(int layerReached)
    {
        if (hitFlashImage != null)
        {
            float currentAlpha = hitFlashImage.color.a;
            hitFlashImage.color = new Color(0, 0, 0, currentAlpha);
        }

        foreach (GameObject ui in uiToHide)
        {
            if (ui != null) ui.SetActive(false);
        }

        if (hitFlashImage != null)
        {
            float elapsed = 0;
            float startAlpha = hitFlashImage.color.a;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float newAlpha = Mathf.Lerp(startAlpha, targetDimAlpha, elapsed / fadeDuration);
                hitFlashImage.color = new Color(0, 0, 0, newAlpha);
                yield return null;
            }
        }

        if (chalkboardLayerText != null)
            chalkboardLayerText.text = "LAYER: " + layerReached;

        float slideElapsed = 0;
        while (slideElapsed < slideDuration)
        {
            slideElapsed += Time.deltaTime;
            float t = slideElapsed / slideDuration;
            t = t * t * (3f - 2f * t);

            chalkboard.anchoredPosition = Vector2.Lerp(chalkboardStartPos, chalkboardEndPos, t);
            yield return null;
        }

        canRestart = true;
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}