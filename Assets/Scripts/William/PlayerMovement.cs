using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMovement : MonoBehaviour
{
    public InputActionAsset inputAsset;
    public InputActionMap playerInputs;
    public Rigidbody2D player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponent<Rigidbody2D>();
        inputAsset = this.gameObject.GetComponent<PlayerInput>().actions;
        playerInputs = inputAsset.FindActionMap("Player");
        inputAsset.FindAction("Jump").performed += Jump;
        inputAsset.FindAction("Jump").canceled += JumpCancelled;
        playerInputs.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        
           
    }

    public void Jump(InputAction.CallbackContext context)
    {
        player.AddForce(Vector2.up * 5f, ForceMode2D.Impulse);
    }

    public void JumpCancelled(InputAction.CallbackContext context)
    {
        if (player.linearVelocity.y > 1f)
        {
            player.linearVelocity = new Vector2(player.linearVelocity.x, player.linearVelocity.y / 3);
        }
    }
}
