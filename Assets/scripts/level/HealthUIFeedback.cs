using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthUIFeedback : MonoBehaviour
{
    [Header("References")]
    public RectTransform healthGroup;
    public Image healthBarFill;
    public Image healthBarGhost;

    [Header("Position Settings")]
    public float hiddenY = -100f;
    public float visibleY = 50f;

    [Header("Timing")]
    public float popDuration = 0.3f;
    public float displayTime = 1.5f;
    public float fallDuration = 0.6f;
    public float ghostDelay = 0.4f;
    public float ghostDrainDuration = 0.4f;

    private AnimationCurve popCurve;
    private AnimationCurve fallCurve;

    private Coroutine moveRoutine;
    private Coroutine ghostRoutine;

    void Awake()
    {
        popCurve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.7f, 1.1f),
            new Keyframe(1f, 1f)
        );

        fallCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        if (healthGroup != null)
            healthGroup.anchoredPosition = new Vector2(healthGroup.anchoredPosition.x, hiddenY);

        healthGroup.gameObject.SetActive(false);
    }

    public void TriggerHit(float healthPercent)
    {
        healthGroup.gameObject.SetActive(true);

        if (healthBarGhost != null && healthBarGhost.fillAmount < healthBarFill.fillAmount)
        {
            healthBarGhost.fillAmount = healthBarFill.fillAmount;
        }

        if (healthBarFill != null) healthBarFill.fillAmount = healthPercent;

        if (ghostRoutine != null) StopCoroutine(ghostRoutine);
        ghostRoutine = StartCoroutine(DrainGhostBar(healthPercent));

        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(AnimateUI());
    }

    private IEnumerator AnimateUI()
    {
        yield return MoveProcess(visibleY, popDuration, popCurve);
        yield return new WaitForSeconds(displayTime);
        yield return MoveProcess(hiddenY, fallDuration, fallCurve);
        healthGroup.gameObject.SetActive(false);
    }

    private IEnumerator MoveProcess(float targetY, float duration, AnimationCurve curve)
    {
        float elapsed = 0;
        float startY = healthGroup.anchoredPosition.y;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float currentY = Mathf.LerpUnclamped(startY, targetY, curve.Evaluate(t));
            healthGroup.anchoredPosition = new Vector2(healthGroup.anchoredPosition.x, Mathf.Round(currentY));
            yield return null;
        }
        healthGroup.anchoredPosition = new Vector2(healthGroup.anchoredPosition.x, targetY);
    }

    private IEnumerator DrainGhostBar(float targetPercent)
    {
        float startPercent = (healthBarGhost != null) ? healthBarGhost.fillAmount : targetPercent;

        yield return new WaitForSeconds(ghostDelay);

        float elapsed = 0;
        while (elapsed < ghostDrainDuration)
        {
            elapsed += Time.deltaTime;
            if (healthBarGhost != null)
            {
                healthBarGhost.fillAmount = Mathf.Lerp(startPercent, targetPercent, elapsed / ghostDrainDuration);
            }
            yield return null;
        }

        if (healthBarGhost != null) healthBarGhost.fillAmount = targetPercent;
    }
}