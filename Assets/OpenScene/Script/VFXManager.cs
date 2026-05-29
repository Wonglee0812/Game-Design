using UnityEngine;

public class VFXManager : MonoBehaviour
{
    // [VI] Tạo Singleton để gọi từ bất kỳ script nào khác
    // [EN] Create Singleton for global access from any other script
    public static VFXManager Instance { get; private set; }

    [Header("VFX Prefabs")]
    public GameObject killEffectPrefab;
    public GameObject collideEffectPrefab;
    public GameObject lightningEffectPrefab;
    public GameObject spawnEffectPrefab;

    private void Awake()
    {
        // [VI] Đảm bảo chỉ có 1 VFXManager tồn tại trong Scene
        // [EN] Ensure only one VFXManager exists in the Scene
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    // --- [VI] CÁC HÀM GỌI EFFECT / [EN] EFFECT SPAWN FUNCTIONS ---

    public void PlayKillEffect(Vector3 position, float destroyDelay = 2f)
    {
        SpawnAndDestroy(killEffectPrefab, position, destroyDelay);
    }

    public void PlayCollideEffect(Vector3 position, float destroyDelay = 2f)
    {
        SpawnAndDestroy(collideEffectPrefab, position, destroyDelay);
    }

    public void PlayLightningEffect(Vector3 position, float destroyDelay = 2f)
    {
        SpawnAndDestroy(lightningEffectPrefab, position, destroyDelay);
    }

    public void PlaySpawnEffect(Vector3 position, float destroyDelay = 2f)
    {
        SpawnAndDestroy(spawnEffectPrefab, position, destroyDelay);
    }

    // [VI] Hàm nội bộ xử lý việc sinh ra và tự động xóa
    // [EN] Internal helper to handle instantiation and auto-destruction
    private void SpawnAndDestroy(GameObject prefab, Vector3 spawnPosition, float destroyTime)
    {
        if (prefab == null)
        {
            Debug.LogWarning("VFXManager: Prefab is missing! / Thiếu Prefab!");
            return;
        }

        // [VI] Sinh ra effect / [EN] Spawn the effect
        GameObject spawnedEffect = Instantiate(prefab, spawnPosition, Quaternion.identity);

        // [VI] Tự động xóa sau thời gian chỉ định / [EN] Auto-destroy after set time
        Destroy(spawnedEffect, destroyTime);
    }
}