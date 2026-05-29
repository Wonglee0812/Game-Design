using System.Collections;
using System.Collections.Generic; // [VI] Thư viện để dùng List / [EN] Library to use List
using UnityEngine;

namespace Portfolio.TriageManagement
{
    public class Triage_PatientSpawner : MonoBehaviour
    {
        [Header("Patient Database")]
        // [VI] Danh sách mảng các loại bệnh nhân / [EN] Array list of patient types
        public List<GameObject> patientPrefabs;

        [Header("Spawn Intervals (Seconds)")]
        public float normalSpawnTime = 5f;
        public float lowRepSpawnTime = 3f;
        public float highRepSpawnTime = 2f;

        [Header("Reputation Thresholds")]
        public int lowRepThreshold = 30;
        public int highRepThreshold = 80;

        private void Start()
        {
            // [VI] Kiểm tra chống lỗi nếu bạn quên bỏ Prefab vào List
            // [EN] Anti-error check in case you forget to put Prefabs in the List
            if (patientPrefabs == null || patientPrefabs.Count == 0)
            {
                Debug.LogError("No patients in the spawner list!");
                return;
            }
            StartCoroutine(SpawnRoutine());
        }

        private IEnumerator SpawnRoutine()
        {
            while (true)
            {
                if (Triage_GameManager.Instance != null && Triage_GameManager.Instance.IsGameOver)
                    yield break;

                float currentInterval = normalSpawnTime;

                if (Triage_GameManager.Instance != null)
                {
                    int rep = Triage_GameManager.Instance.currentReputation;
                    if (rep <= lowRepThreshold) currentInterval = lowRepSpawnTime;
                    else if (rep >= highRepThreshold) currentInterval = highRepSpawnTime;
                }

                yield return new WaitForSeconds(currentInterval);

                // [VI] Random một con số từ 0 đến giới hạn của List
                // [EN] Randomize a number from 0 to the limit of the List
                int randomIndex = Random.Range(0, patientPrefabs.Count);
                GameObject selectedPatient = patientPrefabs[randomIndex];

                // [VI] Sinh ra bệnh nhân được chọn
                // [EN] Spawn the selected patient
                Instantiate(selectedPatient, transform.position, Quaternion.identity);
            }
        }
    }
}