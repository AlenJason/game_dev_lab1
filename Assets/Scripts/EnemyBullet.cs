using UnityEngine;

// Đạn của tàu địch và boss: trúng tàu người chơi thì mất 1 mạng
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBullet : MonoBehaviour
{
    [SerializeField] float speed = 6f;
    [SerializeField] float lifeTime = 4f;

    // Tạo 1 viên đạn bay theo hướng direction
    public static void Fire(GameObject prefab, Vector2 position, Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        var bullet = Instantiate(prefab, position, Quaternion.Euler(0f, 0f, angle));
        bullet.GetComponent<Rigidbody2D>().linearVelocity = direction.normalized * bullet.GetComponent<EnemyBullet>().speed;
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>().TakeHit();
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
