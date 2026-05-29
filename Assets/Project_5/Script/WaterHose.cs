using UnityEngine;
using UnityEngine.UI; // [VI] Thêm thư viện này để dùng Slider / [EN] Add this to use Slider

public class WaterHose : MonoBehaviour
{
    [Header("Settings")]
    public float damagePerSecond = 30f;
    public float range = 10f;
    public float streamRadius = 0.5f;
    public LayerMask fireLayer;
    public ParticleSystem waterParticles;

    [Header("Water Capacity & UI")]
    public float maxWater = 100f;
    public float waterDrainRate = 15f; // [VI] Tốc độ tụt nước mỗi giây / [EN] Water drain rate per second
    private float currentWater;
    public Slider waterSlider; // [VI] Tham chiếu đến UI Thanh Nước / [EN] Reference to Water Slider UI

    private bool isSpraying = false;

    private void Awake()
    {
        currentWater = maxWater;

        // [VI] Thiết lập giá trị ban đầu cho thanh UI / [EN] Set initial values for UI slider
        if (waterSlider != null)
        {
            waterSlider.maxValue = maxWater;
            waterSlider.value = currentWater;
        }
    }

    public void HandleWaterInput(bool isHolding)
    {
        // [VI] Nếu hết nước thì không cho bấm xịt nữa / [EN] If out of water, prevent spraying input
        if (currentWater <= 0) isHolding = false;

        isSpraying = isHolding;

        if (isSpraying && !waterParticles.isPlaying) waterParticles.Play();
        else if (!isSpraying && waterParticles.isPlaying) waterParticles.Stop();
    }

    private void Update()
    {
        // [VI] Xử lý tụt nước và cập nhật UI mỗi khung hình / [EN] Handle water drain and update UI every frame
        if (isSpraying)
        {
            currentWater -= waterDrainRate * Time.deltaTime;

            if (waterSlider != null) waterSlider.value = currentWater;

            // [VI] Khi nước cạn sạch / [EN] When water is completely empty
            if (currentWater <= 0)
            {
                currentWater = 0;
                isSpraying = false;
                if (waterParticles.isPlaying) waterParticles.Stop();
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isSpraying) return;

        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.SphereCast(ray, streamRadius, out RaycastHit hit, range, fireLayer))
        {
            FireSource fire = hit.collider.GetComponent<FireSource>();
            if (fire != null)
            {
                fire.TakeDamage(damagePerSecond * Time.fixedDeltaTime);
            }
        }
    }
}