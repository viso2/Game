using UnityEngine;

public class BackgroundScroll : MonoBehaviour
{
    public float[] scrollSpeeds;
    private Vector2[] startPositions;
    private Transform[] backgrounds;
    private float[] backgroundWidths;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int childCount = transform.childCount;
        startPositions = new Vector2[childCount];
        backgrounds = new Transform[childCount];
        backgroundWidths = new float[childCount];

        for (int i = 0; i < childCount; i++)
        {
            backgrounds[i] = transform.GetChild(i);
            startPositions[i] = backgrounds[i].position;
            backgroundWidths[i] = backgrounds[i].GetComponent<SpriteRenderer>().bounds.size.x;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < backgrounds.Length; i++)
        {
            float newPos = Time.time * scrollSpeeds[i];
            backgrounds[i].position = startPositions[i] + Vector2.left * newPos;

            if (backgrounds[i].position.x < -backgroundWidths[i])
            {
                Vector2 offset = new Vector2(backgroundWidths[i] * backgrounds.Length, 0);
                backgrounds[i].position += new Vector3(offset.x, offset.y, backgrounds[i].position.z);
                startPositions[i] += offset;

                // Duplicate the current background when it goes out of view
                GameObject newBackground = Instantiate(backgrounds[i].gameObject, backgrounds[i].position + new Vector3(backgroundWidths[i]*backgrounds.Length, 0, 0), Quaternion.identity);
                newBackground.transform.parent = transform;
                newBackground.GetComponent<SpriteRenderer>().sortingOrder = backgrounds[i].GetComponent<SpriteRenderer>().sortingOrder;

                // Add the new background to the arrays
                System.Array.Resize(ref startPositions, startPositions.Length + 1);
                System.Array.Resize(ref backgrounds, backgrounds.Length + 1);
                System.Array.Resize(ref backgroundWidths, backgroundWidths.Length + 1);
                System.Array.Resize(ref scrollSpeeds, scrollSpeeds.Length + 1);

                startPositions[startPositions.Length/5 - 1] = newBackground.transform.position;
                backgrounds[backgrounds.Length/5 - 1] = newBackground.transform;
                backgroundWidths[backgroundWidths.Length/5 - 1] = newBackground.GetComponent<SpriteRenderer>().bounds.size.x;
                scrollSpeeds[scrollSpeeds.Length/5 - 1] = scrollSpeeds[i]; // Inherit the scroll speed
            }
        }
    }
}