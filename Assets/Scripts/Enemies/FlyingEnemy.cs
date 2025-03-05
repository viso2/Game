using UnityEngine;

public class FlyingEnemy : EnemyBase
{
    protected override void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }
}
