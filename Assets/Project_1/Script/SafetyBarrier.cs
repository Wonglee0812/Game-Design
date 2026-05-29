using System.Collections;
using UnityEngine;

public class SafetyBarrier : MonoBehaviour
{
    // [VI] Cờ đánh dấu để tránh việc gọi Coroutine nhiều lần khi bị tông liên tục
    // [EN] Flag to prevent calling the Coroutine multiple times on continuous hits
    private bool isTriggered = false;

    private void OnCollisionEnter(Collision collision)
    {
        // [VI] Nếu chưa bị tông và vật tông vào là xe của Player (nhớ gắn Tag "Player" cho xe)
        // [EN] If not yet triggered and the colliding object is a Player car (remember to Tag cars as "Player")
        if (!isTriggered && collision.gameObject.CompareTag("Player"))
        {
            isTriggered = true;
            StartCoroutine(BreakAfterDelay());
        }
    }

    private IEnumerator BreakAfterDelay()
    {
        // [VI] Đợi đúng 3 giây
        // [EN] Wait exactly 3 seconds
        yield return new WaitForSeconds(3f);

        // [VI] Gọi hiệu ứng Spawn từ Singleton để làm khói vỡ
        // [EN] Call the Spawn effect from Singleton to act as breaking dust
        if (VFXManager.Instance != null)
        {
            VFXManager.Instance.PlaySpawnEffect(transform.position, 0.5f);
        }

        // [VI] Hủy rào cản
        // [EN] Destroy the barrier
        Destroy(gameObject);
    }
}