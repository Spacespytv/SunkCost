using UnityEngine;
using UnityEngine.InputSystem;

public class GunController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform gunPivot;
    [SerializeField] Transform gunVisual;
    [SerializeField] Transform playerArt;
    [SerializeField] Transform crosshairTransform; 

    [Header("Shooting Settings")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform muzzlePoint;
    [SerializeField] float fireRate = 0.2f;

    [Header("Visual Effects")]
    [SerializeField] GameObject muzzleFlashPrefab;
    [SerializeField] private CameraShake camShake;
    [SerializeField] private float fireShakePower = 0.1f;
    [SerializeField] private float fireShakeDuration = 0.1f;

    private Vector2 aimInput;
    private Camera cam;
    private bool isFiring;
    private float nextShotTime;
    private SpriteRenderer crosshairSprite;

    private void Awake()
    {
        cam = Camera.main;
        if (crosshairTransform != null)
        {
            crosshairSprite = crosshairTransform.GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void Update()
    {
        HandleInput();
        HandleAutoFire();
    }

    void HandleInput()
    {
        if (!this.enabled) return;

        Vector2 direction = Vector2.zero;

        if (aimInput.sqrMagnitude > 0.1f)
        {
            direction = aimInput;
        }
        else if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f)
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            mousePos.z = Mathf.Abs(cam.transform.position.z - transform.position.z);
            Vector3 mouseWorldPos = cam.ScreenToWorldPoint(mousePos);
            direction = (mouseWorldPos - gunPivot.position);
        }
        else if (crosshairTransform != null)
        {
            direction = (crosshairTransform.position - gunPivot.position);
        }

        if (direction != Vector2.zero)
        {
            RotateAndFlip(direction);
        }
    }

    void RotateAndFlip(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        gunPivot.rotation = Quaternion.Euler(0, 0, angle);

        Vector3 gunScale = Vector3.one;
        gunScale.y = (angle > 90 || angle < -90) ? -1f : 1f;
        gunVisual.localScale = gunScale;

        Vector3 playerScale = playerArt.localScale;
        if (angle > 90 || angle < -90) playerScale.x = -Mathf.Abs(playerScale.x);
        else playerScale.x = Mathf.Abs(playerScale.x);
        playerArt.localScale = playerScale;
    }

    void HandleAutoFire()
    {
        if (isFiring && Time.time >= nextShotTime)
        {
            FireBullet();
            nextShotTime = Time.time + fireRate;
        }
    }

    void FireBullet()
    {
        if (bulletPrefab != null && muzzlePoint != null)
        {
            Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
            AudioManager.Instance.Play("Gun");
            if (camShake != null) camShake.StartShake(fireShakeDuration, fireShakePower);
            if (muzzleFlashPrefab != null) Instantiate(muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation, muzzlePoint);
        }
    }

    public void Shoot(InputAction.CallbackContext context)
    {
        if (context.performed) isFiring = true;
        else if (context.canceled) isFiring = false;
    }

    public void Aim(InputAction.CallbackContext context)
    {
        aimInput = context.ReadValue<Vector2>();
    }

    public void SetControlState(bool active)
    {
        this.enabled = active;

        if (!active)
        {
            isFiring = false;
            aimInput = Vector2.zero;
            float resetAngle = (playerArt.localScale.x > 0) ? 0f : 180f;
            gunPivot.rotation = Quaternion.Euler(0, 0, resetAngle);
            Vector3 gunScale = gunVisual.localScale;
            gunScale.y = 1f;
            gunVisual.localScale = gunScale;
        }
    }
}