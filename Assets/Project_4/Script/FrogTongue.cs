using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(LineRenderer))]
public class FrogTongue : MonoBehaviour
{
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
    [SerializeField] private float spitForce = 30f;
    private GameObject swallowedObject = null;

    [Header("Joint Settings")]
    [SerializeField] private float springForce = 100f;
    [SerializeField] private float springDamper = 10f;

    [Header("Safety Settings")]
    [SerializeField] private float maxZipTime = 2.0f;
    [SerializeField] private float pullStopDistance = 1.5f;

    [Header("Vault Settings (Hất tung khi tới đích)")]
    public float vaultUpwardVelocity = 15f;
    [SerializeField] private float vaultForwardVelocity = 12f;

    [Header("GameFeel Advanced")]
    public bool useAdvancedGameFeel = true;

    [Header("Swallow Squash & Stretch Settings")]
    [SerializeField] private float swallowSquashX = 1.4f;
    [SerializeField] private float swallowSquashY = 0.6f;
    [SerializeField] private float swallowStretchFactor = 1.3f;
    [SerializeField] private float swallowDuration = 0.4f;

    [Header("Spit Squeeze Settings")]
    [SerializeField] private float spitSqueezeX = 0.6f;
    [SerializeField] private float spitSqueezeY = 1.4f;
    [SerializeField] private float spitDuration = 0.4f;

    [Header("Advanced Rotation Settings")]
    [SerializeField] private float advancedRotationSpeed = 15f;

    [Space]
    [Header("Advanced Pitch & Fall Settings")]
    [SerializeField] private float maxPitchAngle = 20f;
    [SerializeField] private float fallingLookDownSpeed = 3f;
    [SerializeField] private float fallingVelocityThreshold = -8f;

    private float initialZipDistance = 0f;
    private float zipTimer = 0f;
    private LineRenderer line;
    private Rigidbody rb;
    private FrogController controller;
    private SpringJoint currentJoint;

    private Vector3 targetPoint;
    private Vector3 currentTipPosition;
    private GameObject hitObject;
    private Rigidbody hitRb;
    private Vector3 initialScale;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<FrogController>();

        line.positionCount = 2;
        line.enabled = false;

