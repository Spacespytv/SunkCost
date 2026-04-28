using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class HitEffects : MonoBehaviour
{
    public static HitEffects Instance;

    [Header("Flash Settings")]
    public Image hitFlashImage;
    public float flashAlpha = 0.4f;
    [SerializeField] private string finishHexColor = "#B45252";
    private Color finishColor;

    [Header("Post Processing")]
    public Volume globalVolume;
    private ChromaticAberration aberration;
    private float defaultAberration = 0.15f;

    [Header("Win Settings")]
    [SerializeField] private float winAberrationIntensity = 1.5f;
    [SerializeField] private float winFlashAlpha = 0.8f;
    [SerializeField] private float winFlashDuration = 0.5f;

    void Awake()
    {
        if (Instance == null) Instance = this;

        if (ColorUtility.TryParseHtmlString(finishHexColor, out Color parsedColor))
        {
            finishColor = parsedColor;
        }
        else
        {
            finishColor = new Color(0.705f, 0.321f, 0.321f);
        }

        if (globalVolume != null && globalVolume.profile.TryGet(out aberration))
        {
            aberration.intensity.value = defaultAberration;
        }
    }

    public void PlayHitFX()
    {
        StopAllCoroutines();
        StartCoroutine(HitFXRoutine());
    }

    private IEnumerator HitFXRoutine()
    {
        if (aberration != null) aberration.intensity.value = 1.0f;

        if (hitFlashImage != null)
        {
            Color c = finishColor;
            c.a = flashAlpha;
            hitFlashImage.color = c;
        }

        yield return new WaitForSecondsRealtime(0.1f);

        float elapsed = 0;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            if (aberration != null)
                aberration.intensity.value = Mathf.Lerp(1.0f, defaultAberration, t);

            if (hitFlashImage != null)
            {
                Color c = finishColor;
                c.a = Mathf.Lerp(flashAlpha, 0, t);
                hitFlashImage.color = c;
            }
            yield return null;
        }

        ResetVisuals();
    }

    public void SetDeathPinch()
    {
        if (aberration != null) aberration.intensity.value = 1.8f;

        if (hitFlashImage != null)
        {
            Color c = finishColor;
            c.a = 0.2f;
            hitFlashImage.color = c;
        }
    }

    public IEnumerator DeathFlashRoutine()
    {
        if (aberration != null) aberration.intensity.value = 2.0f;

        if (hitFlashImage != null)
        {
            Color c = finishColor;
            c.a = 0.5f;
            hitFlashImage.color = c;
        }

        yield return new WaitForSeconds(0.05f);

        float elapsed = 0;
        float duration = 1.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = 1 - Mathf.Pow(2, -10 * (elapsed / duration));

            if (aberration != null)
                aberration.intensity.value = Mathf.Lerp(2.0f, defaultAberration, t);

            if (hitFlashImage != null)
            {
                Color c = finishColor;
                c.a = Mathf.Lerp(0.5f, 0, t);
                hitFlashImage.color = c;
            }
            yield return null;
        }

        ResetVisuals();
    }

    public void PlayWinFlash()
    {
        StartCoroutine(WinFlashRoutine());
    }

    private IEnumerator WinFlashRoutine()
    {
        if (aberration != null) aberration.intensity.value = winAberrationIntensity;

        if (hitFlashImage != null)
        {
            hitFlashImage.color = new Color(1f, 1f, 1f, winFlashAlpha);
        }

        float elapsed = 0;
        while (elapsed < winFlashDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / winFlashDuration;
            float easeOut = 1 - (1 - t) * (1 - t);

            if (aberration != null)
                aberration.intensity.value = Mathf.Lerp(winAberrationIntensity, defaultAberration, easeOut);

            if (hitFlashImage != null)
            {
                Color c = Color.white;
                c.a = Mathf.Lerp(winFlashAlpha, 0, easeOut);
                hitFlashImage.color = c;
            }
            yield return null;
        }

        ResetVisuals();
    }

    private void ResetVisuals()
    {
        if (hitFlashImage != null)
        {
            Color c = finishColor;
            c.a = 0;
            hitFlashImage.color = c;
        }

        if (aberration != null)
        {
            aberration.intensity.value = defaultAberration;
        }
    }
}