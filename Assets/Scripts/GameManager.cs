using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Quản lý điểm, Game Over và chơi lại (phím R)
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] Text scoreText;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] Vector2 mapSize = new Vector2(40f, 24f);

    public bool IsGameOver { get; private set; }

    // Vùng bên trong map (chừa lề 1 đơn vị so với tường)
    public Rect MapBounds => new Rect(-mapSize / 2f + Vector2.one, mapSize - Vector2.one * 2f);

    int score;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        gameOverPanel.SetActive(false);
        UpdateScoreText();
    }

    void Update()
    {
        if (IsGameOver && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void AddScore(int amount)
    {
        if (IsGameOver) { return; }
        score += amount;
        UpdateScoreText();
    }

    public void GameOver()
    {
        IsGameOver = true;
        gameOverPanel.SetActive(true);
    }

    void UpdateScoreText()
    {
        scoreText.text = "SCORE: " + score;
    }
}
