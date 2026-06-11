using UnityEngine;
using System.Collections;
using TMPro;

public enum Difficulty { Easy, Medium, Hard, Terrible }

public class WaySpawner : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Transform enemyEasy;    // Prefab cơ bản / Base prefab
    [SerializeField] private Transform enemyMedium;  // Prefab trung bình / Medium prefab
    [SerializeField] private Transform enemyHard;    // Prefab khó / Hard prefab
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private TextMeshProUGUI curWaveUI;
    [SerializeField] private TextMeshProUGUI maxWaveUI;

    [Header("Difficulty Settings")]
    public Difficulty currentDifficulty = Difficulty.Easy;
    [SerializeField] private int baseEnemyCount = 5;

    [Header("Wave Settings")]
    public int maxWave = 20;
    public int waveNumber = 0;

    private bool isWaveActive = false; // Check xem đang trong turn không / Check if wave is active

    private void Start()
    {
        if (maxWaveUI != null) maxWaveUI.text = maxWave.ToString();
        UpdateUI();
    }

    private void Update()
    {
        // Tự động kiểm tra nếu hết quái trên map thì kết thúc Turn
        // Automatically check if all enemies are gone to end the turn
        if (isWaveActive && GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            isWaveActive = false;
            Debug.Log("Wave Clear! Ready for next turn.");
        }
    }



    // Hàm gọi từ nút bấm UI "Next Turn" / Function called from UI button
    public void NextTurn()
    {
        if (isWaveActive)
        {
            Debug.Log("Wave is still running! Cannot start next one.");
            return;
        }

        if (waveNumber >= maxWave)
        {
            Debug.Log("Final wave reached!");
            return;
        }

        StartCoroutine(SpawnWave());
    }



    IEnumerator SpawnWave()
    {
        isWaveActive = true;
        waveNumber++;
        UpdateUI();

        int count = CalculateEnemyCount();
        Transform prefabToUse = GetEnemyPrefab();

        for (int i = 0; i < count; i++)
        {
            SpawnEnemy(prefabToUse);
            yield return new WaitForSeconds(0.5f);
        }
    }



    private int CalculateEnemyCount()
    {
        int multiplier = 2; // Default Easy
        if (currentDifficulty == Difficulty.Medium) multiplier = 3;
        else if (currentDifficulty == Difficulty.Hard) multiplier = 5;
        else if (currentDifficulty == Difficulty.Terrible) multiplier = 8;

        return baseEnemyCount + (waveNumber * multiplier);
    }



    private Transform GetEnemyPrefab()
    {
        // Logic dò từ cao xuống thấp / Fallback logic from high to low
        if ((currentDifficulty == Difficulty.Hard || currentDifficulty == Difficulty.Terrible) && enemyHard != null)
            return enemyHard;

        if (currentDifficulty >= Difficulty.Medium && enemyMedium != null)
            return enemyMedium;

        return enemyEasy; // Mặc định trả về Easy nếu các cái khác null / Default to Easy
    }



    private void SpawnEnemy(Transform prefab)
    {
        if (prefab == null) return;
        Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
    }



    private void UpdateUI()
    {
        if (curWaveUI != null) curWaveUI.text = waveNumber.ToString();
    }
}