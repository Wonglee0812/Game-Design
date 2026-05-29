using UnityEngine;

public class Camera : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Transform rig;
    [SerializeField] private Transform pivot;
    [SerializeField] private GameObject player;
    [Header("Setting")]
    [SerializeField] private float followSpeed = 20f;
    [SerializeField] private float sensitivity;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 70f;
    [SerializeField] private bool LockCursor;

    private float yaw;
    private float pitch;
    internal float fieldOfView;
    internal static Camera main;

    private void Awake()
    {
        player ??= GameObject.FindWithTag("Player");
        if (rig == null || pivot == null)
            Debug.LogError("PlayerCamera: rig/pivot not assigned.");
    }
    void Start()
    {
        if (LockCursor)
            Cursor.lockState = CursorLockMode.Locked;
    }
    private void FixedUpdate()
    {
        FollowTarget();

    }
    void FollowTarget()
    {
        transform.position = Vector3.Lerp(transform.position, player.transform.position, followSpeed * Time.deltaTime);
    }

    public void LookInput(Vector2 look)
    {
        yaw += look.x * sensitivity * Time.deltaTime;
        pitch -= look.y * sensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        rig.localRotation = Quaternion.Euler(0f, yaw, 0f);
        pivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
