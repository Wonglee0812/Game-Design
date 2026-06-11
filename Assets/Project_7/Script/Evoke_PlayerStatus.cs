using UnityEngine;
using System.Collections.Generic;
using System.Collections; // [VI] Bắt buộc phải có để dùng Coroutine / [EN] Required to use Coroutines

namespace EvokeProject
{
    public class Evoke_PlayerStatus : MonoBehaviour
    {
        [Header("Path Building Settings")]
        public GameObject pathBlockPrefab;
        [Tooltip("Đi bao xa thì rớt 1 cục? (Khuyên dùng: 0.5) / How far to walk before dropping 1 block? (Recommended: 0.5)")]
        public float dropInterval = 0.5f;
        [Tooltip("Độ teo nhỏ cho MỖI LẦN rớt cục đen / Shrink amount PER dropped block")]
        public float shrinkPerDrop = 0.05f;
        public float minScale = 0.1f;

        [Header("Respawn & Effects")]
        public Transform respawnPoint;
        // ==========================================
        // [VI] THÊM BIẾN HIỆU ỨNG CHẾT (KÉO PREFAB HẠT VÀO ĐÂY)
        // [EN] ADD DEATH EFFECT VARIABLE (DRAG PARTICLE PREFAB HERE)
        // ==========================================
        public GameObject deathEffectPrefab;

        private Vector3 spawnScale;
        private Vector3 lastDropPosition;
        private bool isOnSafeGround = true;

        // [VI] Cờ đánh dấu đang chết để không chạy lặp lại / [EN] Flag to mark dying state to prevent looping
        private bool isDying = false;

        private List<GameObject> currentRunBlocks = new List<GameObject>();

        private void Start()
        {
            spawnScale = transform.localScale;
            lastDropPosition = transform.position;

            TrailRenderer tr = GetComponent<TrailRenderer>();
            if (tr != null) tr.enabled = false;
        }

        private void Update()
        {
            // [VI] Nếu đang trong quá trình chết thì dừng mọi hoạt động update khác
            // [EN] If in the dying process, stop all other update activities
            if (isDying) return;

            CheckGroundStatus();

            if (!isOnSafeGround)
            {
                float distanceMoved = Vector3.Distance(transform.position, lastDropPosition);

                if (distanceMoved >= dropInterval)
                {
                    DropPathBlock();
                    ShrinkPlayer();

                    lastDropPosition = transform.position;

                    if (transform.localScale.x <= minScale)
                    {
                        // [VI] Thay vì gọi hàm chết ngay lập tức, ta gọi Coroutine để chờ
                        // [EN] Instead of calling death immediately, call Coroutine to wait
                        StartCoroutine(DieRoutine());
                    }
                }
            }
            else
            {
                lastDropPosition = transform.position;
            }
        }

        private void CheckGroundStatus()
        {
            float dynamicRayLength = (transform.localScale.y / 2f) + 0.2f;
            RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.down, dynamicRayLength);

            bool foundSafe = false;

            foreach (RaycastHit hit in hits)
            {
                GameObject hitObj = hit.collider.gameObject;

                if (currentRunBlocks.Contains(hitObj)) continue;

                if (hitObj.CompareTag("BlackGround") || hitObj.CompareTag("PlayerTrail"))
                {
                    foundSafe = true;
                    break;
                }
            }

            isOnSafeGround = foundSafe;
        }

        private void DropPathBlock()
        {
            if (pathBlockPrefab != null)
            {
                float bottomY = transform.position.y - (transform.localScale.y / 2f);
                float blockThickness = 0.05f;
                Vector3 dropPos = new Vector3(transform.position.x, bottomY + (blockThickness / 2f), transform.position.z);

                GameObject block = Instantiate(pathBlockPrefab, dropPos, transform.rotation);

                Collider col = block.GetComponent<Collider>();
                if (col != null) col.isTrigger = true;

                block.tag = "Untagged";
                currentRunBlocks.Add(block);
            }
        }

        // ==========================================
        // [VI] COROUTINE XỬ LÝ CÁI CHẾT THEO THỜI GIAN
        // [EN] COROUTINE TO HANDLE TIMED DEATH
        // ==========================================
        private IEnumerator DieRoutine()
        {
            isDying = true;

            // 1. [VI] Gọi âm thanh / [EN] Play audio
            if (AudioManager.Instance != null) AudioManager.Instance.Play("die");

            // 2. [VI] Sinh ra hiệu ứng hạt / [EN] Spawn particle effect
            if (deathEffectPrefab != null)
            {
                Destroy(Instantiate(deathEffectPrefab, transform.position, Quaternion.identity), 2f);
            }

            // 3. [VI] Ẩn mesh nhân vật đi để tạo cảm giác tan biến
            // [EN] Hide character mesh to create a dissolving illusion
            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = false;

            // 4. [VI] Chờ 0.3 giây / [EN] Wait for 0.3 seconds
            yield return new WaitForSeconds(0.3f);

            // 5. [VI] Chạy logic dọn dẹp đường đi và hồi sinh
            // [EN] Run path conversion and respawn logic
            DieAndConvertPath();

            // 6. [VI] Hiện lại mesh và xóa cờ chết / [EN] Unhide mesh and clear dying flag
            if (mr != null) mr.enabled = true;
            isDying = false;
        }

        private void DieAndConvertPath()
        {
            foreach (GameObject block in currentRunBlocks)
            {
                if (block != null)
                {
                    block.tag = "PlayerTrail";
                    Collider col = block.GetComponent<Collider>();
                    if (col != null) col.isTrigger = false;
                }
            }
            currentRunBlocks.Clear();

            transform.position = respawnPoint.position;
            transform.localScale = spawnScale;
            lastDropPosition = transform.position;
            isOnSafeGround = true;
        }

        private void ShrinkPlayer()
        {
            transform.localScale -= new Vector3(shrinkPerDrop, shrinkPerDrop, shrinkPerDrop);
        }
    }
}