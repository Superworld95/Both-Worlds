using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player 2 controller handling movement, jumping, attacking with sword, and health management.
/// Uses Unity's New Input System with Arrow keys and Enter for attack.
/// </summary>
public class Player2Controller : MonoBehaviour, IPlayerController
{
    [Header("Player Settings")]
    public string playerName = "Player 2"; // Display name for UI and GameManager
    public int maxHealth = 100;             // Maximum health value
    public float moveSpeed = 5f;            // Horizontal movement speed
    public float jumpForce = 12f;           // Upward force applied on jump

    [Header("Attack Settings")]
    public int attackDamage = 20;           // Damage dealt per attack
    public float attackCooldown = 0.5f;     // Minimum time between attacks in seconds
    public Transform attackPoint;           // Position from where attack range is calculated
    public float attackRange = 1.0f;        // Radius of the attack hit area
    public LayerMask enemyLayer;            // LayerMask to detect enemies in attack range

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private int currentHealth;
    private int kills = 0;
    private bool isGrounded = false;
    private float lastAttackTime = 0f;

    private Vector2 moveInput;

    // Double jump support
    private bool doubleJumpEnabled = false;
    private bool canDoubleJump = false;

    // Speed boost support
    private float speedBoostMultiplier = 1f;
    private float speedBoostTimer = 0f;

    // Attack boost support
    private float attackBoostMultiplier = 1f;
    private float attackBoostTimer = 0f;

    /// <summary>
    /// Initialization of components and starting health.
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        currentHealth = maxHealth;
    }

    /// <summary>
    /// Called every frame to update movement, animation, and powerup timers.
    /// </summary>
    private void Update()
    {
        HandleMovement();
        HandleAnimations();
        UpdatePowerupTimers();
    }

    #region Movement and Input Handling

    /// <summary>
    /// Called by input system when Move action is triggered.
    /// Stores movement input vector.
    /// </summary>
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// Called by input system when Jump action is triggered.
    /// Applies vertical velocity if grounded or double jump if enabled.
    /// </summary>
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                isGrounded = false;
                canDoubleJump = doubleJumpEnabled;
            }
            else if (canDoubleJump)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                canDoubleJump = false;
            }
        }
    }

    /// <summary>
    /// Called by input system when Attack action is triggered.
    /// Starts attack if cooldown allows.
    /// </summary>
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed && Time.time - lastAttackTime >= attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    /// <summary>
    /// Applies horizontal movement velocity and flips sprite accordingly.
    /// </summary>
    private void HandleMovement()
    {
        float horizontal = moveInput.x;
        float currentSpeed = moveSpeed * speedBoostMultiplier;
        rb.linearVelocity = new Vector2(horizontal * currentSpeed, rb.linearVelocity.y);

        if (horizontal > 0.1f)
            spriteRenderer.flipX = false;
        else if (horizontal < -0.1f)
            spriteRenderer.flipX = true;
    }

    /// <summary>
    /// Updates animator parameters for speed and grounded state.
    /// </summary>
    private void HandleAnimations()
    {
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool("IsGrounded", isGrounded);
    }

    /// <summary>
    /// Updates powerup timers for speed and attack boosts and resets multipliers when expired.
    /// </summary>
    private void UpdatePowerupTimers()
    {
        if (speedBoostTimer > 0)
        {
            speedBoostTimer -= Time.deltaTime;
            if (speedBoostTimer <= 0)
            {
                speedBoostMultiplier = 1f;
            }
        }

        if (attackBoostTimer > 0)
        {
            attackBoostTimer -= Time.deltaTime;
            if (attackBoostTimer <= 0)
            {
                attackBoostMultiplier = 1f;
            }
        }
    }

    /// <summary>
    /// Triggers attack animation and applies damage to enemies in range, factoring attack boost.
    /// </summary>
    private void Attack()
    {
        animator.SetTrigger("Attack");

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            EnemyAI enemy = enemyCollider.GetComponent<EnemyAI>();
            if (enemy != null && enemy.IsAlive())
            {
                int totalDamage = Mathf.RoundToInt(attackDamage * attackBoostMultiplier);
                enemy.TakeDamage(totalDamage, this);
            }
        }
    }

    /// <summary>
    /// Draws the attack range sphere in the editor for debugging.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }

    /// <summary>
    /// Checks collision with ground to update grounded status.
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    #endregion

    #region Health and Damage Management

    /// <summary>
    /// Applies damage to player health.
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        if (currentHealth == 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Handles player death state.
    /// </summary>
    private void Die()
    {
        Debug.Log($"{playerName} died!");
        enabled = false;
    }

    /// <summary>
    /// Returns if player is alive.
    /// </summary>
    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    /// <summary>
    /// Gets current health value.
    /// </summary>
    public int GetHealth() => currentHealth;

    #endregion

    #region Kill Tracking

    /// <summary>
    /// Increment kill count.
    /// </summary>
    public void AddKill()
    {
        kills++;
    }

    /// <summary>
    /// Returns kill count.
    /// </summary>
    public int GetKills()
    {
        return kills;
    }

    /// <summary>
    /// Returns player display name.
    /// </summary>
    public string GetPlayerName()
    {
        return playerName;
    }

    /// <summary>
    /// Resets health, kills and re-enables player.
    /// </summary>
    public void ResetPlayer()
    {
        currentHealth = maxHealth;
        kills = 0;
        enabled = true;
        //Resets player position to start position (assumes initial position stored)
        transform.position = Vector3.zero; // Change Vector3.zero to desired start position if needed
    }

    #endregion

    #region IPlayerController Implementation

    public void EnableDoubleJump(bool enabled)
    {
        doubleJumpEnabled = enabled;
        if (!enabled)
            canDoubleJump = false;
    }

    public void SetSpeedBoost(float multiplier, float duration)
    {
        speedBoostMultiplier = multiplier;
        speedBoostTimer = duration;
    }

    public void SetAttackBoost(float multiplier, float duration)
    {
        attackBoostMultiplier = multiplier;
        attackBoostTimer = duration;
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    #endregion
}
