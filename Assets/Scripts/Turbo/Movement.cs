using System.Collections;
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

    // Wall sliding properties
    private bool isWallSliding;
    private float wallSlidingSpeed = 2f;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;

    // WallJumping Properties
    private bool isWallJumping;
    private float wallJumpingDirection;
    [SerializeField] private float wallJumpingTime = 0.2f;
    [SerializeField] private float wallJumpingDuration = 0.4f;
    [SerializeField] private Vector2 wallJumpingPower = new Vector2(8f, 16f);

    // Dashing properties
    [SerializeField] private TrailRenderer trailRenderer;
    private bool canDash = true;
    private bool isDashing;
    [SerializeField] private float dashingPower = 24f; // Increased power for a noticeable effect
    [SerializeField] private float dashingTime = 0.2f;
    [SerializeField] private float dashingCoolDown = 1f;

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

    private void FixedUpdate()
    {
        // If the player is dashing, we want to stop any other movement logic
        // This is important to ensure the dash isn't interrupted by normal movement
        if (isDashing)
        {
            return;
        }

        // Prevent horizontal movement from interfering with the wall jump
        if (!isWallJumping)
        {
            // Horizontal movement (keeps vertical velocity)
            Vector2 moveVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
            rb.linearVelocity = moveVelocity;
        }

        // Call the wall slide method
        wallSlide();
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
            // If wall sliding, perform a wall jump. This takes priority over normal jump.
            if (isWallSliding)
            {
                // This coroutine will handle the wall jump force and temporary movement block
                StartCoroutine(WallJump());
            }
            // Otherwise, perform a normal jump if still able
            else if (!jumpConsumed && jumpsUsed < maxJumps)
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

    // This method is called by the new Input System when the Dash button is pressed.
    public void OnDash(InputValue input)
    {
        // Check if the dash can be performed (not on cooldown) and if the button was pressed.
        if (canDash && input.isPressed)
        {
            // Start the Dash coroutine to handle the entire dash sequence.
            StartCoroutine(Dash());
        }
    }

    private bool isWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    private void wallSlide()
    {
        if (isWalled() && !isGrounded && moveInput.x != 0f)
        {
            isWallSliding = true;
            // Set the direction for wall jumping. It's the opposite of the player's facing direction.
            wallJumpingDirection = -transform.localScale.x;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
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

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true; // Set isDashing to true at the start of the coroutine

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        // Determine the dash direction. Use moveInput.x if it's not zero, otherwise use the player's facing direction.
        float dashDirection = moveInput.x != 0 ? moveInput.x : transform.localScale.x;

        rb.linearVelocity = new Vector2(dashDirection * dashingPower, 0f);

        trailRenderer.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        trailRenderer.emitting = false;

        rb.gravityScale = originalGravity;
        isDashing = false; // Set isDashing to false when the dash duration is over

        yield return new WaitForSeconds(dashingCoolDown);
        canDash = true;
    }

    // Coroutine to handle the wall jumping logic and duration
    private IEnumerator WallJump()
    {
        isWallJumping = true;

        // Reset vertical velocity to ensure consistent jump height
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);

        // Apply the wall jump force
        // The direction is opposite to the wallCheck's normal
        rb.AddForce(new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y), ForceMode2D.Impulse);

        // A small delay to prevent the player from immediately moving away from the wall jump
        yield return new WaitForSeconds(wallJumpingDuration);

        isWallJumping = false;
        jumpsUsed = 0; // Reset jumps after a successful wall jump
    }
}