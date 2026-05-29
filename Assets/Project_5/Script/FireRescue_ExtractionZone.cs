using UnityEngine;

public class FireRescue_ExtractionZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // [VI] Khi Player bước vào vùng an toàn, hệ thống sẽ kiểm tra xem đã đủ điều kiện thắng chưa
        // [EN] When the Player enters the safe zone, the system checks if the win conditions are met
        if (other.CompareTag("Player"))
        {
            if (FireRescue_GameManager.Instance != null)
            {
                FireRescue_GameManager.Instance.CheckWinCondition();
            }
        }
    }
}