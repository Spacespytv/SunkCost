using System.Collections;
using UnityEngine;

public class ElevatorCog : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float degreesPerEnergy = 20f;
    [SerializeField] private float rotationLerpSpeed = 5f;
    public bool isAutoRotating = true;

    [Header("Juice & FX")]
    [SerializeField] private SpriteRenderer flashSprite;
    [SerializeField] private float fadeSpeed = 5f; 
    [SerializeField] private float scalePulseAmount = 1.3f;
    [SerializeField] private float scaleReturnSpeed = 4f;

    private float targetZRotation;
    private Vector3 originalScale;
    private Coroutine fadeCoroutine;

    void Start()
    {
        targetZRotation = transform.eulerAngles.z;
        originalScale = transform.localScale;

        if (flashSprite != null)
        {
            Color c = flashSprite.color;
            c.a = 0;
            flashSprite.color = c;
            flashSprite.enabled = true; 
        }
    }

    void Update()
    {
        if (isAutoRotating)
        {
            float currentZ = Mathf.LerpAngle(transform.eulerAngles.z, targetZRotation, Time.deltaTime * rotationLerpSpeed);
            transform.eulerAngles = new Vector3(0, 0, currentZ);
        }

        transform.localScale = Vector3.MoveTowards(transform.localScale, originalScale, Time.deltaTime * scaleReturnSpeed);
    }

    public void AddEnergy()
    {
        targetZRotation += degreesPerEnergy;
        transform.localScale = originalScale * scalePulseAmount;

        if (flashSprite != null)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeFlash());
        }

        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.UpdateBattery(1f);
        }
    }

    private IEnumerator FadeFlash()
    {
        Color c = flashSprite.color;
        c.a = 1f;
        flashSprite.color = c;

        while (c.a > 0)
        {
            c.a -= Time.deltaTime * fadeSpeed;
            flashSprite.color = c;
            yield return null; 
        }

        c.a = 0;
        flashSprite.color = c;
    }
    public void SyncRotation()
    {
        targetZRotation = transform.eulerAngles.z;
    }
}