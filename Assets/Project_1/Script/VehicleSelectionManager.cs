using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleSelectionManager : MonoBehaviour
{
    public static VehicleSelectionManager Instance;

    [Header("Database & UI")]
    public List<VehicleDataSO> availableVehicles;
    [SerializeField] private GameObject playerSelectionUIPrefab; // UI Panel Prefab
    [SerializeField] private Transform uiHorizontalLayoutGroup;  // Nơi chứa các UI Panel / Container for UI Panels

    [Header("Game Container")]
    [SerializeField] private GameObject selectionCanvas; // Tắt Canvas này khi vào game / Disable this when game starts

    private List<PlayerSelectionHandler> spawnedHandlers = new List<PlayerSelectionHandler>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SpawnUIPanels();
    }

    private void SpawnUIPanels()
    {
        var globalData = GlobalGameManager.Instance;
        if (globalData == null || globalData.joinedPlayerDevices.Count == 0) return;

        for (int i = 0; i < globalData.joinedPlayerDevices.Count; i++)
        {
            InputDevice device = globalData.joinedPlayerDevices[i];

            // [VI] Sinh ra UI Panel và gán device cho PlayerInput của UI đó
            // [EN] Spawn UI Panel and pair device to that UI's PlayerInput
            PlayerInput uiInput = PlayerInput.Instantiate(
                playerSelectionUIPrefab,
                playerIndex: i,
                controlScheme: null,
                splitScreenIndex: -1,
                pairWithDevice: device
            );

            uiInput.transform.SetParent(uiHorizontalLayoutGroup, false);

            PlayerSelectionHandler handler = uiInput.GetComponent<PlayerSelectionHandler>();
            handler.Setup(i, this);
            spawnedHandlers.Add(handler);
        }
    }

    public void CheckAllReady()
    {
        if (spawnedHandlers.Count == 0) return;

        foreach (var handler in spawnedHandlers)
        {
            if (!handler.isReady) return; // Chỉ 1 người chưa Ready là hủy / If even 1 is not ready, abort
        }

        Debug.Log("ALL PLAYERS READY! STARTING GAME...");
        StartGame();
    }

    private void StartGame()
    {
        // 1. Tắt màn hình chọn xe / Disable selection screen
        selectionCanvas.SetActive(false);

        // 2. Gom danh sách xe đã chọn / Collect selected vehicles
        List<VehicleDataSO> finalSelections = new List<VehicleDataSO>();
        foreach (var handler in spawnedHandlers)
        {
            finalSelections.Add(handler.selectedVehicle);

            // Xóa cái Input của UI để nhường Input cho Xe thật
            // Destroy the UI's Input to give Input control to the real Car
            Destroy(handler.gameObject);
        }

        // 3. Gọi GameManager_1 spawn xe / Tell GameManager_1 to spawn cars
        GameManager_1.Instance.StartGameWithSelections(finalSelections);
    }
}