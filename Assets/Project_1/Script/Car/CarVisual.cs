using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CarVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform[] wheelMesh;
    [SerializeField] private ParticleSystem[] wheelSmokes;
    [SerializeField] private List<HingeJoint> damageableParts;

    [Header("Setting")]
    [SerializeField] private float wheelTourqeRatio = 0.35f;
    [SerializeField] private float wheelRadius = 0.3f;

    [Header("Evolution & Nitro Effects")]
    [SerializeField] private ParticleSystem nitroParticle; // Kéo Prefab Nitro vào đây / Drag Nitro Prefab here
    [SerializeField] private Color tier1Color = new Color(1f, 1f, 1f); // Trắng/Khói (White/Smoke)
    [SerializeField] private Color tier2Color = new Color(1f, 0f, 0f); // Đỏ (Red)
    [SerializeField] private Color tier3Color = new Color(0.6f, 0f, 1f); // Tím (Purple)

    private RamTier previousTier = RamTier.Tier_0;



    // Bộ nhớ bí mật để lưu setup gốc / Secret memory to store original setups
    private Dictionary<HingeJoint, JointLimits> originalLimits = new Dictionary<HingeJoint, JointLimits>();
    private List<HingeJoint> allOriginalParts = new List<HingeJoint>();
    private Dictionary<HingeJoint, Vector3> originalLocalPositions = new Dictionary<HingeJoint, Vector3>();
    private Dictionary<HingeJoint, Quaternion> originalLocalRotations = new Dictionary<HingeJoint, Quaternion>();



    private Rigidbody rb;
    private CarPhysics physicsScript;
    private float desireRotation;
    private void Awake()
    {
        physicsScript = GetComponent<CarPhysics>();
        rb = GetComponent<Rigidbody>();
        LockDamageableParts();
        if (nitroParticle != null)
            nitroParticle.Stop();

    }

    private void Update()
    {
        UpdateWheelMovement();
        ApplyDust();
        CheckTierEvolution();
    }


    private void UpdateWheelMovement()
    {
        Transform[] tireTrans = physicsScript.tireTransform;
        for (int i = 0; i < wheelMesh.Length; i++)
        {
            ApplyWheelLocation(i, tireTrans[i]);
            ApplyWheelRotation(i, tireTrans[i]);
        }
    }    
    private void ApplyWheelRotation(int i, Transform tire)
    {
        desireRotation = (physicsScript.carSpeed == 0.0f) ? 0 : (physicsScript.carSpeed * wheelTourqeRatio);
        wheelMesh[i].Rotate(desireRotation, 0, 0, Space.Self);
        
    }   
    private void ApplyWheelLocation(int i, Transform tire)
    {

            RaycastHit hit;
            bool rayDidHit = Physics.Raycast(tire.position, -tire.up, out hit, physicsScript.restDistance);
            if (rayDidHit)
            {
                wheelMesh[i].position = Vector3.Lerp(wheelMesh[i].position, tire.position + new Vector3(0, -(hit.distance - wheelRadius), 0), Time.deltaTime * 30f);
            }
    }   
    private void ApplyDust()
    {
        float carSpeed = physicsScript.carSpeed;

        foreach (var smoke in wheelSmokes)
        {
            var emission = smoke.emission;
            // Chỉ bật khói khi xe chạy nhanh hoặc khi bạn muốn (ví dụ đang phanh/drift)
            // Only enable smoke when speeding or braking/drifting
            emission.enabled = carSpeed > 5f || physicsScript.isDrift;
        }
    }

    // 1. Hàm khóa chặt lúc bắt đầu / Function to lock firmly at start
    public void LockDamageableParts()
    {
        if (damageableParts == null || damageableParts.Count == 0) return;

        foreach (HingeJoint part in damageableParts)
        {
            if (part != null)
            {
                part.useLimits = true;

                // LƯU LẠI setup gốc (Chỉ lưu 1 lần duy nhất lúc mới chơi) / SAVE original setup (Only save once at start)
                if (!originalLimits.ContainsKey(part))
                {
                    originalLimits[part] = part.limits;
                    allOriginalParts.Add(part); // Lưu vào danh sách gốc / Save to original list
                    originalLocalPositions[part] = part.transform.localPosition;
                    originalLocalRotations[part] = part.transform.localRotation;
                }

                // Ép góc về 0 để cửa dính chặt / Force angle to 0 so doors stick firmly
                JointLimits lockedLimits = part.limits;
                lockedLimits.min = 0f;
                lockedLimits.max = 0f;
                part.limits = lockedLimits;
            }
        }
    }

    // 2. Hàm xử lý khi bị tông (Dùng Clone Thế Thân) / Function to handle when rammed (Using Clone Debris)
    public void TakeVisualDamage()
    {
        if (damageableParts == null || damageableParts.Count == 0) return;

        int randomIndex = UnityEngine.Random.Range(0, damageableParts.Count);
        HingeJoint selectedPart = damageableParts[randomIndex];

        if (selectedPart != null)
        {
            if (selectedPart.limits.max == 0f && selectedPart.limits.min == 0f)
            {
                // Trạng thái 1: TRẢ LẠI giới hạn gốc / State 1: RESTORE original limits
                selectedPart.limits = originalLimits[selectedPart];
            }
            else
            {
                // Trạng thái 2: Đã lung lay rồi -> Rụng luôn / State 2: Already swinging -> Fall off completely

                // TẠO BẢN SAO RƠI XUỐNG ĐƯỜNG / CREATE CLONE TO FALL ON ROAD
                GameObject debris = Instantiate(selectedPart.gameObject, selectedPart.transform.position, selectedPart.transform.rotation);

                // FIX LỖI SIZE BỊ TO: Ép scale của bản sao bằng đúng scale tổng thể của cửa gốc
                // FIX ENLARGED SIZE BUG: Force clone's local scale to exactly match the global lossyScale of the original door
                debris.transform.localScale = selectedPart.transform.lossyScale;

                Destroy(debris.GetComponent<HingeJoint>()); // Đập vỡ bản lề của bản sao / Break clone's hinge

                // Giữ lại mảnh vỡ trên sàn đấu 20 giây cho chân thực / Keep debris on the arena for 20 seconds for realism
                Destroy(debris, 20f);

                // ẨN CỬA THẬT ĐI / HIDE THE REAL DOOR
                selectedPart.gameObject.SetActive(false);
                damageableParts.RemoveAt(randomIndex);
            }
        }
    }

    // 3. Hàm gọi khi xe hồi sinh / Function called when car respawns
    public void RepairCar()
    {
        damageableParts.Clear(); // Xóa danh sách cũ / Clear old list

        foreach (HingeJoint part in allOriginalParts)
        {
            if (part != null)
            {
                part.transform.localPosition = originalLocalPositions[part];
                part.transform.localRotation = originalLocalRotations[part];

                part.gameObject.SetActive(true);    // Hiện lại cánh cửa đã tàng hình / Unhide the invisible door
                part.limits = originalLimits[part]; // Trả lại setup gốc / Restore original setup
                damageableParts.Add(part);          // Đưa vào lại danh sách để tông tiếp / Re-add to active list
            }
        }

        LockDamageableParts(); // Khóa chặt lại y như mới! / Lock them tight like new!
    }



    private void CheckTierEvolution()
    {
        // Liên tục soi xem Tier bên Physics có thay đổi không / Continuously watch if Physics Tier changed
        if (physicsScript.CurrentTier != previousTier)
        {
            // Nếu Tier mới LỚN HƠN Tier cũ -> Tiến hóa!
            // If new Tier is GREATER THAN old Tier -> Evolve!
            if (physicsScript.CurrentTier > previousTier)
            {
                TriggerEvolutionPunch();
                UpdateNitroColor(physicsScript.CurrentTier);
            }
            // Nếu rớt đài về Tier 0 -> Tắt Nitro
            // If dropped back to Tier 0 -> Turn off Nitro
            else if (physicsScript.CurrentTier == RamTier.Tier_0)
            {
                if (nitroParticle != null && nitroParticle.isPlaying)
                    nitroParticle.Stop();
            }

            // Cập nhật lại bộ nhớ / Update memory
            previousTier = physicsScript.CurrentTier;
        }
    }

    private void TriggerEvolutionPunch()
    {
        // DOComplete giúp ngắt ngay lập tức hiệu ứng cũ nếu xe lên cấp quá nhanh, tránh lỗi xe bị bự vĩnh viễn
        // DOComplete stops the old effect immediately if leveling up too fast, preventing permanent enlargement bug
        transform.DOComplete();

        // DOPunchScale: Scale bự thêm 20%, kéo dài 0.3 giây, độ rung 10, tính đàn hồi 1
        // DOPunchScale: Punch scale up by 20%, lasts 0.3s, vibrato 10, elasticity 1
        transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f, 10, 1);
    }

    private void UpdateNitroColor(RamTier currentTier)
    {
        if (nitroParticle == null) return;

        // Bật hạt chạy nếu nó đang tắt / Play particles if currently stopped
        if (!nitroParticle.isPlaying) nitroParticle.Play();

        // Để đổi màu Particle System trong Unity, phải lấy Main Module ra trước
        // To change Particle System color in Unity, you must extract the Main Module first
        var mainModule = nitroParticle.main;

        if (currentTier == RamTier.Tier_1)
            mainModule.startColor = tier1Color;
        else if (currentTier == RamTier.Tier_2)
            mainModule.startColor = tier2Color;
        else if (currentTier == RamTier.Tier_3)
            mainModule.startColor = tier3Color;
    }
}
