using UnityEngine;

namespace Core
{
    public class PlayerHealth : MonoBehaviour
    {
        public int maxHealth = 100;
        public int currentHealth;
        public delegate void OnHealthChanged(int currentHealth, int maxHealth);
        public event OnHealthChanged HealthChanged;

        void Start()
        {
            currentHealth = maxHealth;
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            // Handle player death (e.g., reload the scene, show game over screen, etc.)
            Debug.Log("Player died!");
        }

    }
}