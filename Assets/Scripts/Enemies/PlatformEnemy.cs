using UnityEngine;

public class PlatformEnemy : MonoBehaviour
{
    public float speed = 2f;
    public float detectionRange = 5f;
    public float jumpForce = 5f;
    public Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private bool isOnPlatform;
    public LayerMask platformLayer;
    private BoxCollider2D boxCollider; 
    private bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        CheckIfOnPlatform();

        if (player != null && isOnPlatform) {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer < detectionRange) {

                if (Mathf.Abs(player.position.y-transform.position.y) > 1)
                    JumpToPlayer();
                else
                    ChasePlayer();

            } else {
                rb.linearVelocity = Vector2.zero;
            }
        }    
    }

    private void ChasePlayer()
    {
        Vector2 direction = new Vector2(player.position.x-transform.position.x, 0).normalized;
        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocityY);
        if (rb.linearVelocityX < 0 && facingRight)
            Flip();
        else if (rb.linearVelocityX > 0 && !facingRight)
            Flip();

    }

    private void JumpToPlayer()
    {
        if (isOnPlatform)
            rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
    }

    private void CheckIfOnPlatform()
    {
        isOnPlatform = boxCollider.IsTouchingLayers(LayerMask.GetMask("Ground"));
    }

    private void Flip ()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    } 
}