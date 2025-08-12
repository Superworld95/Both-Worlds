using UnityEngine;

/// <summary>
/// AI controller for enemy entities featuring intelligent combat behavior and patrol systems.
/// Enemies attack players when in range and patrol designated areas when alone.
/// Supports three enemy types: Basic, Fast, and Heavy with different stats and behaviors.
/// </summary>
public class EnemyAI : MonoBehaviour
{
    // Different enemy types with varying stats and behaviors
    public enum EnemyType { Basic, Fast, Heavy }

    [Header("Enemy Settings")]
    public EnemyType enemyType = EnemyType.Basic; // Type determines stats and behavior
    public float attackRange = 40f; // Distance at which enemy will attack players
    public int attackDamage = 15; // Damage dealt to players per attack
    public float patrolRange = 100f; // Distance enemy will patrol from spawn point
    public Color enemyColor = Color.red; // Visual color (changes based on enemy type)

    [Header("Visual")]
    public GameObject swordEffect; // Visual effect prefab for enemy sword attacks

    //Component references
    private Rigidbody2D rb; // Physics body for movement
    private SpriteRenderer spriteRenderer; // Visual sprite component
    private BoxCollider2D boxCollider; // Collision detection component

    // Enemy state variables
    private int health; // Current health (set by enemy type)
    private int maxHealth; // Maximum health (set by enemy type)
    private bool isAlive = true; // Whether enemy is alive
    private bool isAttacking = false; // Whether currently attacking
    private float lastAttackTime = 0f; // Time of last attack for cooldown
    private float attackCooldown; // Time between attacks (set by enemy type)
    private float moveSpeed; // Movement speed (set by enemy type)
    private float patrolStartX; // Starting X position for patrol behavior
    private int patrolDirection = 1; // Current patrol direction (1 = right, -1 = left)

    // Player references for AI targeting
    private Player1Controller player1; // Reference to Player 1 for targeting
    private Player2Controller player2; // Reference to Player 2 for targeting

    /// <summary>
    /// Initialize enemy components, find players, and configure enemy type
    /// </summary>
    private void Start()
    {
        // Get required components
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();

        // Find player controllers for AI targeting
        player1 = FindObjectOfType<Player1Controller>();
        player2 = FindObjectOfType<Player2Controller>();
        patrolStartX = transform.position.x; // Remember spawn position for patrol

        // Configure stats based on enemy type and set visual appearance
        SetEnemyType(enemyType);
        if (spriteRenderer != null)
            spriteRenderer.color = enemyColor;
    }

    /// <summary>
    /// Main AI update loop - handles behavior and visual updates
    /// Only runs when game is active and enemy is alive
    /// </summary>
    private void Update()
    {
        // Null check for GameManager Instance
        if (GameManager.Instance == null || !GameManager.Instance.IsGameActive() || !isAlive) return;

        UpdateAI(); // Handle combat and patrol behavior
        UpdateVisuals(); // Update health-based color changes
    }

    /// <summary>
    /// Configure enemy stats and appearance based on enemy type
    /// Basic: Balanced stats, red color
    /// Fast: Low health, high speed/attack rate, orange color
    /// Heavy: High health, slow speed/attack rate, purple color
    /// </summary>
    public void SetEnemyType(EnemyType type)
    {
        enemyType = type;

        switch (type)
        {
            case EnemyType.Basic:
                maxHealth = 50;
                health = maxHealth;
                attackCooldown = 1.5f;
                moveSpeed = 20f;
                patrolRange = 100f;
                enemyColor = Color.red;
                break;

            case EnemyType.Fast:
                maxHealth = 30;
                health = maxHealth;
                attackCooldown = 1f;
                moveSpeed = 30f;
                patrolRange = 150f;
                enemyColor = new Color(1f, 0.67f, 0.27f);
                break;

            case EnemyType.Heavy:
                maxHealth = 80;
                health = maxHealth;
                attackCooldown = 2f;
                moveSpeed = 15f;
                patrolRange = 80f;
                enemyColor = new Color(0.67f, 0.27f, 1f);
                break;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = enemyColor;
    }

    private void UpdateAI()
    {
        // Find closest player
        Component closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        if (player1 != null && player1.IsAlive())
        {
            float dist = Vector2.Distance(transform.position, player1.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestPlayer = player1;
            }
        }

        if (player2 != null && player2.IsAlive())
        {
            float dist = Vector2.Distance(transform.position, player2.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestPlayer = player2;
            }
        }

        // Reset attack state
        isAttacking = false;

        // Combat behavior
        if (closestPlayer != null && closestDistance <= attackRange)
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                Attack(closestPlayer);
            }
            return;
        }

        // Patrol behavior
        Patrol();
    }

    private void Attack(Component target)
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Show sword effect
        if (swordEffect != null)
        {
            GameObject effect = Instantiate(swordEffect, transform.position, Quaternion.identity);
            effect.GetComponent<SpriteRenderer>().color = Color.yellow;
            Destroy(effect, 0.3f);
        }

        // Deal damage
        if (target is Player1Controller p1) p1.TakeDamage(attackDamage);
        if (target is Player2Controller p2) p2.TakeDamage(attackDamage);

        Invoke(nameof(ResetAttack), 0.2f);
    }

    private void ResetAttack()
    {
        isAttacking = false;
    }

    private void Patrol()
    {
        float distanceFromStart = Mathf.Abs(transform.position.x - patrolStartX);

        // Reverse direction at patrol limits
        if (distanceFromStart >= patrolRange)
        {
            patrolDirection *= -1;
        }

        // Apply horizontal movement
        Vector2 velocity = rb.linearVelocity;
        velocity.x = patrolDirection * moveSpeed;
        rb.linearVelocity = velocity;

        // Flip sprite
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * patrolDirection;
        transform.localScale = scale;
    }

    public void TakeDamage(int damage)
    {
        if (!isAlive) return;

        health = Mathf.Max(0, health - damage);

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isAlive = false;

        if (spriteRenderer != null)
            spriteRenderer.color = Color.gray;

        rb.linearVelocity = Vector2.zero;

        if (AudioManager.Instance != null)
            AudioManager.Instance.OnEnemyDeath();
    }

    private void UpdateVisuals()
    {
        if (!isAlive) return;

        float healthPercent = (float)health / maxHealth;
        Color color = Color.Lerp(Color.black, enemyColor, healthPercent);

        if (spriteRenderer != null)
            spriteRenderer.color = color;
    }

    // Public accessor methods for GameManager and player interactions
    public bool IsAlive() => isAlive;
    public int GetHealth() => health;
    public int GetMaxHealth() => maxHealth;
    public bool IsAttacking() => isAttacking;
    public EnemyType GetEnemyType() => enemyType;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Handle collision with players (contact damage)
        Player1Controller p1 = collision.gameObject.GetComponent<Player1Controller>();
        Player2Controller p2 = collision.gameObject.GetComponent<Player2Controller>();

        if (p1 != null && !isAttacking)
        {
            p1.TakeDamage(5);
        }
        else if (p2 != null && !isAttacking)
        {
            p2.TakeDamage(5);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw patrol range
        Gizmos.color = Color.blue;
        Vector3 patrolStart = new Vector3(patrolStartX != 0 ? patrolStartX : transform.position.x, transform.position.y, transform.position.z);
        Gizmos.DrawLine(patrolStart + Vector3.left * patrolRange, patrolStart + Vector3.right * patrolRange);
    }
}
