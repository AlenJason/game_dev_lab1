using UnityEngine;

// Điều khiển tàu vũ trụ: WASD / phím mũi tên để di chuyển, mũi tàu luôn hướng theo chuột
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 6f;

    Rigidbody2D rb;
    Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver) { return; }

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
        // Va vào thiên thạch thì thua
        if (collision.gameObject.CompareTag("Asteroid"))
        {
            GameManager.Instance.GameOver();
            gameObject.SetActive(false);
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
