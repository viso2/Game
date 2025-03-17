using UnityEngine;

namespace UI
{
    public class MainMenu : MonoBehaviour

    {
        public Camera mainCamera;
        public Camera menuCamera;

        private void Start()
        {
            mainCamera.enabled = false;
            menuCamera.enabled = true;
        }
   
        void Update()
        {
        }

        private void PlayGame()
        {
            menuCamera.enabled = false;
            mainCamera.enabled = true;
        
        }

        private void QuitGame()
        {
            Debug.Log("Quit");
            Application.Quit();
        }
    }
}
