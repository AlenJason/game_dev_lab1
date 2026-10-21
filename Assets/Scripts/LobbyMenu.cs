using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Màn hình lobby: Play, Tutorial và History (lịch sử lượt chơi, đánh dấu lượt có điểm cao nhất)
public class LobbyMenu : MonoBehaviour
{
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject tutorialPanel;
    [SerializeField] GameObject historyPanel;
    [SerializeField] Text bestScoreText;
    [SerializeField] Text historyText;
    [SerializeField] int historyRowsShown = 10;

    void Start()
    {
        Time.timeScale = 1f;
        ShowMain();
    }

    public void Play()
    {
        SceneManager.LoadScene("SpaceScene");
    }

    public void ShowMain()
    {
        mainPanel.SetActive(true);
        tutorialPanel.SetActive(false);
        historyPanel.SetActive(false);
        bestScoreText.text = "BEST SCORE: " + RunHistory.BestScore();
    }

    public void ShowTutorial()
    {
        mainPanel.SetActive(false);
        tutorialPanel.SetActive(true);
    }

    public void ShowHistory()
    {
        mainPanel.SetActive(false);
        historyPanel.SetActive(true);

        var runs = RunHistory.Load();
        if (runs.Count == 0)
        {
            historyText.text = "Chưa có lượt chơi nào.";
            return;
        }

        // Lượt có điểm cao nhất (nếu bằng điểm thì lấy lượt chơi sớm hơn)
        int bestIndex = runs.FindIndex(r => r.score == runs.Max(x => x.score));
        var best = runs[bestIndex];

        string text = "<color=#FFD54A>BEST: " + best.score + " điểm  -  Wave " + best.wave + "  -  LV " + best.level +
                      "  -  " + best.playedAt + "</color>\n\n";

        // Các lượt gần nhất, mới nhất ở trên
        for (int i = runs.Count - 1; i >= 0 && i >= runs.Count - historyRowsShown; i--)
        {
            var r = runs[i];
            string row = "#" + (i + 1) + "   " + r.playedAt + "   Score " + r.score + "   Wave " + r.wave + "   LV " + r.level +
                         "   Địch " + r.enemiesKilled + "   Boss " + r.bossesKilled;
            text += i == bestIndex ? "<color=#FFD54A>" + row + "   ★ BEST</color>\n" : row + "\n";
        }
        historyText.text = text;
    }
}
