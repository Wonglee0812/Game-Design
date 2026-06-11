using UnityEngine;

namespace RacismProject
{
    [RequireComponent(typeof(Rigidbody))]
    public class Racism_PlayerController : MonoBehaviour
    {
        [SerializeField] private float speed = 8f;
        [SerializeField] private float turnSpeed = 15f;

        private Rigidbody rb;
        private Vector2 currentMoveInput;

        // [VI] Dùng Transform thay vì Camera để tránh lỗi Type Miss giống FireRescue
        // [EN] Use Transform instead of Camera to avoid Type Mismatch like FireRescue
        private Transform mainCameraTransform;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            // [VI] Khóa trục X và Z để khối lập phương không lăn vòng vòng
            // [EN] Freeze X and Z axes so the cube doesn't roll around
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            // [VI] Tự động lấy Camera có tag "MainCamera"
            // [EN] Automatically get the Camera with the "MainCamera" tag
            if (UnityEngine.Camera.main != null)
            {
                mainCameraTransform = UnityEngine.Camera.main.transform;
            }
            else
            {
                Debug.LogWarning("Racism_PlayerController: No Main Camera found! Đảm bảo Camera của bạn có Tag là 'MainCamera'.");
            }
        }

        public void SetMoveInput(Vector2 move)
        {
            currentMoveInput = move;
        }

        private void FixedUpdate()
        {
            Move();
        }

        private void Move()
        {
            Vector3 moveDir = Vector3.zero;

            if (mainCameraTransform != null)
            {
                // [VI] Ép trục Y về 0 giống hệt logic của FireRescue
                // [EN] Force Y axis to 0 exactly like FireRescue's logic
                Vector3 camForward = mainCameraTransform.forward;
                camForward.y = 0f;
                camForward.Normalize();

                Vector3 camRight = mainCameraTransform.right;
                camRight.y = 0f;
                camRight.Normalize();

                moveDir = (camForward * currentMoveInput.y + camRight * currentMoveInput.x).normalized;
            }
            else
            {
                // [VI] Dự phòng nếu Camera bị mất
                // [EN] Fallback if Camera is missing
                moveDir = new Vector3(currentMoveInput.x, 0, currentMoveInput.y).normalized;
            }

            // [VI] Áp dụng di chuyển
            // [EN] Apply movement
            rb.MovePosition(rb.position + moveDir * speed * Time.fixedDeltaTime);

            // [VI] Xoay vật thể
            // [EN] Rotate object
            if (moveDir.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
            }
        }
    }
}