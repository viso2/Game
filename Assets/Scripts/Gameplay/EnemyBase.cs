using System;
using System.Collections;
using Core;
using UnityEngine;

namespace Gameplay
{
    // EnemyBase er en abstrakt baseklasse, der definerer fælles funktionalitet for fjender i spillet.
    // Den implementerer IEnemy-interfacet og indeholder logik for bevægelse, angreb og tilstandshåndtering.
    public abstract class EnemyBase : MonoBehaviour, IEnemy
    {
        // Fjendens bevægelseshastighed.
        [SerializeField] protected float speed = 2f;

        // Afstand, hvor fjenden kan opdage spilleren.
        [SerializeField] protected float detectionRange = 5f;

        // Fjendens helbred.
        [SerializeField] protected float health = 100f;

        // Skade, som fjenden kan påføre spilleren.
        [SerializeField] protected float damage = 100f;

        // Afstand, hvor fjenden kan angribe spilleren.
        [SerializeField] protected float attackRange = 2.5f;

        // Tidsinterval mellem angreb.
        [SerializeField] protected float attackCooldown = 1f;

        // Referencer til spilleren og fjendens Rigidbody2D.
        protected Transform Player;
        protected Rigidbody2D Rb;

        // Holder styr på, hvilken retning fjenden vender.
        protected bool FacingRight;

        // Reference til fjendens Animator for at spille animationer.
        protected Animator Animator;

        // Tidsmåler for angrebscooldown.
        protected float attackCooldownTimer;

        // Tid siden sidste angreb.
        protected float timeSinceLastAttack;

        // Flag for at indikere, om fjenden er i gang med at angribe.
        protected bool isAttacking = false;

        // Enum, der definerer fjendens forskellige tilstande.
        protected enum EnemyState { Attack, Death, Flying, Hurt, Idle }

        // Fjendens aktuelle tilstand.
        protected EnemyState CurrentState = EnemyState.Idle;

        // Start-metoden initialiserer fjendens komponenter og finder spilleren.
        protected virtual void Start()
        {
            Rb = GetComponent<Rigidbody2D>();
            Player = GameObject.FindGameObjectWithTag("Player")?.transform;
            Rb.freezeRotation = true; // Forhindrer rotation af fjenden.
            Animator = GetComponent<Animator>();
        }

        // Update-metoden håndterer fjendens opførsel baseret på spillerens position.
        protected virtual void Update()
        {
            timeSinceLastAttack = Time.time - attackCooldownTimer;

            // Hvis spilleren ikke findes, afslut opdateringen.
            if (Player == null) return;

            // Beregner afstanden til spilleren.
            float distanceToPlayer = Vector2.Distance(transform.position, Player.position);

            // Hvis spilleren er inden for detektionsområdet, bevæg fjenden mod spilleren.
            if (distanceToPlayer < detectionRange) MoveTowardsPlayer();

            // Hvis spilleren er inden for angrebsområdet og angrebscooldownen er overstået, angrib.
            if (distanceToPlayer < attackRange && timeSinceLastAttack >= attackCooldown && !isAttacking)
            {
                timeSinceLastAttack = 0;
                Debug.Log("Attack");
                ChangeState(EnemyState.Attack);
            }
        }

        // FixedUpdate håndterer fjendens retning baseret på bevægelse.
        void FixedUpdate()
        {
            if (Rb.linearVelocityX > 0 && !FacingRight) Flip();
            else if (Rb.linearVelocityX < 0 && FacingRight) Flip();
        }

        // Abstrakt metode, der skal implementeres af afledte klasser for at definere fjendens bevægelse.
        protected abstract void MoveTowardsPlayer();

        // Metode til at tage skade og ændre tilstand til "Hurt".
        public void TakeDamage(float attackDamage)
        {
            health -= attackDamage;
            ChangeState(EnemyState.Hurt);

            // Hvis fjendens helbred når 0, ændres tilstanden til "Death".
            if (health <= 0)
            {
                ChangeState(EnemyState.Death);
            }
        }

        // Metode til at destruere fjenden, når den dør.
        private void Die()
        {
            Destroy(gameObject);
        }

        // Metode til at vende fjenden, når den skifter retning.
        protected void Flip()
        {
            FacingRight = !FacingRight;
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }

        // Metode til at ændre fjendens tilstand og spille den relevante animation.
        protected void ChangeState(EnemyState newState)
        {
            if (CurrentState == newState) return;

            CurrentState = newState;

            switch (CurrentState)
            {
                case EnemyState.Attack:
                    if (!isAttacking)
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
                    isAttacking = false; // Stop angreb, hvis fjenden bliver såret.
                    StartCoroutine(ReturnToIdleAfterHurt());
                    break;
                case EnemyState.Flying:
                    Animator.Play("Flying");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // Coroutine til at returnere til "Idle"-tilstand efter "Hurt"-animationen.
        private IEnumerator ReturnToIdleAfterHurt()
        {
            yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length);
            ChangeState(EnemyState.Idle);
        }

        // Coroutine til at destruere fjenden efter "Death"-animationen.
        private IEnumerator ReturnAfterDeath()
        {
            yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length - 0.1f);
            Die();
        }

        // Coroutine til at udføre angreb og skade spilleren.
        private IEnumerator PerformAttack()
        {
            yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length);

            // Hvis spilleren stadig er inden for rækkevidde, påfør skade.
            if (Player != null && Vector2.Distance(transform.position, Player.position) <= attackRange)
            {
                PlayerHealth playerHealth = Player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage((int)damage);
                    Debug.Log("Enemy attacked the player!");
                }
            }

            attackCooldownTimer = Time.time;
            isAttacking = false; // Nulstil angrebsstatus.
            ChangeState(EnemyState.Idle); // Returner til "Idle"-tilstand.
        }
    }
}