using UnityEngine;

public class FrogCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;        // Con ếch (Player)
    [SerializeField] private Vector3 targetOffset;    // Độ lệch tâm (ví dụ: nhìn cao hơn đầu ếch tí)

    [Header("Distance Settings")]
    [SerializeField] private float distance = 5f;     // Khoảng cách từ cam đến ếch

    [Header("Rotation Settings")]
    [SerializeField] private float mouseSensitivity = 0.2f;
    [SerializeField] private float smoothTime = 0.12f;    // Độ trễ mượt mà (Smoothing)
    [SerializeField] private Vector2 pitchLimits = new Vector2(-20f, 80f); // Giới hạn ngước lên/ngó xuống

    [Header("Collision Settings")]
    [SerializeField] private LayerMask collisionLayer; // Chọn Layer của tường và sàn
    [SerializeField] private float collisionRadius = 0.2f; // Độ dày của tia dò
    [SerializeField] private float collisionPadding = 0.1f; // Khoảng cách đệm để cam không sát tường quá
    [SerializeField] private float collisionSmoothTime = 10f;

    private Vector2 lookInput;
    private float yaw;   // Xoay quanh trục Y (trái/phải)
    private float pitch; // Xoay quanh trục X (lên/xuống)

    private Vector3 currentRotation;
    private Vector3 rotationSmoothVelocity;

    private Vector3 currentPosition;
    private Vector3 positionSmoothVelocity;

    private float currentDistance;

    private void Start()
    {
        // Khởi tạo góc quay hiện tại dựa trên Rotation của Camera trong Scene
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        currentDistance = distance;
    }

    // Nhận dữ liệu từ FrogInput gửi sang
    public void SetLookInput(Vector2 input)
    {
        lookInput = input;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleRotation();
        HandlePosition();
    }

    private void HandleRotation()
    {
        // Cộng dồn input vào góc quay
        yaw += lookInput.x * mouseSensitivity;
        pitch -= lookInput.y * mouseSensitivity;

        // Giới hạn góc ngước lên/xuống để không bị lộn ngược camera
        pitch = Mathf.Clamp(pitch, pitchLimits.x, pitchLimits.y);

        // Làm mượt chuyển động xoay (Damping)
        Vector3 targetRot = new Vector3(pitch, yaw, 0);
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRot, ref rotationSmoothVelocity, smoothTime);

        transform.eulerAngles = currentRotation;
    }

    private void HandlePosition()
    {
        // 1. Tính toán vị trí lý tưởng (Vị trí camera muốn đứng nếu không có tường)
        Vector3 targetPos = target.position + targetOffset;
        Vector3 direction = -transform.forward;
        Vector3 desiredPosition = targetPos + direction * distance;

        // 2. Dò tìm va chạm (SphereCast)
        RaycastHit hit;
        float adjustedDistance = distance;

        // Bắn một khối cầu từ nhân vật ra hướng Camera
        if (Physics.SphereCast(targetPos, collisionRadius, direction, out hit, distance, collisionLayer))
        {
            // Nếu chạm tường, khoảng cách mới = khoảng cách từ nhân vật đến điểm chạm - một chút đệm
            adjustedDistance = hit.distance - collisionPadding;
        }

        // 3. Làm mượt khoảng cách (để cam không bị giật cái rụp khi gặp vật cản)
        currentDistance = Mathf.Lerp(currentDistance, adjustedDistance, Time.deltaTime * collisionSmoothTime);

        // 4. Cập nhật vị trí cuối cùng
        transform.position = targetPos + direction * currentDistance;
    }
}