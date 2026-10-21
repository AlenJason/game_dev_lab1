using UnityEngine;

// Sinh thiên thạch và sao tại vị trí ngẫu nhiên trong map (không quá gần tàu người chơi).
// Tàu địch và boss do WaveManager quyết định, Spawner chỉ lo chọn vị trí sinh.
public class Spawner : MonoBehaviour
{
    [SerializeField] GameObject asteroidPrefab;
    [SerializeField] GameObject starPrefab;
    [SerializeField] Transform player;
    [SerializeField] int maxAsteroids = 18;
    [SerializeField] int maxStars = 6;
    [SerializeField] float spawnInterval = 1f;
    [SerializeField] float safeDistance = 5f;

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

        if (Count("Asteroid") < maxAsteroids) { Spawn(asteroidPrefab); }
        if (Count("Star") < maxStars) { Spawn(starPrefab); }
    }

    static int Count(string tag) => GameObject.FindGameObjectsWithTag(tag).Length;

    // Sinh prefab ở vị trí ngẫu nhiên, cách tàu người chơi ít nhất minDistance
    public GameObject Spawn(GameObject prefab, float minDistance = -1f)
    {
        if (minDistance < 0f) { minDistance = safeDistance; }
        Rect bounds = GameManager.Instance.MapBounds;
        Vector2 position;
        int tries = 0;
        do
        {
            position = new Vector2(Random.Range(bounds.xMin, bounds.xMax), Random.Range(bounds.yMin, bounds.yMax));
        } while (Vector2.Distance(position, player.position) < minDistance && ++tries < 50);

        return Instantiate(prefab, position, Quaternion.identity);
    }
}
