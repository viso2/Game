using UnityEngine;

namespace Environment
{
    public class Parallax : MonoBehaviour
    {
        public Transform[] backgroundLayers; // Assign the background elements in order (back to front)
        public float[] speedMultipliers; // Define speed for each layer (smallest for back, largest for front)
        public float baseSpeed = 1f;

        private Vector3 _lastCameraPosition;
        private float[] _layerWidths;

        void Start()
        {
            _lastCameraPosition = Camera.main.transform.position;
            _layerWidths = new float[backgroundLayers.Length];

            for (int i = 0; i < backgroundLayers.Length; i++)
            {
                SpriteRenderer spriteRenderer = backgroundLayers[i].GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    _layerWidths[i] = 21.3f/*spriteRenderer.bounds.size.x*/;
                }
            }
        }

        void Update()
        {
            if (Camera.main == null)
            {
                return;
            }
            Vector3 cameraMovement = Camera.main.transform.position - _lastCameraPosition;

            for (int i = 0; i < backgroundLayers.Length; i++)
            {
                float parallaxEffect = speedMultipliers[i] * baseSpeed;
                backgroundLayers[i].position += new Vector3(cameraMovement.x * parallaxEffect, 0, 0);

                // Check if the background layer has exited the screen to the right
                if (Camera.main.transform.position.x - backgroundLayers[i].position.x >= _layerWidths[i])
                {
                    // Reposition the background layer to the end of the sequence
                    Vector3 newPosition = backgroundLayers[i].position;
                    newPosition.x += _layerWidths[i] * backgroundLayers.Length/5;
                    backgroundLayers[i].position = newPosition;
                }
                // Check if the background layer has exited the screen to the left
                else if (Camera.main.transform.position.x - backgroundLayers[i].position.x <= -_layerWidths[i])
                {
                    // Reposition the background layer to the start of the sequence
                    Vector3 newPosition = backgroundLayers[i].position;
                    newPosition.x -= _layerWidths[i] * backgroundLayers.Length/5;
                    backgroundLayers[i].position = newPosition;
                }
            }

            _lastCameraPosition = Camera.main.transform.position;
        }
    }
}
