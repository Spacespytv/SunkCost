using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ElevatorInteraction : MonoBehaviour
{
    [Header("Input Action Reference")]
    [SerializeField] private InputActionReference completeAction;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer promptRenderer;
    [SerializeField] private Sprite arrowSprite;
    [SerializeField] private Sprite buttonSprite;      
    [SerializeField] private Sprite mouseButtonSprite; 

    [Header("Animation Settings")]
    [SerializeField] private float bobIntensity = 0.2f;
    [SerializeField] private float bobSpeed = 5f;

    private bool isPlayerInside = false;
    private bool isReady = false;
    private Vector3 originalPromptPos;
    private Coroutine bobCoroutine;
    private PlayerInput playerInput;

    void Awake()
    {
        originalPromptPos = promptRenderer.transform.localPosition;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerInput = player.GetComponent<PlayerInput>();
    }

    void OnEnable()
    {
        completeAction.action.performed += OnCompletePerformed;
    }

    void OnDisable()
    {
        completeAction.action.performed -= OnCompletePerformed;
    }

    private void OnCompletePerformed(InputAction.CallbackContext context)
    {
        if (isPlayerInside && isReady)
        {
            GameplayManager.Instance.StartExtraction();
            StopBobbing();
        }
    }

    void Start()
    {
        promptRenderer.enabled = false;
    }

    void Update()
    {
        if (GameplayManager.Instance.isExtracting)
        {
            StopBobbing();
            return;
        }

        if (GameplayManager.Instance.currentBattery >= GameplayManager.Instance.maxBattery)
        {
            if (!isReady)
            {
                isReady = true;
                promptRenderer.enabled = true;
                promptRenderer.sprite = arrowSprite;

                if (bobCoroutine == null)
                    bobCoroutine = StartCoroutine(BobPrompt());
            }
        }
        else
        {
            StopBobbing();
        }

        if (isPlayerInside && isReady)
        {
            promptRenderer.transform.localPosition = originalPromptPos;

            if (playerInput != null && playerInput.currentControlScheme == "Gamepad")
            {
                promptRenderer.sprite = buttonSprite;
            }
            else
            {
                promptRenderer.sprite = mouseButtonSprite;
            }
        }
        else if (isReady)
        {
            promptRenderer.sprite = arrowSprite;
        }
    }

    private IEnumerator BobPrompt()
    {
        while (true)
        {
            if (promptRenderer.sprite == arrowSprite)
            {
                float newY = originalPromptPos.y + Mathf.Sin(Time.time * bobSpeed) * bobIntensity;
                promptRenderer.transform.localPosition = new Vector3(originalPromptPos.x, newY, originalPromptPos.z);
            }
            yield return null;
        }
    }

    private void StopBobbing()
    {
        isReady = false;
        promptRenderer.enabled = false;
        if (bobCoroutine != null)
        {
            StopCoroutine(bobCoroutine);
            bobCoroutine = null;
        }
        promptRenderer.transform.localPosition = originalPromptPos;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerInside = false;
    }
}