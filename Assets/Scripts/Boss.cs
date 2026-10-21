using UnityEngine;

// Boss: lượn quanh tàu người chơi, bắn toả tròn và bắn 3 tia nhắm thẳng; dưới 50% máu sẽ bắn dồn dập hơn.
// Mỗi boss sau (mỗi 5 wave) mạnh hơn: nhiều máu hơn, bay nhanh hơn, bắn nhiều và dày hơn.
[RequireComponent(typeof(Rigidbody2D))]
public class Boss : MonoBehaviour, IDamageable
{
    [SerializeField] GameObject enemyBulletPrefab;
    [SerializeField] int baseHealth = 40;
    [SerializeField] int healthPerBoss = 30;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float orbitDistance = 6f;
    [SerializeField] float burstInterval = 3.5f; // bắn toả tròn
    [SerializeField] int burstBullets = 10;
    [SerializeField] float aimedInterval = 1.6f; // bắn 3 tia về phía người chơi
    [SerializeField] int scoreValue = 200;

    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    Transform player;
    HUD hud;
    int maxHealth;
    int health;
    float orbitAngle;
    float nextBurstTime;
    float nextAimedTime;

    bool IsEnraged => health <= maxHealth / 2;

    // WaveManager gọi khi tạo boss; bossNumber = 1, 2, 3... (wave 5, 10, 15...)
    public void Setup(int bossNumber, HUD bossHud)
    {
        int level = bossNumber - 1;
        maxHealth = baseHealth + healthPerBoss * level;
        moveSpeed += 0.3f * level;
        burstBullets = Mathf.Min(burstBullets + 2 * level, 24);
        burstInterval *= Mathf.Pow(0.9f, level);
        aimedInterval *= Mathf.Pow(0.9f, level);
        transform.localScale *= 1f + 0.1f * Mathf.Min(level, 4);
        health = maxHealth;
        hud = bossHud;
        hud.ShowBossBar("BOSS " + bossNumber);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        nextBurstTime = Time.time + 2f;
        nextAimedTime = Time.time + 1f;
    }

    void FixedUpdate()
    {
        if (player == null || !player.gameObject.activeInHierarchy) { return; }

        // Lượn vòng quanh người chơi ở khoảng cách orbitDistance, không ra khỏi map
        orbitAngle += (IsEnraged ? 35f : 20f) * Time.fixedDeltaTime;
        Vector2 offset = Quaternion.Euler(0f, 0f, orbitAngle) * Vector2.up * orbitDistance;
        Rect bounds = GameManager.Instance.MapBounds;
        Vector2 target = (Vector2)player.position + offset;
        target.x = Mathf.Clamp(target.x, bounds.xMin + 1.5f, bounds.xMax - 1.5f);
        target.y = Mathf.Clamp(target.y, bounds.yMin + 1.5f, bounds.yMax - 1.5f);
        rb.MovePosition(Vector2.MoveTowards(rb.position, target, moveSpeed * (IsEnraged ? 1.6f : 1f) * Time.fixedDeltaTime));
    }

    void Update()
    {
        if (player == null || !player.gameObject.activeInHierarchy || GameManager.Instance.IsGameOver) { return; }

        if (Time.time >= nextBurstTime)
        {
            nextBurstTime = Time.time + (IsEnraged ? burstInterval * 0.6f : burstInterval);
            FireBurst();
        }
        if (Time.time >= nextAimedTime)
        {
            nextAimedTime = Time.time + (IsEnraged ? aimedInterval * 0.6f : aimedInterval);
            FireAimed();
        }
    }

    void FireBurst()
    {
        float offset = Random.Range(0f, 360f);
        for (int i = 0; i < burstBullets; i++)
        {
            float angle = (offset + 360f / burstBullets * i) * Mathf.Deg2Rad;
            EnemyBullet.Fire(enemyBulletPrefab, transform.position, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
        }
    }

    void FireAimed()
    {
        Vector2 toPlayer = (player.position - transform.position).normalized;
        for (int i = -1; i <= 1; i++)
        {
            EnemyBullet.Fire(enemyBulletPrefab, transform.position, Quaternion.Euler(0f, 0f, 15f * i) * toPlayer);
        }
    }

    public void TakeDamage(int damage)
    {
        if (health <= 0) { return; }
        health -= damage;
        hud.SetBossHealth((float)health / maxHealth);
        spriteRenderer.color = IsEnraged ? new Color(1f, 0.6f, 0.6f) : Color.white;

        if (health <= 0)
        {
            GameManager.Instance.BossDefeated(scoreValue);
            Destroy(gameObject);
        }
    }
}
