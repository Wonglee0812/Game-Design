using UnityEngine;
using UnityEngine.InputSystem;

namespace Portfolio.TriageManagement
{
    public class Triage_DragDropManager : MonoBehaviour
    {
        [Header("Lift & Drag Settings")]
        [SerializeField] private float liftHeight = 2f;
        [SerializeField] private float dragSpeed = 20f;
        [SerializeField] private float dropRadius = 4f;

        [Header("Layers")]
        [SerializeField] private LayerMask patientLayer;
        [SerializeField] private LayerMask drugLayer;
        [SerializeField] private LayerMask corpseLayer; // [VI] Thêm Layer cho Xác chết / [EN] Add Layer for Corpses
        [SerializeField] private LayerMask staffLayer; // [VI] Thêm Layer này trên đầu / [EN] Add this Layer at the top
        [SerializeField] private LayerMask buttonLayer;
        private Triage_StaffController selectedStaff = null; // [VI] Nhân viên đang được chọn / [EN] Currently selected staff

        private UnityEngine.Camera mainCam;
        private Plane dragPlane;

        // [VI] Quản lý Thuốc / [EN] Drug Management
        private Triage_DrugItem draggedDrug = null;

        // [VI] Quản lý Xác chết / [EN] Corpse Management
        private Rigidbody draggedCorpse = null;

        private void Awake()
        {
            mainCam = UnityEngine.Camera.main;
        }

        private void Update()
        {
            HandleTooltipHover();
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                TryGrabObject();
            }
            else if (Mouse.current.leftButton.isPressed)
            {
                if (draggedDrug != null || draggedCorpse != null)
                {
                    DragObject();
                }
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                if (draggedDrug != null) DropDrug();
                else if (draggedCorpse != null) DropCorpse();
            }
        }

        // [VI] Trong script Triage_DragDropManager.cs / [EN] In script Triage_DragDropManager.cs
        private void TryGrabObject()
        {
            Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            // ==========================================
            // [VI] 2. THÊM ĐOẠN CHECK NÚT BẤM VÀO NGAY ĐẦU HÀM
            // [EN] 2. ADD THE BUTTON CHECK RIGHT AT THE START OF THE FUNCTION
            // ==========================================
            if (Physics.Raycast(ray, out hit, 100f, buttonLayer))
            {
                Triage_WorldButton btn = hit.collider.GetComponent<Triage_WorldButton>();
                if (btn != null)
                {
                    btn.PressButton();
                    return; // [VI] Đã bấm nút thì dừng, không cầm nắm đồ vật nữa / [EN] Stopped to press button, don't grab objects
                }
            }

            // [VI] 1. CLICK CHỌN NHÂN VIÊN / [EN] 1. CLICK TO SELECT STAFF
            if (Physics.Raycast(ray, out hit, 100f, staffLayer))
            {
                Triage_StaffController staff = hit.collider.GetComponent<Triage_StaffController>();
                if (staff != null && staff.isAvailable)
                {
                    if (selectedStaff != null) selectedStaff.SetSelected(false);

                    selectedStaff = staff;
                    selectedStaff.SetSelected(true);
                }
                return;
            }

            // [VI] 2. NẾU ĐANG CHỌN NHÂN VIÊN MÀ CLICK VÀO BỆNH NHÂN -> RA LỆNH CHỮA
            // [EN] 2. IF STAFF IS SELECTED AND CLICKED PATIENT -> COMMAND TO TREAT
            if (selectedStaff != null && Physics.Raycast(ray, out hit, 100f, patientLayer))
            {
                Triage_PatientBiology patient = hit.collider.GetComponent<Triage_PatientBiology>();
                if (patient != null)
                {
                    selectedStaff.CommandToTreat(patient);
                    selectedStaff = null; // [VI] Xóa lựa chọn sau khi ra lệnh / [EN] Clear selection after commanding
                    return;
                }
            }

            // [VI] 3. Bỏ chọn nhân viên nếu click ra ngoài (Nhưng không trúng Thuốc/Xác chết)
            // [EN] 3. Deselect staff if clicking outside (But not hitting Drugs/Corpses)
            if (selectedStaff != null)
            {
                selectedStaff.SetSelected(false);
                selectedStaff = null;
            }

            if (Physics.Raycast(ray, out RaycastHit drugHit, 100f, drugLayer))
            {
                Triage_DrugItem drugDispenser = drugHit.collider.GetComponent<Triage_DrugItem>();
                if (drugDispenser != null)
                {
                    // [VI] Lấy thuốc thoải mái, chưa trừ tiền vội
                    // [EN] Grab the drug freely, don't deduct money yet
                    draggedDrug = Instantiate(drugDispenser, drugDispenser.transform.position, drugDispenser.transform.rotation);
                    SetupDragPlane(drugDispenser.transform.position.y);

                    return; // [VI] Kết thúc hàm tại đây / [EN] End function here
                }
            }

            if (Physics.Raycast(ray, out RaycastHit corpseHit, 100f, corpseLayer))
            {
                draggedCorpse = corpseHit.collider.GetComponent<Rigidbody>();
                if (draggedCorpse != null)
                {
                    draggedCorpse.isKinematic = true;
                    SetupDragPlane(draggedCorpse.transform.position.y);
                }
            }
        }

        private void SetupDragPlane(float startY)
        {
            dragPlane = new Plane(Vector3.up, new Vector3(0, startY + liftHeight, 0));
        }

