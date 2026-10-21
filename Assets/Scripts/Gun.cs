using UnityEngine;

// Súng gắn ở mũi tàu: nhấn chuột trái hoặc Space để bắn đạn theo hướng mũi tàu
public class Gun : MonoBehaviour
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float fireRate = 6f; // số viên mỗi giây khi giữ nút bắn

    float nextFireTime;

    void Update()
    {
        if (GameManager.Instance.IsGameOver) { return; }

        bool firePressed = Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space);
        if (firePressed && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + 1f / fireRate;
            Instantiate(bulletPrefab, transform.position, transform.rotation);
        }
    }
}
