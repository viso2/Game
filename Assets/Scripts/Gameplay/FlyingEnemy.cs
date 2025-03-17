using Gameplay;
using UnityEngine;

public class FlyingEnemy : EnemyBase
{
    protected override void Update()
    {
        base.Update();
        ChangeState(Rb.linearVelocityX == 0 ? EnemyState.Idle : EnemyState.Flying);
    }
    protected override void MoveTowardsPlayer()
    {
        Vector2 direction = (Player.position - transform.position).normalized;
        Rb.linearVelocity = direction * speed;
    }
}
