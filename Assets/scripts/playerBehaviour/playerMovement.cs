using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class playerMovement : MonoBehaviour
{
    [Header("Player Component References")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator anim;

    [Header("Player Movement")]
    [SerializeField] float acceleration = 20f;
    [SerializeField] float deceleration = 25f;
    [SerializeField] float maxSpeed = 8f;

    [Header("Player Jumping")]
    [SerializeField] float fallMultiplier = 2.5f;
    [SerializeField] float jumpingPower;
    [SerializeField] float lowJumpMultiplier = 2f;
    [SerializeField] float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;

    [Header("Grounding")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform groundCheck;

    [Header("Visuals & Juice")]
    [SerializeField] Transform artObject;
    [SerializeField] private ProceduralAnimator procAnim;
    private bool wasGrounded;

    private float horizontal;
    private float currentSpeed;
    private bool jumpHeld;
    void Start()
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
    }
    private void Update()
    {
        bool isGrounded = IsGrounded();
        jumpBufferCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0 && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
            jumpBufferCounter = 0;

            if (ParticleManager.Instance != null)
            {
                ParticleManager.Instance.PlayEffect("Jump", groundCheck.position, Quaternion.identity);
            }
        }

        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !jumpHeld)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }

        if (isGrounded && !wasGrounded)
        {
            if (procAnim != null) procAnim.ApplyLandingSquash();

            if (ParticleManager.Instance != null)
            {
                ParticleManager.Instance.PlayEffect("Land", groundCheck.position, Quaternion.identity);
            }
        }

        wasGrounded = isGrounded;
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        float targetSpeed = horizontal * maxSpeed;

        if (Mathf.Abs(horizontal) > 0.01f)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
        }

        rb.velocity = new Vector2(currentSpeed, rb.velocity.y);
    }

    void UpdateAnimations()
    {
        if (anim == null) return;

        float realMovementSpeed = Mathf.Abs(rb.velocity.x);
        anim.SetFloat("Speed", realMovementSpeed);
        anim.SetBool("isGrounded", IsGrounded());
        anim.SetFloat("verticalVelocity", rb.velocity.y);
    }

    #region PLAYER_CONTROLS

    public void Move(InputAction.CallbackContext context)
    {
        float input = context.ReadValue<Vector2>().x;
        horizontal = Mathf.Abs(input) > 0.1f ? Mathf.Sign(input) : 0f;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            jumpBufferCounter = jumpBufferTime;
            jumpHeld = true;
        }

        if (context.canceled)
        {
            jumpHeld = false;
            if (rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            }
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapBox(groundCheck.position, new Vector2(0.8f, 0.2f), 0f, groundLayer);
    }

    #endregion

    public void SetControlState(bool enabled)
    {
        this.enabled = enabled;
        if (!enabled)
        {
            horizontal = 0;
            currentSpeed = 0;
            rb.velocity = Vector2.zero; 
            if (anim != null) anim.SetFloat("Speed", 0); 
        }
    }
}