using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Magnet_Ability : MonoBehaviour
{
    [Header("Magnet Settings")]
    public float magnetForce = 30f; // [VI] Lực hút / [EN] Pull force
    public float maxRange = 15f;    // [VI] Tầm xa tối đa / [EN] Max range
    public LayerMask metalLayer;    // [VI] Lớp kim loại (tường/trần nhà) / [EN] Metal layer (walls/ceilings)

    private Rigidbody2D rb;
    private UnityEngine.Camera mainCam;

    // [VI] Trạng thái do Input truyền vào / [EN] State passed from Input
    private bool isAttracting = false;
    private Vector2 mouseScreenPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = UnityEngine.Camera.main;
    }

    private void FixedUpdate()
    {
        if (isAttracting)
        {
            ApplyMagnetForce();
        }
    }

    private void ApplyMagnetForce()
    {
        // [VI] 1. Đổi tọa độ chuột từ Màn hình sang Không gian 2D
        // [EN] 1. Convert mouse coordinates from Screen to 2D World space
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, mainCam.nearClipPlane));
        Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        // [VI] 2. Tính hướng từ nhân vật đến vị trí con trỏ chuột
        // [EN] 2. Calculate direction from character to mouse cursor position
        Vector2 currentPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 direction = (mousePos2D - currentPos).normalized;

        // [VI] 3. Bắn tia Raycast để xem có trúng "Kim loại" không
        // [EN] 3. Shoot a Raycast to see if it hits "Metal"
        RaycastHit2D hit = Physics2D.Raycast(currentPos, direction, maxRange, metalLayer);

        if (hit.collider != null)
        {
            // [VI] 4. Nếu trúng Kim loại, tác dụng lực kéo nhân vật về phía đó
            // [EN] 4. If it hits Metal, apply force to pull the character towards it
            rb.AddForce(direction * magnetForce);

            // [VI] Vẽ một đường thẳng màu xanh để debug trong Scene
            // [EN] Draw a blue line for debugging in the Scene
            Debug.DrawLine(currentPos, hit.point, Color.blue);
        }
    }

    // ==========================================
    // [VI] HÀM PUBLIC ĐỂ NHẬN INPUT / [EN] PUBLIC FUNCTIONS FOR INPUT
    // ==========================================
    public void SetMagnetInput(bool attracting, Vector2 mousePos)
    {
        isAttracting = attracting;
        mouseScreenPosition = mousePos;
    }
}