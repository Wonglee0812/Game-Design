using UnityEngine;

public class FireRescue_Camera : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target; // Kéo lính cứu hỏa vào đây

    [Header("Top-Down Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 15f, -10f); // Cao 15, lùi lại 10
    [SerializeField] private float smoothTime = 0.2f;

    private Vector3 currentVelocity;

    private void Start()
    {
        // [VI] Tự động chỉnh góc nhìn cắm xuống vị trí nhân vật ngay khi bắt đầu
        // [EN] Automatically adjust view angle looking down at character position on start
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.LookAt(target.position);
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // [VI] Tìm vị trí đích muốn tới
        // [EN] Find the target position to reach
        Vector3 targetPosition = target.position + offset;

        // [VI] Di chuyển mượt mà tới đó
        // [EN] Smoothly move there
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
    }
}