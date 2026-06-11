using UnityEngine;
using Unity.Collections;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine.EventSystems;
public class Node : MonoBehaviour
{

    [Header("Setting")]
    [SerializeField]private Color hoverColor;

    [Header("UI Feedback")]
    [SerializeField] private GameObject errorTextPrefab;

    [HideInInspector]
    public GameObject turret;
    private Renderer rend;
    private Color startColor;
    [HideInInspector]
    public TurretBluePrint turretBlueprint;
    BuildManager buildManager;
    private void Start()
    {
        rend = GetComponent<Renderer>();
        startColor = rend.material.color;
        buildManager = BuildManager.instance;
    }

    private void OnMouseDown()
    {
        // --- CHẶN CLICK XUYÊN UI / BLOCK UI PASS-THROUGH ---
        // Nếu chuột đang click lên một thành phần UI (như Panel, Button), thì DỪNG LẠI NGAY!
        // If the mouse is clicking on a UI element (like a Panel or Button), STOP IMMEDIATELY!
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        //Kiểm tra an toàn, tránh crack game
        if (buildManager.GetTurretToBuild() == null) return;
        //Kiểm tra có chỗ xây không
        if (turret != null)
        {
            NodeUI.instance.SetTarget(this); // Gọi UI Manager / Call UI Manager
            return;

        }

        TurretBluePrint blueprint = buildManager.GetTurretToBuild();

        //Kiểm tra có thể xây dựng được không?
        if (!buildManager.CanBuild) return;


        //Kiểm tra limit
        if (!GameManager_2.instance.CanBuild())
        { 
            ShowErrorMessage("Limit Reached!");
            return;
        }


        if (!GameManager_2.instance.SpendGold(blueprint.cost))
        {
            ShowErrorMessage("Need More Gold");
            return;
        }


        //Xây dựng
        turret = Instantiate(blueprint.prefab, transform.position, transform.rotation);
        turretBlueprint = blueprint;
        //Cập nhật
        GameManager_2.instance.AddTurret();
    }

    //Sell
    public void SellTurret()
    {
        // Cộng lại nửa tiền / Refund half cost
        GameManager_2.instance.AddGold(turretBlueprint.cost / 2);

        // Trừ đi số lượng tháp hiện tại / Decrement current turret count
        GameManager_2.instance.RemoveTurret();

        // Xóa tháp / Destroy turret
        Destroy(turret);
        turretBlueprint = null;
    }



    //Effect
    private void OnMouseEnter()
    {
        // Chặn luôn cả hiệu ứng đổi màu (hover) nếu chuột đang ở trên UI
        // Also block the hover color effect if the mouse is over UI
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        if (buildManager.GetTurretToBuild() == null) return;
        rend.material.color = hoverColor;
    }
    private void OnMouseExit()
    {
        rend.material.color = startColor;
    }

    private void ShowErrorMessage(string message)
    {
        if (errorTextPrefab != null)
        {
            // Sinh ra thông báo ngay trên đầu Node
            GameObject textObj = Instantiate(errorTextPrefab, transform.position + Vector3.up * 1.5f, UnityEngine.Camera.main.transform.rotation);

            var tmp = textObj.GetComponentInChildren<TMPro.TextMeshPro>();
            if (tmp != null) tmp.text = message;

            // Chạy hiệu ứng mượt mà
            StartCoroutine(AnimateErrorText(textObj));
        }
    }
    private IEnumerator AnimateErrorText(GameObject obj)
    {
        float duration = 1.8f;
        float elapsed = 0f;
        Vector3 startPos = obj.transform.position;
        Vector3 endPos = startPos + Vector3.up * 1.0f; // Bay lên cao thêm 1 mét

        var tmp = obj.GetComponentInChildren<TMPro.TextMeshPro>();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;

            // Hiệu ứng bay lên (Smooth Step)
            obj.transform.position = Vector3.Lerp(startPos, endPos, percent);

            // Hiệu ứng mờ dần
            if (tmp != null)
            {
                Color c = tmp.color;
                c.a = Mathf.Lerp(1, 0, percent);
                tmp.color = c;
            }

            yield return null;
        }

        Destroy(obj); // Tự xóa để nhẹ máy
    }
}
