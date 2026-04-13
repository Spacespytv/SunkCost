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

    [SerializeField] private SpriteRenderer playerSR;

    void Start()
    {
        currentHealth = maxHealth;
        if (playerSR == null) playerSR = GetComponentInChildren<SpriteRenderer>();
    }

    public void TakeDamage(float damage)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        Debug.Log("Player Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(BecomeInvincible());
        }
    }

    private IEnumerator BecomeInvincible()
    {
        isInvincible = true;
        float timer = 0;

        while (timer < invincibilityDuration)
        {
            playerSR.enabled = !playerSR.enabled;

            yield return new WaitForSeconds(blinkRate);
            timer += blinkRate;
        }

        playerSR.enabled = true; 
        isInvincible = false;
    }

    private void Die()
    {
        Debug.Log("Player Died!");
    }
}