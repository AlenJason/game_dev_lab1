using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// Khi lên level: tạm dừng game, cho chọn 1 trong 3 kỹ năng nâng cấp ngẫu nhiên
public class UpgradeManager : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Button[] optionButtons;
    [SerializeField] Text[] optionTexts;
    [SerializeField] Gun gun;
    [SerializeField] PlayerController player;
    [SerializeField] PlayerHealth playerHealth;

    public bool IsOpen => panel.activeSelf;

    class Upgrade
    {
        public string title;
        public string description;
        public Func<bool> isAvailable;
        public Action apply;
    }

    List<Upgrade> upgrades;

    void Awake()
    {
        panel.SetActive(false);
        upgrades = new List<Upgrade>
        {
            new Upgrade { title = "RAPID FIRE", description = "Bắn nhanh hơn 30%",
                isAvailable = () => gun.FireRate < 14f, apply = () => gun.FireRate *= 1.3f },
            new Upgrade { title = "MULTI SHOT", description = "Thêm 1 tia đạn",
                isAvailable = () => gun.BulletCount < 5, apply = () => gun.BulletCount++ },
            new Upgrade { title = "POWER SHOT", description = "Sát thương +1",
                isAvailable = () => gun.Damage < 5, apply = () => gun.Damage++ },
            new Upgrade { title = "ENGINE BOOST", description = "Tốc độ bay +1",
                isAvailable = () => player.MoveSpeed < 10f, apply = () => player.MoveSpeed += 1f },
            new Upgrade { title = "REPAIR", description = "Hồi 1 trái tim",
                isAvailable = () => playerHealth.Lives < playerHealth.MaxLives, apply = () => playerHealth.Heal(1) },
        };
    }

    public void Show()
    {
        // Chọn ngẫu nhiên tối đa 3 kỹ năng còn nâng cấp được
        var choices = upgrades.Where(u => u.isAvailable()).OrderBy(_ => UnityEngine.Random.value).Take(optionButtons.Length).ToList();
        if (choices.Count == 0) { GameManager.Instance.OnUpgradeChosen(); return; }

        for (int i = 0; i < optionButtons.Length; i++)
        {
            bool hasChoice = i < choices.Count;
            optionButtons[i].gameObject.SetActive(hasChoice);
            if (!hasChoice) { continue; }

            Upgrade upgrade = choices[i];
            optionTexts[i].text = upgrade.title + "\n<size=26>" + upgrade.description + "</size>";
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => Choose(upgrade));
        }

        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    void Choose(Upgrade upgrade)
    {
        upgrade.apply();
        panel.SetActive(false);
        Time.timeScale = 1f;
        GameManager.Instance.OnUpgradeChosen();
    }
}
