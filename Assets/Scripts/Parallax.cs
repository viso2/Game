using UnityEngine;

public class Parallax : MonoBehaviour
{
    public Transform[] backgroundLayers; // Assign the background elements in order (back to front)
    public float[] speedMultipliers; // Define speed for each layer (smallest for back, largest for front)
    public float baseSpeed = 1f;

    private Vector3 lastCameraPosition;

    void Start()
    {
        lastCameraPosition = Camera.main.transform.position;
    }

    void Update()
    {
        Vector3 cameraMovement = Camera.main.transform.position - lastCameraPosition;

        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            float parallaxEffect = speedMultipliers[i] * baseSpeed;
            backgroundLayers[i].position += new Vector3(cameraMovement.x * parallaxEffect, 0, 0);
        }

        lastCameraPosition = Camera.main.transform.position;
    }
}