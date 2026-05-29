using UnityEngine;

namespace Portfolio.TriageManagement
{
    public class Triage_CameraController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 20f;
        [SerializeField] private float smoothTime = 0.15f;

        [Header("Boundaries (X, Z)")]
        [SerializeField] private Vector2 minBounds = new Vector2(-15f, -15f);
        [SerializeField] private Vector2 maxBounds = new Vector2(15f, 15f);

        [Header("Zoom Settings (Y)")]
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float minHeight = 10f; // [VI] Zoom cận cảnh tối đa / [EN] Maximum zoom in
        [SerializeField] private float maxHeight = 30f; // [VI] Zoom xa tối đa / [EN] Maximum zoom out

        [Header("Rotation & Game Feel (Tilt)")]
        [SerializeField] private Vector3 defaultRotation = new Vector3(60f, 0f, 0f);
        [SerializeField] private float tiltMultiplier = 0.5f;
        [SerializeField] private float maxTiltAngle = 8f;
        [SerializeField] private float rotationSmoothTime = 0.1f;

        private Vector2 inputDirection;
        private float zoomInput; // [VI] Biến lưu giá trị con lăn / [EN] Variable to store scroll value
        private Vector3 targetPosition;
        private Vector3 currentVelocity;

        private float currentPitchVelocity;
        private float currentRollVelocity;
        private float currentPitch;
        private float currentRoll;

        private void Start()
        {
            targetPosition = transform.position;
            // [VI] Đảm bảo camera bắt đầu ở trong giới hạn độ cao / [EN] Ensure camera starts within height limits
            targetPosition.y = Mathf.Clamp(targetPosition.y, minHeight, maxHeight);

            currentPitch = defaultRotation.x;
            currentRoll = defaultRotation.z;
            transform.rotation = Quaternion.Euler(defaultRotation);
        }

        public void SetMoveInput(Vector2 input)
        {
            inputDirection = input;
        }

        public void SetZoomInput(float zoomVal)
        {
            zoomInput = zoomVal;
        }

        private void LateUpdate()
        {
            // 1. XỬ LÝ DI CHUYỂN & ZOOM (POSITION) / 1. HANDLE MOVEMENT & ZOOM (POSITION)
            Vector3 moveDelta = new Vector3(inputDirection.x, 0f, inputDirection.y) * (moveSpeed * Time.deltaTime);
            targetPosition += moveDelta;

            // [VI] Đọc giá trị con lăn (Normalize về 1 hoặc -1 để tránh tốc độ cuộn khác nhau ở các chuột)
            // [EN] Read scroll value (Normalize to 1 or -1 to avoid different scroll speeds across mice)
            float zoomDelta = 0f;
            if (zoomInput > 0) zoomDelta = -1f; // [VI] Cuộn lên -> Zoom In (giảm Y) / [EN] Scroll up -> Zoom In (decrease Y)
            else if (zoomInput < 0) zoomDelta = 1f; // [VI] Cuộn xuống -> Zoom Out (tăng Y) / [EN] Scroll down -> Zoom Out (increase Y)

            targetPosition.y += zoomDelta * zoomSpeed;

            // [VI] Khóa giới hạn cả 3 trục (X, Y, Z) / [EN] Clamp limits for all 3 axes (X, Y, Z)
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.z = Mathf.Clamp(targetPosition.z, minBounds.y, maxBounds.y);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minHeight, maxHeight);

            // [VI] Lerp tổng hợp cả 3 trục / [EN] Master Lerp for all 3 axes
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);

            // 2. XỬ LÝ QUÁN TÍNH NGHIÊNG (ROTATION TILT) / 2. HANDLE INERTIAL TILT (ROTATION TILT)
            float velocityX = currentVelocity.x;
            float velocityZ = currentVelocity.z;

            float targetPitch = defaultRotation.x - Mathf.Clamp(velocityZ * tiltMultiplier, -maxTiltAngle, maxTiltAngle);
            float targetRoll = defaultRotation.z - Mathf.Clamp(velocityX * tiltMultiplier, -maxTiltAngle, maxTiltAngle);

            currentPitch = Mathf.SmoothDampAngle(currentPitch, targetPitch, ref currentPitchVelocity, rotationSmoothTime);
            currentRoll = Mathf.SmoothDampAngle(currentRoll, targetRoll, ref currentRollVelocity, rotationSmoothTime);

            transform.rotation = Quaternion.Euler(currentPitch, defaultRotation.y, currentRoll);
        }
    }
}