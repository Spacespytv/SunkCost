using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // For URP
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
        if (globalVolume.profile.TryGet(out aberration))
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
}