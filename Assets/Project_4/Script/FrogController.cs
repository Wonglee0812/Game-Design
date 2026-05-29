using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(Rigidbody))]
public class FrogController : MonoBehaviour
{
    [Header("References")]
    public UnityEngine.Camera cam;
    private Rigidbody rb;

    [Header("Suspension (Float) Settings")]
    [SerializeField] private float rideHeight = 1.5f;
    [SerializeField] private float raycastLength = 2.0f;
    [SerializeField] private float springStrength = 250f;
    [SerializeField] private float springDamper = 15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float accelaration = 60f;
    [SerializeField] private float maxAccelaration = 150f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float jumpUpwardMultiplier = 1.5f;
    [SerializeField] private float fallMultiplier = 4f;
    [SerializeField] private float lowJumpMultiplier = 6f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;

    [Header("Kinematic Heading")]
    [SerializeField] private float turnSpeed = 30f;

    [Header("Physics Upright")]
    [SerializeField] private float uprightSpringStrength = 300f;
    [SerializeField] private float uprightSpringDamper = 30f;

    // State Variables
    private Vector3 m_goalVel;
    private bool isGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private Vector2 currentMoveInput;
    private bool isJumpHeld;
    private float timeSinceJump = 1f;
    Quaternion targetRotation;

    public bool isGrappling = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
    }
    private void Update()
    {
        timeSinceJump += Time.deltaTime;
        HandleJumpTimers();
    }
    private void FixedUpdate()
    {
        if (isGrappling) return;

        HandleSuspension();
        HandleMovement();
        HandleJumpAction();
        HandleCustomGravity();
        //HandleUprightForce();
    }
    private void HandleSuspension()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, raycastLength, groundLayer))
        {
            if (timeSinceJump > 0.2f)
            {
                isGrounded = true;
                coyoteTimeCounter = coyoteTime;
            }

            if (timeSinceJump <= 0.2f) return;
            float offset = rideHeight - hit.distance;

            float velocityY = rb.linearVelocity.y;

            float springForce = (offset * springStrength) - (velocityY * springDamper);

            rb.AddForce(Vector3.up * springForce);

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForceAtPosition(Vector3.down * springForce, hit.point);
            }
        }
        else
        {
            isGrounded = false;
        }
    }
    private void HandleMovement()
    {
        Vector2 moveIn = currentMoveInput;
        Vector3 moveDiretion = new Vector3(moveIn.x, 0, moveIn.y);

        if (moveDiretion.magnitude > 1.0f)
            moveDiretion.Normalize();

        Quaternion camRotY = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0);
        Vector3 moveDir = camRotY * moveDiretion;
        Vector3 goalVel = moveDir * maxSpeed;

        m_goalVel = Vector3.MoveTowards(m_goalVel, goalVel, accelaration * Time.fixedDeltaTime);
        Vector3 needAccel = (m_goalVel - rb.linearVelocity) / Time.fixedDeltaTime;
        needAccel = Vector3.ClampMagnitude(needAccel, maxAccelaration);

        rb.AddForce(Vector3.Scale(needAccel * rb.mass, new Vector3(1, 0, 1)));

        if (moveDir.magnitude > 0.1f)
        {
            targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

    }

    private void HandleJumpTimers()
    {
        if (!isGrounded) coyoteTimeCounter -= Time.deltaTime;
        jumpBufferCounter -= Time.deltaTime;
    }
    private void HandleJumpAction()
    {
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;

            timeSinceJump = 0f;
        }
    }
    private void HandleCustomGravity()
    {
        if (isGrounded) return;

        if (rb.linearVelocity.y < 0)
        {
            rb.AddForce(Vector3.down * (fallMultiplier - 1) * Physics.gravity.magnitude * rb.mass);
        }
        else if (rb.linearVelocity.y > 0)
        {
            if (!isJumpHeld)
            {
                rb.AddForce(Vector3.down * (lowJumpMultiplier - 1) * Physics.gravity.magnitude * rb.mass);
            }
            else
            {
                rb.AddForce(Vector3.down * (jumpUpwardMultiplier - 1) * Physics.gravity.magnitude * rb.mass);
            }
        }
    }
    public void SetMoveInput(Vector2 move)
    {
        currentMoveInput = move;
    }
    public void TriggerJumpDown()
    {
        jumpBufferCounter = jumpBufferTime;
    }
    public void SetJumpHeld(bool held)
    {
        isJumpHeld = held;
    }

    private void HandleUprightForce()
    {
        Quaternion characterCurrent = transform.rotation;
        Quaternion toGoal = ShortestDistance(targetRotation, characterCurrent);
        toGoal.ToAngleAxis(out float rotDegrees, out Vector3 rotAxis);
        rotAxis.Normalize();

        float rotRadians = rotDegrees * Mathf.Deg2Rad;

        Vector3 torque = (rotAxis * (rotRadians * uprightSpringStrength)) - (rb.angularVelocity * uprightSpringDamper);

        rb.AddTorque(torque);
    }
    private Quaternion ShortestDistance(Quaternion target, Quaternion current)
    {
        float y = (Mathf.Atan2(target.y, current.y) * Mathf.Deg2Rad - 90f);
        Quaternion goal = target * Quaternion.Inverse(current);

        return Quaternion.Euler(goal.eulerAngles.x, y, goal.eulerAngles.z);
    }
}