using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player 2 controller handling movement, combat, and input for the second player.
/// Uses Arrow Keys + Enter controls through Unity's New Input System.
/// Features sword-based combat, health management, and character progression.
/// </summary>
public class Player2Controller : MonoBehaviour
{
    [Header("Player 2 Settings")]
    public string playerName = "Player 2"; // Display name for UI and victory messages
    public PlayerCharacter characterType = PlayerCharacter.Magical; // Theme: Magical vs Modern

    [Header("Movement")]
    public float moveSpeed = 200f; // Horizontal movement speed in units per second
    public float jumpForce = 400f; // Upward force applied when jumping
    public LayerMask groundLayerMask; // Layers treated as ground for jumping

    [Header("Combat")]
    public float attackRange = 50f; // Radius of sword attack
    public int attackDamage = 25; // Base damage dealt per hit
    public float attackCooldown = 1f; // Time between attacks in seconds

    [Header("Visual")]
    public Color playerColor = Color.magenta; // Visual representation color
    public GameObject swordEffect; // Optional particle or slash effect prefab

    [Header("Input Actions")]
    public InputActionReference moveAction; // Arrow Keys movement input
    public InputActionReference jumpAction; // Up Arrow jump input
    public InputActionReference attackAction; // Enter key attack input

    // Component references
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;

    // State variables
    private int health = 100;
    private int kills = 0;
    private bool isAlive = true;
    private bool isGrounded = false;
    private bool facingRight = true;
    private float lastAttackTime = 0f;
    private bool isAttacking = false;

    // Input handling
    private Vector2 moveInput;

    // Character theme types for visual and thematic differences
    public enum PlayerCharacter { Modern, Magical }

    /// <summary>
    /// Initialize Player 2 components and input setup
    /// </summary>
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();

        // Initial setup
        spriteRenderer.color = playerColor;
        transform.position = new Vector3(100, 100, 0); // Player 2 starts at x=100

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
        // Null check for GameManager
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

        // Flip sprite if needed
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

    private void ResetAttack()
    {
        isAttacking = false;
    }

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
        spriteRenderer.color = playerColor;
        transform.position = new Vector3(100, 100, 0);
        rb.linearVelocity = Vector2.zero;
        isGrounded = false;
        facingRight = true;
        isAttacking = false;
    }

    public void AddKill() => kills++;

    // Public accessors for GameManager, UI, and powerup systems
    public bool IsAttacking() => isAttacking;
    public bool IsAlive() => isAlive;
    public int GetPlayerId() => 2;
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
