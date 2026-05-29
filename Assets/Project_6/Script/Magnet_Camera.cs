using UnityEngine;

public class Magnet_Camera : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // [VI] Kéo nhân vật vào đây / [EN] Drag the character here
    public Vector3 offset = new Vector3(0f, 2f, -10f); // [VI] Độ lệch (Z phải là số âm) / [EN] Offset (Z must be negative)

    [Header("Smooth Settings")]
    [Range(0.01f, 1f)]
    public float smoothTime = 0.25f; // [VI] Thời gian làm mượt (Càng nhỏ càng nhanh) / [EN] Smooth time (Smaller is faster)

    private Vector3 velocity = Vector3.zero;

    private void LateUpdate()
    {
        // [VI] Nếu chưa gán mục tiêu thì không làm gì cả
        // [EN] If no target is assigned, do nothing
        if (target == null) return;

        // [VI] Tính toán vị trí đích đến của Camera
        // [EN] Calculate the target position for the Camera
        Vector3 targetPosition = target.position + offset;

        // [VI] Di chuyển mượt mà từ vị trí hiện tại đến vị trí đích
        // [EN] Smoothly move from current position to target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}