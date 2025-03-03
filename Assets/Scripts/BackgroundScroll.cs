using UnityEngine;

public class BackgroundScroll : MonoBehaviour
{
    public float[] scrollSpeeds;
    public GameObject backgroundPrefab;
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
                backgrounds[i].position += new Vector3(offset.x,offset.y,backgrounds[i].position.z);
                startPositions[i] += offset;

                // Instantiate a new background when the current one goes out of view
                GameObject newBackground = Instantiate(backgroundPrefab, backgrounds[i].position + new Vector3(backgroundWidths[i], 0, 0), Quaternion.identity);
                newBackground.transform.parent = transform;
                newBackground.GetComponent<SpriteRenderer>().sortingOrder = backgrounds[i].GetComponent<SpriteRenderer>().sortingOrder;
            }
        }
    }
}