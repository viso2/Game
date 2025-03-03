using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public InputAction MoveAction;
    public InputAction JumpAction;
    public float speed;
    public float jumpForce;
    public float dashSpeed;
    public float dashDuration;
    private Rigidbody2D rb;
    private bool isGrounded;
    private int jumpCount;
    private const int maxJumps = 2;
    private bool isDashing;
    private float dashTime;
    private Vector2 lastMoveDirection;

    public Transform cameraTransform;
    public float cameraSmoothSpeed = 5f;

    private float lastDashTime;
    private float doubleTapTime = 0.2f;
    private KeyCode lastKeyPressed;

    // Dash cooldown variables
    public float dashCooldown = 1f; // Cooldown time in seconds
    private float dashCooldownTimer = 0f;

    void Start()
    {
        MoveAction.Enable();
        JumpAction.Enable();
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        Vector2 move = MoveAction.ReadValue<Vector2>();
        if (move != Vector2.zero)
        {
            lastMoveDirection = move.normalized;
        }

        if (!isDashing)
        {
            rb.linearVelocity = new Vector2(move.x * speed, rb.linearVelocityY);
        }

        if (IsTouchingWall() && !isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -1f); // Apply slight downward force
        }

        if (JumpAction.WasPressedThisFrame() && jumpCount < maxJumps)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
            jumpCount++;
        }

        HandleDashInput();

        // Update the dash cooldown timer
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        Debug.Log(IsTouchingWall());
    }

    void LateUpdate()
    {
        FollowPlayer();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (Physics2D.Raycast(transform.position, Vector2.down, 1.1f, LayerMask.GetMask("Ground")))
        {
            jumpCount = 0;
        }
    }

    private void HandleDashInput()
    {
        if (dashCooldownTimer <= 0 && (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D)))
        {
            KeyCode currentKey = Input.GetKeyDown(KeyCode.A) ? KeyCode.A : KeyCode.D;

            if (lastKeyPressed == currentKey && Time.time - lastDashTime < doubleTapTime)
            {
                StartCoroutine(Dash(currentKey == KeyCode.A ? Vector2.left : Vector2.right));
                dashCooldownTimer = dashCooldown; // Reset the cooldown timer
            }
            lastDashTime = Time.time;
            lastKeyPressed = currentKey;
        }
    }

    private System.Collections.IEnumerator Dash(Vector2 direction)
    {
        isDashing = true;
        dashTime = dashDuration;
        rb.linearVelocity = direction * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = Vector2.zero;
        isDashing = false;
    }
    
    private void FollowPlayer()

    {
        if (cameraTransform != null)
        {
            // Keep the z position of the camera fixed by only updating the x and y axes
            Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, cameraTransform.position.z);
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, cameraSmoothSpeed * Time.deltaTime);
        }
    }

    private bool IsTouchingWall() {
        return Physics2D.Raycast(transform.position, Vector2.right, 0.6f, LayerMask.GetMask("Ground")) || 
            Physics2D.Raycast(transform.position, Vector2.left, 0.6f, LayerMask.GetMask("Ground"));
    }
}
