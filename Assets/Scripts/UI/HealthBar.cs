using Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HealthBar : MonoBehaviour
    {
        public Slider healthSlider;

        private void Start()
        {
            PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth == null) return;
            playerHealth.HealthChanged += UpdateHealthBar;
            healthSlider.maxValue = playerHealth.maxHealth;
            healthSlider.value = playerHealth.maxHealth;
        }

        public void UpdateHealthBar(int currentHealth, int maxHealth)
        {
            healthSlider.value = currentHealth;
            Debug.Log($"Current Health: {currentHealth}");
        }
    
    }
}