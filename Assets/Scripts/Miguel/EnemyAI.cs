using UnityEngine;

/// <summary>
/// Controls enemy movement, detection, attacking, health, and animations using EnemyTypeData.
/// Attach to enemy prefab with SpriteRenderer, Animator, and Collider2D.
/// Supports patrol areas and waypoint-based patrolling.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    [Header("Enemy Settings")]
    [Tooltip("Data defining health, speed, damage, detection range, etc.")]
    public EnemyTypeData enemyData;

    private int currentHealth;

    [Header("Patrol Settings")]
    [Tooltip("Left boundary of patrol area (optional).")]
    public Transform leftBoundary;

    [Tooltip("Right boundary of patrol area (optional).")]
    public Transform rightBoundary;

    [Tooltip("Optional patrol waypoints. Enemy will move between them if assigned.")]
    public Transform[] patrolPoints;

    [Tooltip("Time to wait at each patrol point (waypoint mode).")]
    public float waitTimeAtPoint = 1.5f;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private int currentPatrolIndex = 0;
    private float waitTimer = 0f;
    private bool movingRight = true;
    private bool isChasing = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Ensure enemies render above background by default
        spriteRenderer.sortingLayerName = "Characters";
        spriteRenderer.sortingOrder = 2;
    }

    private void Start()
    {
        // Initialize enemy if enemyData is already assigned
        if (enemyData != null)
            InitializeEnemy();
    }

    private void Update()
    {
        if (isChasing)
        {
            // TODO: Add chase player logic if needed
        }
        else
        {
            Patrol();
        }
    }

    /// <summary>
    /// Initializes enemy health, visuals, and other data from enemyData.
    /// </summary>
    private void InitializeEnemy()
    {
        currentHealth = enemyData.maxHealth;

        // Apply sprite color from data
        if (spriteRenderer != null)
            spriteRenderer.color = enemyData.enemyColor;

        // Optional: set other parameters like moveSpeed here if needed
        Debug.Log($"{enemyData.enemyName} spawned with {currentHealth} HP and speed {enemyData.moveSpeed}");
    }

    /// <summary>
    /// Assigns new EnemyTypeData and applies settings like move speed, color, etc.
    /// Called externally by GameManager when spawning.
    /// </summary>
    public void SetEnemyTypeData(EnemyTypeData data)
    {
        enemyData = data;
        InitializeEnemy();
    }

    /// <summary>
    /// Handles patrol movement logic:
    /// - If patrolPoints are assigned → move between waypoints.
    /// - Otherwise, if left/right boundaries assigned → patrol between them.
    /// - If neither → idle in place.
    /// </summary>
    private void Patrol()
    {
        // --- Waypoint patrol mode ---
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Transform targetPoint = patrolPoints[currentPatrolIndex];
            transform.position = Vector2.MoveTowards(
                transform.position,
                targetPoint.position,
                enemyData.moveSpeed * Time.deltaTime
            );

            // Check if enemy reached the waypoint
            if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
            {
                if (waitTimer <= 0f)
                {
                    // Advance to next waypoint
                    currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                    waitTimer = waitTimeAtPoint;
                    FlipTowards(targetPoint.position.x);
                }
                else
                {
                    waitTimer -= Time.deltaTime;
                }
            }
        }
        // --- Boundary patrol mode ---
        else if (leftBoundary != null && rightBoundary != null)
        {
            if (movingRight)
            {
                transform.Translate(Vector2.right * enemyData.moveSpeed * Time.deltaTime);
                if (transform.position.x >= rightBoundary.position.x)
                {
                    Flip();
                    movingRight = false;
                }
            }
            else
            {
                transform.Translate(Vector2.left * enemyData.moveSpeed * Time.deltaTime);
                if (transform.position.x <= leftBoundary.position.x)
                {
                    Flip();
                    movingRight = true;
                }
            }
        }
        // --- Idle mode ---
        else
        {
            animator.Play("Idle"); // fallback animation
        }
    }

    /// <summary>
    /// Flips the enemy sprite horizontally to face the opposite direction.
    /// </summary>
    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    /// <summary>
    /// Flips the enemy to face a target x position (used for waypoint patrol).
    /// </summary>
    /// <param name="targetX">World x position to face</param>
    private void FlipTowards(float targetX)
    {
        if ((targetX > transform.position.x && transform.localScale.x < 0) ||
            (targetX < transform.position.x && transform.localScale.x > 0))
        {
            Flip();
        }
    }
}
