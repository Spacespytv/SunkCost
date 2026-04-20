using System.Collections;
using UnityEngine;

public class ElevatorCog : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float degreesPerEnergy = 20f;
    [SerializeField] private float rotationLerpSpeed = 5f;

    [Header("Juice & FX")]
    [SerializeField] private SpriteRenderer flashSprite; // Drag the White Sprite child here
    [SerializeField] private float flashDuration = 0.05f; // Very short for that "snap"
    [SerializeField] private float scalePulseAmount = 1.3f;
    [SerializeField] private float scaleReturnSpeed = 4f;

    private float targetZRotation;
    private Vector3 originalScale;

    void Start()
    {
        targetZRotation = transform.eulerAngles.z;
        originalScale = transform.localScale;

        if (flashSprite != null)
        {
            flashSprite.enabled = false;
        }
    }

    void Update()
    {
        float currentZ = Mathf.LerpAngle(transform.eulerAngles.z, targetZRotation, Time.deltaTime * rotationLerpSpeed);
        transform.eulerAngles = new Vector3(0, 0, currentZ);
        transform.localScale = Vector3.MoveTowards(transform.localScale, originalScale, Time.deltaTime * scaleReturnSpeed);
    }

    public void AddEnergy()
    {
        targetZRotation += degreesPerEnergy;
        transform.localScale = originalScale * scalePulseAmount;

        if (flashSprite != null)
        {
            StopCoroutine("FlashWhite");
            StartCoroutine("FlashWhite");
        }

        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayEffect("CogImpact", transform.position, Quaternion.identity);
        }

        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.UpdateBattery(1f);
        }
    }

    private IEnumerator FlashWhite()
    {
        flashSprite.enabled = true;
        yield return new WaitForSeconds(flashDuration);
        flashSprite.enabled = false;
    }
}