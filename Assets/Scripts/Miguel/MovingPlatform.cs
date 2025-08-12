using UnityEngine;

/// <summary>
/// Moving platform controller that creates dynamic platforming challenges.
/// Platforms oscillate between two points and can carry players smoothly.
/// Supports both oscillating and one-way movement patterns.
/// </summary>
public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    public Vector3 moveDirection = Vector3.right; // Direction of platform movement
    public float moveSpeed = 30f; // Speed of platform movement in units per second
    public float moveDistance = 200f; // Total distance platform travels
    public bool oscillate = true; // If true, platform moves back and forth; if false, resets to start
    
    [Header("Platform Settings")]
    public bool carryPlayers = true; // Whether platform carries players as children for smooth movement
    
    // Movement state variables
    private Vector3 startPosition; // Starting position of the platform
    private Vector3 targetPosition; // End position of the platform movement
    private bool movingToTarget = true; // Whether currently moving toward target or back to start
    private Rigidbody2D rb; // Physics body for kinematic movement
    
    /// <summary>
    /// Initialize platform movement parameters and configure physics
    /// </summary>
    private void Start()
    {
        // Calculate movement path
        startPosition = transform.position;
        targetPosition = startPosition + moveDirection.normalized * moveDistance;
        
        // Get or add rigidbody component for physics-based movement
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Configure rigidbody for kinematic movement (not affected by physics forces)
        rb.isKinematic = true; // Move via script, not physics
        rb.freezeRotation = true; // Prevent rotation
    }
    
    /// <summary>
    /// Handle platform movement in FixedUpdate for consistent physics timing
    /// Only moves when game is active
    /// </summary>
    private void FixedUpdate()
    {
        // Null check for GameManager Instance
        if (GameManager.Instance == null || !GameManager.Instance.IsGameActive()) return;
        
        MovePlatform(); // Update platform position
    }
    
    private void MovePlatform()
    {
        Vector3 currentTarget = movingToTarget ? targetPosition : startPosition;
        Vector3 direction = (currentTarget - transform.position).normalized;
        
        // Move towards target
        Vector3 movement = direction * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(transform.position + movement);
        
        // Check if reached target
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget);
        if (distanceToTarget < 0.5f)
        {
            if (oscillate)
            {
                movingToTarget = !movingToTarget;
            }
            else
            {
                // Reset to start position for one-way movement
                transform.position = startPosition;
            }
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!carryPlayers) return;
        
        // Make players children of platform for smooth movement
        Player1Controller player1 = collision.gameObject.GetComponent<Player1Controller>();
        Player2Controller player2 = collision.gameObject.GetComponent<Player2Controller>();
        
        if (player1 != null || player2 != null)
        {
            // Check if player is on top of platform
            if (collision.contacts[0].normal.y < -0.5f)
            {
                collision.transform.SetParent(transform);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!carryPlayers) return;
        
        Player1Controller player1 = collision.gameObject.GetComponent<Player1Controller>();
        Player2Controller player2 = collision.gameObject.GetComponent<Player2Controller>();
        
        if (player1 != null || player2 != null)
        {
            collision.transform.SetParent(null);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Vector3 start = Application.isPlaying ? startPosition : transform.position;
        Vector3 end = start + moveDirection.normalized * moveDistance;
        
        // Draw movement path
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(start, end);
        
        // Draw start and end points
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(start, Vector3.one * 0.5f);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(end, Vector3.one * 0.5f);
    }
}