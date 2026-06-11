using UnityEngine;

public class FireRescue_Camera : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target; // Kéo lính cứu hỏa vào đây / Drag firefighter here

    [Header("Position Settings")]
    // [VI] Đổi Z thành 10 (số dương) để camera đứng trước mặt nhìn lại
    // [EN] Change Z to 10 (positive) so the camera stands in front and looks back
    [SerializeField] private Vector3 positionOffset = new Vector3(0f, 15f, 10f);
    [SerializeField] private float smoothTime = 0.2f;

    [Header("Rotation Settings")]
    [SerializeField] private bool lookAtTarget = true; // [VI] Tự động bám góc / [EN] Auto track angle
    [SerializeField] private Vector3 rotationOffset = new Vector3(50f, 180f, 0f); // [VI] Dùng khi tắt lookAtTarget / [EN] Used when lookAtTarget is false

    private Vector3 currentVelocity;

    private void Start()
    {
        if (target != null && !lookAtTarget)
        {
            // [VI] Gán góc quay cố định ngay từ đầu nếu không dùng LookAt
            // [EN] Assign fixed rotation right from the start if not using LookAt
            transform.rotation = Quaternion.Euler(rotationOffset);
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // [VI] 1. Cập nhật vị trí mượt mà
        // [EN] 1. Smoothly update position
        Vector3 targetPosition = target.position + positionOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);

        // [VI] 2. Xử lý xoay Camera
        // [EN] 2. Handle Camera rotation
        if (lookAtTarget)
        {
            // [VI] Liên tục ngoái nhìn theo nhân vật
            // [EN] Continuously look at the character
            transform.LookAt(target.position);
        }
        else
        {
            // [VI] Giữ một góc nhìn cố định (Isometric style)
            // [EN] Maintain a fixed viewing angle (Isometric style)
            transform.rotation = Quaternion.Euler(rotationOffset);
        }
    }
}