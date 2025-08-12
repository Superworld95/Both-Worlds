using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// Player 1 controller handling movement, combat, and input for the first player.
/// Uses WASD + Space controls through Unity's New Input System.
/// Features sword-based combat, health management, character progression, and double jump.
/// </summary>
public class Player1Controller : MonoBehaviour, IPlayerController
{
    [Header("Player 1 Settings")]
    public string playerName = "Player 1";               // Display name for UI and victory messages
    public PlayerCharacter characterType = PlayerCharacter.Modern; // Character theme type

    [Header("Movement")]
    public float baseMoveSpeed = 200f;                   // Base horizontal movement speed
    public float jumpForce = 400f;                        // Upward force applied when jumping
    public LayerMask groundLayerMask;                     // Layers considered as ground

    [Header("Combat")]
    public float attackRange = 50f;                       // Radius for sword attack hit detection
    public int baseAttackDamage = 25;                     // Base damage per attack
    public float attackCooldown = 1f;                     // Time between attacks (seconds)

    [Header("Visual")]
    public Color playerColor = Color.green;               // Player sprite tint color
    public GameObject swordEffect;                         // Optional sword slash particle effect prefab

    [Header("Input Actions")]
    public InputActionReference moveAction;               // Movement input (WASD)
    public InputActionReference jumpAction;               // Jump input (Space)
    public InputActionReference attackAction;             // Attack input (e.g., Left Ctrl)

    // Cached component references
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;

    // Player state variables
    private int health = 100;                              // Current health points
    private int kills = 0;                                 // Enemy kills count
    private bool isAlive = true;                           // Is player alive
    private bool isGrounded = false;                        // Is player touching ground
    private bool facingRight = true;                        // Is player facing right
    private float lastAttackTime = 0f;                      // Timestamp of last attack for cooldown
    private bool isAttacking = false;                        // Is player currently attacking

    // Movement input vector
    private Vector2 moveInput;

    // Variables for modified stats from powerups
    private float currentMoveSpeed;
    private int currentAttackDamage;
    private bool doubleJumpEnabled = false;                 // Double jump ability toggle

    private int jumpCount = 0;                               // Tracks jumps to support double jump

    // Enum for player character themes
    public enum PlayerCharacter { Modern, Magical }

    /// <summary>
    /// Initialization: cache components, set initial position and colors, enable input
    /// </summary>
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();

        spriteRenderer.color = playerColor;
        transform.position = new Vector3(50, 100, 0);      // Starting position for Player 1

        currentMoveSpeed = baseMoveSpeed;
        currentAttackDamage = baseAttackDamage;

