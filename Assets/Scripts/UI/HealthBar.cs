using Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    // HealthBar er ansvarlig for at vise spillerens helbred visuelt i UI'et.
    public class HealthBar : MonoBehaviour
    {
        // Reference til UI Slider, der repræsenterer spillerens helbred.
        public Slider healthSlider;

        // Start-metoden initialiserer HealthBar og forbinder den med PlayerHealth.
        private void Start()
        {
            // Finder den første instans af PlayerHealth i scenen.
            PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
            
            // Hvis PlayerHealth ikke findes, afslut metoden.
            if (playerHealth == null) return;

            // Abonnerer på HealthChanged-eventet for at opdatere helbredet dynamisk.
            playerHealth.HealthChanged += UpdateHealthBar;

            // Initialiserer sliderens maksimumværdi og startværdi baseret på spillerens helbred.
            healthSlider.maxValue = playerHealth.maxHealth;
            healthSlider.value = playerHealth.maxHealth;
        }

        // Metode til at opdatere helbredsbjælken, når spillerens helbred ændres.
        public void UpdateHealthBar(int currentHealth, int maxHealth)
        {
            // Sætter sliderens værdi til det aktuelle helbred.
            healthSlider.value = currentHealth;

            // Logger det aktuelle helbred til konsollen for debugging.
            Debug.Log($"Current Health: {currentHealth}");
        }
    }
}