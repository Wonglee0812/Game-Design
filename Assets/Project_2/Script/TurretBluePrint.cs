using UnityEngine;

// Dòng này giúp bạn tạo file data bằng cách click chuột phải trong Project
// This line allows you to create data files by right-clicking in the Project
[CreateAssetMenu(fileName = "New Turret Data", menuName = "Tower Defense/Turret Blueprint")]
public class TurretBluePrint : ScriptableObject
{
    public string turretName; // Tên hiển thị (Tùy chọn) / Display name (Optional)
    public GameObject prefab; // Prefab của tháp / Turret Prefab
    public int cost;          // Giá tiền / Cost
}