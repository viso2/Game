using System;
using System.Collections;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IEnemy
{
    [SerializeField] protected float speed = 2f;
    [SerializeField] protected float detectionRange = 5f;
    [SerializeField] protected float health = 100f;
    [SerializeField] protected float damage = 100f;
    protected Transform player;
    protected Rigidbody2D rb;
    protected bool facingRight = false;
    protected Animator animator;

    protected enum EnemyState { Attack, Death, Flying, Hurt, Idle }
    protected EnemyState currentState = EnemyState.Idle;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb.freezeRotation = true;
        animator = GetComponent<Animator>();
    }

    protected virtual void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer < detectionRange) MoveTowardsPlayer();
    }

    void FixedUpdate()
    {
        if (rb.linearVelocityX > 0 && !facingRight) Flip(); else if (rb.linearVelocityX < 0 && facingRight) Flip();
    }

    protected abstract void MoveTowardsPlayer();

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            Debug.Log("Hit");
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage((int)damage);
            }
    }

    public void TakeDamage(float attackDamage)
    {
        health -= attackDamage;
        ChangeState(EnemyState.Hurt);
        if (health <= 0)
        {
            ChangeState(EnemyState.Death);
            StartCoroutine(DeathCoroutine());
        }
    }

    private IEnumerator DeathCoroutine()
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length-0.05f);
        Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    protected void Flip()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    protected void ChangeState(EnemyState newState)
    {
        Debug.Log($"Changing state to: {newState}");
        if (currentState != newState)
        {
            currentState = newState;
            switch (currentState)
            {
                case EnemyState.Attack:
                    animator.Play("Attack");
                    break;
                case EnemyState.Death:
                    animator.Play("Death");
                    break;
                case EnemyState.Idle:
                    animator.Play("Idle");
                    break;
                case EnemyState.Hurt:
                    animator.Play("Hurt");
                    break;
                case EnemyState.Flying:
                    animator.Play("Flying");
                    break;
            }
        }
    }
}
