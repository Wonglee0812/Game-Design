using Unity.Mathematics;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
public enum RamTier { Tier_0, Tier_1, Tier_2, Tier_3 };
[RequireComponent(typeof(Rigidbody))]

public class CarPhysics : MonoBehaviour
{

    [Header("References")]
    [SerializeField] public Transform[] tireTransform;
    [SerializeField] public Transform[] flipTransform;
    [SerializeField] private Transform[] forcePoint;
    
    private Rigidbody rb;

    [Header("Suspension Setting")]
    [SerializeField] public float restDistance;
    [SerializeField] private float springStrength;
    [SerializeField] private float springDamping;

    [Header("Steering Setting")]
    [SerializeField] private AnimationCurve frontGripFactor;
    [SerializeField] private AnimationCurve rearGripFactor;
    [SerializeField] private float frontFrictionMutiply = 1;
    [SerializeField] private float rearFrictionMutiply = 1;
    [HideInInspector] public bool isDrift;
    [SerializeField] private float massCenter = 0.05f;


    [Header("Accelaration Setting")]
    [SerializeField] private float maxSteerAngle = 50f;
    [Tooltip("m/s")]
    [SerializeField] private float carMaxSpeed;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private AnimationCurve powerCurve;
    [SerializeField] private AnimationCurve frictionCurve;
    [HideInInspector] public float carSpeed;


    [Header("Braking Setting")]
    [SerializeField] private AnimationCurve brakeCurve;


    [Header("Ram Setting")]
    [SerializeField] private bool isRamEnabled = true; 
    [SerializeField] private float minSpeedToRam = 5f; 
    [SerializeField] private float ramTierStep = 3f;
    [SerializeField] private float speedBoostPerTier = 2f;  
    [SerializeField] private float massShiftPerTier = -0.04f;
    private float count;
    public RamTier CurrentTier;
    private float baseMaxSpeed;
    private float baseMassCenter;


    [Header("Input Data")]
    private Vector2 moveDir;   
    private float brakeStre;
    private bool driftCall;

