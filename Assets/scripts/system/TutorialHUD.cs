using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using System;

public class TutorialHUD : MonoBehaviour
{
    public static Action OnElevatorLanded;

    [System.Serializable]
    public struct TutorialStep
    {
        public string actionName;
        public string inputActionKey;
        public Sprite keyboardSprite;
        public Sprite gamepadSprite;
    }

    [Header("Sequence")]
    [SerializeField] private TutorialStep[] steps;
    private int currentStepIndex = 0;

    [Header("UI References")]
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private Image inputIcon;
    [SerializeField] private TextMeshProUGUI actionText;    
    [SerializeField] private TextMeshProUGUI impactText;    
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation Settings (Slide)")]
    [SerializeField] private float slideDuration = 0.8f;
    [SerializeField] private Vector2 offScreenOffset = new Vector2(0, -200);

    [Header("Impact Settings")]
    [SerializeField] private float impactDuration = 0.15f;
    [SerializeField] private float impactScaleMult = 1.2f;
    [SerializeField] private Color impactColor = new Color(0.93f, 0.88f, 0.62f); 

    private Color originalImpactTextColor;
    private Vector3 originalScale;

    private Vector2 targetAnchoredPos;
    private PlayerInput playerInput;
    private bool tutorialComplete = false;
    private bool elevatorHasLanded = false;
    private Coroutine impactCoroutine;

    void Awake()
    {
        if (panelRect != null)
        {
            targetAnchoredPos = panelRect.anchoredPosition;
            panelRect.anchoredPosition = targetAnchoredPos + offScreenOffset;
            originalScale = panelRect.localScale;
        }

        if (impactText != null) originalImpactTextColor = impactText.color;

        canvasGroup.alpha = 0;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerInput = player.GetComponent<PlayerInput>();
            UpdateStepVisuals();
        }

        OnElevatorLanded += StartTutorialSequence;
    }

    void Start()
    {
        if (playerInput == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerInput = player.GetComponent<PlayerInput>();
        }

        if (GameplayManager.Instance != null && GameplayManager.Instance.currentLayer > 1)
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            UpdateStepVisuals();
        }
    }

    private void OnDestroy()
    {
        OnElevatorLanded -= StartTutorialSequence;
    }

    private void StartTutorialSequence()
    {
        if (this.gameObject.activeInHierarchy && !elevatorHasLanded)
        {
            UpdateStepVisuals();
            StartCoroutine(ManualSlideIn());
            inputIcon.SetNativeSize();
        }
    }

    private IEnumerator ManualSlideIn()
    {
        float elapsed = 0;
        Vector2 startPos = panelRect.anchoredPosition;

        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / slideDuration;

            float easedT = 1f - Mathf.Pow(1f - t, 3f);

            panelRect.anchoredPosition = Vector2.Lerp(startPos, targetAnchoredPos, easedT);
            canvasGroup.alpha = t;
            yield return null;
        }

        panelRect.anchoredPosition = targetAnchoredPos;
        canvasGroup.alpha = 1;
        elevatorHasLanded = true;
    }

    void Update()
    {
        if (!elevatorHasLanded || tutorialComplete || playerInput == null) return;

        bool isGamepad = playerInput.currentControlScheme == "Gamepad" || playerInput.currentControlScheme == "Joystick";
        TutorialStep currentStep = steps[currentStepIndex];

        if (currentStep.actionName.ToUpper() == "AIM" && !isGamepad)
        {
            AdvanceStep(false);
            return;
        }

        bool stepConditionMet = false;
        if (currentStep.actionName.ToUpper() == "AIM")
        {
            Vector2 stick = playerInput.actions["Aim"].ReadValue<Vector2>();
            if (stick.magnitude > 0.5f) stepConditionMet = true;
        }
        else if (currentStep.inputActionKey == "Move")
        {
            if (playerInput.actions["Move"].ReadValue<Vector2>().magnitude > 0.5f)
                stepConditionMet = true;
        }
        else
        {
            if (playerInput.actions[currentStep.inputActionKey].WasPerformedThisFrame())
                stepConditionMet = true;
        }

        if (stepConditionMet) AdvanceStep(true);

        UpdateStepVisuals();
    }

    private void UpdateStepVisuals()
    {
        if (currentStepIndex >= steps.Length || playerInput == null) return;

        TutorialStep currentStep = steps[currentStepIndex];
        bool isGamepad = playerInput.currentControlScheme == "Gamepad" || playerInput.currentControlScheme == "Joystick";
        Sprite targetSprite = isGamepad ? currentStep.gamepadSprite : currentStep.keyboardSprite;

        if (inputIcon.sprite != targetSprite)
        {
            inputIcon.sprite = targetSprite;
            inputIcon.SetNativeSize();
        }

        if (actionText != null)
            actionText.text = currentStep.actionName;
    }

    void AdvanceStep(bool showImpact)
    {
        if (showImpact && this.gameObject.activeInHierarchy)
        {
            if (impactCoroutine != null) StopCoroutine(impactCoroutine);
            impactCoroutine = StartCoroutine(ObjectiveClearImpact());
        }

        currentStepIndex++;

        if (currentStepIndex >= steps.Length)
        {
            tutorialComplete = true;
            StartCoroutine(FadeOut(impactDuration));
        }
        else if (!showImpact)
        {
            UpdateStepVisuals();
        }
    }

    IEnumerator ObjectiveClearImpact()
    {
        float elapsed = 0;
        Vector3 targetScale = originalScale * impactScaleMult;

        if (impactText != null) impactText.color = impactColor;

        while (elapsed < impactDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / impactDuration;
            float easedT = 1f - Mathf.Pow(1f - t, 3f);

            panelRect.localScale = Vector3.Lerp(originalScale, targetScale, easedT);
            yield return null;
        }

        elapsed = 0;
        float returnDuration = impactDuration * 1.5f;

        while (elapsed < returnDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / returnDuration;
            float easedT = Mathf.Sin(t * Mathf.PI * 0.5f);

            panelRect.localScale = Vector3.Lerp(targetScale, originalScale, easedT);

            if (impactText != null)
                impactText.color = Color.Lerp(impactColor, originalImpactTextColor, t);

            yield return null;
        }

        panelRect.localScale = originalScale;
        if (impactText != null) impactText.color = originalImpactTextColor;

        UpdateStepVisuals();
        impactCoroutine = null;
    }

    IEnumerator FadeOut(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        float elapsed = 0;
        float duration = 0.5f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = 1f - (elapsed / duration);
            yield return null;
        }
        this.gameObject.SetActive(false);
    }
}