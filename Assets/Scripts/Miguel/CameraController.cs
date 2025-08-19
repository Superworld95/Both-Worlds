/*using UnityEngine;
using System.Collections;

/// <summary>
/// Dynamic camera controller that follows both players with intelligent positioning and zoom.
/// Automatically adjusts camera position to keep both players in view and smoothly zooms
/// based on the distance between players. Includes camera bounds and shake effects.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Player1Controller player1; // Reference to Player 1 for following
    public Player2Controller player2; // Reference to Player 2 for following
    public float smoothTime = 0.3f; // Time for camera to reach target position
    public Vector3 offset = new Vector3(0, 2, -10); // Camera offset from center point
    public float minZoom = 3f; // Minimum camera zoom level
    public float maxZoom = 8f; // Maximum camera zoom level
    public float zoomBorder = 5f; // Distance between players that triggers zoom adjustment

    [Header("Bounds")]
    public float minX = 0f; // Leftmost camera position
    public float maxX = 3000f; // Rightmost camera position (finish line area)
    public float minY = -10f; // Lowest camera position
    public float maxY = 20f; // Highest camera position

    // Component references and movement tracking
    private Camera cam; // Camera component for zoom adjustments
    private Vector3 velocity; // Velocity for smooth camera movement

    /// <summary>
    /// Initialize camera component and find player references if not assigned
    /// </summary>
    private void Start()
    {
        cam = GetComponent<Camera>();

        // Automatically find players if not manually assigned in inspector
        if (player1 == null) player1 = FindObjectOfType<Player1Controller>();
        if (player2 == null) player2 = FindObjectOfType<Player2Controller>();
    }

    /// <summary>
    /// Update camera position and zoom in LateUpdate to ensure smooth following after all player movement
    /// Calculates center point between players, applies bounds, and adjusts zoom dynamically
    /// </summary>
    private void LateUpdate()
    {
        if (player1 == null && player2 == null) return;

        // Calculate the center point between active players
        Vector3 centerPoint = GetCenterPoint();

        // Apply camera offset for better positioning
        Vector3 targetPosition = centerPoint + offset;

        // Ensure camera stays within defined bounds
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);

        // Smoothly move camera to target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        // Dynamically adjust zoom based on player separation
        AdjustZoom();
    }

    /// <summary>
    /// Calculate center point between active players for camera to follow
    /// </summary>
    private Vector3 GetCenterPoint()
    {
        Vector3 center = Vector3.zero;
        int activePlayerCount = 0;

        if (player1 != null && player1.IsAlive())
        {
            center += player1.transform.position;
            activePlayerCount++;
        }

        if (player2 != null && player2.IsAlive())
        {
            center += player2.transform.position;
            activePlayerCount++;
        }

        if (activePlayerCount > 0)
        {
            center /= activePlayerCount;
        }
        else if (player1 != null)
        {
            center = player1.transform.position;
        }
        else if (player2 != null)
        {
            center = player2.transform.position;
        }

        return center;
    }

    /// <summary>
    /// Adjust zoom based on distance between active players to keep them in view
    /// </summary>
    private void AdjustZoom()
    {
        if (player1 == null && player2 == null) return;

        float distance = 0f;

        if (player1 != null && player2 != null && player1.IsAlive() && player2.IsAlive())
        {
            // Calculate distance between both players
            distance = Vector3.Distance(player1.transform.position, player2.transform.position);
        }

        // Calculate required zoom
        float requiredZoom = distance / zoomBorder;
        requiredZoom = Mathf.Clamp(requiredZoom, minZoom, maxZoom);

        // Smooth zoom transition
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, requiredZoom, Time.deltaTime * 2f);
    }

    /// <summary>
    /// Set Player 1 reference at runtime
    /// </summary>
    public void SetPlayer1(Player1Controller p1)
    {
        player1 = p1;
    }

    /// <summary>
    /// Set Player 2 reference at runtime
    /// </summary>
    public void SetPlayer2(Player2Controller p2)
    {
        player2 = p2;
    }

    /// <summary>
    /// Trigger camera shake effect with optional intensity and duration
    /// </summary>
    public void ShakeCamera(float intensity = 1f, float duration = 0.5f)
    {
        StartCoroutine(CameraShake(intensity, duration));
    }

    /// <summary>
    /// Coroutine that performs the actual shake motion
    /// </summary>
    private IEnumerator CameraShake(float intensity, float duration)
    {
        Vector3 originalPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;

            transform.position = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPosition;
    }
}*/
