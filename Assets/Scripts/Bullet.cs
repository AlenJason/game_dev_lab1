using UnityEngine;

// Viên đạn: bay thẳng theo hướng bắn, phá thiên thạch khi trúng, tự huỷ sau một thời gian
[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] float speed = 14f;
    [SerializeField] float lifeTime = 2f;

    void Start()
    {
        GetComponent<Rigidbody2D>().linearVelocity = transform.up * speed;
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Asteroid"))
        {
            other.GetComponent<Asteroid>().Hit();
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
