using UnityEngine;
using UnityEngine.AI;

namespace Portfolio.TriageManagement
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Triage_PatientMovement : MonoBehaviour
    {
        public enum MovementState { Waiting, Leaving, Dead }
        public MovementState currentState = MovementState.Waiting;

        [Header("Zone References")]
        public Collider waitingZone;
        public Transform exitPoint;

        [Header("Wander Settings")]
        public float wanderInterval = 3f;

        private NavMeshAgent agent;
        private Rigidbody rb;
        private float timer;

        private bool isPausedByPlayer = false;
        public bool isBeingTreated = false;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            if (waitingZone == null)
            {
                GameObject zoneObj = GameObject.FindWithTag("WaitingZone");
                if (zoneObj != null) waitingZone = zoneObj.GetComponent<Collider>();
            }
            if (exitPoint == null)
            {
                GameObject exitObj = GameObject.FindWithTag("ExitPoint");
                if (exitObj != null) exitPoint = exitObj.transform;
            }
            timer = wanderInterval;
        }

        private void Update()
        {
            if (currentState == MovementState.Dead) return;

            if (currentState == MovementState.Waiting)
            {
                HandleWandering();
            }
            else if (currentState == MovementState.Leaving)
            {
                // [VI] ÉP BUỘC ĐI VỀ CỬA: Luôn mở khóa và kiểm tra điểm đến liên tục
                // [EN] FORCE GOING TO EXIT: Always unlock and continuously check destination
                if (agent.isOnNavMesh && exitPoint != null)
                {
                    agent.isStopped = false; // [VI] Đảm bảo không bị dính phanh / [EN] Ensure no brake is stuck
                }
                CheckExitReached();
            }
        }

        private void HandleWandering()
        {
            if (!agent.isOnNavMesh || isPausedByPlayer || isBeingTreated) return;

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                timer += Time.deltaTime;
                if (timer >= wanderInterval && waitingZone != null)
                {
                    Vector3 randomPoint = GetRandomPointInBounds(waitingZone.bounds);
                    agent.SetDestination(randomPoint);
                    timer = 0;
                }
            }
        }

        private Vector3 GetRandomPointInBounds(Bounds bounds)
        {
            return new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                transform.position.y,
                Random.Range(bounds.min.z, bounds.max.z)
            );
        }

        // --- TƯƠNG TÁC CHUỘT ĐÃ ĐƯỢC BẢO VỆ CHẶT CHẼ HƠN ---
        // --- MOUSE INTERACTION IS NOW STRICTLY PROTECTED ---

        private void OnMouseEnter()
        {
            // [VI] Chỉ cho phép ngó thông tin nếu bệnh nhân đang ở trạng thái Chờ
            // [EN] Only allow peeking at info if the patient is in the Waiting state
            if (currentState != MovementState.Waiting) return;

            isPausedByPlayer = true;
            if (agent.isOnNavMesh) agent.isStopped = true;
        }

        private void OnMouseExit()
        {
            if (currentState != MovementState.Waiting) return;

            isPausedByPlayer = false;

            // [VI] QUAN TRỌNG: Nếu đang bị bác sĩ khám thì rút chuột ra vẫn phải đứng im!
            // [EN] IMPORTANT: If being examined by a doctor, withdrawing the mouse must still keep them stopped!
            if (agent.isOnNavMesh && !isBeingTreated)
            {
                agent.isStopped = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("WaitingZone") && currentState == MovementState.Waiting)
            {
                GetComponent<Triage_PatientBiology>()?.StartDisease();
            }
        }

        public void GoToExit()
        {
            currentState = MovementState.Leaving;
            isPausedByPlayer = false;
            isBeingTreated = false;

            if (agent.isOnNavMesh && exitPoint != null)
            {
                agent.isStopped = false;
                agent.speed += 2f;
                agent.SetDestination(exitPoint.position);
            }
        }

        private void CheckExitReached()
        {
            if (exitPoint != null)
            {
                if (Vector3.Distance(transform.position, exitPoint.position) <= 2f)
                {
                    Destroy(gameObject);
                }
            }
        }

        public void BecomeCorpse()
        {
            currentState = MovementState.Dead;
            agent.enabled = false;

            rb = gameObject.GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

            rb.isKinematic = false;
            rb.mass = 50f;

            Vector3 pushForce = new Vector3(Random.Range(-5f, 5f), 2f, Random.Range(-5f, 5f));
            rb.AddForce(pushForce, ForceMode.Impulse);
            rb.AddTorque(new Vector3(Random.Range(-50f, 50f), 0, Random.Range(-50f, 50f)));

            gameObject.layer = LayerMask.NameToLayer("Corpse");
            Triage_SymptomUI.Instance?.HideUI();
        }
    }
}