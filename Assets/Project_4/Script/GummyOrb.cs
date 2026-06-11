using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class GummyOrb : MonoBehaviour
{
    [Header("Sticky Settings")]
    [SerializeField] private float stickDuration = 3f;

    private Rigidbody rb;
    private bool isSpatByPlayer = false;

    // [VI] Định nghĩa 2 Tag rõ ràng / [EN] Define 2 explicit Tags
    private string defaultTag = "FrogInteractable";
    private string grappleTag = "Grappleable";

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        gameObject.tag = defaultTag; // [VI] Mặc định ban đầu là đồ vật / [EN] Default as object
    }

    public void OnSpatOut()
    {
        isSpatByPlayer = true;
        StopAllCoroutines();

        // ==========================================
        // [VI] XÓA CHA BẤT KỲ LÚC NÀO NÓ BỊ PHUN RA
        // [EN] CLEAR PARENT WHENEVER IT IS SPAT OUT
        // ==========================================
        transform.SetParent(null);

        rb.isKinematic = false;
        gameObject.tag = defaultTag; // [VI] Đảm bảo trả lại Tag khi bay / [EN] Ensure Tag is reset when flying
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isSpatByPlayer) return;

        // [VI] Truyền object mà nó tông trúng vào hàm
        // [EN] Pass the object it collided with into the function
        StickToSurface(collision.transform);
    }

    private void StickToSurface(Transform hitSurface)
    {
        isSpatByPlayer = false;

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        var col = rb.GetComponent<Collider>();
        col.isTrigger = true;

        // ==========================================
        // [VI] NHẬN VẬT THỂ BỊ TÔNG TRÚNG LÀM CHA
        // [EN] MAKE THE HIT OBJECT ITS PARENT
        // ==========================================
        transform.SetParent(hitSurface);

        // [VI] Biến thành điểm đu dây khi dính tường
        // [EN] Turn into a grapple point when stuck to wall
        gameObject.tag = grappleTag;

        StartCoroutine(UnstickRoutine());
    }

    private IEnumerator UnstickRoutine()
    {
        yield return new WaitForSeconds(stickDuration);

        // ==========================================
        // [VI] RỚT XUỐNG THÌ TÁCH KHỎI CHA ĐỂ RƠI TỰ DO
        // [EN] WHEN DROPPING, DETACH FROM PARENT TO FREEFALL
        // ==========================================
        transform.SetParent(null);

        // [VI] Rớt xuống thì biến lại thành đồ vật
        // [EN] When dropping, turn back into an object
        rb.isKinematic = false;
        var col = rb.GetComponent<Collider>();
        col.isTrigger = false;
        gameObject.tag = defaultTag;
    }
}