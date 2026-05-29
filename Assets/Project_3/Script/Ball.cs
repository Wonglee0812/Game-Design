using TMPro;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countUI;
    [SerializeField] private Transform canvasUI;
    [SerializeField] private Transform camTransform;
    private Rigidbody rb;
    private float count;
    private float maxTime = 20;

    private void Start()
    {
        count = maxTime;
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        countExplore();
        updateUI();
        ApplyFrictionForce();
    }
    void LateUpdate()
    {
        canvasUI.LookAt(canvasUI.position + camTransform.forward);
    }
    private void countExplore()
    {
        if (count > 0)
        {
            count -= Time.deltaTime;

        }
        else
        {
        }    
    }    
    private void ApplyFrictionForce()
    {
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if (horizontalVelocity.magnitude > 0.5f)
        {
            float frictionCoefficient = 0.5f;
            Vector3 frictionForce = -horizontalVelocity * frictionCoefficient;

            rb.AddForce(frictionForce, ForceMode.Acceleration);
        }
    }

    private void updateUI()
    {
        canvasUI.position = Vector3.Lerp(canvasUI.position, transform.position + new Vector3(0, 5, 0), Time.deltaTime * 10f);
        int displayUi = Mathf.CeilToInt(count);
        countUI.text = displayUi.ToString();
    }    
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            count = maxTime;
        }
    }
}
