using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;

    private CanvasGroup canvasGroup;
    private Image fadeImage;

    void Awake()
    {
        Instance = this;
        fadeImage = GetComponent<Image>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        fadeImage.color = Color.black;
    }

    void Start()
    {
        StartCoroutine(FadeRoutine(0f, 1.0f));
    }

    public void FadeToBlack(float duration) => StartFade(1f, duration);
    public void FadeToDim(float targetAlpha, float duration) => StartFade(targetAlpha, duration);

    private void StartFade(float targetAlpha, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(targetAlpha, duration));
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        canvasGroup.blocksRaycasts = targetAlpha > 0;

        float elapsed = 0;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        if (targetAlpha <= 0) canvasGroup.blocksRaycasts = false;
    }
}