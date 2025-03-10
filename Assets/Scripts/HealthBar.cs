using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider healthSlider;

    // Set the maximum health value for the slider
    public void SetMaxHealth(int maxHealth)
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
    }

    // Update the health value for the slider
    public void SetHealth(int health)
    {
        healthSlider.value = health;
    }
}