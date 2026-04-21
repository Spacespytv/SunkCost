using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("I-Frames")]
    [SerializeField] private float invincibilityDuration = 1.5f;
    [SerializeField] private float blinkRate = 0.1f;
    private bool isInvincible = false;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer playerSR;
    [SerializeField] private Color flashColor = Color.white;
    private Color originalColor;

    [Header("Camera Shake Settings")]
    [SerializeField] private float hitShakePower = 0.2f;
    [SerializeField] private float hitShakeDuration = 0.1f;
    [Space]
    [SerializeField] private float deathShakePower = 0.4f;
    [SerializeField] private float deathShakeDuration = 0.2f;

    [Header("Hit Stop Settings")]
    [SerializeField] private float hitStopDuration = 0.1f;
    private bool isHitStopping = false;

    [Header("UI Reference")]
    [SerializeField] private HealthUIFeedback healthUI;

    [Header("Juice")]
    [SerializeField] private HitEffects hitFX; 

    void Start()
    {
        currentHealth = maxHealth;

        if (playerSR == null) playerSR = GetComponentInChildren<SpriteRenderer>();
        if (playerSR != null) originalColor = playerSR.color;
    }

    public void TakeDamage(float damage)
    {
        if (isInvincible) return;

        if (hitFX != null) hitFX.PlayHitFX();

        currentHealth -= damage;

        if (healthUI != null)
        {
            float healthPercent = Mathf.Clamp01(currentHealth / maxHealth);
            healthUI.TriggerHit(healthPercent);
        }

        if (currentHealth > 0)
        {
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.StartShake(hitShakeDuration, hitShakePower);
            }

            StartCoroutine(HitStopRoutine(hitStopDuration));
            StartCoroutine(BecomeInvincible());
        }
        else
        {
            Die();
        }
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        if (isHitStopping) yield break;
        isHitStopping = true;

        if (playerSR != null) playerSR.color = flashColor;

        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = originalTimeScale;

        if (playerSR != null) playerSR.color = originalColor;

        isHitStopping = false;
    }

    private IEnumerator BecomeInvincible()
    {
        isInvincible = true;
        float timer = 0;

        if (playerSR != null) playerSR.enabled = true;

        while (timer < invincibilityDuration)
        {
            if (playerSR != null) playerSR.enabled = !playerSR.enabled;

            yield return new WaitForSecondsRealtime(blinkRate);
            timer += blinkRate;
        }

        if (playerSR != null) playerSR.enabled = true;
        isInvincible = false;
    }

    private void Die()
    {
        Debug.Log("Player Died!");

        if (CameraShake.Instance != null && deathShakePower > 0)
        {
            CameraShake.Instance.StartShake(deathShakeDuration, deathShakePower);
        }

    }
}