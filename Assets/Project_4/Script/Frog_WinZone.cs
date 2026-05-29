using UnityEngine;

public class Frog_WinZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // [VI] Ki?m tra xem ng??i ch?m vào có ph?i là Player không
        // [EN] Check if the object touching is the Player
        if (other.CompareTag("Player"))
        {
            if (Frog_GameManager.Instance != null)
            {
                Frog_GameManager.Instance.TriggerWin();
            }
        }
    }
}