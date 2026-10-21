using UnityEngine;

// Điều khiển tàu vũ trụ: WASD / phím mũi tên để di chuyển, mũi tàu luôn hướng theo chuột
[RequireComponent(typeof(Rigidbody2D), typeof(PlayerHealth))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 6f;

    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }

    Rigidbody2D rb;
    PlayerHealth health;
    Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver || Time.timeScale == 0f) { return; }

        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        // Xoay tàu về phía con trỏ chuột (sprite tàu mặc định hướng lên trên)
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDirection = mouseWorld - transform.position;
        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = GameManager.Instance.IsGameOver ? Vector2.zero : moveInput * moveSpeed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject other = collision.gameObject;

        // Đâm vào thiên thạch hoặc tàu địch: mất 1 mạng, vật đó bị phá huỷ
        if (other.CompareTag("Asteroid") || other.CompareTag("Enemy"))
        {
            if (health.TakeHit()) { Destroy(other); }
        }
        else if (other.CompareTag("Boss"))
        {
            health.TakeHit();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Nhặt sao được cộng điểm
        if (other.CompareTag("Star"))
        {
            GameManager.Instance.AddScore(other.GetComponent<Star>().ScoreValue);
            Destroy(other.gameObject);
        }
    }
}
