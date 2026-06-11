using UnityEngine;

namespace EvokeProject
{
    public class Evoke_FinishLine : MonoBehaviour
    {
        [Header("UI Canvas")]
        [Tooltip("Kéo Panel chứa dòng chữ thông điệp chiến thắng vào đây / Drag the Panel containing the win message here")]
        public GameObject epiphanyUI;

        private void Start()
        {
            if (epiphanyUI != null)
            {
                epiphanyUI.SetActive(false); // [VI] Ẩn UI lúc mới vào game / [EN] Hide UI at the start of the game
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // [VI] 1. Nếu NPC chạm đích, chúng nó đã tự biến mất nhờ script Evoke_NPC rồi, không cần viết lại ở đây.
            // [EN] 1. If NPCs hit the finish, they already destroy themselves via Evoke_NPC script, no need to rewrite here.

            // [VI] 2. Nếu Player chạm đích
            // [EN] 2. If the Player hits the finish
            if (other.CompareTag("Player"))
            {
                // [VI] Bật màn hình thông điệp
                // [EN] Turn on the message screen
                if (epiphanyUI != null)
                {
                    epiphanyUI.SetActive(true);
                }

                // [VI] Tắt input script để không di chuyển được nữa
                // [EN] Disable input script so no more movement is allowed
                MonoBehaviour[] allScripts = other.GetComponents<MonoBehaviour>();
                foreach (var script in allScripts)
                {
                    // [VI] Giữ lại PlayerStatus để không bị lỗi, tắt mọi thứ khác
                    // [EN] Keep PlayerStatus alive to avoid errors, disable everything else
                    if (script != this && script.GetType().Name != "Evoke_PlayerStatus")
                    {
                        script.enabled = false;
                    }
                }
            }
        }
    }
}