using UnityEngine;

namespace Portfolio.LevelDesign
{
    [RequireComponent(typeof(Rigidbody))]
    public class BouncyPlatform : MonoBehaviour
    {
        [Header("Spring (Game Feel) Settings")]
        [Tooltip("Lực nảy của platform / The bounce strength of the platform")]
        public float springStrength = 500f;
        [Tooltip("Lực cản giúp platform dừng lắc lư / Damping force to stop oscillation")]
        public float springDamper = 20f;

        private Rigidbody rb;
        private float targetYAnchor;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            
            // [VI] Khóa các trục X, Z và trục xoay để platform chỉ nhún lên xuống theo trục Y
            // [EN] Freeze X, Z axes and rotation so the platform only bounces up and down on the Y axis
            rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            
            // [VI] Vẫn bật trọng lực để có tương tác vật lý tự nhiên
            // [EN] Keep gravity enabled for natural physics interaction
            rb.useGravity = true; 
        }

        private void Start()
        {
            // [VI] Lấy chính xác độ cao bạn đã đặt trên Scene làm mốc vĩnh viễn!
            // [EN] Take the exact height you placed it in the Scene as the permanent anchor!
            targetYAnchor = transform.position.y;
        }

        private void FixedUpdate()
        {
            // [VI] 1. Tính độ lệch so với vị trí mốc lý tưởng ban đầu
            // [EN] 1. Calculate the offset from the initial ideal anchor position
            float offset = targetYAnchor - transform.position.y;

            // [VI] 2. Lấy vận tốc hiện tại (Dùng linearVelocity cho Unity 6)
            // [EN] 2. Get current velocity (Using linearVelocity for Unity 6)
            float velocityY = rb.linearVelocity.y;

            // [VI] 3. Tính lực lò xo (Hooke's Law)
            // [EN] 3. Calculate spring force (Hooke's Law)
            float springForce = (offset * springStrength) - (velocityY * springDamper);

            // [VI] 4. Tính toán lực đối trọng để triệt tiêu trọng lực kéo xuống của hệ thống
            // [EN] 4. Calculate counter-weight force to negate the system's downward gravity
            float antiGravityForce = rb.mass * Mathf.Abs(Physics.gravity.y);

            // [VI] 5. Ép lực đẩy lên platform
            // [EN] 5. Apply the upward force to the platform
            rb.AddForce(Vector3.up * (springForce + antiGravityForce));
        }
    }
}