        EnableInputActions();
    }

    private void OnEnable() => EnableInputActions();
    private void OnDisable() => DisableInputActions();

    /// <summary>
    /// Enable and subscribe to input actions
    /// </summary>
    private void EnableInputActions()
    {
        if (moveAction != null)
        {
            moveAction.action.Enable();
            moveAction.action.performed += OnMove;
            moveAction.action.canceled += OnMove;
        }

        if (jumpAction != null)
        {
            jumpAction.action.Enable();
            jumpAction.action.performed += OnJump;
        }

        if (attackAction != null)
        {
            attackAction.action.Enable();
            attackAction.action.performed += OnAttack;
        }
    }

    /// <summary>
    /// Disable and unsubscribe from input actions
    /// </summary>
    private void DisableInputActions()
    {
        if (moveAction != null)
        {
            moveAction.action.performed -= OnMove;
            moveAction.action.canceled -= OnMove;
            moveAction.action.Disable();
        }

        if (jumpAction != null)
        {
            jumpAction.action.performed -= OnJump;
            jumpAction.action.Disable();
        }

        if (attackAction != null)
        {
            attackAction.action.performed -= OnAttack;
            attackAction.action.Disable();
        }
    }

    /// <summary>
    /// Called every frame: handle movement, ground check, and update visuals
    /// </summary>
    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameActive() || !isAlive)
            return;

        HandleMovement();
        CheckGrounded();
        UpdateVisuals();
    }

    /// <summary>
    /// Handle horizontal movement based on player input
    /// </summary>
    private void HandleMovement()
    {
        float horizontal = moveInput.x;
        Vector2 velocity = rb.linearVelocity;
        velocity.x = horizontal * currentMoveSpeed * Time.deltaTime;
        rb.linearVelocity = velocity;

        // Flip sprite if needed depending on movement direction
        if (horizontal > 0 && !facingRight) Flip();
        else if (horizontal < 0 && facingRight) Flip();
    }

    /// <summary>
    /// Input callback for movement input (WASD)
    /// </summary>
    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// Input callback for jump input; supports double jump if enabled
    /// </summary>
    private void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isAlive)
        {
            if (isGrounded)
            {
                Jump();
                jumpCount = 1;
            }
            else if (doubleJumpEnabled && jumpCount < 2)
            {
                Jump();
                jumpCount++;
            }
        }
    }

    /// <summary>
    /// Executes jump by resetting vertical velocity and applying upward force
    /// </summary>
    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // Reset vertical velocity before jumping
        rb.AddForce(Vector2.up * jumpForce);
        isGrounded = false;

        if (AudioManager.Instance != null)
            AudioManager.Instance.OnPlayerJump();
    }

    /// <summary>
    /// Input callback for attack input; attacks if cooldown allows
    /// </summary>
    private void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed && Time.time - lastAttackTime >= attackCooldown && isAlive)
        {
            Attack();
        }
    }

    /// <summary>
    /// Perform sword attack: play effects, deal damage to enemies in range, handle kills
    /// </summary>
    private void Attack()
    {
        lastAttackTime = Time.time;
        isAttacking = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.OnPlayerAttack();

        if (swordEffect != null)
        {
            GameObject effect = Instantiate(swordEffect, transform.position, Quaternion.identity);
            Destroy(effect, 0.3f);
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (var hit in hitEnemies)
        {
            EnemyAI enemy = hit.GetComponent<EnemyAI>();
            if (enemy != null && enemy.IsAlive())
            {
                enemy.TakeDamage(currentAttackDamage);
                if (!enemy.IsAlive())
                {
                    kills++;
                    GameManager.Instance.AddEnemyKill(this);
                }
            }
        }

        Invoke(nameof(ResetAttack), 0.2f);
    }

    /// <summary>
    /// Reset attack state after short cooldown
    /// </summary>
    private void ResetAttack()
    {
        isAttacking = false;
    }

    /// <summary>
    /// Check if player is grounded using a small overlap box at feet
    /// </summary>
    private void CheckGrounded()
    {
        Vector2 boxSize = new Vector2(boxCollider.size.x * 0.8f, 0.1f);
        Vector2 boxCenter = (Vector2)transform.position + boxCollider.offset + Vector2.down * (boxCollider.size.y / 2 + 0.05f);

        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapBox(boxCenter, boxSize, 0f, groundLayerMask);

        // Reset jump count when landing
        if (isGrounded && !wasGrounded)
            jumpCount = 0;
    }

    /// <summary>
    /// Flip player sprite horizontally when changing facing direction
    /// </summary>
    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    /// <summary>
    /// Update player sprite color based on health percentage for visual feedback
    /// </summary>
    private void UpdateVisuals()
    {
        float healthPercent = health / 100f;
        Color color = Color.Lerp(Color.red, playerColor, healthPercent);
        spriteRenderer.color = color;
    }

    /// <summary>
    /// Receive damage and handle death if health reaches zero
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (!isAlive) return;

        health = Mathf.Max(0, health - damage);

        if (AudioManager.Instance != null)
            AudioManager.Instance.OnPlayerDamage();

        if (health <= 0)
            Die();
    }

    /// <summary>
    /// Set player dead state and change visuals
    /// </summary>
    private void Die()
    {
        isAlive = false;
        spriteRenderer.color = Color.gray;
        rb.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// Reset player state for new game or respawn
    /// </summary>
    public void ResetPlayer()
    {
        health = 100;
        kills = 0;
        isAlive = true;
        spriteRenderer.color = playerColor;
        transform.position = new Vector3(50, 100, 0);
        rb.linearVelocity = Vector2.zero;
        isGrounded = false;
        facingRight = true;
        isAttacking = false;
        currentMoveSpeed = baseMoveSpeed;
        currentAttackDamage = baseAttackDamage;
        doubleJumpEnabled = false;
        jumpCount = 0;
    }

    /// <summary>
    /// Increment kill count (called when enemy dies)
    /// </summary>
    public void AddKill() => kills++;

    // IPlayerController interface implementations

    public int GetHealth() => health;

    public void Heal(int amount)
    {
        health = Mathf.Min(100, health + amount);
    }

    public void EnableDoubleJump(bool enabled)
    {
        doubleJumpEnabled = enabled;
        if (!enabled) jumpCount = 0; // Reset jump count when double jump is disabled
    }

    public void SetSpeedBoost(float multiplier, float duration)
    {
        StopCoroutine("SpeedBoostRoutine");
        StartCoroutine(SpeedBoostRoutine(multiplier, duration));
    }

    private IEnumerator SpeedBoostRoutine(float multiplier, float duration)
    {
        currentMoveSpeed = baseMoveSpeed * multiplier;
        yield return new WaitForSeconds(duration);
        currentMoveSpeed = baseMoveSpeed;
    }

    public void SetAttackBoost(float multiplier, float duration)
    {
        StopCoroutine("AttackBoostRoutine");
        StartCoroutine(AttackBoostRoutine(multiplier, duration));
    }

    private IEnumerator AttackBoostRoutine(float multiplier, float duration)
    {
        currentAttackDamage = Mathf.RoundToInt(baseAttackDamage * multiplier);
        yield return new WaitForSeconds(duration);
        currentAttackDamage = baseAttackDamage;
    }

    /// <summary>
    /// Returns whether the player is currently attacking
    /// </summary>
    public bool IsAttacking() => isAttacking;

    /// <summary>
    /// Returns whether the player is alive
    /// </summary>
    public bool IsAlive() => isAlive;

    /// <summary>
    /// Returns the number of kills this player has
    /// </summary>
    public int GetKills() => kills;

    /// <summary>
    /// Returns the player display name
    /// </summary>
    public string GetPlayerName() => playerName;

    /// <summary>
    /// Detect collisions with ground, platforms, or traps
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform"))
            isGrounded = true;

        if (collision.gameObject.CompareTag("Trap"))
            TakeDamage(20);
    }

    /// <summary>
    /// Detect entering trigger colliders such as powerups
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Powerup"))
        {
            Powerup powerup = other.GetComponent<Powerup>();
            if (powerup != null)
                powerup.Collect(this);
        }
    }

    /// <summary>
    /// Draw debug gizmos for attack range and ground check area in editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (boxCollider != null)
        {
            Gizmos.color = Color.green;
            Vector2 boxSize = new Vector2(boxCollider.size.x * 0.8f, 0.1f);
            Vector2 boxCenter = (Vector2)transform.position + boxCollider.offset + Vector2.down * (boxCollider.size.y / 2 + 0.05f);
            Gizmos.DrawWireCube(boxCenter, boxSize);
        }
    }
}
