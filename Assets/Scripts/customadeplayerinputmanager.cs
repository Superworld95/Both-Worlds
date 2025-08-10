using UnityEngine;
using UnityEngine.InputSystem;

public class customadeplayerinputmanager : MonoBehaviour
{
    public GameObject player1Prefab;
    public GameObject player2Prefab;

    public Transform player1Spawn;
    public Transform player2Spawn;

    private bool player1Joined = false;
    private bool player2Joined = false;

    void Update()
    {
        foreach (var gamepad in Gamepad.all)
        {
            if (gamepad.startButton.wasPressedThisFrame)
            {
                // If gamepad already used, skip
                if (IsGamepadAlreadyUsed(gamepad)) continue;

                if (!player1Joined)
                {
                    var p1 = PlayerInput.Instantiate(player1Prefab,
                        controlScheme: "Gamepad",
                        pairWithDevice: gamepad);
                    p1.transform.position = player1Spawn.position;
                    player1Joined = true;
                    Debug.Log("Player 1 joined with " + gamepad.displayName);
                }
                else if (!player2Joined)
                {
                    var p2 = PlayerInput.Instantiate(player2Prefab,
                        controlScheme: "Gamepad",
                        pairWithDevice: gamepad);
                    p2.transform.position = player2Spawn.position;
                    player2Joined = true;
                    Debug.Log("Player 2 joined with " + gamepad.displayName);
                }
            }
        }
    }

    private bool IsGamepadAlreadyUsed(Gamepad gamepad)
    {
        foreach (var player in PlayerInput.all)
        {
            foreach (var device in player.devices)
            {
                if (device == gamepad)
                    return true; // Already taken
            }
        }
        return false;
    }
}
