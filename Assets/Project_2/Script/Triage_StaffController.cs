using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace Portfolio.TriageManagement
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Triage_StaffController : MonoBehaviour
    {
        [Header("Stats")]
        public float treatmentTime = 3f; // [VI] Thời gian đứng chữa bệnh / [EN] Time spent treating
        public float cooldownTime = 5f;
        [Range(0f, 1f)] public float errorRate = 0.2f; // [VI] 20% tỷ lệ sai thuốc / [EN] 20% wrong drug chance

        [Header("References")]
        public GameObject selectionIndicator; // [VI] Vòng sáng dưới chân / [EN] Highlight circle under feet

        private NavMeshAgent agent;
        private Triage_PatientBiology targetPatient;
        private bool isTreating = false;
        public bool isAvailable { get; private set; } = true;
        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (!isAvailable && selected) return;
            if (selectionIndicator != null) selectionIndicator.SetActive(selected);
        }

        public void CommandToTreat(Triage_PatientBiology patient)
        {
            if (!isAvailable) return;

            isAvailable = false;
            targetPatient = patient;
            agent.SetDestination(patient.transform.position);
            SetSelected(false);

            // ==========================================
            // [VI] KHÓA CHÂN BỆNH NHÂN NGAY LẬP TỨC KHI VỪA ĐƯỢC CHỌN
            // [EN] LOCK PATIENT'S FEET IMMEDIATELY UPON BEING SELECTED
            // ==========================================
            Triage_PatientMovement patientMovement = targetPatient.GetComponent<Triage_PatientMovement>();
            if (patientMovement != null)
            {
                patientMovement.isBeingTreated = true; // [VI] Khóa script đi dạo / [EN] Lock wandering script

                NavMeshAgent pAgent = targetPatient.GetComponent<NavMeshAgent>();
                if (pAgent != null && pAgent.isOnNavMesh)
                {
                    pAgent.isStopped = true; // [VI] Kích hoạt phanh NavMesh / [EN] Activate NavMesh brake
                }
            }
        }

        private void Update()
        {
            // [VI] Nếu có mục tiêu, chưa bắt đầu chữa, và đã đến nơi
            // [EN] If there is a target, hasn't started treating, and has arrived
            if (targetPatient != null && !isTreating)
            {
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
                {
                    StartCoroutine(TreatRoutine());
                }
            }
        }

        private IEnumerator TreatRoutine()
        {
            isTreating = true;
            agent.isStopped = true; // [VI] Đứng lại để chữa / [EN] Stop to treat
            Triage_PatientMovement patientMovement = targetPatient.GetComponent<Triage_PatientMovement>();
            if (patientMovement != null)
            {
                patientMovement.isBeingTreated = true; // [VI] Bật khóa đi dạo / [EN] Turn on wander lock
                NavMeshAgent pAgent = targetPatient.GetComponent<NavMeshAgent>();
                if (pAgent != null && pAgent.isOnNavMesh) pAgent.isStopped = true;
            }
            yield return new WaitForSeconds(treatmentTime);

            if (targetPatient != null)
            {
                float roll = Random.value;
                if (roll > errorRate)
                {
                    // [VI] Chữa đúng: Đưa đúng loại thuốc bệnh nhân cần / [EN] Correct cure: Give the exact drug needed
                    targetPatient.AdministerDrug(targetPatient.currentDisease.correctCure);
                }
                else
                {
                    // [VI] Chữa sai: Đưa một Enum rác (999) để kích hoạt hiệu ứng chết do sai thuốc
                    // [EN] Wrong cure: Give a garbage Enum (999) to trigger the wrong-drug death effect
                    targetPatient.AdministerDrug((DrugType)999);
                    Debug.Log("Staff made a fatal mistake!");
                }
                if (patientMovement != null && patientMovement.currentState != Triage_PatientMovement.MovementState.Dead)
                {
                    patientMovement.isBeingTreated = false;
                    NavMeshAgent pAgent = targetPatient.GetComponent<NavMeshAgent>();
                    if (pAgent != null && pAgent.isOnNavMesh) pAgent.isStopped = false;
                }
            }

            targetPatient = null;
            isTreating = false;
            agent.isStopped = false; // [VI] Sẵn sàng đi tiếp / [EN] Ready to move again
            yield return new WaitForSeconds(cooldownTime);

            // [VI] Hết Cooldown mới được nhận lệnh tiếp / [EN] Only available for commands after Cooldown
            isAvailable = true;

        }
    }
}