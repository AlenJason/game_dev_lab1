using UnityEngine;
using UnityEngine.SceneManagement;

// Tạm dừng game: bấm Esc hoặc P để mở/đóng bảng PAUSED (Resume, Restart, Lobby)
public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pausePanel;
    [SerializeField] UpgradeManager upgradeManager;

    public bool IsPaused { get; private set; }

    void Start()
    {
        pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (IsPaused) { Resume(); }
            else { Pause(); }
        }
    }

    public void Pause()
    {
        // Không cho pause khi đang chọn kỹ năng hoặc đã Game Over (để các bảng không chồng lên nhau)
        if (GameManager.Instance.IsGameOver || upgradeManager.IsOpen) { return; }

        IsPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // Gắn vào nút RESUME
    public void Resume()
    {
        IsPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // Gắn vào nút RESTART
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Gắn vào nút LOBBY
    public void BackToLobby()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Lobby");
    }
}
