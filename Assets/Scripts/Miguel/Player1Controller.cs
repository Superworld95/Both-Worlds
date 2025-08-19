using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// Player 1 controller handling movement, traversal mechanics (dash, wall climbing),
/// multi-jump via power-up, attacks, and health system.
/// Uses WASD + Space for controls with Unity's New Input System.
/// </summary>
public class Player1Controller : MonoBehaviour, IPlayerController
{
    [Header("Player Settings")]
    public string playerName = "Player 1";
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public int maxJumps = 1; // default single jump
    private int jumpsUsed = 0;
    private bool jumpConsumed;

    [Header("Dash Settings")]
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool canDash = true;
    private bool isDashing = false;

    [Header("Wall Mechanics")]
    public Transform wallCheck;
    public float wallCheckRadius = 0.2f;
    public LayerMask wallLayer;
    public float wallSlideSpeed = 2f;
    public Vector2 wallJumpPower = new Vector2(12f, 16f);
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool isWallJumping;
    private float wallJumpDirection;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;

    [Header("Input")]
    private Vector2 moveInput;
    private bool jumpPressed = false;

    [Header("State")]
    public bool IsAlive { get; private set; } = true;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (!IsAlive) return;

        CheckGround();
        CheckWall();
    }

    private void FixedUpdate()
    {
        if (!IsAlive || isDashing) return;

        HandleMovement();
        HandleJump();
        HandleWallSlide();
    }

    // ----------------- Input -----------------
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!IsAlive) return;

        if (context.started)
        {
            jumpPressed = true;
            jumpConsumed = false;
        }

        if (context.canceled)
            jumpConsumed = true;
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!IsAlive) return;

        if (context.started && canDash)
            StartCoroutine(Dash());
    }

    // ----------------- Movement -----------------
    private void HandleMovement()
    {
        if (!isWallJumping)
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    private void HandleJump()
    {
        if (jumpPressed && !jumpConsumed && jumpsUsed < maxJumps)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpsUsed++;
            jumpConsumed = true;
            jumpPressed = false;
        }
    }

    private void HandleWallSlide()
    {
        if (isWallSliding)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue));
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded)
        {
            jumpsUsed = 0;
            jumpConsumed = false;
        }
    }

    private void CheckWall()
    {
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);
        isWallSliding = isTouchingWall && !isGrounded && rb.linearVelocity.y < 0;
        if (isWallSliding)
            wallJumpDirection = -Mathf.Sign(transform.localScale.x);
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        float dashDir = moveInput.x != 0 ? moveInput.x : transform.localScale.x;
        rb.linearVelocity = new Vector2(dashDir * dashForce, 0f);

        yield return new WaitForSeconds(dashDuration);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private IEnumerator WallJump()
    {
        isWallJumping = true;
        rb.linearVelocity = new Vector2(0, 0);
        rb.AddForce(new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y), ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.2f);
        isWallJumping = false;
        jumpsUsed = 0;
    }

    // ----------------- Health -----------------
    public void TakeDamage(int damage)
    {
        if (!IsAlive) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        IsAlive = false;
        rb.linearVelocity = Vector2.zero;
        // Optional: disable renderer/collider
    }

    public int GetHealth() => currentHealth;

    // ----------------- IPlayerController -----------------
    public void EnableDoubleJump(bool enabled) => maxJumps = enabled ? 2 : 1;

    public void SetSpeedBoost(float multiplier, float duration) => StartCoroutine(SpeedBoost(multiplier, duration));

    private IEnumerator SpeedBoost(float multiplier, float duration)
    {
        float originalSpeed = moveSpeed;
        moveSpeed *= multiplier;
        yield return new WaitForSeconds(duration);
        moveSpeed = originalSpeed;
    }

    public void SetAttackBoost(float multiplier, float duration) { /* implement if attack logic exists */ }

    public void Heal(int amount)
    {
        if (!IsAlive) return;
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
    }

    public string GetPlayerName() => playerName;
}
