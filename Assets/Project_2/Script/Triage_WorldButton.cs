using UnityEngine;
using UnityEngine.Events; // [VI] Bắt buộc để dùng UnityEvent / [EN] Required to use UnityEvent

namespace Portfolio.TriageManagement
{
    public class Triage_WorldButton : MonoBehaviour
    {
        [Header("Button Action")]
        public UnityEvent onClick; // [VI] Bảng sự kiện hiện ra ở Inspector / [EN] Event panel shows in Inspector

        private Vector3 originalScale;

        private void Start()
        {
            originalScale = transform.localScale;
        }

        public void PressButton()
        {
            // [VI] 1. Nhún vật thể nhỏ lại một chút để tạo cảm giác bị bấm
            // [EN] 1. Shrink the object slightly to create a pressed feel
            transform.localScale = originalScale * 0.8f;
            Invoke(nameof(ResetScale), 0.15f);

            // [VI] 2. Kích hoạt hàm được gán trong Inspector
            // [EN] 2. Invoke the function assigned in the Inspector
            onClick?.Invoke();
        }

        private void ResetScale()
        {
            transform.localScale = originalScale;
        }
    }
}