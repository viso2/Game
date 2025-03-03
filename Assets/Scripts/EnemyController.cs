using UnityEngine;

public class EnemyController : MonoBehaviour
{
    
    public float speed = 2f;
    public float detectionRange = 5f;
    public Transform player;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer < detectionRange) ChasePlayer();
        }
    }

    private void ChasePlayer() {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("player")) Debug.Log("Player Hit");
    }
}
