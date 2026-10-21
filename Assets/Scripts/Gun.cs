using UnityEngine;

// Súng gắn ở mũi tàu: nhấn chuột trái hoặc Space để bắn đạn theo hướng mũi tàu
public class Gun : MonoBehaviour
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float fireRate = 5f;    // số lượt bắn mỗi giây khi giữ nút bắn
    [SerializeField] int bulletCount = 1;    // số tia đạn mỗi lượt (nâng cấp Multi Shot)
    [SerializeField] int damage = 1;         // sát thương mỗi viên (nâng cấp Power Shot)
    [SerializeField] float spreadAngle = 12f;

    public float FireRate { get => fireRate; set => fireRate = value; }
    public int BulletCount { get => bulletCount; set => bulletCount = value; }
    public int Damage { get => damage; set => damage = value; }

    float nextFireTime;

    void Update()
    {
        if (GameManager.Instance.IsGameOver || Time.timeScale == 0f) { return; }

        bool firePressed = Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space);
        if (firePressed && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + 1f / fireRate;
            Fire();
        }
    }

    void Fire()
    {
        // Các tia đạn toả đều quanh hướng mũi tàu
        float startAngle = -spreadAngle * (bulletCount - 1) / 2f;
        for (int i = 0; i < bulletCount; i++)
        {
            Quaternion rotation = transform.rotation * Quaternion.Euler(0f, 0f, startAngle + spreadAngle * i);
            var bullet = Instantiate(bulletPrefab, transform.position, rotation);
            bullet.GetComponent<Bullet>().Damage = damage;
        }
    }
}
