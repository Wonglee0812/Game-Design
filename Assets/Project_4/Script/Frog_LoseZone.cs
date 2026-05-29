using UnityEngine;

public class Frog_LoseZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // [VI] Nếu Player rớt vào bẫy, lập tức kích hoạt Game Over
        // [EN] If the Player falls into the trap, immediately trigger Game Over
        if (other.CompareTag("Player"))
        {
            if (Frog_GameManager.Instance != null)
            {
                Frog_GameManager.Instance.TriggerLose();
            }
        }
    }
}