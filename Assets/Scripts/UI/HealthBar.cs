using UnityEngine;
using UnityEngine.UI;
 
public class HealthBar : MonoBehaviour
{
    public Slider healthSlider;

    void Start()
    {
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.HealthChanged += UpdateHealthBar;
            healthSlider.maxValue = playerHealth.maxHealth;
            healthSlider.value = playerHealth.maxHealth;
        }
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        healthSlider.value = currentHealth;
    }
    
}