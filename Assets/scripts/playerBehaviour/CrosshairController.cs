using UnityEngine;
using UnityEngine.InputSystem;

public class CrosshairController : MonoBehaviour
{
    [Header("Orbital Settings (Controller)")]
    [SerializeField] private float pinnedRadius = 3f;
    [SerializeField] private float fadeSpeed = 20f;
    [Range(0f, 1f)]
    [SerializeField] private float controllerDeadzone = 0.45f; 

    [Header("References")]
    [SerializeField] private SpriteRenderer orbitalCrosshair;
    [SerializeField] private SpriteRenderer mouseCursor;
    [SerializeField] private Transform playerTransform;

    private PlayerInput playerInput;
    private InputAction aimAction;
    private Vector2 lastMousePos;
    private float currentAlpha = 0f;
    private Vector2 lastValidDirection = Vector2.right;
    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        if (playerTransform != null)
        {
            playerInput = playerTransform.GetComponent<PlayerInput>();
            if (playerInput != null)
                aimAction = playerInput.actions.FindAction("Aim");
        }

        Cursor.visible = false;
    }

    void Update()
    {
        if (playerTransform == null) return;

        bool isUsingGamepad = playerInput.currentControlScheme == "Gamepad";

        Vector2 stickInput = Vector2.zero;
        if (aimAction != null) stickInput = aimAction.ReadValue<Vector2>();
        Vector2 currentMousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;

        if (isUsingGamepad)
        {
            mouseCursor.enabled = false;
            orbitalCrosshair.enabled = true;

            bool isCurrentlyAiming = stickInput.magnitude > controllerDeadzone;

            if (isCurrentlyAiming)
            {
                lastValidDirection = stickInput.normalized;
            }

            transform.position = (Vector2)playerTransform.position + (lastValidDirection * pinnedRadius);

            float targetAlpha = isCurrentlyAiming ? 1f : 0f;
            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
            orbitalCrosshair.color = new Color(1, 1, 1, currentAlpha);
        }
        else
        {
            orbitalCrosshair.enabled = false;
            mouseCursor.enabled = true;
            mouseCursor.color = new Color(1, 1, 1, 1f);

            Vector3 mouseWorldPos = cam.ScreenToWorldPoint(new Vector3(currentMousePos.x, currentMousePos.y, -cam.transform.position.z));
            transform.position = mouseWorldPos;

            lastMousePos = currentMousePos;
        }
    }
}