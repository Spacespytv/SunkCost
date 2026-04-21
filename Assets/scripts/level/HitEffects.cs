using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class HitEffects : MonoBehaviour
{
    [Header("Flash Settings")]
    public Image hitFlashImage;
    public float flashAlpha = 0.4f;

    [Header("Post Processing")]
    public Volume globalVolume;
    private ChromaticAberration aberration;
    private float defaultAberration = 0.15f;

    void Awake()
    {
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
            Color c = hitFlashImage.color;
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
                Color c = hitFlashImage.color;
                c.a = Mathf.Lerp(flashAlpha, 0, t);
                hitFlashImage.color = c;
            }
            yield return null;
        }

        if (aberration != null) aberration.intensity.value = defaultAberration;
    }

    public void SetDeathPinch()
    {
        if (aberration != null) aberration.intensity.value = 1.8f;

        if (hitFlashImage != null)
        {
            Color c = hitFlashImage.color;
            c.a = 0.2f; 
            hitFlashImage.color = c;
        }
    }

    public IEnumerator DeathFlashRoutine()
    {
        if (aberration != null) aberration.intensity.value = 2.0f;

        if (hitFlashImage != null)
        {
            Color c = hitFlashImage.color;
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
                Color c = hitFlashImage.color;
                c.a = Mathf.Lerp(0.5f, 0, t);
                hitFlashImage.color = c;
            }
            yield return null;
        }

        if (aberration != null) aberration.intensity.value = defaultAberration;
    }
}