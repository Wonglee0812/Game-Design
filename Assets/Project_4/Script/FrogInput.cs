using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class FrogInput : MonoBehaviour
{
    [Header("Reference")]
    private PlayerInput inputActions;
    private FrogController player;
    private FrogCamera playerCamera;
    private FrogTongue frogTongue;

    private InputAction moveAction;
    private Vector2 moveValue;
    private InputAction jumpAction;
    private InputAction lookAction;
    private InputAction tongueAction;

    public bool canInput { get; private set; } = true;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        player ??= GetComponent<FrogController>();
        playerCamera = FindFirstObjectByType<FrogCamera>();
        frogTongue = GetComponent<FrogTongue>();
    }

    private void Awake()
    {
        inputActions = GetComponent<PlayerInput>();
        var map = inputActions.actions.FindActionMap("Player");

        moveAction = map.FindAction("Move", true);
        jumpAction = map.FindAction("Jump", true);
        lookAction = map.FindAction("Look", true);
        tongueAction = map.FindAction("Attack", true);

        jumpAction.started += ctx => GetJumpDownInput();
        tongueAction.started += ctx => GetTongueInput();
    }

    private void OnEnable() => inputActions.actions.FindActionMap("Player").Enable();
    private void OnDisable() => inputActions.actions.FindActionMap("Player").Disable();
    private void OnDestroy() { if (jumpAction != null) jumpAction.started -= ctx => GetJumpDownInput(); }

    void Update()
    {
        if (!canInput) return;
        GetMoveInput();
        GetLookInput();
        GetJumpHeldInput();
    }

    private void GetLookInput()
    {
        if (playerCamera != null)
        {
            Vector2 lookValue = lookAction.ReadValue<Vector2>();

            bool isGamepad = false;
            if (lookAction.activeControl != null)
            {
                isGamepad = lookAction.activeControl.device is Gamepad;
            }

            playerCamera.SetLookInput(lookValue, isGamepad);
        }
    }

    private void GetMoveInput()
    {
        moveValue = moveAction.ReadValue<Vector2>();
        if (player != null) player.SetMoveInput(moveValue);
    }

    private void GetJumpHeldInput()
    {
        bool isHeld = jumpAction.ReadValue<float>() > 0.5f;
        if (player != null) player.SetJumpHeld(isHeld);
    }

    private void GetJumpDownInput()
    {
        if (!canInput) return;
        if (player != null) player.TriggerJumpDown();
    }

    private void GetTongueInput()
    {
        if (!canInput) return;
        if (frogTongue != null) frogTongue.HandleTongueInput();
    }
}