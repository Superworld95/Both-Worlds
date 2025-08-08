using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player 1 controller handling movement, combat, and input for the first player.
/// Uses WASD + Space controls through Unity's New Input System.
/// Features sword-based combat, health management, and character progression.
/// </summary>
public class Player1Controller : MonoBehaviour
{
    [Header("Player 1 Settings")]
    public string playerName = "Player 1"; // Display name for UI and victory messages
    public PlayerCharacter characterType = PlayerCharacter.Modern; // Theme: Modern vs Magical

    [Header("Movement")]
    public float moveSpeed = 200f; // Horizontal movement speed in units per second
    public float jumpForce = 400f; // Upward force applied when jumping
    public LayerMask groundLayerMask; // Which layers count as ground for jumping

    [Header("Combat")]
    public float attackRange = 50f; // Range of sword attacks in units
    public int attackDamage = 25; // Damage dealt to enemies per attack
    public float attackCooldown = 1f; // Time between attacks in seconds

    [Header("Visual")]
    public Color playerColor = Color.green; // Player's visual color (green for Player 1)
    public GameObject swordEffect; // Visual effect prefab for sword attacks

    [Header("Input Actions")]
    public InputActionReference moveAction; // WASD movement input from PlayerInputActions
    public InputActionReference jumpAction; // W key jump input from PlayerInputActions
    public InputActionReference attackAction; // Space key attack input from PlayerInputActions

    // Component references
    private Rigidbody2D rb; // Physics body for movement
    private SpriteRenderer spriteRenderer; // Visual sprite component
    private BoxCollider2D boxCollider; // Collision detection component

    // Player state variables
    private int health = 100; // Current health (0-100)
    private int kills = 0; // Number of enemies defeated
    private bool isAlive = true; // Whether player is alive
    private bool isGrounded = false; // Whether player is touching ground
    private bool facingRight = true; // Direction player is facing
    private float lastAttackTime = 0f; // Time of last attack for cooldown
    private bool isAttacking = false; // Whether currently in attack animation

    // Input handling
    private Vector2 moveInput; // Current movement input from New Input System

    // Character theme types for visual and thematic differences
    public enum PlayerCharacter { Modern, Magical }

    /// <summary>
    /// Initialize Player 1 components, set starting position, and enable input
    /// </summary>
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();

        spriteRenderer.color = playerColor;
        transform.position = new Vector3(50, 100, 0);

        EnableInputActions();
    }

    private void OnEnable() => EnableInputActions();
    private void OnDisable() => DisableInputActions();

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

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameActive() || !isAlive) return;

        HandleMovement();
        CheckGrounded();
        UpdateVisuals();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded && isAlive)
        {
            Jump();
        }
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed && Time.time - lastAttackTime >= attackCooldown && isAlive)
        {
            Attack();
        }
    }

    private void HandleMovement()
    {
        float horizontal = moveInput.x;

        Vector2 velocity = rb.linearVelocity;
        velocity.x = horizontal * moveSpeed * Time.deltaTime;
        rb.linearVelocity = velocity;

        if (horizontal > 0 && !facingRight) Flip();
        else if (horizontal < 0 && facingRight) Flip();
    }

    private void Jump()
    {
        rb.AddForce(Vector2.up * jumpForce);
        isGrounded = false;

        if (AudioManager.Instance != null)
            AudioManager.Instance.OnPlayerJump();
    }

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
                enemy.TakeDamage(attackDamage);
                if (!enemy.IsAlive())
                {
                    kills++;
                    GameManager.Instance.AddEnemyKill(this);
                }
            }
        }

        Invoke(nameof(ResetAttack), 0.2f);
    }

    private void ResetAttack() => isAttacking = false;

    private void CheckGrounded()
    {
        Vector2 boxSize = new Vector2(boxCollider.size.x * 0.8f, 0.1f);
        Vector2 boxCenter = (Vector2)transform.position + boxCollider.offset + Vector2.down * (boxCollider.size.y / 2 + 0.05f);

        isGrounded = Physics2D.OverlapBox(boxCenter, boxSize, 0f, groundLayerMask);
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void UpdateVisuals()
    {
        float healthPercent = health / 100f;
        Color color = Color.Lerp(Color.red, playerColor, healthPercent);
        spriteRenderer.color = color;
    }

    public void TakeDamage(int damage)
    {
        if (!isAlive) return;

        health = Mathf.Max(0, health - damage);

        if (AudioManager.Instance != null)
            AudioManager.Instance.OnPlayerDamage();

        if (health <= 0)
            Die();
    }

    private void Die()
    {
        isAlive = false;
        spriteRenderer.color = Color.gray;
        rb.linearVelocity = Vector2.zero;
    }

    public void ResetPlayer()
    {
        health = 100;
        kills = 0;
        isAlive = true;
        rb.linearVelocity = Vector2.zero;
        isGrounded = false;
        facingRight = true;
        isAttacking = false;
    }

    public void AddKill() => kills++;

    public bool IsAttacking() => isAttacking;
    public int GetPlayerId() => 1;
    public bool IsAlive() => isAlive;
    public int GetHealth() => health;
    public int GetKills() => kills;
    public string GetPlayerName() => playerName;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform"))
            isGrounded = true;

        if (collision.gameObject.CompareTag("Trap"))
            TakeDamage(20);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Powerup"))
        {
            Powerup powerup = other.GetComponent<Powerup>();
            if (powerup != null)
                powerup.Collect(this);
        }
    }

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
