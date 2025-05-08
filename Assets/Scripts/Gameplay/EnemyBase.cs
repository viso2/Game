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
        protected Transform Player;
        protected Rigidbody2D Rb;
        protected bool FacingRight;
        protected Animator Animator;

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
            if (Player == null) return;

            float distanceToPlayer = Vector2.Distance(transform.position, Player.position);
            if (distanceToPlayer < detectionRange) MoveTowardsPlayer();
        }

        void FixedUpdate()
        {
            if (Rb.linearVelocityX > 0 && !FacingRight) Flip(); else if (Rb.linearVelocityX < 0 && FacingRight) Flip();
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
                Debug.Log("player hit");
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
            yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length-0.05f);
            Die();
        }

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

        protected void ChangeState(EnemyState newState)
        {
            //Debug.Log($"Changing state to: {newState}");
            if (CurrentState == newState) return;
            CurrentState = newState;
            switch (CurrentState)
            {
                case EnemyState.Attack:
                    Animator.Play("Attack");
                    break;
                case EnemyState.Death:
                    Animator.Play("Death");
                    break;
                case EnemyState.Idle:
                    Animator.Play("Idle");
                    break;
                case EnemyState.Hurt:
                    Animator.Play("Hurt");
                    break;
                case EnemyState.Flying:
                    Animator.Play("Flying");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
