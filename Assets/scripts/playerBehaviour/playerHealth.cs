using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Visuals & Equipment")]
    [SerializeField] private SpriteRenderer playerSR;
    [SerializeField] private Color flashColor = Color.white;
    private Color originalColor;

    [Space]
    [SerializeField] private GameObject gunObject;
    [SerializeField] private GameObject hatTorchLight;

    [Header("Juice References")]
    [SerializeField] private HealthUIFeedback healthUI;
    [SerializeField] private HitEffects hitFX;
    [SerializeField] private Light2D supernovaLight;
    [SerializeField] private string deadParticleName = "deathParticle";

    [Header("Settings")]
    [SerializeField] private float hitStopDuration = 0.1f;
    [SerializeField] private float invincibilityDuration = 1.5f;
    [SerializeField] private float blinkRate = 0.1f;

    [Header("Death Settings")]
    [SerializeField] private float deathFreezeDuration = 0.35f;
    [SerializeField] private float deathShakePower = 1.2f;
    [SerializeField] private float lightFlashIntensity = 5f;

    [Header("Audio Settings")]
    [SerializeField] private string musicName = "MainMusic";
    [SerializeField] private float musicFadeDuration = 0.5f;

    [Header("Audio Warp Settings")]
    [SerializeField] private string mainMusicName = "MainMusic";
    [SerializeField] private float hitPitchDrop = 0.5f;
    [SerializeField] private float pitchRecoveryTime = 2f;

    private bool isInvincible = false;
    private bool isDead = false;

    private Rigidbody2D rb;
    private playerMovement movementScript;
    private BoxCollider2D[] allColliders;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        movementScript = GetComponent<playerMovement>();
        allColliders = GetComponents<BoxCollider2D>(); 

        if (playerSR == null) playerSR = GetComponentInChildren<SpriteRenderer>();
        if (playerSR != null) originalColor = playerSR.color;

        if (supernovaLight != null) supernovaLight.enabled = false;
    }

    public void TakeDamage(float damage)
    {
        if (isInvincible || isDead) return;

        currentHealth -= damage;
        AudioManager.Instance.Play("Hurt");
        AudioManager.Instance.Play("Slice");

        if (AudioManager.Instance != null)
        {
            Sound s = AudioManager.Instance.GetSound(mainMusicName);
            if (s != null) s.source.pitch = hitPitchDrop;

            AudioManager.Instance.ShiftMusicPitch(mainMusicName, 1.0f, pitchRecoveryTime);
        }

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (healthUI != null) healthUI.TriggerHit(currentHealth / maxHealth);
        if (hitFX != null) hitFX.PlayHitFX();
        if (CameraShake.Instance != null) CameraShake.Instance.StartShake(0.1f, 0.2f);

        StartCoroutine(HitStopRoutine(hitStopDuration));
        StartCoroutine(BecomeInvincible());
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (movementScript != null) movementScript.enabled = false;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        foreach (BoxCollider2D col in allColliders)
        {
            col.enabled = false;
        }

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        if (healthUI != null) healthUI.TriggerHit(0);
        if (hitFX != null) hitFX.SetDeathPinch();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeSound(musicName, 0f, musicFadeDuration);
        }

        AudioManager.Instance.Play("Build");

        if (hatTorchLight != null) hatTorchLight.SetActive(false);

        Time.timeScale = 0f;
        if (CameraShake.Instance != null)
            CameraShake.Instance.StartShake(deathFreezeDuration, deathShakePower);

        yield return new WaitForSecondsRealtime(deathFreezeDuration);

        Time.timeScale = 1f;

        if (hitFX != null) StartCoroutine(hitFX.DeathFlashRoutine());
        if (supernovaLight != null) StartCoroutine(LightFlashRoutine());

        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayEffect(deadParticleName, transform.position, Quaternion.identity);
        }

        if (gunObject != null) Destroy(gunObject);
        if (playerSR != null) playerSR.enabled = false;

        if (healthUI != null) StartCoroutine(healthUI.ForceDismiss());

        if (CameraShake.Instance != null)
            CameraShake.Instance.StartShake(1.5f, 0.5f);
        AudioManager.Instance.Play("PlayerDeath");
        AudioManager.Instance.Play("PlayerExplosion");

        yield return new WaitForSeconds(0.8f);

        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.TriggerGameOver();
        }

    }

    private IEnumerator LightFlashRoutine()
    {
        supernovaLight.enabled = true;
        supernovaLight.intensity = lightFlashIntensity;

        float elapsed = 0;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            supernovaLight.intensity = Mathf.Lerp(lightFlashIntensity, 0, elapsed / duration);
            yield return null;
        }

        supernovaLight.enabled = false;
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        if (playerSR != null) playerSR.color = flashColor;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        if (playerSR != null) playerSR.color = originalColor;
    }

    private IEnumerator BecomeInvincible()
    {
        isInvincible = true;
        float timer = 0;
        while (timer < invincibilityDuration)
        {
            if (playerSR != null) playerSR.enabled = !playerSR.enabled;
            yield return new WaitForSecondsRealtime(blinkRate);
            timer += blinkRate;
        }
        if (playerSR != null) playerSR.enabled = true;
        isInvincible = false;
    }
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        isInvincible = false;

        if (healthUI != null)
        {
            healthUI.TriggerHit(1f); 
        }

        if (playerSR != null)
        {
            playerSR.enabled = true;
            playerSR.color = originalColor;
        }
    }
}