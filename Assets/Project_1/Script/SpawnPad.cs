using System.Collections;
using UnityEngine;

public class SpawnPad : MonoBehaviour
{
    [Header("Pad Settings")]
    [Tooltip("Thời gian chờ (giây) trước khi bệ ẩn đi / Time to wait (seconds) before hiding")]
    [SerializeField] private float hideDelay = 2.0f;

    [SerializeField] private MeshRenderer visualRenderer;

    private Coroutine hideCoroutine;
    private int playersOnPad = 0;

    private void Awake()
    {
        if (visualRenderer == null) visualRenderer = GetComponent<MeshRenderer>();
        if (visualRenderer != null) visualRenderer.enabled = false;

        Debug.Log($"[SpawnPad - {gameObject.name}] AWAKE: Bệ đệm đã khởi tạo và tàng hình.");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Báo cáo ngay khi có bất kỳ vật gì chạm vào / Report immediately when anything touches
        Debug.Log($"[SpawnPad - {gameObject.name}] ENTER: Vật chạm vào là '{other.name}', Tag đang có: '{other.tag}'");

        // KIỂM TRA BẰNG TAG / CHECK BY TAG
        if (other.CompareTag("Player"))
        {
            playersOnPad++;
            Debug.Log($"[SpawnPad - {gameObject.name}] ++ Tăng số người: {playersOnPad}");
            ShowPad();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Hàm bảo hiểm phòng trường hợp rơi quá nhanh
        // Insurance function in case of falling too fast
        if (playersOnPad <= 0 && other.CompareTag("Player"))
        {
            Debug.Log($"[SpawnPad - {gameObject.name}] STAY (BẢO HIỂM): Phát hiện Player '{other.name}' nhưng đếm = 0 -> Khôi phục!");
            playersOnPad = 1;
            ShowPad();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"[SpawnPad - {gameObject.name}] EXIT: Vật rời đi là '{other.name}', Tag đang có: '{other.tag}'");

        if (other.CompareTag("Player"))
        {
            playersOnPad--;
            Debug.Log($"[SpawnPad - {gameObject.name}] -- Giảm số người: {playersOnPad}");

            if (playersOnPad <= 0)
            {
                playersOnPad = 0;
                if (hideCoroutine != null) StopCoroutine(hideCoroutine);

                Debug.Log($"[SpawnPad - {gameObject.name}] Bắt đầu đếm ngược {hideDelay}s để ẩn bệ.");
                hideCoroutine = StartCoroutine(HideRoutine());
            }
        }
    }

    private void ShowPad()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
            Debug.Log($"[SpawnPad - {gameObject.name}] Đã hủy đếm ngược ẩn bệ.");
        }

        if (visualRenderer != null && !visualRenderer.enabled)
        {
            visualRenderer.enabled = true;
            Debug.Log($"[SpawnPad - {gameObject.name}] ĐÃ HIỆN BỆ!");
        }
    }

    private IEnumerator HideRoutine()
    {
        yield return new WaitForSeconds(hideDelay);

        if (visualRenderer != null)
        {
            visualRenderer.enabled = false;
            Debug.Log($"[SpawnPad - {gameObject.name}] ĐÃ ẨN BỆ sau {hideDelay}s!");
        }
    }
}