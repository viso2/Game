using UnityEngine;

public class Parallax : MonoBehaviour
{
    public Transform[] backgroundLayers; // Assign the background elements in order (back to front)
    public float[] speedMultipliers; // Define speed for each layer (smallest for back, largest for front)
    public float baseSpeed = 1f;

    private Vector3 lastCameraPosition;
    private float[] layerWidths;

    void Start()
    {
        lastCameraPosition = Camera.main.transform.position;
        layerWidths = new float[backgroundLayers.Length];

        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            SpriteRenderer spriteRenderer = backgroundLayers[i].GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                layerWidths[i] = 21.3f/*spriteRenderer.bounds.size.x*/;
            }
        }
    }

    void Update()
    {
        Vector3 cameraMovement = Camera.main.transform.position - lastCameraPosition;

        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            float parallaxEffect = speedMultipliers[i] * baseSpeed;
            backgroundLayers[i].position += new Vector3(cameraMovement.x * parallaxEffect, 0, 0);

            // Check if the background layer has exited the screen to the right
            if (Camera.main.transform.position.x - backgroundLayers[i].position.x >= layerWidths[i])
            {
                // Reposition the background layer to the end of the sequence
                Vector3 newPosition = backgroundLayers[i].position;
                newPosition.x += layerWidths[i] * backgroundLayers.Length/5;
                backgroundLayers[i].position = newPosition;
            }
            // Check if the background layer has exited the screen to the left
            else if (Camera.main.transform.position.x - backgroundLayers[i].position.x <= -layerWidths[i])
            {
                // Reposition the background layer to the start of the sequence
                Vector3 newPosition = backgroundLayers[i].position;
                newPosition.x -= layerWidths[i] * backgroundLayers.Length/5;
                backgroundLayers[i].position = newPosition;
            }
        }

        lastCameraPosition = Camera.main.transform.position;
    }
}
