using UnityEngine;

public class FireRescue_Collectible : MonoBehaviour
{
    private void Start()
    {
        // [VI] Báo cáo với GameManager rằng vật thể này có tồn tại
        // [EN] Report to the GameManager that this object exists
        if (FireRescue_GameManager.Instance != null)
        {
            FireRescue_GameManager.Instance.RegisterCollectible();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // [VI] Đảm bảo đối tượng chạm vào có Tag là "Player"
        // [EN] Ensure the touching object is Tagged "Player"
        if (other.CompareTag("Player"))
        {
            if (FireRescue_GameManager.Instance != null)
            {
                FireRescue_GameManager.Instance.CollectObject();
            }

            // [VI] Xóa vật thể sau khi nhặt / [EN] Destroy object after pickup
            Destroy(gameObject);
        }
    }
}