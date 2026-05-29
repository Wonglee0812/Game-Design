using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Magnet_PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;

    [Header("Game Feel Gravity")]
    public float fallMultiplier = 2.5f; // [VI] Trọng lực khi đang rớt / [EN] Gravity when falling
    public float lowJumpMultiplier = 2f; // [VI] Trọng lực khi nhả nút nhảy sớm / [EN] Gravity when releasing jump early

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private bool jumpRequested;
    private bool isJumpHeld; // [VI] Trạng thái có đang giữ nút nhảy không / [EN] State of holding the jump button

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void FixedUpdate()
    {
        // 1. Di chuyển ngang / Horizontal movement
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // 2. Kích hoạt nhảy / Trigger jump
        if (jumpRequested)
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
            jumpRequested = false;
        }

        // 3. Kích hoạt Trọng lực tùy chỉnh / Trigger Custom Gravity
        HandleCustomGravity();
    }

    private void HandleCustomGravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            // [VI] Đang rơi xuống -> Rơi nhanh hơn / [EN] Falling down -> Fall faster
            rb.gravityScale = fallMultiplier;
        }
        else if (rb.linearVelocity.y > 0 && !isJumpHeld)
        {
            // [VI] Đang bay lên nhưng đã nhả nút nhảy -> Hãm đà lại / [EN] Moving up but released jump -> Brake the momentum
            rb.gravityScale = lowJumpMultiplier;
        }
        else
        {
            // [VI] Trạng thái bình thường / [EN] Normal state
            rb.gravityScale = 1f;
        }
    }

    public void SetMoveInput(float inputX)
    {
        moveInput = inputX;
    }

    public void TriggerJump()
    {
        jumpRequested = true;
    }

    public void SetJumpHeld(bool held)
    {
        isJumpHeld = held;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}