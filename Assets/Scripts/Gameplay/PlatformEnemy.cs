using UnityEngine;

public class PlatformEnemy : EnemyBase
{

    [SerializeField] private float jumpForce = 5f;
    private BoxCollider2D boxCollider;
    private bool isOnPlatform;

    protected override void Start()
    {
        base.Start();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    protected override void Update()
    {
        CheckIfOnPlatform();
        base.Update();
    }

    private void JumpToPlayer()
    {
        if (isOnPlatform)
            rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
    }

    protected override void MoveTowardsPlayer()
    {
        if (!isOnPlatform) return;

        float verticalDifference = Mathf.Abs(player.position.y - transform.position.y);
        if (verticalDifference > 1) JumpToPlayer();
        else ChasePlayer();
    }

    private void ChasePlayer()
    {
        Vector2 direction = new Vector2(player.position.x - transform.position.x, 0).normalized;
        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocityY);
    }

    private void CheckIfOnPlatform()
    {
        isOnPlatform = boxCollider.IsTouchingLayers(LayerMask.GetMask("Ground"));
    }
}