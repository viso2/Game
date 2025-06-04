using UnityEngine;

namespace Environment
{
    public class Parallax : MonoBehaviour
    {
        // En liste af baggrundslag, der skal have parallax-effekt.
        // Lagene skal tilføjes i rækkefølge fra bageste til forreste.
        public Transform[] backgroundLayers;

        // Hastighedsmultiplikatorer for hvert lag.
        // Mindre værdier for bageste lag og større værdier for forreste lag.
        public float[] speedMultipliers;

        // Basis-hastighed for parallax-effekten.
        public float baseSpeed = 1f;

        // Holder styr på kameraets position fra sidste frame.
        private Vector3 _lastCameraPosition;

        // Bredderne af hvert lag, bruges til repositionering.
        private float[] _layerWidths;

        void Start()
        {
            // Initialiserer kameraets position og lagbredder.
            _lastCameraPosition = Camera.main.transform.position;
            _layerWidths = new float[backgroundLayers.Length];

            // Beregner bredderne af hvert lag baseret på deres SpriteRenderer-komponent.
            for (int i = 0; i < backgroundLayers.Length; i++)
            {
                SpriteRenderer spriteRenderer = backgroundLayers[i].GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    _layerWidths[i] = 21.3f; // Hardcoded bredde, kan erstattes med spriteRenderer.bounds.size.x.
                }
            }
        }

        void Update()
        {
            // Hvis der ikke er et kamera, afslut opdateringen.
            if (Camera.main == null)
            {
                return;
            }

            // Beregner kameraets bevægelse siden sidste frame.
            Vector3 cameraMovement = Camera.main.transform.position - _lastCameraPosition;

            // Itererer gennem hvert lag for at anvende parallax-effekten.
            for (int i = 0; i < backgroundLayers.Length; i++)
            {
                // Beregner parallax-effekten baseret på lagets hastighedsmultiplikator.
                float parallaxEffect = speedMultipliers[i] * baseSpeed;
                backgroundLayers[i].position += new Vector3(cameraMovement.x * parallaxEffect, 0, 0);

                // Tjekker, om laget er gået ud af skærmen til højre.
                if (Camera.main.transform.position.x - backgroundLayers[i].position.x >= _layerWidths[i])
                {
                    // Repositionerer laget til slutningen af sekvensen.
                    Vector3 newPosition = backgroundLayers[i].position;
                    newPosition.x += _layerWidths[i] * backgroundLayers.Length / 5;
                    backgroundLayers[i].position = newPosition;
                }
                // Tjekker, om laget er gået ud af skærmen til venstre.
                else if (Camera.main.transform.position.x - backgroundLayers[i].position.x <= -_layerWidths[i])
                {
                    // Repositionerer laget til starten af sekvensen.
                    Vector3 newPosition = backgroundLayers[i].position;
                    newPosition.x -= _layerWidths[i] * backgroundLayers.Length / 5;
                    backgroundLayers[i].position = newPosition;
                }
            }

            // Opdaterer kameraets position til den aktuelle frame.
            _lastCameraPosition = Camera.main.transform.position;
        }
    }
}