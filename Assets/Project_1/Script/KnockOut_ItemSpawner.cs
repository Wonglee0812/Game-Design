using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockOut_ItemSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    public GameObject itemPrefab; // [VI] Kéo prefab đồng tiền/ngôi sao vào đây / [EN] Drag coin/star prefab here
    public List<Transform> spawnPoints; // [VI] Các điểm có thể sinh đồ / [EN] Possible spawn points
    public float spawnInterval = 5f; // [VI] Thời gian giữa mỗi lần sinh (giây) / [EN] Time between spawns (seconds)
    public int maxItemsOnMap = 5; // [VI] Số lượng đồ tối đa trên map / [EN] Max items allowed on the map

    private int currentItemCount = 0;

    private void Start()
    {
        if (spawnPoints.Count > 0 && itemPrefab != null)
        {
            StartCoroutine(SpawnRoutine());
        }
        else
        {
            Debug.LogWarning("[ItemSpawner] Thiếu ItemPrefab hoặc danh sách SpawnPoints! / Missing ItemPrefab or SpawnPoints list!");
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // [VI] Chỉ sinh đồ khi game đang Play và chưa vượt quá giới hạn
            // [EN] Only spawn items when the game is Playing and hasn't exceeded the limit
            if (currentItemCount < maxItemsOnMap && KnockOut_GameManager.Instance.currentState == KnockOut_GameManager.GameState.Playing)
            {
                SpawnItem();
            }
        }
    }

    private void SpawnItem()
    {
        // [VI] Chọn ngẫu nhiên 1 vị trí / [EN] Pick a random location
        Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

        GameObject newItem = Instantiate(itemPrefab, randomPoint.position, Quaternion.identity);

        // [VI] Báo cho vật phẩm biết ai là người đẻ ra nó để cập nhật số lượng khi bị ăn mất
        // [EN] Tell the item who spawned it to update the count when it gets collected
        KnockOut_ScoreItem scoreItem = newItem.GetComponent<KnockOut_ScoreItem>();
        if (scoreItem != null)
        {
            scoreItem.spawner = this;
        }

        currentItemCount++;
    }

    // [VI] Hàm này được gọi bởi cục item khi nó bị ăn / [EN] This is called by the item when it gets collected
    public void OnItemCollected()
    {
        currentItemCount--;
    }
}