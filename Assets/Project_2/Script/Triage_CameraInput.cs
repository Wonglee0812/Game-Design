using UnityEngine;
using UnityEngine.InputSystem;

namespace Portfolio.TriageManagement
{
    [RequireComponent(typeof(PlayerInput))]
    public class Triage_CameraInput : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Triage_CameraController cameraController;

        private PlayerInput inputActions;
        private InputAction moveAction;
        private InputAction zoomAction; // [VI] Thêm biến đọc Zoom / [EN] Add Zoom reading variable

        private void Awake()
        {
            inputActions = GetComponent<PlayerInput>();

            var map = inputActions.actions.FindActionMap("Player");
            if (map != null)
            {
                moveAction = map.FindAction("Move", true);
                zoomAction = map.FindAction("Zoom", true); // [VI] Tìm Action Zoom / [EN] Find Zoom Action
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
            if (cameraController != null)
            {
                if (moveAction != null)
                {
                    Vector2 moveValue = moveAction.ReadValue<Vector2>();
                    cameraController.SetMoveInput(moveValue);
                }

                if (zoomAction != null)
                {
                    // [VI] Con lăn chuột trả về Vector2, ta chỉ cần trục Y
                    // [EN] Mouse scroll returns a Vector2, we only need the Y axis
                    float zoomValue = zoomAction.ReadValue<Vector2>().y;
                    cameraController.SetZoomInput(zoomValue);
                }
            }
        }
    }
}