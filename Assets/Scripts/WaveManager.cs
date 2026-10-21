using System.Collections;
using UnityEngine;

// Chế độ wave: mỗi wave sinh một lượng tàu địch, hạ hết thì sang wave mới (đông hơn, mạnh hơn).
// Cứ mỗi 5 wave là wave boss: chỉ có boss, không có tàu địch con; boss sau mạnh hơn boss trước.
public class WaveManager : MonoBehaviour
{
    [SerializeField] Spawner spawner;
    [SerializeField] HUD hud;
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject bossPrefab;
    [SerializeField] int bossEveryWaves = 5;
    [SerializeField] int firstWaveEnemies = 3;
    [SerializeField] int extraEnemiesPerWave = 2;
    [SerializeField] float enemySpawnDelay = 0.8f;   // khoảng cách giữa 2 lần sinh tàu địch trong 1 wave
    [SerializeField] float timeBetweenWaves = 3f;

    public int Wave { get; private set; }
    public bool IsBossWave { get; private set; }

    int enemiesToSpawn;

    void Start()
    {
        StartCoroutine(RunWaves());
    }

    void Update()
    {
        int remaining = enemiesToSpawn + GameObject.FindGameObjectsWithTag("Enemy").Length;
        hud.SetWave(Wave, IsBossWave, remaining);
    }

    IEnumerator RunWaves()
    {
        yield return new WaitForSeconds(1.5f);

        while (!GameManager.Instance.IsGameOver)
        {
            Wave++;
            IsBossWave = Wave % bossEveryWaves == 0;
            int enemyCount = firstWaveEnemies + (Wave - 1) * extraEnemiesPerWave;
            enemiesToSpawn = IsBossWave ? 0 : enemyCount; // để HUD hiện sẵn số địch trong lúc hiện banner
            hud.ShowWaveBanner(IsBossWave ? "BOSS WAVE " + Wave : "WAVE " + Wave);
            yield return new WaitForSeconds(2f);

            if (IsBossWave)
            {
                yield return BossWave(Wave / bossEveryWaves);
            }
            else
            {
                yield return EnemyWave(enemyCount);
            }

            if (GameManager.Instance.IsGameOver) { yield break; }
            hud.ShowWaveBanner("WAVE " + Wave + " CLEAR!");
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    IEnumerator EnemyWave(int enemyCount)
    {
        // Sinh lần lượt từng tàu địch, độ khó tăng theo số wave
        enemiesToSpawn = enemyCount;
        while (enemiesToSpawn > 0)
        {
            if (GameManager.Instance.IsGameOver) { yield break; }
            spawner.Spawn(enemyPrefab).GetComponent<Enemy>().Setup(Wave);
            enemiesToSpawn--;
            yield return new WaitForSeconds(enemySpawnDelay);
        }

        // Chờ người chơi hạ hết tàu địch của wave này
        while (GameObject.FindGameObjectsWithTag("Enemy").Length > 0)
        {
            if (GameManager.Instance.IsGameOver) { yield break; }
            yield return null;
        }
    }

    IEnumerator BossWave(int bossNumber)
    {
        var boss = spawner.Spawn(bossPrefab, 8f).GetComponent<Boss>();
        boss.Setup(bossNumber, hud);

        // Chờ tới khi boss bị hạ (Unity coi object đã Destroy là null)
        while (boss != null)
        {
            if (GameManager.Instance.IsGameOver) { yield break; }
            yield return null;
        }
        IsBossWave = false;
    }
}