        private void DragObject()
        {
            Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (dragPlane.Raycast(ray, out float distance))
            {
                Vector3 targetPoint = ray.GetPoint(distance);

                // [VI] Di chuyển vật thể đang được cầm (Thuốc hoặc Xác) / [EN] Move the held object (Drug or Corpse)
                if (draggedDrug != null)
                {
                    draggedDrug.transform.position = Vector3.Lerp(draggedDrug.transform.position, targetPoint, Time.deltaTime * dragSpeed);
                }
                else if (draggedCorpse != null)
                {
                    draggedCorpse.transform.position = Vector3.Lerp(draggedCorpse.transform.position, targetPoint, Time.deltaTime * dragSpeed);
                }
            }
        }

        private void DropDrug()
        {
            Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
            Collider[] hitPatients = Physics.OverlapSphere(draggedDrug.transform.position, dropRadius, patientLayer);

            if (hitPatients.Length > 0)
            {
                Triage_PatientBiology closestPatient = null;
                float minDistance = float.MaxValue;

                foreach (Collider hit in hitPatients)
                {
                    float distance = Vector3.Distance(draggedDrug.transform.position, hit.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestPatient = hit.GetComponent<Triage_PatientBiology>();
                    }
                }

                if (closestPatient != null)
                {
                    // [VI] KIỂM TRA VÀ TRỪ TIỀN NGAY TRƯỚC KHI TIÊM THUỐC
                    // [EN] CHECK AND DEDUCT MONEY RIGHT BEFORE ADMINISTERING DRUG
                    if (Triage_GameManager.Instance.SpendGold(draggedDrug.cost))
                    {
                        closestPatient.AdministerDrug(draggedDrug.drugType);
                    }
                    else
                    {
                        Debug.Log("Not enough gold to use this drug!");
                        // [VI] Bạn có thể thêm UI chớp đỏ hoặc âm thanh báo lỗi ở đây
                        // [EN] You can add a flashing red UI or an error sound here
                    }
                }
            }

            // [VI] Dù tiêm thành công, không đủ tiền, hay thả ra ngoài không khí thì cũng hủy viên thuốc đang cầm trên tay đi
            // [EN] Whether successful, insufficient funds, or dropped in thin air, destroy the dragged drug
            Destroy(draggedDrug.gameObject);
            draggedDrug = null;
        }

        private void DropCorpse()
        {
            // [VI] TRẢ LẠI VẬT LÝ: Bật lại trọng lực để xác rớt uỵch xuống góc phòng
            // [EN] RETURN PHYSICS: Re-enable gravity so the corpse thuds down into the room corner
            draggedCorpse.isKinematic = false;

            // [VI] Thêm một lực ném nhỏ để tạo cảm giác "vứt" đi cho đã tay
            // [EN] Add a small throw force to create a satisfying "tossing" feel
            draggedCorpse.AddForce(UnityEngine.Camera.main.transform.forward * 5f, ForceMode.Impulse);

            draggedCorpse = null;
        }

        private void OnDrawGizmosSelected()
        {
            if (draggedDrug != null)
            {
                Gizmos.color = new Color(0, 1, 0, 0.3f);
                Gizmos.DrawSphere(draggedDrug.transform.position, dropRadius);
            }
        }
        // [VI] VIẾT THÊM HÀM MỚI NÀY VÀO TRONG SCRIPT / [EN] ADD THIS NEW FUNCTION INTO THE SCRIPT
        private void HandleTooltipHover()
        {
            // [VI] Nếu đang cầm đồ (kéo thả) thì ẩn hết Tooltip cho đỡ rối mắt
            // [EN] If holding an object (dragging), hide all Tooltips to reduce clutter
            if (draggedDrug != null || draggedCorpse != null)
            {
                Triage_SymptomUI.Instance.HideUI();
                return;
            }

            Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            // [VI] 1. Quét trúng Bệnh nhân / [EN] 1. Raycast hits Patient
            if (Physics.Raycast(ray, out hit, 100f, patientLayer))
            {
                Triage_PatientBiology patient = hit.collider.GetComponent<Triage_PatientBiology>();
                if (patient != null)
                {
                    Triage_SymptomUI.Instance.ShowPatientUI(patient);
                    return; // [VI] Dừng ở đây để không hiện đè cái khác / [EN] Stop here to avoid overlapping
                }
            }

            // [VI] 2. Quét trúng Thuốc / [EN] 2. Raycast hits Drug
            if (Physics.Raycast(ray, out hit, 100f, drugLayer))
            {
                Triage_DrugItem drug = hit.collider.GetComponent<Triage_DrugItem>();
                if (drug != null)
                {
                    // [VI] Gọi tên thuốc, giá tiền, và (tùy chọn) tự thêm mô tả
                    // [EN] Call drug name, price, and (optionally) add description
                    Triage_SymptomUI.Instance.ShowDrugUI(
                        drug.drugType.ToString(), // Chuyển ENUM thành chữ (VD: "Adrenaline")
                        drug.cost,
                        "Cures specific symptoms." // Bạn có thể thêm biến string description vào script Triage_DrugItem sau
                    );
                    return;
                }
            }

            // [VI] Thêm đoạn kiểm tra tia Raycast trúng Staff vào HandleTooltipHover
            // [EN] Add the Staff Raycast check into HandleTooltipHover
            if (Physics.Raycast(ray, out hit, 100f, staffLayer))
            {
                Triage_StaffController staff = hit.collider.GetComponent<Triage_StaffController>();
                if (staff != null)
                {
                    // [VI] Truyền mức lương và tỷ lệ sai sót vào / [EN] Pass wage and error rate
                    Triage_SymptomUI.Instance.ShowStaffUI(10f, staff.errorRate);
                    return;
                }
            }

            // [VI] 3. Không trúng gì cả -> Tắt Tooltip
            // [EN] 3. Hit nothing -> Hide Tooltip
            Triage_SymptomUI.Instance.HideUI();
        }
    }
}