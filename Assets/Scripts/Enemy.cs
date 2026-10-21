using UnityEngine;

// Tàu địch: bay đuổi theo tàu người chơi và bắn laser khi ở đủ gần
[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] Sprite[] sprites;
    [SerializeField] GameObject enemyBulletPrefab;
    [SerializeField] float moveSpeed = 2.5f;
    [SerializeField] float keepDistance = 3f;   // dừng lại khi đã đủ gần tàu người chơi
    [SerializeField] float fireInterval = 2f;
    [SerializeField] float fireRange = 9f;
    [SerializeField] int maxHealth = 2;
    [SerializeField] int scoreValue = 20;

    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    Transform player;
    int health;
    float nextFireTime;

    // WaveManager gọi để tăng độ khó theo wave
    public void Setup(int wave)
    {
        maxHealth += wave / 3;
        moveSpeed = Mathf.Min(moveSpeed + wave * 0.1f, 5f);
        fireInterval = Mathf.Max(fireInterval - wave * 0.08f, 0.8f);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
        player = GameObject.FindGameObjectWithTag("Player").transform;
        health = maxHealth;
        nextFireTime = Time.time + Random.Range(1f, fireInterval);
    }

    void FixedUpdate()
    {
        if (player == null || !player.gameObject.activeInHierarchy) { rb.linearVelocity = Vector2.zero; return; }

        Vector2 toPlayer = player.position - transform.position;
        // Sprite tàu địch mặc định hướng xuống dưới nên cộng thêm 90 độ
        float angle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg + 90f;
        rb.MoveRotation(angle);
        rb.linearVelocity = toPlayer.magnitude > keepDistance ? toPlayer.normalized * moveSpeed : Vector2.zero;
    }

    void Update()
    {
        if (player == null || !player.gameObject.activeInHierarchy || GameManager.Instance.IsGameOver) { return; }

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= fireRange && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireInterval;
            Vector2 direction = (player.position - transform.position).normalized;
            EnemyBullet.Fire(enemyBulletPrefab, transform.position, direction);
        }
    }

    public void TakeDamage(int damage)
    {
        if (health <= 0) { return; }
        health -= damage;
        spriteRenderer.color = Color.Lerp(Color.white, Color.red, 1f - (float)health / maxHealth);
        if (health <= 0)
        {
            GameManager.Instance.EnemyKilled(scoreValue);
            Destroy(gameObject);
        }
    }
}
