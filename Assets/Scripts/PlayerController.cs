using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
    public float attackSpeed;
    private float attackCooldownTimer = 0f;
    private bool isAttacking;
    public float attackRange = 1.5f;
    public float attackDamage = 10f;
    private float lastDashTime;
    private float doubleTapTime = 0.2f;
    private KeyCode lastKeyPressed;

    // Dash cooldown variables
    public float dashCooldown = 1f; // Cooldown time in seconds
    private float dashCooldownTimer = 0f;

    //Animation
    private Animator animator;
    private enum PlayerState { Attack, Block, Die, Idle, Run, Walk }
    private PlayerState currentState = PlayerState.Idle;

    private bool facingRight = true;
    private BoxCollider2D boxCollider;
    public Camera mainCamera;
    public Camera menuCamera;
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
            if (rb.linearVelocityX == 0 && !isAttacking) ChangeState(PlayerState.Idle); else if (!isAttacking) ChangeState(PlayerState.Walk);
        }



        if (JumpAction.WasPressedThisFrame() && jumpCount < maxJumps)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
            jumpCount++;
        }
        else if (isGrounded && !IsTouchingWall())
        {
            jumpCount = 0;
            dashedSinceGrounded = false;
        }

        HandleDashInput();
        FollowPlayer();
        Attack();
        GoToMainMenu();

        // Update the dash cooldown timer
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        isGrounded = IsGrounded();
        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

    }

    void FixedUpdate()
    {
        if (rb.linearVelocityX > 0 && !facingRight) Flip(); else if (rb.linearVelocityX < 0 && facingRight) Flip();
    }
    private bool IsGrounded()
    {
        return boxCollider.IsTouchingLayers(LayerMask.GetMask("Ground"));
    }

    private void GoToMainMenu()
    { if (Input.GetKeyDown(KeyCode.Escape))
        {
            menuCamera.enabled = true;
            mainCamera.enabled = false;
        }
    }
    
    private void Attack()
    {
        if (attackCooldownTimer <= 0 && Input.GetKeyDown(KeyCode.X))
        {
            isAttacking = true;
            ChangeState(PlayerState.Attack);
            StartCoroutine(StopAttackAfterDelay(0.5f));
            PeformAttack();
        }
    }

    private void PeformAttack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, LayerMask.GetMask("Enemy"));
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<EnemyBase>(out EnemyBase enemyComponent))
            {
                enemyComponent.TakeDamage(attackDamage);
            }
        }
    }

    private System.Collections.IEnumerator StopAttackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isAttacking = false;
        attackCooldownTimer = attackSpeed;
        ChangeState(PlayerState.Idle);
    }


    private void HandleDashInput()
    {
        if (dashCooldownTimer <= 0 && (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D)) && !dashedSinceGrounded)
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

    private void ChangeState(PlayerState newState)
    {
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

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}