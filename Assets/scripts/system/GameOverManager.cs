using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    [Header("UI References")]
    [SerializeField] private Image hitFlashImage;
    [SerializeField] private RectTransform chalkboard;
    [SerializeField] private GameObject[] uiToHide;
    [SerializeField] private TextMeshProUGUI chalkboardLayerText;
    [SerializeField] private TextMeshProUGUI chalkboardBestText;

    [Header("Restart Prompt Settings")]
    [SerializeField] private Image restartPromptImage;
    [SerializeField] private Sprite gamepadRestartSprite;
    [SerializeField] private Sprite keyboardRestartSprite;
    [SerializeField] private float promptFadeSpeed = 5f;

    [Header("Settings")]
    [SerializeField] private float targetDimAlpha = 0.8f;
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private Vector2 chalkboardStartPos = new Vector2(0, 1200);
    [SerializeField] private Vector2 chalkboardEndPos = new Vector2(0, 0);
    [SerializeField] private float slideDuration = 0.8f;

    [SerializeField] private SceneFader sceneFaderObject;

    private bool canRestart = false;
    private PlayerInput playerInput;

    void Awake()
    {
        if (Instance == null) Instance = this;
        if (chalkboard != null) chalkboard.anchoredPosition = chalkboardStartPos;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerInput = player.GetComponent<PlayerInput>();

        if (restartPromptImage != null)
        {
            Color c = restartPromptImage.color;
            c.a = 0;
            restartPromptImage.color = c;
        }
    }

    void Update()
    {
        if (canRestart && restartPromptImage != null && playerInput != null)
        {
            bool isGamepad = playerInput.currentControlScheme == "Gamepad";
            Sprite targetSprite = isGamepad ? gamepadRestartSprite : keyboardRestartSprite;

            if (restartPromptImage.sprite != targetSprite)
            {
                restartPromptImage.sprite = targetSprite;
                restartPromptImage.SetNativeSize();
            }

            Color c = restartPromptImage.color;
            c.a = Mathf.MoveTowards(c.a, 1f, promptFadeSpeed * Time.unscaledDeltaTime);
            restartPromptImage.color = c;
        }
    }

    public void TriggerGameOver()
    {
        int layerReached = (GameplayManager.Instance != null) ? GameplayManager.Instance.currentLayer : 1;
        int previousBest = PlayerPrefs.GetInt("HighScoreLayer", 1);

        if (layerReached > previousBest)
        {
            PlayerPrefs.SetInt("HighScoreLayer", layerReached);
            PlayerPrefs.Save();
        }

        StartCoroutine(GameOverRoutine(layerReached, previousBest));

        HitEffects hitFX = FindFirstObjectByType<HitEffects>();
        if (hitFX != null) hitFX.StopAllCoroutines();
    }

    private IEnumerator GameOverRoutine(int layerReached, int bestLayer)
    {
        foreach (GameObject ui in uiToHide)
        {
            if (ui != null) ui.SetActive(false);
        }

        if (hitFlashImage != null)
        {
            hitFlashImage.gameObject.SetActive(true);
            float elapsed = 0;
            float startAlpha = hitFlashImage.color.a;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float currentAlpha = Mathf.Lerp(startAlpha, targetDimAlpha, elapsed / fadeDuration);
                hitFlashImage.color = new Color(0, 0, 0, currentAlpha);
                yield return null;
            }
            hitFlashImage.color = new Color(0, 0, 0, targetDimAlpha);
        }

        if (chalkboardLayerText != null)
            chalkboardLayerText.text = "LAYER: " + layerReached;

        if (chalkboardBestText != null)
        {
            if (layerReached > bestLayer)
            {
                chalkboardBestText.text = "BEST: " + layerReached + " <color=#EDE19E>NEW!</color>";
            }
            else
            {
                chalkboardBestText.text = "BEST: " + bestLayer;
            }
        }

        float slideElapsed = 0;
        while (slideElapsed < slideDuration)
        {
            slideElapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(slideElapsed / slideDuration);
            t = t * t * (3f - 2f * t); 
            chalkboard.anchoredPosition = Vector2.Lerp(chalkboardStartPos, chalkboardEndPos, t);
            yield return null;
        }

        canRestart = true;
    }

    public void RestartLevel()
    {
        if (canRestart)
        {
            canRestart = false;
            AudioManager.Instance.Play("Mission");
            StartCoroutine(FadeAndReload());
        }
    }

    private IEnumerator FadeAndReload()
    {
        if (sceneFaderObject != null)
        {
            sceneFaderObject.FadeOut();
        }

        yield return new WaitForSecondsRealtime(0.6f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResetHighScore()
    {
        PlayerPrefs.SetInt("HighScoreLayer", 1);
        PlayerPrefs.Save();
        if (chalkboardBestText != null) chalkboardBestText.text = "BEST: 1";
    }
}