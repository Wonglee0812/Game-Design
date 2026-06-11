using UnityEngine;
using System.Collections;

namespace EvokeProject
{
    public class Evoke_WorldFlipper : MonoBehaviour
    {
        [Header("Flip Settings")]
        [Tooltip("Kéo object WorldAnchor vào đây / Drag the WorldAnchor object here")]
        public Transform worldAnchor;

        [Tooltip("Thời gian lật thế giới (giây) / Time it takes to flip the world (seconds)")]
        public float flipDuration = 1.0f;

        private bool hasFlipped = false;

        private void OnTriggerEnter(Collider other)
        {
            if (!hasFlipped && other.CompareTag("Player"))
            {
                hasFlipped = true;

                // ==========================================
                // [VI] ÂM THANH: LẬT THẾ GIỚI / [EN] AUDIO: WORLD FLIP
                // ==========================================
                if (AudioManager.Instance != null) AudioManager.Instance.Play("flip");

                StartCoroutine(FlipWorldRoutine());
            }
        }

        private IEnumerator FlipWorldRoutine()
        {
            Quaternion startRot = worldAnchor.rotation;
            Quaternion endRot = startRot * Quaternion.Euler(0, 0, 180);
            float elapsed = 0f;

            Time.timeScale = 0.5f;

            while (elapsed < flipDuration)
            {
                worldAnchor.rotation = Quaternion.Slerp(startRot, endRot, elapsed / flipDuration);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            worldAnchor.rotation = endRot;
            Time.timeScale = 1f;
        }
    }
}