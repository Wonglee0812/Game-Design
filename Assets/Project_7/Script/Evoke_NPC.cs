using UnityEngine;

namespace EvokeProject
{
    public class Evoke_NPC : MonoBehaviour
    {
        [Header("NPC Identity")]
        public bool isWhiteNPC = false; // [VI] Tích nếu là NPC Trắng / [EN] Check if White NPC
        public float speed = 5f;

        [Header("Bleed & Path Settings")]
        [Tooltip("Kéo Prefab gạch lót đường tương ứng màu sắc vào đây / Drag the corresponding colored paving brick Prefab here")]
        public GameObject pathBlockPrefab;
        public float dropInterval = 0.5f;
        public float shrinkPerDrop = 0.05f;
        public float minScale = 0.1f;

        private Vector3 lastDropPosition;

        private void Start()
        {
            lastDropPosition = transform.position;
        }

        private void Update()
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);

            // [VI] Chỉ chạy logic trừ máu nếu NPC dẫm sai mặt sàn
            // [EN] Only run bleed logic if the NPC steps on the wrong floor
            if (IsOnWrongGround())
            {
                float distanceMoved = Vector3.Distance(transform.position, lastDropPosition);

                if (distanceMoved >= dropInterval)
                {
                    DropPathBlock();
                    ShrinkNPC();
                    lastDropPosition = transform.position;

                    if (transform.localScale.x <= minScale)
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }

        private bool IsOnWrongGround()
        {
            float rayLength = (transform.localScale.y / 2f) + 0.2f;
            RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.down, rayLength);

            // [VI] Thuật toán động: NPC Trắng tìm tag BlackGround, NPC Đen tìm tag WhiteGround
            // [EN] Dynamic algorithm: White NPC looks for BlackGround tag, Black NPC looks for WhiteGround tag
            string deadlyTag = isWhiteNPC ? "BlackGround" : "WhiteGround";

            foreach (var hit in hits)
            {
                if (hit.collider.CompareTag(deadlyTag)) return true;
            }
            return false;
        }

        private void DropPathBlock()
        {
            if (pathBlockPrefab != null)
            {
                float bottomY = transform.position.y - (transform.localScale.y / 2f);
                float blockThickness = 0.05f;

                Vector3 dropPos = new Vector3(transform.position.x, bottomY + (blockThickness / 2f), transform.position.z);

                GameObject block = Instantiate(pathBlockPrefab, dropPos, transform.rotation);
                //block.transform.localScale = new Vector3(transform.localScale.x, blockThickness, transform.localScale.z);

                block.tag = "Untagged";
                Destroy(block, 45f); // [VI] Dọn dẹp rác sau 45s / [EN] Clean up trash after 45s
            }
        }

        private void ShrinkNPC()
        {
            transform.localScale -= new Vector3(shrinkPerDrop, shrinkPerDrop, shrinkPerDrop);

            // [VI] Ép gót chân lún xuống đất
            // [EN] Force heels to stick to the ground
            transform.position -= new Vector3(0, shrinkPerDrop / 2f, 0);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Finish"))
            {
                Destroy(gameObject);
            }
        }
    }
}