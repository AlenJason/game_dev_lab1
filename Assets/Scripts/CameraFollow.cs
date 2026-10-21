using UnityEngine;

// Camera bám theo tàu nhưng không vượt ra ngoài biên map
public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector2 mapSize = new Vector2(40f, 24f);
    [SerializeField] float smoothSpeed = 5f;

    Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (target == null || !target.gameObject.activeInHierarchy) { return; }

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        float x = Mathf.Clamp(target.position.x, -mapSize.x / 2f + halfWidth, mapSize.x / 2f - halfWidth);
        float y = Mathf.Clamp(target.position.y, -mapSize.y / 2f + halfHeight, mapSize.y / 2f - halfHeight);

        Vector3 desired = new Vector3(x, y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }
}