    void Start()
    {
       rb ??= GetComponent<Rigidbody>();
       rb.centerOfMass = new Vector3(0, massCenter, 0);
        baseMaxSpeed = carMaxSpeed;
        baseMassCenter = massCenter;
        CurrentTier = RamTier.Tier_0;
    }
    private void Update()
    {
        ApplyTurn();
        Debug.Log(CurrentTier);
    }
    private void FixedUpdate()
    {
        ApplyDriftForce();
        ApplyForce();
        ApplyFlipForce();
        ApplyRamSystem();
    }
    private void ApplyForce()
    {
        //Applying force for front tires
        for (int i = 0; i < tireTransform.Length / 2; i++)
        {
            RaycastHit hit;
            bool rayDidHit = Physics.Raycast(tireTransform[i].position,-tireTransform[i].up, out hit, restDistance, groundLayer);
            if (rayDidHit)
            {
                ApplySuspensionForce(tireTransform[i], hit);
                ApplySteeringForce(tireTransform[i], hit, i);
                ApplyAccelarationForce(tireTransform[i], hit);
                ApplyFrictionForce(tireTransform[i], hit);
                ApplyBrakingForce(tireTransform[i], hit);
            }
        }
        //Applying force for rear tires
        for (int i = tireTransform.Length - 1; i >= tireTransform.Length / 2; i--)
        {
            RaycastHit hit;
            bool rayDidHit = Physics.Raycast(tireTransform[i].position,-tireTransform[i].up, out hit, restDistance, groundLayer);
            if (rayDidHit)
            {
                ApplySuspensionForce(tireTransform[i], hit);
                ApplySteeringForce(tireTransform[i], hit, i);
                ApplyAccelarationForce(tireTransform[i], hit);
                ApplyFrictionForce(tireTransform[i], hit);
                ApplyBrakingForce(tireTransform[i], hit);


            }
        }
    }    
    private void ApplyTurn()
    {
        float v = moveDir.x;
        float angel = v * maxSteerAngle;
        for (int i = 0; i < tireTransform.Length / 2; i++)
        {
            tireTransform[i].localRotation = Quaternion.Euler(0, angel, 0);
        }
    }    
    private void ApplySuspensionForce(Transform tire, RaycastHit hit)
    {
        Vector3 springDir = tire.up;

        float offset = restDistance - hit.distance;

        float Vel = Vector3.Dot(springDir, rb.GetPointVelocity(tire.position));

        float suspensionForce = (offset * springStrength) - (Vel * springDamping);

        rb.AddForceAtPosition(springDir * suspensionForce, tire.position);
    }
    private void ApplySteeringForce(Transform tire, RaycastHit hit, int i)
    {
        //Xác định bánh trước hay sau
        bool isFront = i < 2;
        //Lấy hướng của lực
        Vector3 steeringDir = tire.right;

        //Lấy velocity bánh xe
        Vector3 tireWorldVelo = rb.GetPointVelocity(tire.position);

        //Tính tích vô hướng
        float steeringVel = Vector3.Dot(steeringDir, tireWorldVelo); // Xe đang trượt ngang bao nhiêu, âm hay dương

        //Sự thay đổi của velocity mà ta mong muốn
        //tireGrip chạy từ 0-1 là độ lớn, 0 là không grip, 1 là full grip
        float desireGripFactor = isFront ? ((frontGripFactor.Evaluate(Mathf.Abs(steeringVel))) * frontFrictionMutiply)
            : ((rearGripFactor.Evaluate(Mathf.Abs(steeringVel))) * rearFrictionMutiply);
        float desireVelChange = -steeringVel * desireGripFactor;

        //Gia tốc mong muốn
        float desireAccel = desireVelChange / Time.fixedDeltaTime;

        float tireMass = rb.mass / 4;

        rb.AddForceAtPosition((steeringDir * (tireMass * desireAccel)), tire.position);
 
    }    
    private void ApplyAccelarationForce(Transform tire, RaycastHit hit)
    {
        //Lấy input
        float h = moveDir.y;
        Vector3 accelDir = tire.forward;
        carSpeed = Vector3.Dot(accelDir, rb.linearVelocity);
        if (h == 0.0f) return;
        float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / carMaxSpeed);
        float availableTourqe = (carSpeed >= carMaxSpeed) ? 0 : powerCurve.Evaluate(normalizedSpeed) * h * 5000;
        //Debug.Log("Speed" + "" + carSpeed + "" + "m/s");

