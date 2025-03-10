using UnityEngine;  
public class MainMenu : MonoBehaviour

{
    public Camera mainCamera;
    public Camera menuCamera;
    void Start()
    {
        mainCamera.enabled = false;
        menuCamera.enabled = true;
    }
   
    void Update()
    {
    }
    void PlayGame()
    {
        menuCamera.enabled = false;
        mainCamera.enabled = true;
        
    }
    void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
