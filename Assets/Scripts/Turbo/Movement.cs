using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpForce = 7f;
    [SerializeField] int maxJumps = 2; // 2 = double jump
    private Vector2 moveInput;
    private Rigidbody2D rb;

    // jump state
    private int jumpsUsed = 0;
    private bool isGrounded = false;

    // prevents repeated jumps while holding the button
    private bool jumpConsumed = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Give each player a random color
        GetComponent<SpriteRenderer>().color = new Color(
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f)
        );
    }

    // Called by the new Input System when Move is performed
    public void OnMove(InputValue input)
    {
        moveInput = input.Get<Vector2>();
    }

    // Called by the new Input System when Jump is performed/cancelled
    public void OnJump(InputValue input)
    {
        // InputValue.isPressed will be true on button press, false on release.
        bool pressed = input.isPressed;

        if (pressed)
        {
            // Only jump if we still have jumps left and the press hasn't been consumed
            if (!jumpConsumed && jumpsUsed < maxJumps)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpsUsed++;
                isGrounded = false;
                jumpConsumed = true;
            }
        }
        else
        {
            // button released -> allow next press to trigger a jump
            jumpConsumed = false;
        }
    }

    private void FixedUpdate()
    {
        // Horizontal movement (keeps vertical velocity)
        Vector2 moveVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        rb.linearVelocity = moveVelocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Only set grounded if we landed on something tagged as Ground and the contact normal indicates landing
        if (collision.gameObject.CompareTag("Ground"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    isGrounded = true;
                    jumpsUsed = 0;       // reset jumps when landing
                    jumpConsumed = false;
                    break;
                }
            }
        }
    }
}
