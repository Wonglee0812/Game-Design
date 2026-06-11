using UnityEngine;

namespace EvokeProject
{
    // [VI] Tự động thêm AudioSource vào vật thể khi gắn script này
    // [EN] Automatically adds an AudioSource to the object when attaching this script
    [RequireComponent(typeof(AudioSource))]
    public class Evoke_SoundtrackManager : MonoBehaviour
    {
        [Header("Soundtrack Settings")]
        [Tooltip("Kéo bài nhạc nền vào đây / Drag the background music clip here")]
        public AudioClip backgroundMusicClip;

        [Range(0f, 1f)]
        public float defaultVolume = 0.8f;

        private AudioSource bgmSource;

        private void Awake()
        {
            bgmSource = GetComponent<AudioSource>();

            // [VI] Setup tự động chạy lặp lại bài nhạc / [EN] Setup automatic looping music
            bgmSource.loop = true;
            bgmSource.playOnAwake = true;
            bgmSource.volume = defaultVolume;

            if (backgroundMusicClip != null)
            {
                bgmSource.clip = backgroundMusicClip;
                bgmSource.Play();
            }
            else
            {
                Debug.LogWarning("[Evoke_SoundtrackManager] Chưa gắn file nhạc nền! / Missing background music file!");
            }
        }

        // [VI] Hàm hỗ trợ đổi bài nhạc giữa chừng (Nếu sau này cần chuyển màn)
        // [EN] Helper function to change tracks mid-game (If needed for level transitions later)
        public void ChangeSoundtrack(AudioClip newClip)
        {
            if (bgmSource.clip == newClip) return;

            bgmSource.Stop();
            bgmSource.clip = newClip;
            bgmSource.Play();
        }
    }
}