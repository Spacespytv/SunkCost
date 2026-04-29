using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BatteryUI : MonoBehaviour
{
    [Header("Sprites (Size: 5)")]
    [SerializeField] private Sprite[] batteryStates;
    [SerializeField] private Image batteryImage;

    [Header("Flash Settings")]
    [SerializeField] private Image flashOverlay;
    [SerializeField] private float fadeSpeed = 4f;

    private int lastStateIndex = 0;
    private Coroutine flashRoutine;

    private void Start()
    {
        if (batteryImage == null) batteryImage = GetComponent<Image>();
        if (flashOverlay != null)
        {
            Color c = flashOverlay.color;
            c.a = 0f;
            flashOverlay.color = c;
        }
        UpdateUI(0, 100);
    }

    public void UpdateUI(float currentEnergy, float maxEnergy)
    {
        float percentage = Mathf.Clamp01(currentEnergy / maxEnergy);
        int newStateIndex = Mathf.FloorToInt(percentage * 4);
        if (percentage >= 1f) newStateIndex = 4;
        batteryImage.sprite = batteryStates[newStateIndex];
        batteryImage.SetNativeSize();

        if (newStateIndex > lastStateIndex)
        {
            TriggerFlash();
        }

        lastStateIndex = newStateIndex;
    }

    private void TriggerFlash()
    {
        if (flashOverlay == null) return;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FadeFlash());
        AudioManager.Instance.PlayRisingManual("Battery");
    }

    private IEnumerator FadeFlash()
    {
        Color c = flashOverlay.color;
        c.a = 1f;
        flashOverlay.color = c;

        while (c.a > 0)
        {
            c.a -= Time.deltaTime * fadeSpeed;
            flashOverlay.color = c;
            yield return null;
        }

        c.a = 0f;
        flashOverlay.color = c;
    }
}