using UnityEngine;

namespace UI
{
    // MainMenu håndterer logikken for hovedmenuen i spillet, herunder start af spillet og afslutning.
    public class MainMenu : MonoBehaviour
    {
        // Reference til kameraet, der bruges under gameplay.
        public Camera mainCamera;

        // Reference til kameraet, der bruges til hovedmenuen.
        public Camera menuCamera;

        // Start-metoden initialiserer menuen ved at aktivere menu-kameraet og deaktivere gameplay-kameraet.
        private void Start()
        {
            mainCamera.enabled = false; // Deaktiverer gameplay-kameraet.
            menuCamera.enabled = true;  // Aktiverer menu-kameraet.
        }

        // Update-metoden er tom, men kan bruges til at håndtere input eller animationer i menuen.
        void Update()
        {
        }

        // PlayGame-metoden aktiverer gameplay-kameraet og deaktiverer menu-kameraet for at starte spillet.
        private void PlayGame()
        {
            menuCamera.enabled = false; // Deaktiverer menu-kameraet.
            mainCamera.enabled = true;  // Aktiverer gameplay-kameraet.
        }

        // QuitGame-metoden afslutter spillet og logger en besked til konsollen.
        private void QuitGame()
        {
            Debug.Log("Quit"); // Logger en besked for debugging.
            Application.Quit(); // Afslutter applikationen.
        }
    }
}