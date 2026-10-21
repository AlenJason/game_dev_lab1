using UnityEngine;

// Thiên thạch: chọn hình, hướng bay, tốc độ và tốc độ xoay ngẫu nhiên; nảy khi chạm biên map
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class Asteroid : MonoBehaviour, IDamageable
{
    [SerializeField] Sprite[] sprites;
    [SerializeField] float minSpeed = 1f;
    [SerializeField] float maxSpeed = 3f;
    [SerializeField] float maxSpin = 90f;
    [SerializeField] int scoreValue = 10;

    bool isDestroyed;

    void Start()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
        // Chỉnh bán kính collider cho khớp với hình vừa chọn
        GetComponent<CircleCollider2D>().radius = spriteRenderer.sprite.bounds.extents.x * 0.85f;

        var rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Random.insideUnitCircle.normalized * Random.Range(minSpeed, maxSpeed);
        rb.angularVelocity = Random.Range(-maxSpin, maxSpin);
    }

    // Thiên thạch vỡ sau 1 phát bắn
    public void TakeDamage(int damage)
    {
        if (isDestroyed) { return; } // tránh cộng điểm 2 lần khi nhiều viên đạn trúng cùng lúc
        isDestroyed = true;
        GameManager.Instance.AddScore(scoreValue);
        Destroy(gameObject);
    }
}
