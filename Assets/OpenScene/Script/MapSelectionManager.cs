using UnityEngine;

public class MapSelectionManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mapSelectionPanel;
    [SerializeField] private GameObject registerPanel;

    [Header("Managers")]
    [SerializeField] private LobbyManager registerManager;

    // [VI] Hàm chính cho Button gọi / [EN] Main function for Button to call
    public void OnMapSelected(GameLevelData selectedLevelData)
    {
        SetLevelData(selectedLevelData);
        TransitionToRegister();
    }

    // [VI] 1. Hàm gửi data về GameManager / [EN] 1. Function to send data to GameManager
    private void SetLevelData(GameLevelData data)
    {
        GlobalGameManager.Instance.currentLevelData = data;

        if (registerManager != null)
        {
            registerManager.currentLevelData = data;
        }
    }

    // [VI] 2. Hàm tắt/mở Panel và Manager / [EN] 2. Function to toggle Panels and Manager
    private void TransitionToRegister()
    {
        mapSelectionPanel.SetActive(false);
        registerPanel.SetActive(true);

        if (registerManager != null)
        {
            // [VI] Bật GameObject chứa script lên trước / [EN] Enable the GameObject containing the script first
            registerManager.gameObject.SetActive(true);
            // [VI] Bật component / [EN] Enable the component
            registerManager.enabled = true;
        }
    }
}