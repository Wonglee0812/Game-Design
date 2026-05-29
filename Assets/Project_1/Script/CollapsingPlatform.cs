using UnityEngine;
using DG.Tweening;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class CollapsingPlatform : MonoBehaviour
{
    [Header("Detection Settings (BoxCast)")]
    [SerializeField] private float castDistance = 2.0f;     // Độ cao khối hộp quét lên trời
    [SerializeField] private LayerMask detectionLayer;    // Layer nhận diện (xe)
    [SerializeField] private float boxSizeMultiplier = 0.95f; // Thu nhỏ nhẹ để tránh đụng bệ bên cạnh

    [Header("Timing Settings")]
    [SerializeField] private float totalTimeBeforeFall = 5.0f; // Tổng thời gian từ lúc chạm đến lúc rụng
    [SerializeField] private float shakeLeadTime = 2.0f;      // Thời gian sẽ rung trước khi rụng (Ví dụ: rung 2s cuối)
    [SerializeField] private float waitToHide = 3.0f;         // Chờ rụng xong mới ẩn
    [SerializeField] private float waitToRespawn = 3.0f;      // Chờ ẩn xong mới hiện lại

    [Header("Visual & Physics")]
    [SerializeField] private float shakeStrength = 0.15f;
    [SerializeField] private MeshRenderer visualRenderer;

    private Rigidbody rb;
    private BoxCollider boxCollider;
    private Vector3 originPos;
    private Quaternion originRot;
    private bool isProcessing = false;
    private Tweener shakeTweener;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        if (visualRenderer == null) visualRenderer = GetComponent<MeshRenderer>();

        originPos = transform.position;
        originRot = transform.rotation;

        rb.isKinematic = true;
    }

    private void Update()
    {
        if (isProcessing) return;

        // Bắn một khối hộp (BoxCast) có kích thước bằng với BoxCollider đang gắn trên bệ
        if (CheckBoxCast())
        {
            StartCoroutine(CollapseSequence());
        }
    }

    private bool CheckBoxCast()
    {
        // Lấy kích thước của Collider để làm kích thước cho BoxCast
        Vector3 halfExtents = new Vector3(
            (boxCollider.size.x * transform.lossyScale.x) / 2 * boxSizeMultiplier,
            0.1f, // Độ dày của khối quét
            (boxCollider.size.z * transform.lossyScale.z) / 2 * boxSizeMultiplier
        );

        // Bắn khối hộp lên phía trên
        return Physics.BoxCast(transform.position, halfExtents, Vector3.up, transform.rotation, castDistance, detectionLayer);
    }

    private IEnumerator CollapseSequence()
    {
        isProcessing = true;

        // 1. Giai đoạn im lặng ban đầu
        float silentDuration = totalTimeBeforeFall - shakeLeadTime;
        if (silentDuration > 0) yield return new WaitForSeconds(silentDuration);

        // 2. Giai đoạn rung lắc (Bắt đầu từ thời điểm bạn muốn cho đến khi rụng)
        // Dùng DOShakePosition và lưu vào biến để có thể dừng nếu cần
        shakeTweener = transform.DOShakePosition(shakeLeadTime, shakeStrength, 10, 90, false, true);
        yield return new WaitForSeconds(shakeLeadTime);

        // 3. Rụng
        rb.isKinematic = false;
        rb.useGravity = true;

        // 4. Chờ rụng xong rồi ẩn
        yield return new WaitForSeconds(waitToHide);
        visualRenderer.enabled = false;
        boxCollider.enabled = false;

        // 5. Chờ tái sinh
        yield return new WaitForSeconds(waitToRespawn);

        ResetPlatform();
    }

    private void ResetPlatform()
    {
        // Dừng mọi hiệu ứng DOTween đang chạy trên bệ này
        if (shakeTweener != null) shakeTweener.Kill();


        rb.isKinematic = true;
        rb.useGravity = false;

        transform.position = originPos;
        transform.rotation = originRot;

        if (VFXManager.Instance != null) VFXManager.Instance.PlaySpawnEffect(transform.position, 0.5f);

        visualRenderer.enabled = true;
        boxCollider.enabled = true;

        isProcessing = false;
    }

    // Vẽ khối hộp trong Editor để bạn căn chỉnh
    private void OnDrawGizmosSelected()
    {
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider>();

        Gizmos.color = Color.yellow;
        Vector3 halfExtents = new Vector3(
            (boxCollider.size.x * transform.lossyScale.x) / 2 * boxSizeMultiplier,
            0.1f,
            (boxCollider.size.z * transform.lossyScale.z) / 2 * boxSizeMultiplier
        );

        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.matrix = rotationMatrix;

        // Vẽ khối hộp đại diện cho BoxCast
        Gizmos.DrawWireCube(Vector3.up * castDistance, boxCollider.size * boxSizeMultiplier);
    }
}