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

        // [VI] Biến kiểm tra xem người chơi có đang rọi chuột vào không
        // [EN] Variable to check if the player is currently hovering the mouse
        private bool isPausedByPlayer = false;

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
                CheckExitReached();
            }
        }

        private void HandleWandering()
        {
            // [VI] NẾU ĐANG BỊ NGƯỜI CHƠI HOVER -> KHÔNG LÀM GÌ CẢ (ĐỨNG IM)
            // [EN] IF HOVERED BY PLAYER -> DO NOTHING (STAND STILL)
            if (!agent.isOnNavMesh || isPausedByPlayer) return;

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

        // --- TƯƠNG TÁC VỚI CHUỘT (HOVER TO STOP) ---
        // --- MOUSE INTERACTION (HOVER TO STOP) ---

        private void OnMouseEnter()
        {
            if (currentState == MovementState.Dead) return;
            isPausedByPlayer = true;
            if (agent.isOnNavMesh) agent.isStopped = true; // [VI] Phanh gấp / [EN] Hard brake

            // [VI] Bật UI Sinh hiệu để người chơi xem bệnh
            // [EN] Turn on Vitals UI so the player can check the disease
            Triage_SymptomUI.Instance?.ShowUI(GetComponent<Triage_PatientBiology>());
        }

        private void OnMouseExit()
        {
            if (currentState == MovementState.Dead) return;
            isPausedByPlayer = false;
            if (agent.isOnNavMesh) agent.isStopped = false; // [VI] Cho đi tiếp / [EN] Allow moving again

            // [VI] Tắt UI Sinh hiệu khi người chơi quay ra lấy thuốc
            // [EN] Turn off Vitals UI when player turns away to grab medicine
            Triage_SymptomUI.Instance?.HideUI();
        }

        // --- TƯƠNG TÁC VỚI MÔI TRƯỜNG (START DISEASE) ---
        // --- ENVIRONMENT INTERACTION (START DISEASE) ---

        private void OnTriggerEnter(Collider other)
        {
            // [VI] Khi cơ thể chạm vào khu vực có Tag là "WaitingZone"
            // [EN] When the body touches the area Tagged "WaitingZone"
            if (other.CompareTag("WaitingZone") && currentState == MovementState.Waiting)
            {
                GetComponent<Triage_PatientBiology>()?.StartDisease();
            }
        }

        // --- PUBLIC FUNCTIONS CALLED BY BIOLOGY SCRIPT ---

        public void GoToExit()
        {
            currentState = MovementState.Leaving;
            isPausedByPlayer = false;
            if (agent.isOnNavMesh && exitPoint != null)
            {
                agent.isStopped = false;
                agent.SetDestination(exitPoint.position);
                agent.speed += 2f;
            }
        }

        private void CheckExitReached()
        {
            if (!agent.pathPending && agent.remainingDistance <= 1.5f)
            {
                Destroy(gameObject);
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

            // [VI] ĐỔI THÀNH LAYER "Corpse" ĐỂ CHUỘT DỄ NHẶT LÊN
            // [EN] CHANGE TO "Corpse" LAYER FOR EASY MOUSE PICKUP
            gameObject.layer = LayerMask.NameToLayer("Corpse");

            Triage_SymptomUI.Instance?.HideUI();
        }
    }
}