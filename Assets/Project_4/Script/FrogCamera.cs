using UnityEngine;

public class FrogCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset;

    [Header("Distance Settings")]
    [SerializeField] private float distance = 5f;

    [Header("Rotation Settings")]
    [SerializeField] private float mouseSensitivity = 0.2f;
    [SerializeField] private float gamepadSensitivity = 150f;
    [SerializeField] private float smoothTime = 0.12f;
    [SerializeField] private Vector2 pitchLimits = new Vector2(-20f, 80f);

    [Header("Collision Settings")]
    [SerializeField] private LayerMask collisionLayer;
    [SerializeField] private float collisionRadius = 0.2f;
    [SerializeField] private float collisionPadding = 0.1f;
    [SerializeField] private float collisionSmoothTime = 10f;

    // ==========================================
    // [VI] KHU VỰC GAME FEEL NÂNG CAO (CAMERA)
    // [EN] ADVANCED GAME FEEL SECTION (CAMERA)
    // ==========================================
    [Header("GameFeel Advanced")]
    [Tooltip("Bật/Tắt hiệu ứng FOV giãn ra khi đu dây")]
    public bool useAdvancedGameFeel = true;

    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float grappleFOV = 90f;
    [Tooltip("Tốc độ chuyển đổi FOV (Khuyên dùng: 15 để không bị giật)")]
    [SerializeField] private float fovTransitionSpeed = 15f;

    private UnityEngine.Camera camComponent;
    private FrogController frogController;

    private Vector2 lookInput;
    private bool isUsingGamepad;
    private float yaw;
    private float pitch;

    private Vector3 currentRotation;
    private Vector3 rotationSmoothVelocity;
    private float currentDistance;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        currentDistance = distance;

        camComponent = GetComponent<UnityEngine.Camera>();
        if (target != null)
        {
            frogController = target.GetComponent<FrogController>();
        }

        if (camComponent != null) camComponent.fieldOfView = normalFOV;
    }

    public void SetLookInput(Vector2 input, bool isGamepadDevice = false)
    {
        lookInput = input;
        isUsingGamepad = isGamepadDevice;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleRotation();
        HandlePosition();
        HandleFOV();
    }

    private void HandleFOV()
    {
        if (camComponent == null || frogController == null) return;

        // [VI] NẾU TẮT GAME FEEL -> Ép góc nhìn cứng ngắc về 60
        // [EN] IF GAME FEEL IS OFF -> Force FOV to 60 rigidly
        if (!useAdvancedGameFeel)
        {
            camComponent.fieldOfView = normalFOV;
            return;
        }

        // [VI] NẾU BẬT GAME FEEL -> Xử lý Lerp FOV mượt mà
        float targetFOV = frogController.isGrappling ? grappleFOV : normalFOV;
        camComponent.fieldOfView = Mathf.Lerp(camComponent.fieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
    }

    private void HandleRotation()
    {
        float currentSensitivityX;
        float currentSensitivityY;

        if (isUsingGamepad)
        {
            // [VI] Gamepad cần nhân với Time.deltaTime để xoay mượt theo khung hình
            // [EN] Gamepad needs to multiply by Time.deltaTime for frame-rate independent smooth rotation
            currentSensitivityX = lookInput.x * gamepadSensitivity * Time.deltaTime;
            currentSensitivityY = lookInput.y * gamepadSensitivity * Time.deltaTime;
        }
        else
        {
            // [VI] Chuột giữ nguyên vì đã là số lượng pixel thay đổi
            // [EN] Mouse remains the same as it's already a pixel delta
            currentSensitivityX = lookInput.x * mouseSensitivity;
            currentSensitivityY = lookInput.y * mouseSensitivity;
        }

        yaw += currentSensitivityX;
        pitch -= currentSensitivityY;
        pitch = Mathf.Clamp(pitch, pitchLimits.x, pitchLimits.y);

        Vector3 targetRot = new Vector3(pitch, yaw, 0);
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRot, ref rotationSmoothVelocity, smoothTime);

        transform.eulerAngles = currentRotation;
    }

    private void HandlePosition()
    {
        Vector3 targetPos = target.position + targetOffset;
        Vector3 direction = -transform.forward;
        Vector3 desiredPosition = targetPos + direction * distance;

        RaycastHit hit;
        float adjustedDistance = distance;

        if (Physics.SphereCast(targetPos, collisionRadius, direction, out hit, distance, collisionLayer))
        {
            adjustedDistance = hit.distance - collisionPadding;
        }

        currentDistance = Mathf.Lerp(currentDistance, adjustedDistance, Time.deltaTime * collisionSmoothTime);
        transform.position = targetPos + direction * currentDistance;
    }
}