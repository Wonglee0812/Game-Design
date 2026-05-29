using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerSelectionHandler : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI vehicleNameText;
    public TextMeshProUGUI statsText;
    public Image vehicleImage;
    public GameObject readyOverlay; // Màn mờ hoặc chữ READY / Ready overlay or text

    [HideInInspector] public int playerIndex;
    [HideInInspector] public VehicleDataSO selectedVehicle;
    [HideInInspector] public bool isReady = false;

    [Header("Input Settings")]
    public float inputThreshold = 0.8f; // Ngưỡng nhận diện / Detection threshold
    private bool isAxisInUse = false;   // Biến khóa chống lặp / Anti-repeat lock variable

    private VehicleSelectionManager selectionManager;
    private int currentVehicleIndex = 0;

    public void Setup(int pIndex, VehicleSelectionManager manager)
    {
        playerIndex = pIndex;
        selectionManager = manager;
        playerNameText.text = $"P{playerIndex + 1}";
        readyOverlay.SetActive(false);

        UpdateUI();
    }

    // [VI] Nhận Input Trái/Phải để đổi xe / [EN] Receive Left/Right Input to change car
    public void OnNavigate(InputAction.CallbackContext context)
    {
        if (isReady) return; // Nếu đã Ready thì không cho đổi xe nữa

        Vector2 input = context.ReadValue<Vector2>();

        // [VI] Kiểm tra ngưỡng (ngưỡng 0.8 để tránh Joycon bị trôi)
        // [EN] Check threshold (0.8 threshold to avoid Joycon drift)
        if (Mathf.Abs(input.x) > inputThreshold)
        {
            // [VI] Chỉ thực hiện nếu trục CHƯA bị khóa
            // [EN] Only execute if the axis is NOT locked
            if (!isAxisInUse)
            {
                if (input.x > 0) ChangeVehicle(1);  // Gạt phải
                else if (input.x < 0) ChangeVehicle(-1); // Gạt trái

                isAxisInUse = true; // [VI] KHÓA: Đã đổi xe, phải nhả tay mới được đổi tiếp
            }
        }
        else
        {
            // [VI] MỞ KHÓA: Khi cần gạt thả về giữa (dưới ngưỡng 0.8)
            // [EN] UNLOCK: When stick returns to center (below 0.8 threshold)
            isAxisInUse = false;
        }
    }

    // [VI] Nhận Input nút (ví dụ: Phím A/South) để Ready / [EN] Receive button Input (e.g., A/South) to Ready
    public void OnSubmit(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        isReady = !isReady; // Toggle ready
        readyOverlay.SetActive(isReady);

        // Báo cho Manager kiểm tra / Tell Manager to check
        selectionManager.CheckAllReady();
    }

    private void ChangeVehicle(int direction)
    {
        var db = selectionManager.availableVehicles;
        currentVehicleIndex += direction;

        // Vòng lặp index / Loop index
        if (currentVehicleIndex < 0) currentVehicleIndex = db.Count - 1;
        if (currentVehicleIndex >= db.Count) currentVehicleIndex = 0;

        UpdateUI();
    }

    private void UpdateUI()
    {
        selectedVehicle = selectionManager.availableVehicles[currentVehicleIndex];
        vehicleNameText.text = selectedVehicle.vehicleName;
        statsText.text = $"Mass: {selectedVehicle.mass}\nSpeed: {selectedVehicle.speed}";

        if (selectedVehicle.vehicleIcon != null)
            vehicleImage.sprite = selectedVehicle.vehicleIcon;
    }
}