using System;
using System.Collections;
using Core;
using UnityEngine;

namespace Gameplay
{
    public abstract class EnemyBase : MonoBehaviour, IEnemy
    {
        [SerializeField] protected float speed = 2f;
        [SerializeField] protected float detectionRange = 5f;
        [SerializeField] protected float health = 100f;
        [SerializeField] protected float damage = 100f;
        [SerializeField] protected float attackRange = 2.5f;
        [SerializeField] protected float attackCooldown = 1f;
        protected Transform Player;
        protected Rigidbody2D Rb;
        protected bool FacingRight;
        protected Animator Animator;
        protected float attackCooldownTimer;
        protected float timeSinceLastAttack;
        protected bool isAttacking = false;

        protected enum EnemyState { Attack, Death, Flying, Hurt, Idle }
        protected EnemyState CurrentState = EnemyState.Idle;

        protected virtual void Start()
        {
            Rb = GetComponent<Rigidbody2D>();
            Player = GameObject.FindGameObjectWithTag("Player")?.transform;
            Rb.freezeRotation = true;
            Animator = GetComponent<Animator>();
        }

        protected virtual void Update()
        {
            timeSinceLastAttack = Time.time - attackCooldownTimer;
            if (Player == null) return;

            float distanceToPlayer = Vector2.Distance(transform.position, Player.position);

            if (distanceToPlayer < detectionRange) MoveTowardsPlayer();

            if (distanceToPlayer < attackRange && timeSinceLastAttack >= attackCooldown && !isAttacking)
            {
                timeSinceLastAttack = 0;
                Debug.Log("Attack");
                ChangeState(EnemyState.Attack);
            }
            
        }

        void FixedUpdate()
        {
            if (Rb.linearVelocityX > 0 && !FacingRight) Flip(); else if (Rb.linearVelocityX < 0 && FacingRight) Flip();
        }

        protected abstract void MoveTowardsPlayer();

        /*protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
                Debug.Log("Hit");
            ChangeState(EnemyState.Attack);
            timeSinceLastAttack = 0;
        }*/

        public void TakeDamage(float attackDamage)
        {
            health -= attackDamage;
            ChangeState(EnemyState.Hurt);
            if (health <= 0)
            {
                ChangeState(EnemyState.Death);
                //StartCoroutine(DeathCoroutine());
            }
        }

        /*private IEnumerator DeathCoroutine()
        {
            yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length-0.05f);
            Die();
        }*/

        private void Die()
        {
            Destroy(gameObject);
        }

        protected void Flip()
        {
            FacingRight = !FacingRight;
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }
        protected Coroutine currentCoroutine;

        protected void ChangeState(EnemyState newState)
        {
            //Debug.Log($"Changing state to: {newState}");
            if (CurrentState == newState)
        { 
            return;
        }
       
            CurrentState = newState;

            switch (CurrentState)
            {
                case EnemyState.Attack:
                   if (!isAttacking) // Only start attacking if not already attacking
                    {
                    isAttacking = true;
                    Animator.Play("Attack");
                    StartCoroutine(PerformAttack());
                    }
                    break;
                case EnemyState.Death:
                    Animator.Play("Death");
                    StartCoroutine(ReturnAfterDeath());
                    break;
                case EnemyState.Idle:
                    Animator.Play("Idle");
                    break;
                case EnemyState.Hurt:
                    Animator.Play("Hurt");
                    isAttacking = false; // Cancel attack when hurt
                    StartCoroutine(ReturnToIdleAfterHurt());
                    break;
                case EnemyState.Flying:
                    Animator.Play("Flying");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private IEnumerator ReturnToIdleAfterHurt()
{
    // Wait for the duration of the Hurt animation
    yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length);
    ChangeState(EnemyState.Idle);
}
private IEnumerator ReturnAfterDeath()
{
    // Wait for the duration of the Hurt animation
    yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length-0.1f);
    Die();
}
private IEnumerator PerformAttack()
{
    // Wait for the attack animation to finish
    yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length);

    // Check if the player is still in range and deal damage
    if (Player != null && Vector2.Distance(transform.position, Player.position) <= attackRange) // Adjust attack range as needed
    {
        PlayerHealth playerHealth = Player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage((int)damage);
            Debug.Log("Enemy attacked the player!");
        }
    }
    attackCooldownTimer = Time.time;
    isAttacking = false; // Reset attacking state
    // Return to idle state after attacking
    ChangeState(EnemyState.Idle);
}
    }
}
