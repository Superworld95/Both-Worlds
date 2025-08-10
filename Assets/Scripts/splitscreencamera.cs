using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Camera))]
public class splitscreencamera : MonoBehaviour
{
    private Camera cam;
    private int index;
    private int totalPlayers;

    private void Awake()
    {
        // Subscribe to the PlayerJoined event
        PlayerInputManager.instance.onPlayerJoined += HandlePlayerJoined;
    }

    void Start()
    {
        // Get this player's index from PlayerInput
        index = GetComponentInParent<PlayerInput>().playerIndex;

        // Get current total players
        totalPlayers = PlayerInput.all.Count;

        cam = GetComponent<Camera>();
        cam.depth = index;

        SetupCamera();
    }

    private void HandlePlayerJoined(PlayerInput player)
    {
        Debug.Log($"Updating Camera for Player {index}");

        // Update player count and camera setup when a player joins
        totalPlayers = PlayerInput.all.Count;
        SetupCamera();
    }

    private void SetupCamera()
    {
        switch (totalPlayers)
        {
            case 1:
                cam.rect = new Rect(0, 0, 1, 1);
                break;

            case 2:
                // Split screen vertically
                cam.rect = new Rect(index == 0 ? 0f : 0.5f, 0f, 0.5f, 1f);
                break;

            case 3:
                // Two players on top, one at bottom full width
                cam.rect = new Rect(
                    index == 0 ? 0f : (index == 1 ? 0.5f : 0f),
                    index < 2 ? 0.5f : 0f,
                    index < 2 ? 0.5f : 1f,
                    0.5f
                );
                break;

            default:
                // Four or more players, 2x2 grid split
                cam.rect = new Rect(
                    (index % 2) * 0.5f,
                    (index < 2) ? 0.5f : 0f,
                    0.5f,
                    0.5f
                );
                break;
        }
    }
}
