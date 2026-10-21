using UnityEngine;

// Sinh thiên thạch và sao tại vị trí ngẫu nhiên trong map, không sinh quá gần tàu
public class Spawner : MonoBehaviour
{
    [SerializeField] GameObject asteroidPrefab;
    [SerializeField] GameObject starPrefab;
    [SerializeField] Transform player;
    [SerializeField] int maxAsteroids = 12;
    [SerializeField] int maxStars = 6;
    [SerializeField] float spawnInterval = 1.5f;
    [SerializeField] float safeDistance = 4f;

    float timer;

    void Start()
    {
        // Lúc bắt đầu sinh sẵn một nửa số lượng
        for (int i = 0; i < maxAsteroids / 2; i++) { Spawn(asteroidPrefab); }
        for (int i = 0; i < maxStars / 2; i++) { Spawn(starPrefab); }
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver) { return; }

        timer -= Time.deltaTime;
        if (timer > 0f) { return; }
        timer = spawnInterval;

        if (GameObject.FindGameObjectsWithTag("Asteroid").Length < maxAsteroids) { Spawn(asteroidPrefab); }
        if (GameObject.FindGameObjectsWithTag("Star").Length < maxStars) { Spawn(starPrefab); }
    }

    void Spawn(GameObject prefab)
    {
        Rect bounds = GameManager.Instance.MapBounds;
        Vector2 position;
        do
        {
            position = new Vector2(Random.Range(bounds.xMin, bounds.xMax), Random.Range(bounds.yMin, bounds.yMax));
        } while (Vector2.Distance(position, player.position) < safeDistance);

        Instantiate(prefab, position, Quaternion.identity);
    }
}
