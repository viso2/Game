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
    private const int maxJumps = 1;
    private bool isDashing;
    private float originalGravityScale;
    private float dashTime;
    private bool dashedSinceGrounded = false;
    private Vector2 lastMoveDirection;

    public Transform cameraTransform;
    public float cameraSmoothSpeed = 5f;

    private float lastDashTime;
    private float doubleTapTime = 0.2f;
    private KeyCode lastKeyPressed;

    // Dash cooldown variables
    public float dashCooldown = 1f; // Cooldown time in seconds
    private float dashCooldownTimer = 0f;

    //Animation
    private Animator animator;
    private enum PlayerState {Attack, Block, Die, Idle, Run, Walk}
    private PlayerState currentState = PlayerState.Idle;

    private bool facingRight = true;
    private BoxCollider2D boxCollider;
    void Start()
    {
        MoveAction.Enable();
        JumpAction.Enable();
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        animator = GetComponent<Animator>();
        originalGravityScale = rb.gravityScale;
        boxCollider = GetComponent<BoxCollider2D>();
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
            if (rb.linearVelocityX == 0) ChangeState(PlayerState.Idle);  else ChangeState(PlayerState.Walk); 
        }
        


        if (JumpAction.WasPressedThisFrame() && jumpCount < maxJumps)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
            jumpCount++;
        }else if (isGrounded && !IsTouchingWall())
        {
            jumpCount = 0;
            dashedSinceGrounded = false;
        }

        HandleDashInput();
        FollowPlayer();

        // Update the dash cooldown timer
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
       
        isGrounded = IsGrounded();
        Debug.Log("isGrounded: " + isGrounded);
        Debug.Log("IsTouchingWall: " + IsTouchingWall());   
        Debug.Log("Position"+ transform.position);
    }

    void FixedUpdate()
    {
        if (rb.linearVelocityX > 0 && !facingRight) Flip(); else if(rb.linearVelocityX < 0 && facingRight) Flip();
    }
    private bool IsGrounded()
    {
        /*float extraHeight = 0.1f;
        Vector2 boxSize = new Vector2(1f, 2.4f); // Adjust the width and height of the box as needed
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, boxSize, 0f, Vector2.down, extraHeight, LayerMask.GetMask("Ground"));
         Color boxColor = hit.collider != null ? Color.green : Color.red;
         Vector3 boxCenter = transform.position + Vector3.down * (boxSize.y / 2 + extraHeight / 2);
        

        return hit.collider != null;*/
        return boxCollider.IsTouchingLayers(LayerMask.GetMask("Ground"));
    }


    private void HandleDashInput()
    {
        if (dashCooldownTimer <= 0 && (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))&& !dashedSinceGrounded)
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
        rb.gravityScale = 0;

        ChangeState(PlayerState.Run);
        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = Vector2.zero;
        isDashing = false;
        rb.gravityScale = originalGravityScale;
        dashedSinceGrounded = true;
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

    private bool IsTouchingWall() 
    {
        return Physics2D.Raycast(transform.position, Vector2.right, 0.7f, LayerMask.GetMask("Ground")) || 
            Physics2D.Raycast(transform.position, Vector2.left, 0.7f, LayerMask.GetMask("Ground"));
    }

    private void ChangeState(PlayerState newState) {
        if (currentState != newState)
        {
            currentState = newState;
            switch (currentState)
            {
                case PlayerState.Attack:
                    animator.Play("Attack");
                    break;
                case PlayerState.Block:
                    animator.Play("Block");
                    break;
                case PlayerState.Die:
                    animator.Play("Die");
                    break;
                case PlayerState.Idle:
                    animator.Play("Idle");
                    break;
                case PlayerState.Run:
                    animator.Play("Run");
                    break;
                case PlayerState.Walk:
                    animator.Play("Walk");
                    break;
            }
        }
    }

    private void Flip ()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    } 
}