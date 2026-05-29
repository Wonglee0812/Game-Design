using UnityEngine;
using UnityEngine.UI; // [VI] Thêm thư viện này để dùng Slider / [EN] Add this to use Slider

[RequireComponent(typeof(Rigidbody))]
public class FireRescue_PlayerController : MonoBehaviour
{
    private Rigidbody rb;

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
    public Slider healthSlider; // [VI] Tham chiếu đến UI Thanh Máu / [EN] Reference to Health Slider UI

    // State Variables
    private Vector3 m_goalVel;
    private Vector2 currentMoveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        currentHealth = maxHealth;

        // [VI] Thiết lập giá trị ban đầu cho thanh UI / [EN] Set initial values for UI slider
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
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
        Vector3 moveDir = new Vector3(currentMoveInput.x, 0, currentMoveInput.y).normalized;
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
        // [VI] Trừ máu theo thời gian (FixedDeltaTime) nếu gọi từ OnTriggerStay
        // [EN] Drain health over time (FixedDeltaTime) if called from OnTriggerStay
        currentHealth -= amount * Time.fixedDeltaTime;

        // [VI] Cập nhật UI Thanh Máu / [EN] Update Health Slider UI
        if (healthSlider != null) healthSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("Firefighter down!");
        }
    }
}