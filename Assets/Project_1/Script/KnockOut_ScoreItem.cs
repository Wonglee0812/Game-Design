using UnityEngine;
using UnityEngine.InputSystem;

public class KnockOut_ScoreItem : MonoBehaviour
{
    [Header("Item Settings")]
    public int scoreValue = 15; // [VI] Điểm cộng khi ăn / [EN] Points added when collected

    [Header("Animation Settings")]
    public float rotationSpeed = 100f;
    public float bounceSpeed = 2f;
    public float bounceHeight = 0.5f;

    [HideInInspector] public KnockOut_ItemSpawner spawner;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        // [VI] Xoay tròn / [EN] Rotate around
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        // [VI] Nhấp nhô lên xuống bằng sóng Sin / [EN] Bob up and down using Sine wave
        float newY = startPos.y + Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        // [VI] Kiểm tra xem người chạm vào có phải là xe (Player) không
        // [EN] Check if the object touching it is a car (Player)
        if (other.TryGetComponent<PlayerInput>(out var playerInput))
        {
            CollectItem(playerInput);
        }
        else
        {
            // [VI] Đề phòng Collider nằm ở vật thể con (Child) của xe
            // [EN] In case the Collider is on a Child object of the car
            PlayerInput parentInput = other.GetComponentInParent<PlayerInput>();
            if (parentInput != null)
            {
                CollectItem(parentInput);
            }
        }
    }

    private void CollectItem(PlayerInput player)
    {
        // 1. [VI] Cộng điểm / [EN] Add points
        KnockOut_GameManager.Instance.AddScore(player, scoreValue);

        // 2. [VI] Phát âm thanh & Hiệu ứng (Nổ particle)
        // [EN] Play audio & Visual effects (Particle explosion)
        if (AudioManager.Instance != null) AudioManager.Instance.Play("coin"); // Thay "coin" bằng tên âm thanh của bạn
        if (VFXManager.Instance != null) VFXManager.Instance.PlaySpawnEffect(transform.position, 0.5f);

        // 3. [VI] Báo cáo về Spawner là đã bị ăn / [EN] Report back to Spawner that it was collected
        if (spawner != null) spawner.OnItemCollected();

        // 4. [VI] Tự hủy / [EN] Self-destruct
        Destroy(gameObject);
    }
}