using UnityEngine;

/// <summary>
/// AI controller for enemy entities featuring intelligent combat behavior and patrol systems.
/// Enemies attack players when in range and patrol designated areas when alone.
/// Enemy stats and behavior are defined by EnemyTypeData ScriptableObjects.
/// </summary>
public class EnemyAI : MonoBehaviour
{
    [Header("Enemy Settings")]
    /// <summary>
    /// Reference to ScriptableObject holding stats and visual info for this enemy type
    /// </summary>
    public EnemyTypeData enemyTypeData;

    /// <summary>
    /// Distance at which enemy will attack players
    /// </summary>
    public float attackRange = 40f;

    [Header("Visual")]
    /// <summary>
    /// Visual effect prefab for enemy sword attacks
    /// </summary>
    public GameObject swordEffect;

    // Component references
    private Rigidbody2D rb;                 // Physics body for movement
    private SpriteRenderer spriteRenderer; // Visual sprite component
    private BoxCollider2D boxCollider;     // Collision detection component

    // Enemy state variables
    private int health;                     // Current health (set by enemyTypeData)
    private int maxHealth;                  // Maximum health (set by enemyTypeData)
    private bool isAlive = true;            // Whether enemy is alive
    private bool isAttacking = false;       // Whether currently attacking
    private float lastAttackTime = 0f;      // Time of last attack for cooldown
    private float attackCooldown;           // Time between attacks (set by enemyTypeData)
    private float moveSpeed;                // Movement speed (set by enemyTypeData)
    private float patrolRange;              // Distance enemy will patrol from spawn point (set by enemyTypeData)
    private int attackDamage;               // Damage dealt to players per attack (set by enemyTypeData)
    private Color enemyColor;               // Visual color (set by enemyTypeData)

    // Patrol state
    private float patrolStartX;             // Starting X position for patrol behavior
    private int patrolDirection = 1;       // Current patrol direction (1 = right, -1 = left)

    // Player references for AI targeting
    private Player1Controller player1;     // Reference to Player 1 for targeting
    private Player2Controller player2;     // Reference to Player 2 for targeting

    /// <summary>
    /// Initialize enemy components, find players, and configure enemy stats from ScriptableObject
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

        // Configure stats based on ScriptableObject data and set visual appearance
        if (enemyTypeData != null)
        {
            ApplyEnemyTypeData(enemyTypeData);
        }
        else
        {
            Debug.LogWarning($"EnemyAI ({gameObject.name}): EnemyTypeData not assigned!");
        }
    }

    /// <summary>
    /// Main AI update loop - handles behavior and visual updates
    /// Only runs when game is active and enemy is alive
    /// </summary>
    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameActive() || !isAlive)
            return;

        UpdateAI();       // Handle combat and patrol behavior
        UpdateVisuals();  // Update health-based color changes
    }

    /// <summary>
    /// Assign EnemyTypeData ScriptableObject and update stats and visuals accordingly
    /// </summary>
    /// <param name="data">EnemyTypeData ScriptableObject</param>
    public void SetEnemyTypeData(EnemyTypeData data)
    {
        enemyTypeData = data;

        if (enemyTypeData != null)
        {
            ApplyEnemyTypeData(enemyTypeData);
        }
    }

    /// <summary>
    /// Helper to apply data from EnemyTypeData to internal variables and visuals
    /// </summary>
    private void ApplyEnemyTypeData(EnemyTypeData data)
    {
        maxHealth = data.maxHealth;
        health = maxHealth;
        attackCooldown = data.attackCooldown;
        moveSpeed = data.moveSpeed;
        patrolRange = data.patrolRange;
        attackDamage = data.attackDamage;
        enemyColor = data.enemyColor;

        if (spriteRenderer != null)
            spriteRenderer.color = enemyColor;
    }

    /// <summary>
    /// Core AI logic:
    /// Finds closest alive player, attacks if in range with cooldown, else patrols
    /// </summary>
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

        // Reset attack state before deciding action
        isAttacking = false;

        // Combat behavior: attack if player is in range and cooldown elapsed
        if (closestPlayer != null && closestDistance <= attackRange)
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                Attack(closestPlayer);
            }
            return; // Skip patrol when attacking or in attack cooldown
        }

        // Patrol behavior when no player is in attack range
        Patrol();
    }

    /// <summary>
    /// Attack the target player: show sword effect, deal damage, and set cooldown
    /// </summary>
    /// <param name="target">Player to attack</param>
    private void Attack(Component target)
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Show sword effect with yellow color
        if (swordEffect != null)
        {
            GameObject effect = Instantiate(swordEffect, transform.position, Quaternion.identity);
            effect.GetComponent<SpriteRenderer>().color = Color.yellow;
            Destroy(effect, 0.3f);
        }

        // Deal damage depending on player type
        if (target is Player1Controller p1) p1.TakeDamage(attackDamage);
        if (target is Player2Controller p2) p2.TakeDamage(attackDamage);

        // Reset attack flag shortly after attack animation
        Invoke(nameof(ResetAttack), 0.2f);
    }

    /// <summary>
    /// Reset attack state to allow next attack
    /// </summary>
    private void ResetAttack()
    {
        isAttacking = false;
    }

    /// <summary>
    /// Patrol horizontally back and forth within patrolRange around spawn point
    /// </summary>
    private void Patrol()
    {
        float distanceFromStart = Mathf.Abs(transform.position.x - patrolStartX);

        // Reverse direction at patrol limits
        if (distanceFromStart >= patrolRange)
        {
            patrolDirection *= -1;
        }

        // Apply horizontal movement using Rigidbody2D velocity
        Vector2 velocity = rb.linearVelocity;
        velocity.x = patrolDirection * moveSpeed;
        rb.linearVelocity = velocity;

        // Flip sprite to face movement direction
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * patrolDirection;
        transform.localScale = scale;
    }

    /// <summary>
    /// Called when enemy takes damage: reduce health and check for death
    /// </summary>
    /// <param name="damage">Damage amount</param>
    public void TakeDamage(int damage)
    {
        if (!isAlive) return;

        health = Mathf.Max(0, health - damage);

        if (health <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Handle enemy death: disable movement, change color, and notify audio manager
    /// </summary>
    private void Die()
    {
        isAlive = false;

        if (spriteRenderer != null)
            spriteRenderer.color = Color.gray;

        rb.linearVelocity = Vector2.zero;

        if (AudioManager.Instance != null)
            AudioManager.Instance.OnEnemyDeath();
    }

    /// <summary>
    /// Update enemy sprite color based on current health percentage for visual feedback
    /// </summary>
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

    /// <summary>
    /// Handle collision with players: deal contact damage if not currently attacking
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
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

    /// <summary>
    /// Draw debug gizmos in editor showing attack and patrol ranges
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Draw attack range in red
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw patrol range as blue horizontal line
        Gizmos.color = Color.blue;
        Vector3 patrolStart = new Vector3(patrolStartX != 0 ? patrolStartX : transform.position.x, transform.position.y, transform.position.z);
        Gizmos.DrawLine(patrolStart + Vector3.left * patrolRange, patrolStart + Vector3.right * patrolRange);
    }
}
