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

            if (Physics.Raycast(ray, out RaycastHit drugHit, 100f, drugLayer))
            {
                Triage_DrugItem drugDispenser = drugHit.collider.GetComponent<Triage_DrugItem>();
                if (drugDispenser != null)
                {
                    // [VI] KIỂM TRA TIỀN TRƯỚC KHI LẤY THUỐC / [EN] CHECK GOLD BEFORE TAKING DRUG
                    if (Triage_GameManager.Instance.SpendGold(drugDispenser.cost))
                    {
                        draggedDrug = Instantiate(drugDispenser, drugDispenser.transform.position, drugDispenser.transform.rotation);
                        SetupDragPlane(drugDispenser.transform.position.y);
                    }
                    else
                    {
                        Debug.Log("Not enough gold to buy this drug!");
                        // [VI] Có thể thêm âm thanh "Tít tít" báo lỗi ở đây / [EN] Can add a "Beep" error sound here
                    }
                    return;
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
                    closestPatient.AdministerDrug(draggedDrug.drugType);
                }
            }

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
    }
}