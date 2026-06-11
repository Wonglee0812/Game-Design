using UnityEngine;

public class Frog_GummySpawner : MonoBehaviour
{
    [Header("Gummy Prefab Settings")]
    public GameObject gummyPrefab;
    [Tooltip("Vị trí sinh ra Gummy. Nếu để trống, sẽ tự lấy vị trí của Spawner này")]
    public Transform spawnPoint;

    [Header("Timing")]
    public float spawnDelay = 3f;

    private GameObject currentGummy;
    private float timer = 0f;

    private void Start()
    {
        if (spawnPoint == null) spawnPoint = transform;
        SpawnGummy();
    }

    private void Update()
    {
        bool isSpawnPointEmpty = true;

        // [VI] Nếu Gummy cũ vẫn tồn tại, kiểm tra xem nó có đang nằm đúng ở Spawner không
        // [EN] If the old Gummy still exists, check if it is still exactly at the Spawner
        if (currentGummy != null)
        {
            // [VI] Nếu nó cách Spawner hơn 1 mét, coi như Spawner đã bị trống
            // [EN] If it is more than 1 meter away from the Spawner, consider the Spawner empty
            if (Vector3.Distance(currentGummy.transform.position, spawnPoint.position) > 1.0f)
            {
                currentGummy = null; // [VI] Quên viên cũ đi / [EN] Forget the old one
            }
            else
            {
                isSpawnPointEmpty = false; // [VI] Nó vẫn đang nằm im ở đây / [EN] It's still sitting here
            }
        }

        // [VI] Đếm ngược đẻ viên mới nếu Spawner trống
        // [EN] Countdown to spawn new one if Spawner is empty
        if (isSpawnPointEmpty)
        {
            timer += Time.deltaTime;
            if (timer >= spawnDelay)
            {
                SpawnGummy();
                timer = 0f;
            }
        }
        else
        {
            timer = 0f;
        }
    }

    private void SpawnGummy()
    {
        if (gummyPrefab != null && spawnPoint != null)
        {
            currentGummy = Instantiate(gummyPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }
}