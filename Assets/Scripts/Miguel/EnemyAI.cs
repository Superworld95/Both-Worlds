using UnityEngine;

/// <summary>
/// Controls enemy movement, detection, attacking, health, and animations using EnemyTypeData.
/// Attach to enemy prefab with SpriteRenderer, Animator, and Collider2D.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    [Header("ScriptableObject Data")]
    public EnemyTypeData enemyData; // Stats & settings from ScriptableObject

    private int currentHealth;
    private float attackCooldownTimer;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private Vector3 startPos;
    private bool movingRight = true;
    private bool isDead = false;

    private Transform detectedPlayer; // Reference to detected player

    private void Awake()
    {
        // Initialize components early so they are ready for InitializeEnemy()
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        // Only initialize if enemyData was assigned in Inspector
        if (enemyData != null)
            InitializeEnemy();
    }

    /// <summary>
    /// Initializes the enemy stats and visuals based on enemyData.
    /// </summary>
    private void InitializeEnemy()
    {
        // Ensure components exist
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null)
            animator = GetComponent<Animator>();

        if (enemyData != null)
        {
            currentHealth = enemyData.maxHealth;
            spriteRenderer.color = enemyData.enemyColor;
            startPos = transform.position;

            Debug.Log($"{enemyData.enemyName} spawned with {currentHealth} HP, Speed: {enemyData.moveSpeed}, Damage: {enemyData.attackDamage}");
        }
        else
        {
            Debug.LogError($"{name}: EnemyData not assigned!");
        }
    }

    private void Update()
    {
        if (isDead) return;

        HandleAttackCooldown();
        DetectPlayer();

        if (detectedPlayer != null)
        {
            float distance = Vector2.Distance(transform.position, detectedPlayer.position);
            if (distance <= enemyData.attackRange && attackCooldownTimer <= 0)
            {
                Attack();
            }
            else
            {
                ChasePlayer();
            }
        }
        else
        {
            Patrol();
        }

        UpdateAnimation();
    }

    /// <summary>
    /// Detects player within detection range using OverlapCircle on Player layer.
    /// </summary>
    private void DetectPlayer()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, enemyData.detectionRange, LayerMask.GetMask("Player"));
        detectedPlayer = hit != null ? hit.transform : null;
    }

    /// <summary>
    /// Moves enemy left/right within patrol range around start position.
    /// </summary>
    private void Patrol()
    {
        float patrolDistance = enemyData.patrolRange;
        float speed = enemyData.moveSpeed;

        if (movingRight)
        {
            transform.Translate(Vector2.right * speed * Time.deltaTime);
            if (transform.position.x >= startPos.x + patrolDistance)
                movingRight = false;
        }
        else
        {
            transform.Translate(Vector2.left * speed * Time.deltaTime);
            if (transform.position.x <= startPos.x - patrolDistance)
                movingRight = true;
        }
    }

    /// <summary>
    /// Moves enemy toward the detected player.
    /// </summary>
    private void ChasePlayer()
    {
        if (detectedPlayer == null) return;

        float speed = enemyData.moveSpeed * 1.2f; // Slightly faster chasing speed

        Vector2 direction = (detectedPlayer.position - transform.position).normalized;
        transform.Translate(direction * speed * Time.deltaTime);
    }

    /// <summary>
    /// Performs an attack on the detected player, triggers attack animation and resets cooldown.
    /// </summary>
    private void Attack()
    {
        attackCooldownTimer = enemyData.attackCooldown;
        animator.SetTrigger("Attack");

        // Damage player on attack (assuming player has health interface IPlayerController)
        var playerHealth = detectedPlayer.GetComponent<IPlayerController>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(enemyData.attackDamage);
        }
    }

    /// <summary>
    /// Decrements attack cooldown timer.
    /// </summary>
    private void HandleAttackCooldown()
    {
        if (attackCooldownTimer > 0)
            attackCooldownTimer -= Time.deltaTime;
    }

    /// <summary>
    /// Updates animator parameters for movement and facing direction.
    /// </summary>
    private void UpdateAnimation()
    {
        bool isMoving = false;

        if (detectedPlayer != null)
        {
            isMoving = Vector2.Distance(transform.position, detectedPlayer.position) > enemyData.attackRange;
            FaceTarget(detectedPlayer.position);
        }
        else
        {
            // During patrol, check movingRight for facing
            isMoving = true;
            spriteRenderer.flipX = !movingRight;
        }

        animator.SetBool("IsMoving", isMoving);
    }

    /// <summary>
    /// Flips sprite to face the target position.
    /// </summary>
    /// <param name="targetPos">Target world position</param>
    private void FaceTarget(Vector3 targetPos)
    {
        if (targetPos.x < transform.position.x)
            spriteRenderer.flipX = true;
        else
            spriteRenderer.flipX = false;
    }

    /// <summary>
    /// Called externally when enemy takes damage.
    /// Plays hit animation, reduces health, and dies if health <= 0.
    /// </summary>
    /// <param name="damage">Amount of damage taken</param>
    /// <param name="attacker">The entity that dealt damage</param>
    public void TakeDamage(int damage, object attacker)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"{enemyData.enemyName} took {damage} damage. Remaining HP: {currentHealth}");
        animator.SetTrigger("Injured");

        if (currentHealth <= 0)
            Die(attacker);
    }

    /// <summary>
    /// Handles death logic: triggers death animation, disables colliders, and notifies GameManager.
    /// </summary>
    private void Die(object killer)
    {
        isDead = true;
        Debug.Log($"{enemyData.enemyName} was killed.");

        animator.SetBool("IsDead", true);
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Notify GameManager about the kill for scoring
        GameManager.Instance.AddEnemyKill(killer);

        // Disable or destroy enemy after delay (or use pooling)
        Destroy(gameObject, 2f);
    }

    /// <summary>
    /// Returns whether the enemy is alive.
    /// </summary>
    public bool IsAlive()
    {
        return !isDead && currentHealth > 0;
    }

    /// <summary>
    /// Assigns new EnemyTypeData and initializes the enemy with it.
    /// </summary>
    public void SetEnemyTypeData(EnemyTypeData data)
    {
        enemyData = data;
        InitializeEnemy();
    }

    // Optional: visualize detection and attack ranges in editor
    private void OnDrawGizmosSelected()
    {
        if (enemyData == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyData.detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);
    }
}
