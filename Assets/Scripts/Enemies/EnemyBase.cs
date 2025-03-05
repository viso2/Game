using System;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] protected float speed = 2f;
    [SerializeField] protected float detectionRange = 5f;
    [SerializeField] protected float health = 100f;
    [SerializeField] protected float damage = 100f;
    protected Transform player;
    protected Rigidbody2D rb;
    protected bool facingRight = true;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb.freezeRotation = true;
    }

    protected virtual void Update() {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer < detectionRange) MoveTowardsPlayer();
    }

    protected abstract void MoveTowardsPlayer();

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            Debug.Log("Hit");
    }

    internal void TakeDamage(float attackDamage)
    {
       health -= attackDamage;
       if (health <= 0) Die();
       Debug.Log(health); 
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
