using UnityEngine;

public class FireRescue_ItemSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject itemPrefab; // [VI] Kéo Prefab vật phẩm vào đây / [EN] Drag Item Prefab here
    public int numberOfItems = 5; // [VI] Số lượng muốn tạo ra / [EN] Number of items to spawn
    public float spawnRadius = 15f; // [VI] Bán kính khu vực rải đồ / [EN] Radius of the scatter area

    private void Start()
    {
        // [VI] Kiểm tra xem có quên gắn Prefab không / [EN] Check if Prefab is missing
        if (itemPrefab == null)
        {
            Debug.LogError("Chưa gắn Item Prefab vào Spawner! / Item Prefab not assigned to Spawner!");
            return;
        }

        SpawnItems();
    }

    private void SpawnItems()
    {
        for (int i = 0; i < numberOfItems; i++)
        {
            // [VI] 1. Lấy ngẫu nhiên một điểm trong mặt phẳng 2D (hình tròn)
            // [EN] 1. Get a random point in a 2D plane (circle)
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;

            // [VI] 2. Chuyển nó thành tọa độ 3D. Giữ nguyên trục Y (độ cao) của Spawner.
            // [EN] 2. Convert it to 3D coordinates. Keep the Y axis (height) of the Spawner.
            Vector3 spawnPosition = new Vector3(
                transform.position.x + randomCircle.x,
                transform.position.y,
                transform.position.z + randomCircle.y
            );

            // [VI] 3. Sinh ra vật thể tại vị trí đó / [EN] 3. Spawn the object at that position
            Instantiate(itemPrefab, spawnPosition, Quaternion.identity);
        }
    }

    // [VI] Đoạn code này giúp bạn nhìn thấy vòng tròn giới hạn trong cửa sổ Scene của Unity
    // [EN] This code helps you see the boundary circle in Unity's Scene window
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}