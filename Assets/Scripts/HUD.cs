using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Giao diện trong game: điểm, level, thanh kinh nghiệm, wave, 3 trái tim và thanh máu boss
public class HUD : MonoBehaviour
{
    [SerializeField] Text scoreText;
    [SerializeField] Text levelText;
    [SerializeField] Image levelFill;
    [SerializeField] Image[] hearts;
    [SerializeField] GameObject bossBar;
    [SerializeField] Image bossFill;
    [SerializeField] Text bossNameText;
    [SerializeField] Text waveText;
    [SerializeField] Text bannerText;

    static readonly Color LostHeart = new Color(1f, 1f, 1f, 0.2f);

    void Awake()
    {
        bossBar.SetActive(false);
        bannerText.gameObject.SetActive(false);
    }

    public void SetScore(int score)
    {
        scoreText.text = "SCORE: " + score;
    }

    public void SetLevel(int level, float progress)
    {
        levelText.text = "LV " + level;
        levelFill.fillAmount = progress;
    }

    public void SetWave(int wave, bool isBossWave, int enemiesLeft)
    {
        waveText.text = wave == 0 ? "" : isBossWave ? "WAVE " + wave + " - BOSS" : "WAVE " + wave + " - ENEMIES: " + enemiesLeft;
    }

    // Hiện chữ lớn giữa màn hình trong 2 giây (ví dụ "WAVE 3", "BOSS WAVE 5")
    public void ShowWaveBanner(string message)
    {
        StopAllCoroutines();
        StartCoroutine(BannerRoutine(message));
    }

    IEnumerator BannerRoutine(string message)
    {
        bannerText.text = message;
        bannerText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        bannerText.gameObject.SetActive(false);
    }

    public void SetLives(int lives)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].color = i < lives ? Color.white : LostHeart;
        }
    }

    public void ShowBossBar(string bossName)
    {
        bossNameText.text = bossName;
        bossFill.fillAmount = 1f;
        bossBar.SetActive(true);
    }

    public void SetBossHealth(float ratio)
    {
        bossFill.fillAmount = ratio;
    }

    public void HideBossBar()
    {
        bossBar.SetActive(false);
    }
}
