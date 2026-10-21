using UnityEngine;

// Đạn của tàu: bay thẳng theo hướng bắn, gây sát thương cho thiên thạch / tàu địch / boss
[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] float speed = 14f;
    [SerializeField] float lifeTime = 2f;

    public int Damage { get; set; } = 1;

    void Start()
    {
        GetComponent<Rigidbody2D>().linearVelocity = transform.up * speed;
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
            return;
        }

        var target = other.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(Damage);
            Destroy(gameObject);
        }
    }
}
