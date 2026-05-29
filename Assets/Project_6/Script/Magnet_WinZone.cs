using UnityEngine;

public class Magnet_WinZone : MonoBehaviour
{
    // [VI] CHÚ Ý CỰC MẠNH: Phải là OnTriggerEnter2D và Collider2D
    // [EN] HUGE ATTENTION: Must be OnTriggerEnter2D and Collider2D
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (Magnet_GameManager.Instance != null)
            {
                Magnet_GameManager.Instance.TriggerWin();
            }
        }
    }
}