        initialScale = transform.localScale;
    }

    public void HandleTongueInput()
    {
        if (swallowedObject != null && currentState == TongueState.Idle)
        {
            SpitObject();
            return;
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

        GummyOrb gummy = swallowedObject.GetComponent<GummyOrb>();
        if (gummy != null) gummy.OnSpatOut();

        swallowedObject = null;

        // [VI] ÂM THANH NHỔ ĐỒ VẬT / [EN] SPITTING SOUND
        if (AudioManager.Instance != null) AudioManager.Instance.Play("spiting");
        if (GamepadRumble.Instance != null) GamepadRumble.Instance.Rumble(0.4f, 0.9f, 0.2f);

        if (useAdvancedGameFeel)
        {
            transform.DOKill();
            Sequence spitSeq = DOTween.Sequence();
            spitSeq.Append(transform.DOScale(new Vector3(initialScale.x * spitSqueezeX, initialScale.y * spitSqueezeY, initialScale.z * spitSqueezeX), 0.1f));
            spitSeq.Append(transform.DOScale(initialScale, spitDuration).SetEase(Ease.OutElastic));
        }
        else
        {
            transform.localScale = initialScale;
        }
    }

    private void ShootTongue()
    {
        // [VI] ÂM THANH BẮN LƯỠI / [EN] SHOOT TONGUE SOUND
        if (AudioManager.Instance != null) AudioManager.Instance.Play("shoot");

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
            rb.linearVelocity = dir * zipSpeed + Vector3.up * (zipSpeed * 0.2f);

            if (Vector3.Distance(transform.position, targetPoint) < 1.5f || zipTimer >= maxZipTime)
            {
                Vector3 finalDir = dir;
                finalDir.y = 0;
                finalDir.Normalize();

                float distanceMultiplier = Mathf.Clamp(initialZipDistance / maxDistance, 0.5f, 1.2f);
                StopEverything();

                Vector3 vaultVel = (finalDir * vaultForwardVelocity + Vector3.up * vaultUpwardVelocity) * distanceMultiplier;
                controller.ApplyVault(vaultVel);
            }
        }
        else if (currentState == TongueState.PullingObject)
        {
            if (hitRb == null) { StopEverything(); return; }

            Vector3 dir = (mouthPoint.position - hitRb.position).normalized;
            hitRb.linearVelocity = dir * pullSpeed;

            float currentDist = Vector3.Distance(mouthPoint.position, hitRb.position);
            if (currentDist > maxDistance)
            {
                hitRb.position = mouthPoint.position - dir * maxDistance;
            }

            float distToMouth = Vector3.Distance(mouthPoint.position, hitRb.position);
            if (distToMouth <= pullStopDistance * 1.5f)
            {
                swallowedObject = hitObject;
                hitRb.linearVelocity = Vector3.zero;
                hitObject.SetActive(false);
                StopEverything();

                // [VI] ÂM THANH NUỐT ĐỒ VẬT / [EN] SWALLOW SOUND
                if (AudioManager.Instance != null) AudioManager.Instance.Play("swallow");
                if (GamepadRumble.Instance != null) GamepadRumble.Instance.Rumble(0.4f, 0.9f, 0.2f);

                if (useAdvancedGameFeel)
                {
                    transform.DOKill();
                    Sequence swallowSeq = DOTween.Sequence();
                    swallowSeq.Append(transform.DOScale(new Vector3(initialScale.x * swallowSquashX, initialScale.y * swallowSquashY, initialScale.z * swallowSquashX), 0.1f));
                    swallowSeq.Append(transform.DOScale(initialScale * swallowStretchFactor, swallowDuration).SetEase(Ease.OutElastic));
                }
                else
                {
                    transform.localScale = initialScale;
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

        HandleAdvancedRotation();
    }

    private void HandleAdvancedRotation()
    {
        if (!useAdvancedGameFeel)
        {
            controller.disableRotationByTongue = false;
            return;
        }

        if (currentState == TongueState.Extending || currentState == TongueState.Attached || currentState == TongueState.ZippingPlayer || currentState == TongueState.PullingObject)
        {
            controller.disableRotationByTongue = true;

            Vector3 lookDirection = currentTipPosition - transform.position;

            if (lookDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDirection);
                Vector3 targetEuler = targetRot.eulerAngles;

                float pitch = targetEuler.x;
                if (pitch > 180f) pitch -= 360f;
                pitch = Mathf.Clamp(pitch, -maxPitchAngle, maxPitchAngle);

                targetEuler.x = pitch;
                targetEuler.z = 0f;

                targetRot = Quaternion.Euler(targetEuler);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, advancedRotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            if (rb != null && rb.linearVelocity.y <= fallingVelocityThreshold)
            {
                controller.disableRotationByTongue = true;

                Vector3 fallDirection = rb.linearVelocity.normalized;
                if (fallDirection.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(fallDirection);

                    Vector3 targetEuler = targetRot.eulerAngles;
                    targetEuler.z = 0f;
                    targetRot = Quaternion.Euler(targetEuler);

                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, fallingLookDownSpeed * Time.deltaTime);
                }
            }
            else
            {
                controller.disableRotationByTongue = false;

                Vector3 currentEuler = transform.eulerAngles;
                Quaternion uprightRotation = Quaternion.Euler(0, currentEuler.y, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, uprightRotation, advancedRotationSpeed * Time.deltaTime);
            }
        }
    }

    private void OnHitTarget()
    {
        if (hitObject == null) { currentState = TongueState.Retracting; return; }

        if (hitObject.CompareTag("Grappleable") || hitObject.CompareTag("FrogInteractable"))
        {
            // [VI] ÂM THANH DÍNH TƯỜNG / [EN] ATTACHED SOUND
            if (AudioManager.Instance != null) AudioManager.Instance.Play("Attached");

            if (hitObject.CompareTag("Grappleable"))
            {
                currentJoint = gameObject.AddComponent<SpringJoint>();
                currentJoint.autoConfigureConnectedAnchor = false;

                if (hitRb != null)
                {
                    currentJoint.connectedBody = hitRb;
                    currentJoint.connectedAnchor = hitRb.transform.InverseTransformPoint(targetPoint);
                }
                else
                {
                    currentJoint.connectedAnchor = targetPoint;
                }

                currentJoint.spring = springForce;
                currentJoint.damper = springDamper;
                currentJoint.maxDistance = Vector3.Distance(transform.position, targetPoint);
            }

            currentState = TongueState.Attached;
        }
        else
        {
            currentState = TongueState.Retracting;
        }
    }

    private void StartRetraction()
    {
        if (currentJoint != null) Destroy(currentJoint);

        if (hitObject != null)
        {
            if (hitObject.CompareTag("Grappleable"))
            {
                currentState = TongueState.ZippingPlayer;
                controller.isGrappling = true;
                rb.useGravity = false;
                zipTimer = 0f;
                initialZipDistance = Vector3.Distance(transform.position, targetPoint);

                // [VI] BẬT LOOP GIÓ BAY / [EN] PLAY FLY LOOP
                if (AudioManager.Instance != null) AudioManager.Instance.Play("fly");
            }
            else if (hitObject.CompareTag("FrogInteractable") && hitRb != null)
            {
                currentState = TongueState.PullingObject;
                hitRb.useGravity = false;
            }
            else
            {
                currentState = TongueState.Retracting;
            }
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

        // [VI] TẮT LOOP GIÓ BAY / [EN] STOP FLY LOOP
        if (AudioManager.Instance != null) AudioManager.Instance.Stop("fly");
    }

    private void ResetToIdle()
    {
        line.enabled = false;
        currentState = TongueState.Idle;

        if (!useAdvancedGameFeel) transform.localScale = initialScale;
    }
}