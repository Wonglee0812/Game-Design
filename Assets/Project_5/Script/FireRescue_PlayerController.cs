using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class FireRescue_PlayerController : MonoBehaviour
{
    private Rigidbody rb;

    // [VI] Biến lưu trữ Transform của Camera / [EN] Variable to store Camera's Transform
    private Transform mainCameraTransform;

    [Header("Suspension (Float) Settings")]
    [SerializeField] private float rideHeight = 1.0f;
    [SerializeField] private float raycastLength = 2.0f;
    [SerializeField] private float springStrength = 250f;
    [SerializeField] private float springDamper = 15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 6f;
    [SerializeField] private float accelaration = 50f;
    [SerializeField] private float maxAccelaration = 100f;
    [SerializeField] private float turnSpeed = 15f;

    [Header("Health & UI Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    public Slider healthSlider;

    // State Variables
    private Vector3 m_goalVel;
    private Vector2 currentMoveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        // [VI] Tìm và lưu trữ Camera chính trong Scene / [EN] Find and store the main Camera in the Scene
        if (UnityEngine.Camera.main != null)
        {
            mainCameraTransform = UnityEngine.Camera.main.transform;
        }
        else
        {
            Debug.LogWarning("FireRescue_PlayerController: No Main Camera found in the scene!");
        }
    }

    private void FixedUpdate()
    {
        HandleSuspension();
        HandleMovement();
    }

    private void HandleSuspension()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, raycastLength, groundLayer))
        {
            float offset = rideHeight - hit.distance;
            float velocityY = rb.linearVelocity.y;
            float springForce = (offset * springStrength) - (velocityY * springDamper);
            rb.AddForce(Vector3.up * springForce);
        }
    }

    private void HandleMovement()
    {
        Vector3 moveDir = Vector3.zero;

        // [VI] 1. TÍNH TOÁN HƯỚNG DI CHUYỂN THEO CAMERA
        // [EN] 1. CALCULATE MOVEMENT DIRECTION RELATIVE TO CAMERA
        if (mainCameraTransform != null)
        {
            // [VI] Lấy hướng trước và phải của camera, ép trục Y về 0 để chiếu lên mặt đất
            // [EN] Get camera forward and right, force Y to 0 to project onto the ground
            Vector3 camForward = mainCameraTransform.forward;
            camForward.y = 0f;
            camForward.Normalize();

            Vector3 camRight = mainCameraTransform.right;
            camRight.y = 0f;
            camRight.Normalize();

            // [VI] Nhân input với các vector hướng của camera
            // [EN] Multiply input by the camera's direction vectors
            moveDir = (camForward * currentMoveInput.y + camRight * currentMoveInput.x).normalized;
        }
        else
        {
            // [VI] Dự phòng nếu không có camera / [EN] Fallback if no camera exists
            moveDir = new Vector3(currentMoveInput.x, 0, currentMoveInput.y).normalized;
        }

        // [VI] 2. ÁP DỤNG LỰC DI CHUYỂN (Giữ nguyên logic cũ của bạn)
        // [EN] 2. APPLY MOVEMENT FORCE (Kept your old logic)
        Vector3 goalVel = moveDir * maxSpeed;

        m_goalVel = Vector3.MoveTowards(m_goalVel, goalVel, accelaration * Time.fixedDeltaTime);
        Vector3 needAccel = (m_goalVel - rb.linearVelocity) / Time.fixedDeltaTime;
        needAccel = Vector3.ClampMagnitude(needAccel, maxAccelaration);

        rb.AddForce(Vector3.Scale(needAccel * rb.mass, new Vector3(1, 0, 1)));

        if (moveDir.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
        }
    }

    public void SetMoveInput(Vector2 move)
    {
        currentMoveInput = move;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount * Time.fixedDeltaTime;

        if (healthSlider != null) healthSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("Firefighter down!");
        }
    }
}