        rb.AddForceAtPosition(accelDir * availableTourqe, tire.position);
    }    
    private void ApplyFrictionForce(Transform tire, RaycastHit hit)
    {
        float h = moveDir.y;
        if (h != 0.0f) return;

        Vector3 frictionDir = tire.forward;

        Vector3 tireWorldVel = rb.GetPointVelocity(tire.position);

        float frictionVel = Vector3.Dot(frictionDir, tireWorldVel);

        float normalizedFrictionSpeed = Mathf.Clamp01(Mathf.Abs(frictionVel) / carMaxSpeed);

        float desireFrictionChange = -frictionVel * frictionCurve.Evaluate(normalizedFrictionSpeed);

        float desireFriction = desireFrictionChange / Time.fixedDeltaTime;
        //Debug.Log("Friction" + "" + desireFriction);
        float tireMass = rb.mass / 4;

        rb.AddForceAtPosition((frictionDir * (tireMass * desireFriction)) * frontFrictionMutiply, tire.position);
    }
    private void ApplyBrakingForce(Transform tire, RaycastHit hit)
    {
        float braking = brakeStre;
        if (braking == 0.0f) return;

        Vector3 frictionDir = tire.forward;

        Vector3 tireWorldVel = rb.GetPointVelocity(tire.position);

        float brakeVel = Vector3.Dot(frictionDir, tireWorldVel);

        float normalizedBrakeSpeedSpeed = Mathf.Clamp01(Mathf.Abs(brakeVel) / carMaxSpeed);

        float desireFrictionChange = -brakeVel * brakeCurve.Evaluate(normalizedBrakeSpeedSpeed);

        float desireFriction = desireFrictionChange / Time.fixedDeltaTime;

        float tireMass = rb.mass / 4;

        rb.AddForceAtPosition((frictionDir * (tireMass * desireFriction)), tire.position);
    }
    private void ApplyDriftForce()
    {
        if (driftCall)
        {
            frontFrictionMutiply = 1.4f;
            rearFrictionMutiply = 0.3f;
            isDrift = true;
        }
        else
        {
            frontFrictionMutiply = 1;
            rearFrictionMutiply = 1;
            isDrift = false;

        }

    }
    private void ApplyFlipForce()
    {
        for (int i = 1; i < flipTransform.Length; i++)
        {
            RaycastHit hit;
            bool rayDidHit = Physics.Raycast(flipTransform[i].position, flipTransform[i].forward, out hit, 1f, groundLayer);
            if(rayDidHit)
            {
                Debug.Log("Nằm");
                float offSet = 1 - hit.distance;

                float vel = Vector3.Dot(flipTransform[i].forward, rb.GetPointVelocity(flipTransform[i].position));

                float flipForce = (offSet * 20000f) - (vel * 800f);

                rb.AddForceAtPosition(-flipTransform[i].forward * flipForce, flipTransform[i].position);
            }
            bool rayDidUpHit = Physics.Raycast(flipTransform[0].position, flipTransform[0].up, out hit, 1f, groundLayer);
            if (rayDidUpHit)
            {
                Debug.Log("Nằm Ngửa");
                float offSet = 1 - hit.distance;
                float offSetUp = 1 - hit.distance;

                float vel = Vector3.Dot(flipTransform[i].forward, rb.GetPointVelocity(flipTransform[i].position));
                float velUp = Vector3.Dot(flipTransform[0].up, rb.GetPointVelocity(flipTransform[0].position));

                float flipForce = (offSet * 20000f) - (vel * 500f);

                float upFlipForce = (offSetUp * 5000f) - (velUp * 100f);

                rb.AddForceAtPosition(-flipTransform[0].up * upFlipForce, flipTransform[0].position);
                transform.Rotate(0,0,Mathf.Lerp(transform.rotation.z, 0, Time.deltaTime));
            }
        }

    }
    private void ApplyRamSystem()
    {
        if (!isRamEnabled)
        {
            count = 0;
            CurrentTier = RamTier.Tier_0;
            UpdatRamMutiply();
            return;
        }

        if (carSpeed >= minSpeedToRam) count += Time.deltaTime;
        else count = 0;

        float t1 = ramTierStep / 2f;       
        float t2 = t1 + ramTierStep;        
        float t3 = t2 + ramTierStep;


        if (count >= t3) CurrentTier = RamTier.Tier_3;
        else if (count >= t2) CurrentTier = RamTier.Tier_2;
        else if (count >= t1) CurrentTier = RamTier.Tier_1;
        else CurrentTier = RamTier.Tier_0;
        UpdatRamMutiply();
    }    
    private void UpdatRamMutiply()
    {
        int tierLevel = (int)CurrentTier;

        carMaxSpeed = baseMaxSpeed + (speedBoostPerTier * tierLevel);
        massCenter = baseMassCenter + (massShiftPerTier * tierLevel);

        rb.centerOfMass = new Vector3(0, massCenter, 0);
    }




    private void OnCollisionEnter(Collision col)
    {
        Vector3 hitPosi = col.GetContact(0).point;
        Vector3 localPosi = transform.InverseTransformPoint(hitPosi);
        Rigidbody rb = col.transform.GetComponent<Rigidbody>();

        if(isRamEnabled && localPosi.z > 1f)
        {
            if(rb)
            {
                RamEnemy(rb, col);
                if(CurrentTier >= RamTier.Tier_1) TriggerEnemyDamageVisual(rb);
                if(CurrentTier >= RamTier.Tier_3)
                {
                    VFXManager.Instance.PlayCollideEffect(hitPosi, 0.5f);

                }
                Debug.Log("Enemy");
            }
            else
            {
                RamConstruction(1f);
                if(CurrentTier >= RamTier.Tier_1) TriggerSelfDamageVisual();
                Debug.Log("Construct");

            }
        }    
    }
    private void RamConstruction(float mutiply)
    {
        float aganistForce = (rb.mass * carSpeed * 10f) * mutiply * ((int)CurrentTier + 0.2f);
        rb.AddForceAtPosition(transform.up * aganistForce * 0.6f, forcePoint[1].position);
        rb.AddForceAtPosition(-transform.forward * aganistForce * 1.2f, forcePoint[0].position);
    }    

    private void RamEnemy(Rigidbody rg, Collision col)
    {
        // [VI] 1. Tỷ lệ lợi thế khối lượng (Attacker / Defender)
        // Dùng Clamp để xe nhỏ nhất không bị vô dụng (0.4x) và xe to nhất không quá lố (2.5x)
        // [EN] 1. Mass advantage ratio (Attacker / Defender)
        // Clamp so the smallest car isn't useless (0.4x) and the biggest isn't broken (2.5x)
        float massAdvantage = Mathf.Clamp(rb.mass / rg.mass, 0.4f, 2.5f);

        // [VI] 2. Hệ số Tier: Ép khoảng cách từ 5m (Tier 1) lên 20m (Tier 3)
        // [EN] 2. Tier Multiplier: Force the distance from 5m (Tier 1) to 20m (Tier 3)
        float tierMultiplier = 1f;
        if (CurrentTier == RamTier.Tier_2) tierMultiplier = 2.5f;
        else if (CurrentTier == RamTier.Tier_3) tierMultiplier = 4.0f;

        // [VI] 3. Tính lực tổng: Động lượng (Mass * Speed) * Tier * Tỷ lệ khối lượng
        // [EN] 3. Total Force: Momentum (Mass * Speed) * Tier * Mass Ratio
        float baseMomentum = rb.mass * carSpeed * 0.5f; // Hằng số 0.5f để scale cho Impulse
        float ramForce = baseMomentum * tierMultiplier * massAdvantage;

        // [VI] 4. Hướng hất: Thêm một chút lực đẩy lên trên (y = 0.2f) để xe bay đẹp hơn
        // [EN] 4. Knockback direction: Add slight upward force (y = 0.2f) for better airtime
        Vector3 ramDir = (col.transform.position - transform.position).normalized;
        ramDir.y = 0.2f;

        // [VI] Sử dụng ForceMode.Impulse cho tác động tức thời (chuẩn vật lý va chạm)
        // [EN] Use ForceMode.Impulse for instant impact (accurate collision physics)
        rg.AddForce(ramDir * ramForce, ForceMode.Impulse);
        rg.AddTorque(UnityEngine.Random.insideUnitSphere * ramForce * 0.1f, ForceMode.Impulse);

        // [VI] 5. Phản lực cho xe tấn công (Recoil)
        // [EN] 5. Recoil for the attacker
        float recoilPenalty = Mathf.Clamp(rg.mass / rb.mass, 0.5f, 2f);
        RamConstruction(recoilPenalty);
    }
    private void TriggerEnemyDamageVisual(Rigidbody enemyRb)
    {
        CarVisual enemyVisual = enemyRb.GetComponentInChildren<CarVisual>();

        if (enemyVisual != null)
            enemyVisual.TakeVisualDamage();
    }
    private void TriggerSelfDamageVisual()
    {
        CarVisual myVisual = GetComponent<CarVisual>();
        if (myVisual != null) myVisual.TakeVisualDamage();
    }




    /// <summary>
    /// Read moving input
    /// </summary>
    public void MoveInput(Vector2 moveValue)
    {
        moveDir = moveValue;
    }    
    public void BrakeInput(float brakeValue)
    {
        brakeStre = brakeValue;
    }    
    public void DriftInput(bool driftValue)
    {
        driftCall = driftValue;
    }    




    //Debug
    private void OnDrawGizmos()
    {
        foreach (Transform tire in tireTransform)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(tire.position, tire.up * 1);

            Gizmos.color = Color.red;
            Gizmos.DrawRay(tire.position, tire.right * 1);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(tire.position, tire.forward * 1);
        }
    }
}
