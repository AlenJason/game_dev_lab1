using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Quản lý điểm, level (chọn kỹ năng), Game Over và lưu lịch sử lượt chơi
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] HUD hud;
    [SerializeField] UpgradeManager upgradeManager;
    [SerializeField] WaveManager waveManager;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] Text gameOverScoreText;
    [SerializeField] Vector2 mapSize = new Vector2(40f, 24f);

    public bool IsGameOver { get; private set; }
    public int Level { get; private set; } = 1;

    // Vùng bên trong map (chừa lề 1 đơn vị so với tường)
    public Rect MapBounds => new Rect(-mapSize / 2f + Vector2.one, mapSize - Vector2.one * 2f);

    int score;
    int enemiesKilled;
    int bossesKilled;
    int pendingLevelUps;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        gameOverPanel.SetActive(false);
        hud.SetScore(score);
        hud.SetLevel(Level, LevelProgress());
    }

    // Điểm cần có để đạt level: 1 -> 0, 2 -> 80, 3 -> 240, 4 -> 480, 5 -> 800 ...
    static int ScoreForLevel(int level) => 40 * level * (level - 1);

    float LevelProgress() => Mathf.InverseLerp(ScoreForLevel(Level), ScoreForLevel(Level + 1), score);

    public void AddScore(int amount)
    {
        if (IsGameOver) { return; }
        score += amount;
        hud.SetScore(score);

        while (score >= ScoreForLevel(Level + 1))
        {
            Level++;
            pendingLevelUps++;
        }
        hud.SetLevel(Level, LevelProgress());

        if (pendingLevelUps > 0 && !upgradeManager.IsOpen) { upgradeManager.Show(); }
    }

    // UpgradeManager gọi sau khi người chơi chọn xong 1 kỹ năng
    public void OnUpgradeChosen()
    {
        pendingLevelUps--;
        if (pendingLevelUps > 0) { upgradeManager.Show(); }
    }

    public void EnemyKilled(int scoreValue)
    {
        enemiesKilled++;
        AddScore(scoreValue);
    }

    public void BossDefeated(int scoreValue)
    {
        bossesKilled++;
        hud.HideBossBar();
        FindFirstObjectByType<PlayerHealth>()?.Heal(1);
        AddScore(scoreValue);
    }

    public void GameOver()
    {
        if (IsGameOver) { return; }
        IsGameOver = true;

        bool isBest = RunHistory.Save(new RunRecord
        {
            score = score,
            level = Level,
            wave = waveManager.Wave,
            enemiesKilled = enemiesKilled,
            bossesKilled = bossesKilled
        });

        gameOverScoreText.text = "WAVE " + waveManager.Wave + "   SCORE: " + score + (isBest ? "\n<color=#FFD54A>NEW BEST!</color>" : "\nBEST: " + RunHistory.BestScore());
        gameOverPanel.SetActive(true);
    }

    // Gắn vào nút trên màn hình Game Over
    public void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToLobby()
    {
        SceneManager.LoadScene("Lobby");
    }
}
