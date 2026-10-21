using UnityEngine;

// Ngôi sao: trôi ngẫu nhiên trong không gian, thỉnh thoảng đổi hướng; tàu chạm vào sẽ được cộng điểm
[RequireComponent(typeof(Rigidbody2D))]
public class Star : MonoBehaviour
{
    [SerializeField] float speed = 1.5f;
    [SerializeField] float changeDirectionTime = 2f;
    [SerializeField] float spinSpeed = 60f;
    [SerializeField] int scoreValue = 5;

    public int ScoreValue => scoreValue;

    Rigidbody2D rb;
    float timer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.angularVelocity = spinSpeed;
        PickRandomDirection();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f) { PickRandomDirection(); }

        // Không cho sao trôi ra khỏi map: chạm biên thì quay đầu
        Vector2 pos = transform.position;
        Vector2 vel = rb.linearVelocity;
        Rect bounds = GameManager.Instance.MapBounds;
        if ((pos.x < bounds.xMin && vel.x < 0) || (pos.x > bounds.xMax && vel.x > 0)) { vel.x = -vel.x; }
        if ((pos.y < bounds.yMin && vel.y < 0) || (pos.y > bounds.yMax && vel.y > 0)) { vel.y = -vel.y; }
        rb.linearVelocity = vel;
    }

    void PickRandomDirection()
    {
        rb.linearVelocity = Random.insideUnitCircle.normalized * speed;
        timer = changeDirectionTime;
    }
}
