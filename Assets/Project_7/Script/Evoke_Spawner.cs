using UnityEngine;

namespace EvokeProject
{
    public class Evoke_Spawner : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject blackNPCPrefab;
        public GameObject whiteNPCPrefab;

        [Header("Spawn Settings")]
        public float spawnInterval = 2f;
        public float laneWidth = 4f;

        // [VI] Nơi chứa NPC để cửa sổ làm việc không bị rác
        // [EN] Container for NPCs so the workspace doesn't get cluttered
        private GameObject npcContainer;
        private float timer = 0f;

        private void Start()
        {
            // [VI] Tự động tạo thư mục chứa
            // [EN] Auto-create container folder
            npcContainer = new GameObject("--- NPC HERD ---");
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                SpawnWave();
                timer = 0f;
            }
        }

        private void SpawnWave()
        {
            // [VI] Tính tọa độ X ngẫu nhiên
            // [EN] Calculate random X coordinates
            Vector3 randomPosBlack = transform.position + new Vector3(Random.Range(-laneWidth, laneWidth), 0, 0);
            Vector3 randomPosWhite = transform.position + new Vector3(Random.Range(-laneWidth, laneWidth), 0, 0);

            // [VI] Sinh khối Đen
            // [EN] Spawn Black block
            if (blackNPCPrefab != null)
            {
                GameObject b = Instantiate(blackNPCPrefab, randomPosBlack, transform.rotation);
                b.transform.SetParent(npcContainer.transform);
            }

            // [VI] Sinh khối Trắng
            // [EN] Spawn White block
            if (whiteNPCPrefab != null)
            {
                GameObject w = Instantiate(whiteNPCPrefab, randomPosWhite, transform.rotation);
                w.transform.SetParent(npcContainer.transform);
            }
        }
    }
}