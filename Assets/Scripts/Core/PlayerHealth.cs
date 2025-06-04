using UnityEngine;
using System.Collections;

namespace Core
{
    // Klassen PlayerHealth håndterer spillerens helbred og relaterede funktioner.
    public class PlayerHealth : MonoBehaviour
    {
        // Maksimalt helbred for spilleren. Kan justeres i Unity-editoren.
        public int maxHealth = 100;

        // Det aktuelle helbred for spilleren. Initialiseres i Start-metoden.
        public int currentHealth;

        // En delegeret type, der bruges til at definere en event for ændringer i helbred.
        public delegate void OnHealthChanged(int currentHealth, int maxHealth);

        // Event, der udløses, når spillerens helbred ændres.
        public event OnHealthChanged HealthChanged;

        // Start-metoden initialiserer spillerens helbred og udløser HealthChanged-eventet.
        void Start()
        {
            currentHealth = maxHealth; // Sætter det aktuelle helbred til det maksimale helbred.
            HealthChanged?.Invoke(currentHealth, maxHealth); // Udløser eventet, hvis der er abonnenter.
        }

        // Metode til at tage skade. Reducerer spillerens helbred og håndterer dødslogik.
        public void TakeDamage(int damage)
        {
            currentHealth -= damage; // Reducerer det aktuelle helbred med den angivne skade.
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Sikrer, at helbredet forbliver inden for gyldige grænser.
            HealthChanged?.Invoke(currentHealth, maxHealth); // Udløser eventet for at opdatere abonnenter.

            Debug.Log($"Current Health: {currentHealth}"); // Logger det aktuelle helbred til konsollen.

            if (currentHealth <= 0) // Tjekker, om spilleren er død.
            {
                Die(); // Kalder Die-metoden for at håndtere spillerens død.
            }
        }

        // Metode, der håndterer spillerens død.
        private void Die()
        {
            // Her kan du implementere logik for spillerens død, såsom at genindlæse scenen eller vise en "Game Over"-skærm.
            Debug.Log("Player died!"); // Logger en besked om, at spilleren er død.
        }
    }
}