using System.Collections;
using UnityEngine;

// 3 mạng của tàu: trúng đòn mất 1 tim, nhấp nháy và bất tử trong thời gian ngắn
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] int maxLives = 3;
    [SerializeField] float invincibleTime = 2f;
    [SerializeField] HUD hud;

    public int Lives { get; private set; }
    public int MaxLives => maxLives;

    SpriteRenderer spriteRenderer;
    bool isInvincible;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Lives = maxLives;
        hud.SetLives(Lives);
    }

    // Trả về true nếu đòn đánh có tác dụng (không đang bất tử)
    public bool TakeHit()
    {
        if (isInvincible || GameManager.Instance.IsGameOver) { return false; }

        Lives--;
        hud.SetLives(Lives);

        if (Lives <= 0)
        {
            GameManager.Instance.GameOver();
            gameObject.SetActive(false);
        }
        else
        {
            StartCoroutine(InvincibleRoutine());
        }
        return true;
    }

    public void Heal(int amount)
    {
        Lives = Mathf.Min(Lives + amount, maxLives);
        hud.SetLives(Lives);
    }

    IEnumerator InvincibleRoutine()
    {
        isInvincible = true;
        float endTime = Time.time + invincibleTime;
        while (Time.time < endTime)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.1f);
        }
        spriteRenderer.enabled = true;
        isInvincible = false;
    }
}
