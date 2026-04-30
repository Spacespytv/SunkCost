using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected float health = 50f;
    [SerializeField] protected float contactDamage = 10f;
    [SerializeField] protected LayerMask playerLayer;
    private bool isDead = false; 

    [Header("Visuals & FX")]
    [SerializeField] protected SpriteRenderer sr;
    [SerializeField] protected string deathParticleName = "EnemyExplosion";
    [SerializeField] protected string deathSoundName = "Rat";
    [SerializeField] protected GameObject deathLightPrefab; 

    [Header("Camera Shake Settings")]
    [SerializeField] protected CameraShake camShake;
    [Space]
    [SerializeField] protected float hitShakePower = 0.1f;    
    [SerializeField] protected float hitShakeDuration = 0.05f;
    [Space]
    [SerializeField] protected float deathShakePower = 0.4f;  
    [SerializeField] protected float deathShakeDuration = 0.2f;

    [Header("Material Swap Flash")]
    [SerializeField] private Material redFlashMat;
    private Material originalMat;
    private Coroutine flashCoroutine;

    [Header("Drops")]
    [SerializeField] GameObject energyPrefab;
    [SerializeField] protected int energyDropAmount = 1; 
    [SerializeField] protected float dropSpread = 0.5f;   
    protected BoxCollider2D hurtbox;

    protected virtual void Start()
    {
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();

        if (sr != null)
        {
            originalMat = sr.material;
        }

        if (camShake == null)
        {
            camShake = Object.FindAnyObjectByType<CameraShake>();
        }

        BoxCollider2D[] colliders = GetComponents<BoxCollider2D>();
        foreach (var c in colliders)
        {
            if (c.isTrigger) hurtbox = c;
        }
    }

    protected virtual void Update()
    {
        HandleContactDamage();
    }

    protected void HandleContactDamage()
    {
        if (hurtbox == null) return;
        Collider2D hit = Physics2D.OverlapBox(hurtbox.bounds.center, hurtbox.size, 0f, playerLayer);

        if (hit != null && hit.CompareTag("Player"))
        {
            ProcessDamage(hit);
        }
    }

    private void ProcessDamage(Collider2D playerCollider)
    {
        PlayerHealth playerHealth = playerCollider.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage);
        }
    }

    public virtual void TakeDamage(float amount)
    {
        health -= amount;

        if (camShake != null && health > 0)
        {
            camShake.StartShake(hitShakeDuration, hitShakePower);
            AudioManager.Instance.Play("EnemyHit");
        }

        if (sr != null && redFlashMat != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(HitFlash());
        }

        if (health <= 0) Die();
    }

    private IEnumerator HitFlash()
    {
        sr.material = redFlashMat;
        yield return new WaitForSeconds(0.1f);
        sr.material = originalMat;
        flashCoroutine = null;
    }

    public virtual void Die(bool spawnDrops = true)
    {
        if (isDead) return;
        isDead = true;
        AudioManager.Instance.Play(deathSoundName);
        AudioManager.Instance.Play("Explosion");

        if (ParticleManager.Instance != null && !string.IsNullOrEmpty(deathParticleName))
        {
            ParticleManager.Instance.PlayEffect(deathParticleName, transform.position, Quaternion.identity);
        }

        if (camShake != null)
        {
            camShake.StartShake(deathShakeDuration, deathShakePower);
        }

        if (deathLightPrefab != null)
        {
            Instantiate(deathLightPrefab, transform.position, Quaternion.identity);
        }

        if (spawnDrops && energyPrefab != null)
        {
            for (int i = 0; i < energyDropAmount; i++)
            {
                Vector3 spawnOffset = new Vector3(
                    Random.Range(-dropSpread, dropSpread),
                    Random.Range(-dropSpread, dropSpread),
                    0
                );

                Instantiate(energyPrefab, transform.position + spawnOffset, Quaternion.identity);
            }
        }

        Destroy(gameObject);
    }

}