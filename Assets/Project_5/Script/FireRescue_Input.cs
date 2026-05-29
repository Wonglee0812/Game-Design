using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class FireRescue_Input : MonoBehaviour
{
    private PlayerInput inputActions;
    private FireRescue_PlayerController player;
    private WaterHose waterHose; // Gắn script vòi rồng của bạn vào đây

    private InputAction moveAction;
    private InputAction sprayAction;

    private void Awake()
    {
        player = GetComponent<FireRescue_PlayerController>();
        waterHose = GetComponent<WaterHose>();
        inputActions = GetComponent<PlayerInput>();

        var map = inputActions.actions.FindActionMap("Player");
        moveAction = map.FindAction("Move", true);
        sprayAction = map.FindAction("Attack", true);
    }
    private void Start()
    {
        // [VI] Ẩn hình ảnh con trỏ chuột đi
        // [EN] Hide the mouse cursor image
        Cursor.visible = false;

        // [VI] Khóa con trỏ chuột vào giữa màn hình (giúp bạn không bị click nhầm ra ngoài cửa sổ game)
        // [EN] Lock the cursor to the center of the screen (prevents you from accidentally clicking outside the game window)
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable() => inputActions.actions.FindActionMap("Player").Enable();
    private void OnDisable() => inputActions.actions.FindActionMap("Player").Disable();

    private void Update()
    {
        GetMoveInput();
        GetSprayInput();
    }

    private void GetMoveInput()
    {
        if (player != null)
        {
            player.SetMoveInput(moveAction.ReadValue<Vector2>());
        }
    }

    private void GetSprayInput()
    {
        if (waterHose != null)
        {
            // [VI] Kiểm tra xem nút có đang bị đè không
            // [EN] Check if the button is being held down
            bool isHoldingSpray = sprayAction.ReadValue<float>() > 0.5f;
            waterHose.HandleWaterInput(isHoldingSpray);
        }
    }
}