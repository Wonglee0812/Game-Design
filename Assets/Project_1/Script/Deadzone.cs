using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class Deadzone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerInput>(out var playerInput))
        {
            if (KnockOut_GameManager.Instance != null)
            {
                // [VI] Gọi hàm rớt đài của GameManager mới
                // [EN] Call the falling function of the new GameManager
                KnockOut_GameManager.Instance.OnPlayerFall(playerInput);
            }
            else
            {
                Debug.LogError("Cảnh báo: Không tìm thấy KnockOut_GameManager trong Scene!");
            }
        }
    }
}