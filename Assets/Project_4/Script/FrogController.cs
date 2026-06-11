using UnityEngine;
using UnityEngine.XR;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody))]
public class FrogController : MonoBehaviour
{
    [Header("References")]
    [HideInInspector]
    public UnityEngine.Camera cam;
    private Rigidbody rb;
    private Animator animator;
    [Space]

    [Header("Suspension (Float) Settings")]
    [SerializeField] private float rideHeight = 1.5f;
    [SerializeField] private float raycastLength = 2.0f;
    [SerializeField] private float springStrength = 250f;
    [SerializeField] private float springDamper = 15f;
    [SerializeField] private LayerMask groundLayer;
    [Space]

    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float accelaration = 60f;
    [SerializeField] private float maxAccelaration = 150f;
    [Space]

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float jumpUpwardMultiplier = 1.5f;
    [SerializeField] private float fallMultiplier = 4f;
    [SerializeField] private float lowJumpMultiplier = 6f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [Space]

    [Header("Kinematic Heading")]
    [SerializeField] private float turnSpeed = 30f;
    [Space]

    [Header("Physics Upright")]
    [SerializeField] private float uprightSpringStrength = 300f;
    [SerializeField] private float uprightSpringDamper = 30f;
    [Space]

    [Header("GameFeel Advanced")]
    public bool useAdvancedGameFeel = true;
    [Space]
    [Header("Landing Squash (Slime Feel)")]
    [SerializeField] private float landSquashX = 1.15f;
    [SerializeField] private float landSquashY = 0.85f;
    [SerializeField] private float landDuration = 0.1f;
    [SerializeField] private float landBounceDuration = 0.3f;
    [Space]
    [Header("Particles (Game Feel)")]
    [SerializeField] private ParticleSystem walkDust;
    [SerializeField] private ParticleSystem flyDust;
    [SerializeField] private ParticleSystem landDust;
    [SerializeField] private float heavyLandVelocityThreshold = -8f;

    private Vector3 m_goalVel;
    private bool isGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private Vector2 currentMoveInput;
    private bool isJumpHeld;
    private float timeSinceJump = 1f;
    Quaternion targetRotation;

    public bool isGrappling = false;
    public bool disableRotationByTongue = false;

    private Vector3 initialScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        animator = GetComponent<Animator>();
        initialScale = transform.localScale;
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }

    private void Update()
    {
        timeSinceJump += Time.deltaTime;
        HandleJumpTimers();
        UpdateAnimationAndParticles();
    }

    private void FixedUpdate()
    {
        if (isGrappling) return;

        HandleSuspension();
        HandleMovement();
        HandleJumpAction();
        HandleCustomGravity();

        // [VI] GỌI HÀM NÀY ĐỂ GIỮ NGUYÊN PHYSICS VÀ XÓA CẢNH BÁO VÀNG CS0414
        // [EN] CALL THIS FUNCTION TO KEEP PHYSICS AND CLEAR YELLOW WARNING CS0414
        HandleUprightForce();
    }

    private void UpdateAnimationAndParticles()
    {
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        bool isWalking = horizontalVelocity.sqrMagnitude > 0.1f && isGrounded && !isGrappling;

        if (animator != null)
        {
            animator.SetBool("isWalk", isWalking);
        }

        if (!useAdvancedGameFeel)
        {
            if (walkDust != null && walkDust.isPlaying) walkDust.Stop();
            if (flyDust != null && flyDust.isPlaying) flyDust.Stop();
            return;
        }

        if (isWalking && walkDust != null && !walkDust.isPlaying)
        {
            walkDust.Play();
        }
        else if (!isWalking && walkDust != null && walkDust.isPlaying)
        {
            walkDust.Stop();
        }
    }

    private void HandleSuspension()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        bool hitGround = Physics.Raycast(ray, out RaycastHit hit, raycastLength, groundLayer);

        if (hitGround)
        {
            if (timeSinceJump > 0.2f)
            {
                if (!isGrounded)
                {
                    OnLand(rb.linearVelocity.y);
                }
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
            if (isGrounded)
            {
                OnLeaveGround(rb.linearVelocity.y);
            }
            isGrounded = false;
        }
    }

    private void OnLand(float fallVelocity)
    {
        if (!useAdvancedGameFeel) return;

        if (fallVelocity <= heavyLandVelocityThreshold)
        {
            transform.DOKill();
            Sequence landSeq = DOTween.Sequence();
            landSeq.Append(transform.DOScale(new Vector3(initialScale.x * landSquashX, initialScale.y * landSquashY, initialScale.z * landSquashX), landDuration));
            landSeq.Append(transform.DOScale(initialScale, landBounceDuration).SetEase(Ease.OutElastic));

            if (landDust != null) landDust.Play();

            // [VI] ÂM THANH ĐÁP ĐẤT MẠNH / [EN] HEAVY LANDING SOUND
            if (AudioManager.Instance != null) AudioManager.Instance.Play("landing");
            if (GamepadRumble.Instance != null) GamepadRumble.Instance.Rumble(0.8f, 0.2f, 0.25f);
        }

        if (flyDust != null) flyDust.Stop();
    }

    private void OnLeaveGround(float upwardVelocity)
    {
        if (!useAdvancedGameFeel) return;

        if (walkDust != null) walkDust.Stop();

        if (upwardVelocity > 0.1f && flyDust != null && !flyDust.isPlaying)
        {
            flyDust.Play();
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

        if (moveDir.magnitude > 0.1f && !disableRotationByTongue)
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

            // [VI] ÂM THANH BẬT NHẢY / [EN] JUMP SOUND
            if (AudioManager.Instance != null) AudioManager.Instance.Play("jump");
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

    public void ApplyVault(Vector3 vaultVelocity)
    {
        rb.linearVelocity = vaultVelocity;
        m_goalVel = new Vector3(vaultVelocity.x, 0, vaultVelocity.z);
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

    // [VI] HÀM CHO ANIMATION EVENT / [EN] FUNCTION FOR ANIMATION EVENT
    public void PlayWalkSound()
    {
        if (isGrounded && !isGrappling)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.Play("walking");
        }
    }
}