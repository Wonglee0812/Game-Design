using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class FrogTongue : MonoBehaviour
{
    // [VI] 5 Trạng thái của lưỡi / [EN] 5 States of the tongue
    public enum TongueState { Idle, Extending, Attached, ZippingPlayer, PullingObject, Retracting }
    public TongueState currentState = TongueState.Idle;

    [Header("Tongue Settings")]
    [SerializeField] private Transform mouthPoint;
    [SerializeField] private float maxDistance = 15f;
    [SerializeField] private float shootSpeed = 50f;
    [SerializeField] private float zipSpeed = 25f;
    [SerializeField] private float pullSpeed = 15f;
    [SerializeField] private float tongueRadius = 0.5f;
    [SerializeField] private LayerMask hitLayer;

    [Header("Stomach Settings (Nuốt & Nhổ)")]
    [SerializeField] private float spitForce = 30f; // [VI] Lực phun đồ vật / [EN] Force to spit object
    private GameObject swallowedObject = null;      // [VI] Dạ dày lưu trữ / [EN] Stomach storage

    [Header("Joint Settings")]
    [SerializeField] private float springForce = 100f;
    [SerializeField] private float springDamper = 10f;

    [Header("Safety Settings")]
    [SerializeField] private float maxZipTime = 2.0f;
    [SerializeField] private float pullStopDistance = 1.5f;

    private float zipTimer = 0f;
    private LineRenderer line;
    private Rigidbody rb;
    private FrogController controller;
    private SpringJoint currentJoint;

    private Vector3 targetPoint;
    private Vector3 currentTipPosition;
    private GameObject hitObject;
    private Rigidbody hitRb;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<FrogController>();

        line.positionCount = 2;
        line.enabled = false;
    }

    // =========================================================
    // [VI] GOM CHUNG LOGIC VÀO 1 NÚT BẤM (NHƯ BẠN YÊU CẦU)
    // [EN] COMBINED LOGIC INTO 1 BUTTON PRESS (AS REQUESTED)
    // =========================================================
    public void HandleTongueInput()
    {
        // [VI] Nếu đang ngậm đồ và lưỡi đang rảnh rỗi -> Phun ra
        // [EN] If holding an object and tongue is idle -> Spit it out
        if (swallowedObject != null && currentState == TongueState.Idle)
        {
            SpitObject();
            return; // [VI] Ngắt luôn, không chạy các lệnh bên dưới / [EN] Stop here, don't run below commands
        }

        switch (currentState)
        {
            case TongueState.Idle:
                ShootTongue();
                break;
            case TongueState.Attached:
                StartRetraction();
                break;
            case TongueState.ZippingPlayer:
            case TongueState.PullingObject:
                StopEverything();
                break;
        }
    }

    private void SpitObject()
    {
        swallowedObject.SetActive(true);
        swallowedObject.transform.position = mouthPoint.position + transform.forward * 1.5f;

        Rigidbody spitRb = swallowedObject.GetComponent<Rigidbody>();
        if (spitRb != null)
        {
            spitRb.linearVelocity = Vector3.zero;
            spitRb.AddForce(transform.forward * spitForce * spitRb.mass, ForceMode.Impulse);
        }

        swallowedObject = null; // [VI] Làm rỗng bụng / [EN] Empty stomach
    }

    private void ShootTongue()
    {
        Ray ray = new Ray(mouthPoint.position, transform.forward);
        if (Physics.SphereCast(ray, tongueRadius, out RaycastHit hit, maxDistance, hitLayer))
        {
            targetPoint = hit.point;
            hitObject = hit.collider.gameObject;
            hitRb = hit.collider.GetComponent<Rigidbody>();
        }
        else
        {
            targetPoint = mouthPoint.position + transform.forward * maxDistance;
            hitObject = null;
        }

        currentTipPosition = mouthPoint.position;
        line.enabled = true;
        currentState = TongueState.Extending;
    }

    private void StartRetraction()
    {
        if (currentJoint != null) Destroy(currentJoint);

        if (hitObject != null && hitObject.CompareTag("Grappleable"))
        {
            if (hitRb == null)
            {
                currentState = TongueState.ZippingPlayer;
                controller.isGrappling = true;
                rb.useGravity = false;
                zipTimer = 0f;
            }
            else
            {
                currentState = TongueState.PullingObject;
                hitRb.useGravity = false;
            }
        }
        else
        {
            currentState = TongueState.Retracting;
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case TongueState.Extending:
                currentTipPosition = Vector3.MoveTowards(currentTipPosition, targetPoint, shootSpeed * Time.deltaTime);
                if (Vector3.Distance(currentTipPosition, targetPoint) < 0.1f) OnHitTarget();
                break;

            case TongueState.Retracting:
                currentTipPosition = Vector3.MoveTowards(currentTipPosition, mouthPoint.position, shootSpeed * 1.5f * Time.deltaTime);
                if (Vector3.Distance(currentTipPosition, mouthPoint.position) < 0.1f) ResetToIdle();
                break;

            case TongueState.Attached:
            case TongueState.ZippingPlayer:
            case TongueState.PullingObject:
                if (hitObject != null)
                {
                    currentTipPosition = (hitRb != null) ? hitRb.position : targetPoint;
                }
                break;
        }
    }

    private void FixedUpdate()
    {
        if (currentState == TongueState.ZippingPlayer)
        {
            zipTimer += Time.fixedDeltaTime;

            Vector3 dir = (targetPoint - transform.position).normalized;
            rb.linearVelocity = dir * zipSpeed;

            if (Vector3.Distance(transform.position, targetPoint) < 1.5f || zipTimer >= maxZipTime)
            {
                StopEverything();
            }
        }
        else if (currentState == TongueState.PullingObject)
        {
            if (hitRb == null) { StopEverything(); return; }

            Vector3 dir = (mouthPoint.position - hitRb.position).normalized;
            hitRb.linearVelocity = dir * pullSpeed;

            Collider[] hitColliders = Physics.OverlapSphere(mouthPoint.position, pullStopDistance, hitLayer);
            foreach (Collider col in hitColliders)
            {
                if (col.gameObject == hitObject)
                {
                    swallowedObject = hitObject;

                    hitRb.linearVelocity = Vector3.zero;
                    hitObject.SetActive(false);
                    StopEverything();
                    break;
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (line.enabled)
        {
            line.SetPosition(0, mouthPoint.position);
            line.SetPosition(1, currentTipPosition);
        }
    }

    private void OnHitTarget()
    {
        if (hitObject == null) { currentState = TongueState.Retracting; return; }

        if (hitObject.CompareTag("Grappleable"))
        {
            currentJoint = gameObject.AddComponent<SpringJoint>();
            currentJoint.autoConfigureConnectedAnchor = false;
            currentJoint.connectedAnchor = targetPoint;
            if (hitRb != null) currentJoint.connectedBody = hitRb;

            currentJoint.spring = springForce;
            currentJoint.damper = springDamper;
            currentJoint.maxDistance = Vector3.Distance(transform.position, targetPoint) * 0.9f;

            currentState = TongueState.Attached;
        }
        else if (hitObject.CompareTag("FrogInteractable"))
        {
            IFrogInteractable interact = hitObject.GetComponent<IFrogInteractable>();
            if (interact != null) interact.OnTongueHit();
            currentState = TongueState.Retracting;
        }
        else
        {
            currentState = TongueState.Retracting;
        }
    }

    private void StopEverything()
    {
        if (hitRb != null) hitRb.useGravity = true;
        controller.isGrappling = false;
        rb.useGravity = true;
        currentState = TongueState.Retracting;
    }

    private void ResetToIdle()
    {
        line.enabled = false;
        currentState = TongueState.Idle;
    }
}