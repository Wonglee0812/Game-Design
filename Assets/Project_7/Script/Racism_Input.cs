using UnityEngine;
using UnityEngine.InputSystem;

namespace RacismProject
{
    [RequireComponent(typeof(PlayerInput))]
    public class Racism_Input : MonoBehaviour
    {
        private PlayerInput inputActions;
        private Racism_PlayerController player;

        private InputAction moveAction;

        private void Awake()
        {
            player = GetComponent<Racism_PlayerController>();
            inputActions = GetComponent<PlayerInput>();

            // [VI] TÌm Action Map và hành động giống hệt FireRescue
            // [EN] Find the Action Map and action exactly like FireRescue
            var map = inputActions.actions.FindActionMap("Player");
            if (map != null)
            {
                moveAction = map.FindAction("Move", true);
            }
        }

        private void OnEnable()
        {
            if (inputActions != null && inputActions.actions != null)
                inputActions.actions.FindActionMap("Player")?.Enable();
        }

        private void OnDisable()
        {
            if (inputActions != null && inputActions.actions != null)
                inputActions.actions.FindActionMap("Player")?.Disable();
        }

        private void Update()
        {
            GetMoveInput();
        }

        private void GetMoveInput()
        {
            if (player != null && moveAction != null)
            {
                // [VI] Đọc giá trị và truyền sang Controller
                // [EN] Read the value and pass it to the Controller
                player.SetMoveInput(moveAction.ReadValue<Vector2>());
            }
        }
    }
}