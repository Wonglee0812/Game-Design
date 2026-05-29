using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class Magnet_Input : MonoBehaviour
{
    private PlayerInput inputActions;
    private Magnet_PlayerController playerController;
    private Magnet_Ability playerMagnet;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction magnetAction;
    private InputAction mousePosAction;

    private void Awake()
    {
        playerController = GetComponent<Magnet_PlayerController>();
        playerMagnet = GetComponent<Magnet_Ability>();
        inputActions = GetComponent<PlayerInput>();

        var map = inputActions.actions.FindActionMap("Player");
        moveAction = map.FindAction("Move", true);
        jumpAction = map.FindAction("Jump", true);
        magnetAction = map.FindAction("Attack", true);
        mousePosAction = map.FindAction("Look", true);

        jumpAction.started += ctx => playerController?.TriggerJump();
    }

    private void OnEnable() => inputActions.actions.FindActionMap("Player").Enable();
    private void OnDisable() => inputActions.actions.FindActionMap("Player").Disable();
    private void OnDestroy() { if (jumpAction != null) jumpAction.started -= ctx => playerController?.TriggerJump(); }

    private void Update()
    {
        if (playerController != null)
        {
            Vector2 moveValue = moveAction.ReadValue<Vector2>();
            playerController.SetMoveInput(moveValue.x);

            // [VI] Gửi tín hiệu xem nút nhảy có đang bị đè không / [EN] Send signal if jump button is being held
            bool isHeld = jumpAction.ReadValue<float>() > 0.5f;
            playerController.SetJumpHeld(isHeld);
        }

        if (playerMagnet != null)
        {
            bool isClicking = Mouse.current.leftButton.isPressed;
            Vector2 mousePos = Mouse.current.position.ReadValue();
            playerMagnet.SetMagnetInput(isClicking, mousePos);
        }
    }
}