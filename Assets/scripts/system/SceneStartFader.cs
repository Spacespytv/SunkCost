using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private float fadeOutDuration = 0.5f; 
    [SerializeField] private float initialDelay = 0.3f;

    private Image fadeImage;

    void Awake()
    {
        fadeImage = GetComponent<Image>();
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(0, 0, 0, 1f);
        }
    }

    void Start()
    {
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        yield return new WaitForSeconds(initialDelay);

        float elapsed = 0;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeInDuration);
            if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        gameObject.SetActive(false);
    }

    public void FadeOut()
    {
        gameObject.SetActive(true);
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        float elapsed = 0;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeOutDuration);
            if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, 1f);
    }
}