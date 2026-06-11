using UnityEngine;
using TMPro;

public class NodeUI : MonoBehaviour
{
    public static NodeUI instance;

    [Header("UI Reference")]
    public GameObject uiPanel; // Kéo khối UI chính vào đây / Drag main UI Panel here
    public TextMeshProUGUI sellAmountText; // Chữ hiển thị số tiền nhận lại / Text showing refund amount

    private Node target; // Lưu Node đang được chọn / Store currently selected Node


    private void Awake()
    {
        instance = this;
        uiPanel.SetActive(false); // Ẩn UI lúc mới vào / Hide UI at start
    }



    // Node sẽ gọi hàm này khi bị Click / Node calls this when Clicked
    public void SetTarget(Node _target)
    {
        target = _target;

        // Hiển thị số tiền bán (Chia 2) / Show sell amount (Divided by 2)
        sellAmountText.text = "+ $" + (target.turretBlueprint.cost / 2);

        uiPanel.SetActive(true);
    }

    public void Hide()
    {
        uiPanel.SetActive(false);
        target = null;
    }



    // --- CÁC HÀM GIAO TIẾP VỚI NÚT BẤM TRÊN UI / FUNCTIONS FOR UI BUTTONS ---
    public void SetRank(int rank)
    {
        if (target != null && target.turret != null)
        {
            target.turret.GetComponent<Turret>().SetCardRank(rank);
        }
    }

    public void SetSuit(string suit)
    {
        if (target != null && target.turret != null)
        {
            target.turret.GetComponent<Turret>().SetCardSuit(suit);
        }
    }

    public void Sell()
    {
        target.SellTurret(); // Gọi hàm xóa ở Node / Call delete on Node
        Hide(); // Bán xong thì ẩn bảng đi / Hide panel after selling
    }
}