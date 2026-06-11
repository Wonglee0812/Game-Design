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

        // ==========================================
        // [VI] CẤU HÌNH COLLIDER KHI CHẾT
        // [EN] DEATH COLLIDER CONFIGURATION
        // ==========================================
        [Header("Death Physics")]
        [Tooltip("Kéo CapsuleCollider (đứng) vào đây / Drag standing CapsuleCollider here")]
        public Collider standingCollider;

        [Tooltip("Kéo BoxCollider/Capsule (nằm) vào đây / Drag lying BoxCollider/Capsule here")]
        public Collider corpseCollider;

        [Header("Audio Settings")]
        [Range(0f, 100f)] public float walkSoundChance = 30f;
        public float minVillageSoundInterval = 8f;
        public float maxVillageSoundInterval = 20f;

        private float villageSoundTimer = 0f;
        private float currentVillageSoundTarget = 0f;

        private NavMeshAgent agent;
        private Rigidbody rb;
        private float timer;
        private Animator animator;

        private bool isPausedByPlayer = false;
        public bool isBeingTreated = false;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();

            // [VI] Tự động tìm collider đứng nếu chưa gắn / [EN] Auto-find standing collider if not assigned
            if (standingCollider == null) standingCollider = GetComponent<CapsuleCollider>();

            // [VI] Corpse collider PHẢI được kéo tay vào Inspector
            // [EN] Corpse collider MUST be manually assigned in the Inspector

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

            currentVillageSoundTarget = Random.Range(minVillageSoundInterval, maxVillageSoundInterval);
        }

        private void Update()
        {
            if (currentState == MovementState.Dead)
            {
                if (animator != null) animator.SetBool("isWalk", false);
                return;
            }

            UpdateAnimationState();

            if (currentState == MovementState.Waiting)
            {
                HandleWandering();
                HandleVillageSound();
            }
            else if (currentState == MovementState.Leaving)
            {
                if (agent.isOnNavMesh && exitPoint != null)
                {
                    agent.isStopped = false;
                }
                CheckExitReached();
            }
        }

        private void UpdateAnimationState()
        {
            if (animator != null && agent.isOnNavMesh)
            {
                bool isWalking = agent.velocity.sqrMagnitude > 0.1f && !agent.isStopped;
                animator.SetBool("isWalk", isWalking);
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

        private void HandleVillageSound()
        {
            villageSoundTimer += Time.deltaTime;
            if (villageSoundTimer >= currentVillageSoundTarget)
            {
                if (AudioManager.Instance != null) AudioManager.Instance.Play("village");

                villageSoundTimer = 0f;
                currentVillageSoundTarget = Random.Range(minVillageSoundInterval, maxVillageSoundInterval);
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

        private void OnMouseEnter()
        {
            if (currentState != MovementState.Waiting) return;

            isPausedByPlayer = true;
            if (agent.isOnNavMesh) agent.isStopped = true;
        }

        private void OnMouseExit()
        {
            if (currentState != MovementState.Waiting) return;

            isPausedByPlayer = false;

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

            if (animator != null) animator.SetTrigger("isCure");

            if (exitPoint == null)
            {
                Debug.LogError("LỖI NGHIÊM TRỌNG: Bệnh nhân không biết cửa ra ở đâu (ExitPoint đang bị NULL)!");
                return;
            }

            if (agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.ResetPath();
                agent.speed += 2f;

                agent.SetDestination(exitPoint.position);
                Debug.Log("Bệnh nhân đã nhận lệnh đi về cửa thành công!");
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

        // =========================================================================
        // [VI] CẬP NHẬT HÀM XỬ LÝ KHI CHẾT / [EN] UPDATED DEATH HANDLING FUNCTION
        // =========================================================================
        public void BecomeCorpse()
        {
            currentState = MovementState.Dead;
            agent.enabled = false;

            if (animator != null) animator.SetTrigger("isDie");

            if (standingCollider != null) standingCollider.enabled = false;
            if (corpseCollider != null) corpseCollider.enabled = true;

            rb = gameObject.GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

            rb.isKinematic = false;
            rb.mass = 50f;

            Vector3 pushForce = new Vector3(Random.Range(-5f, 5f), 2f, Random.Range(-5f, 5f));
            rb.AddForce(pushForce, ForceMode.Impulse);
            rb.AddTorque(new Vector3(Random.Range(-50f, 50f), 0, Random.Range(-50f, 50f)));

            // ==========================================
            // [VI] FIX: Đổi Layer cho cả Parent LẪN cái Collider đang nằm dưới đất
            // [EN] FIX: Change Layer for BOTH the Parent AND the Collider lying on the ground
            // ==========================================
            gameObject.layer = LayerMask.NameToLayer("Corpse");
            if (corpseCollider != null)
            {
                corpseCollider.gameObject.layer = LayerMask.NameToLayer("Corpse");
            }

            Triage_SymptomUI.Instance?.HideUI();
        }

        public void PlayWalkSound()
        {
            if (currentState == MovementState.Dead) return;

            float roll = Random.Range(0f, 100f);
            if (roll <= walkSoundChance)
            {
                if (AudioManager.Instance != null) AudioManager.Instance.Play("walk");
            }
        }
    }
}