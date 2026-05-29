using UnityEngine;

[CreateAssetMenu(fileName = "NewVehicle", menuName = "PartyGame/Vehicle Data")]
public class VehicleDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public string vehicleName = "New Car";
    public GameObject vehiclePrefab;
    public Sprite vehicleIcon; // Ảnh hiển thị trên UI / Image for UI

    [Header("Auto-Calculated Stats")]
    public float mass;
    public float speed; // (Bạn cần gán tay hoặc thêm code đọc từ CarController của bạn / Assign manually or add code to read from your CarController)

    // [VI] Hàm này tự động chạy trong Editor mỗi khi có thay đổi
    // [EN] This function runs automatically in the Editor on any change
    private void OnValidate()
    {
        if (vehiclePrefab != null)
        {
            // Tự động lấy khối lượng / Auto-get mass
            Rigidbody rb = vehiclePrefab.GetComponent<Rigidbody>();
            if (rb != null)
            {
                mass = rb.mass;
            }

            // Note: Để lấy Speed, bạn thêm code GetComponent<Tên_Script_Chạy_Xe_Của_Bạn>()
            // Note: To get Speed, add code GetComponent<Your_Car_Movement_Script_Name>()
        }
    }
}