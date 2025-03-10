using UnityEngine;

public class FlyingEnemy : EnemyBase
{
    protected override void Update()
    {
        base.Update();
        if (rb.linearVelocityX == 0) {
            ChangeState(EnemyState.Idle);
        } else {
            ChangeState(EnemyState.Flying);
        }
    }
    protected override void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }
}
