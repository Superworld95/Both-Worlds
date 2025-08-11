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
        // player's index from PlayerInput
        index = GetComponentInParent<PlayerInput>().playerIndex;

        // current total players
        totalPlayers = PlayerInput.all.Count;

        cam = GetComponent<Camera>();
        cam.depth = index;

        SetupCamera();
    }

    private void HandlePlayerJoined(PlayerInput player)
    {
        Debug.Log($"Updating Camera for Player {index}");

        // Updates player count and camera setup when a player joins
        totalPlayers = PlayerInput.all.Count;
        SetupCamera();
    }

    private void SetupCamera()
    {
        if (totalPlayers == 1)
        {
            // sets to full screen if only one player
            cam.rect = new Rect(0, 0, 1, 1);
        }
        else if (totalPlayers == 2)
        {
            // horizontal split: top for player 0, bottom for player 1
            if (index == 0)
                cam.rect = new Rect(0f, 0.5f, 1f, 0.5f); // Top half
            else
                cam.rect = new Rect(0f, 0f, 1f, 0.5f);   // Bottom half
        }
    }
}
