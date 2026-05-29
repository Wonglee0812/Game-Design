using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform avatarLayoutGroup;     // [VI] Panel chứa Layout / [EN] Panel containing Layout
    [SerializeField] private GameObject avatarPrefab;         // [VI] Prefab của Avatar / [EN] Avatar Prefab
    [SerializeField] private TextMeshProUGUI playerCountText; // [VI] Text hiển thị Player / Max / [EN] Text displaying Player / Max
    [SerializeField] private RectTransform teamSetBanner;

    [HideInInspector] public GameLevelData currentLevelData;
    public int currentPlayerCount = 0;

    private void Start()
    {
        if (teamSetBanner != null)
        {
            teamSetBanner.anchoredPosition = new Vector2(-1500, teamSetBanner.anchoredPosition.y);
        }
        UpdatePlayerCountText();
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        // [VI] 1. Kiểm tra an toàn dữ liệu / [EN] 1. Data safety check
        if (currentLevelData == null || currentPlayerCount >= currentLevelData.maxPlayers)
        {
            Debug.Log("Phòng đầy hoặc chưa chọn Map!");
            Destroy(playerInput.gameObject);
            return;
        }

        // [VI] 2. Kiểm tra loại Input / [EN] 2. Check Input type
        if (!IsInputValid(playerInput))
        {
            Debug.LogWarning($"Từ chối: Sai thiết bị. Yêu cầu: {currentLevelData.inputType}");
            Destroy(playerInput.gameObject);
            return;
        }

        // [VI] 3. Đăng ký người chơi với GlobalGameManager / [EN] 3. Register player to GlobalGameManager
        GlobalGameManager.Instance.AddPlayer(playerInput);
        currentPlayerCount = GlobalGameManager.Instance.GetPlayerCount();

        // [VI] 4. Debug loại Input và số thứ tự Player / [EN] 4. Debug Input type and Player number
        InputDevice device = playerInput.devices[0];
        Debug.Log($"Player {currentPlayerCount} joined. Device: {device.name} (Layout: {device.layout})");

        // [VI] 5. Cập nhật UI / [EN] 5. Update UI
        UpdatePlayerCountText();
        CreateAvatar();

        // [VI] 6. Kiểm tra đủ người để hiện Banner / [EN] 6. Check if full to show Banner
        if (currentPlayerCount == currentLevelData.maxPlayers)
        {
            ShowTeamSetBanner();
        }
    }

    // [VI] Hàm tạo Avatar bằng Prefab / [EN] Function to create Avatar using Prefab
    private void CreateAvatar()
    {
        if (avatarPrefab == null || avatarLayoutGroup == null)
        {
            Debug.LogError("Lỗi: Thiếu Avatar Prefab hoặc Layout Group trong Inspector!");
            return;
        }

        Instantiate(avatarPrefab, avatarLayoutGroup);
    }

    // [VI] Hàm cập nhật Text Player/Max / [EN] Function to update Player/Max Text
    private void UpdatePlayerCountText()
    {
        if (playerCountText != null && currentLevelData != null)
        {
            playerCountText.text = $"{currentPlayerCount} / {currentLevelData.maxPlayers}";
        }
    }

    private void ShowTeamSetBanner()
    {
        if (teamSetBanner != null)
        {
            teamSetBanner.DOAnchorPosX(0, 0.6f).SetEase(Ease.OutBack);
        }
    }

    private bool IsInputValid(PlayerInput playerInput)
    {
        if (playerInput.devices.Count == 0) return false;

        InputDevice device = playerInput.devices[0];
        SupportedInput requiredInput = currentLevelData.inputType;

        if (requiredInput == SupportedInput.Any) return true;
        if (requiredInput == SupportedInput.GamepadOnly && device is Gamepad) return true;
        if (requiredInput == SupportedInput.KeyboardOnly && device is Keyboard) return true;

        return false;
    }
}