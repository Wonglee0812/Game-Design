using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class Deadzone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerInput>(out var playerInput))
        {
            if (GameManager_1.Instance != null)
            {
                // Gọi hàm báo rơi xuống thay vì báo thua trực tiếp
                // Call the falling function instead of instant lose
                GameManager_1.Instance.OnPlayerFall(playerInput);
            }
            else
            {
                Debug.LogError("Cảnh báo: Không tìm thấy GameManager_1 trong Scene!");
            }
        }
    }
}