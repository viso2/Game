using UnityEngine;

public class PlatformGenerator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject platformPrefab; // Assign a platform prefab in Unity
    public Transform player;
    public float minY = -2f, maxY = 2f;
    public float platformSpacing = 10f;
    public int initialPlatforms = 5;
    private float lastSpawnX;

    void Start()
    {
        lastSpawnX = player.position.x;
        for (int i = 0; i < 5; i++)
            SpawnPlatform();
    }

    // Update is called once per frame
    void Update()
    {
        if (player.position.x > lastSpawnX - (initialPlatforms*platformSpacing))
            SpawnPlatform();
    }

    void SpawnPlatform() {
        float y = Random.Range(minY, maxY);
        lastSpawnX += platformSpacing;
        GameObject newPlatform = Instantiate(platformPrefab, new Vector2(lastSpawnX, y), Quaternion.identity);

        float randonWidth = Random.Range(1.5f, 4f);
        newPlatform.transform.localScale = new Vector2(randonWidth, 1);

        newPlatform.tag = "Ground";
    }
}
