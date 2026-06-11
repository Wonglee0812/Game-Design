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
        [SerializeField] private LayerMask corpseLayer;
        [SerializeField] private LayerMask staffLayer;
        [SerializeField] private LayerMask buttonLayer;
        private Triage_StaffController selectedStaff = null;

        private UnityEngine.Camera mainCam;
        private Plane dragPlane;

        private Triage_DrugItem draggedDrug = null;
        private Rigidbody draggedCorpse = null;

        // ==========================================
        // [VI] Biến Transform để kiểm tra chống spam âm thanh
        // [EN] Transform variable to check and prevent audio spam
        // ==========================================
        private Transform currentHoverTarget = null;

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

        private void TryGrabObject()
        {
            Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, buttonLayer))
            {
                Triage_WorldButton btn = hit.collider.GetComponentInParent<Triage_WorldButton>();
                if (btn != null)
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.Play("grab");
                    btn.PressButton();
                    return;
                }
            }

            if (Physics.Raycast(ray, out hit, 100f, staffLayer))
            {
                Triage_StaffController staff = hit.collider.GetComponentInParent<Triage_StaffController>();
                if (staff != null && staff.isAvailable)
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.Play("grab");
                    if (selectedStaff != null) selectedStaff.SetSelected(false);

                    selectedStaff = staff;
                    selectedStaff.SetSelected(true);
                }
                return;
            }

            if (selectedStaff != null && Physics.Raycast(ray, out hit, 100f, patientLayer))
            {
                Triage_PatientBiology patient = hit.collider.GetComponentInParent<Triage_PatientBiology>();
                if (patient != null)
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.Play("grab");
                    selectedStaff.CommandToTreat(patient);
                    selectedStaff = null;
                    return;
                }
            }

            if (selectedStaff != null)
            {
                selectedStaff.SetSelected(false);
                selectedStaff = null;
            }

            if (Physics.Raycast(ray, out RaycastHit drugHit, 100f, drugLayer))
            {
                Triage_DrugItem drugDispenser = drugHit.collider.GetComponentInParent<Triage_DrugItem>();
                if (drugDispenser != null)
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.Play("grab");
                    draggedDrug = Instantiate(drugDispenser, drugDispenser.transform.position, drugDispenser.transform.rotation);
                    draggedDrug.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);

                    Rigidbody drugRb = draggedDrug.GetComponentInChildren<Rigidbody>();
                    if (drugRb != null) drugRb.isKinematic = true;

                    Collider[] drugCols = draggedDrug.GetComponentsInChildren<Collider>();
                    foreach (Collider c in drugCols) c.enabled = false;

                    SetupDragPlane(drugDispenser.transform.position.y);
                    return;
                }
            }

            if (Physics.Raycast(ray, out RaycastHit corpseHit, 100f, corpseLayer))
            {
                draggedCorpse = corpseHit.collider.GetComponentInParent<Rigidbody>();
                if (draggedCorpse != null)
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.Play("grab");
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
            if (AudioManager.Instance != null) AudioManager.Instance.Play("grab");

            Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, patientLayer))
            {
                Triage_PatientBiology targetPatient = hit.collider.GetComponentInParent<Triage_PatientBiology>();

                if (targetPatient != null)
                {
                    if (Triage_GameManager.Instance.SpendGold(draggedDrug.cost))
                    {
                        targetPatient.AdministerDrug(draggedDrug.drugType);
                        Debug.Log("Đã tiêm thuốc thành công cho: " + targetPatient.name);
                    }
                    else
                    {
                        Debug.Log("Not enough gold to use this drug!");
                    }
                }
            }

            Destroy(draggedDrug.gameObject);
            draggedDrug = null;
        }

        private void DropCorpse()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.Play("grab");

            if (draggedCorpse != null)
            {
                draggedCorpse.isKinematic = false;
                draggedCorpse.AddForce(UnityEngine.Camera.main.transform.forward * 5f, ForceMode.Impulse);
                draggedCorpse = null;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (draggedDrug != null)
            {
                Gizmos.color = new Color(0, 1, 0, 0.3f);
                Gizmos.DrawSphere(draggedDrug.transform.position, dropRadius);
            }
        }

        private void HandleTooltipHover()
        {
            if (draggedDrug != null || draggedCorpse != null)
            {
                currentHoverTarget = null;
                Triage_SymptomUI.Instance.HideUI();
                return;
            }

            Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            LayerMask interactableLayers = patientLayer | drugLayer | staffLayer | buttonLayer | corpseLayer;

            if (Physics.Raycast(ray, out hit, 100f, interactableLayers))
            {
                Transform hitTransform = hit.collider.transform;

                // ==========================================
                // [VI] Lấy Transform cao nhất để tránh trùng lặp giữa các bộ phận cơ thể
                // [EN] Get the root Transform to avoid duplication between body parts
                // ==========================================
                Transform rootTransform = hitTransform.root;

                bool isValidTarget = false;

                if (((1 << hitTransform.gameObject.layer) & patientLayer) != 0)
                {
                    Triage_PatientBiology patient = hitTransform.GetComponentInParent<Triage_PatientBiology>();
                    if (patient != null)
                    {
                        isValidTarget = true;
                        Triage_SymptomUI.Instance.ShowPatientUI(patient);
                    }
                }
                else if (((1 << hitTransform.gameObject.layer) & drugLayer) != 0)
                {
                    Triage_DrugItem drug = hitTransform.GetComponentInParent<Triage_DrugItem>();
                    if (drug != null)
                    {
                        isValidTarget = true;
                        Triage_SymptomUI.Instance.ShowDrugUI(drug.drugType.ToString(), drug.cost, "Cures specific symptoms.");
                    }
                }
                else if (((1 << hitTransform.gameObject.layer) & staffLayer) != 0)
                {
                    Triage_StaffController staff = hitTransform.GetComponentInParent<Triage_StaffController>();
                    if (staff != null)
                    {
                        isValidTarget = true;
                        Triage_SymptomUI.Instance.ShowStaffUI(10f, staff.errorRate);
                    }
                }
                else if (((1 << hitTransform.gameObject.layer) & buttonLayer) != 0)
                {
                    Triage_WorldButton btn = hitTransform.GetComponentInParent<Triage_WorldButton>();
                    if (btn != null) isValidTarget = true;
                }
                else if (((1 << hitTransform.gameObject.layer) & corpseLayer) != 0)
                {
                    isValidTarget = true;
                }

                // ==========================================
                // [VI] Nếu mục tiêu hợp lệ VÀ khác với mục tiêu frame trước -> Phát âm thanh
                // [EN] If target is valid AND different from previous frame target -> Play sound
                // ==========================================
                if (isValidTarget)
                {
                    if (currentHoverTarget != rootTransform)
                    {
                        currentHoverTarget = rootTransform;
                        if (AudioManager.Instance != null) AudioManager.Instance.Play("popup");
                    }
                }
                else
                {
                    // [VI] Xử lý lỗi trúng vật thể có layer nhưng thiếu script
                    // [EN] Handle error of hitting object with layer but missing script
                    currentHoverTarget = null;
                    Triage_SymptomUI.Instance.HideUI();
                }
            }
            else
            {
                currentHoverTarget = null;
                Triage_SymptomUI.Instance.HideUI();
            }
        }
    }
}