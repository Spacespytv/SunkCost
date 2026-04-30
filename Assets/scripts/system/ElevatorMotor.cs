using UnityEngine;
using System.Collections;

public class ElevatorMotor : MonoBehaviour
{
    public static ElevatorMotor Instance;

    [Header("Components")]
    [SerializeField] private Transform cogTransform;

    [Header("Settings")]
    [SerializeField] private float totalRotation = 720f;
    [SerializeField] private string motorSoundName = "ElevatorLoop";

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public IEnumerator DescendRoutine(float duration, float targetYOffset, bool playBellAtEnd, bool playMotor)
    {
        float elapsed = 0;
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(startPos.x, startPos.y - targetYOffset, startPos.z);

        float fadeTime = 0.5f;
        Sound motorSound = null;

        if (playMotor && AudioManager.Instance != null)
        {
            motorSound = AudioManager.Instance.GetSound(motorSoundName);
            if (motorSound != null)
            {
                motorSound.source.volume = 0;
                AudioManager.Instance.Play(motorSoundName);
                AudioManager.Instance.FadeSound(motorSoundName, motorSound.volume, fadeTime);
            }
        }

        float startRotation = cogTransform != null ? cogTransform.eulerAngles.z : 0;
        ElevatorCog cogScript = (cogTransform != null) ? cogTransform.GetComponent<ElevatorCog>() : null;
        if (cogScript != null) cogScript.isAutoRotating = false;

        bool hasTriggeredFadeOut = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPos, endPos, easedT);

            if (cogTransform != null)
            {
                float currentZ = Mathf.Lerp(startRotation, startRotation - totalRotation, easedT);
                cogTransform.eulerAngles = new Vector3(0, 0, currentZ);
            }

            if (playMotor && !hasTriggeredFadeOut && elapsed >= duration - fadeTime)
            {
                hasTriggeredFadeOut = true;
                AudioManager.Instance.FadeSound(motorSoundName, 0f, fadeTime);
            }

            yield return null;
        }

        transform.position = endPos;

        if (playBellAtEnd && AudioManager.Instance != null)
        {
            AudioManager.Instance.Play("Bell");
        }

        if (cogScript != null)
        {
            cogScript.SyncRotation();
            cogScript.isAutoRotating = true;
        }
    }
}