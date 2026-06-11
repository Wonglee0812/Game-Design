using UnityEngine;
using System.Collections.Generic;

// =========================================================
// [VI] CLASS SOUND: Quản lý từng file âm thanh trên Inspector
// [EN] SOUND CLASS: Manages individual audio files in the Inspector
// =========================================================
[System.Serializable]
public class Sound
{
    [Tooltip("Tên để gọi trong code, VD: 'Jump', 'Click' / Name to call in code, e.g., 'Jump', 'Click'")]
    public string name;

    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Range(0.1f, 3f)]
    public float pitch = 1f;

    [Tooltip("Độ dao động random của Pitch để tạo Game Feel / Random variance of Pitch for Game Feel")]
    [Range(0f, 0.5f)]
    public float pitchVariance = 0.1f;

    public bool loop = false;
}

// =========================================================
// [VI] AUDIOMANAGER: Hệ thống điều phối trung tâm
// [EN] AUDIOMANAGER: The central coordination system
// =========================================================
public class AudioManager : MonoBehaviour
{
    // [VI] Biến Singleton duy nhất / [EN] The single Singleton instance
    public static AudioManager Instance;

    [Header("Sound Library")]
    [Tooltip("Thêm các âm thanh của bạn vào đây / Add your sounds here")]
    public Sound[] sounds;

    [Header("Object Pooling")]
    [Tooltip("Số lượng âm thanh tối đa có thể phát cùng lúc / Max overlapping sounds")]
    [SerializeField] private int poolSize = 15;

    private List<AudioSource> audioSourcePool;

    private void Awake()
    {
        // [VI] THIẾT LẬP SINGLETON: Đảm bảo chỉ có 1 AudioManager tồn tại
        // [EN] SINGLETON SETUP: Ensure only 1 AudioManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // [VI] Không bị hủy khi đổi Scene / [EN] Do not destroy when changing Scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializePool();
    }

    // [VI] Tạo sẵn một danh sách các AudioSource ẩn trên Game Object này
    // [EN] Pre-generate a list of hidden AudioSources on this Game Object
    private void InitializePool()
    {
        audioSourcePool = new List<AudioSource>();
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            audioSourcePool.Add(newSource);
        }
    }

    // [VI] Hàm chính để các script khác gọi: AudioManager.Instance.Play("Tên_Âm_Thanh");
    // [EN] Main function for other scripts to call: AudioManager.Instance.Play("Sound_Name");
    public void Play(string soundName)
    {
        Sound s = System.Array.Find(sounds, sound => sound.name == soundName);
        if (s == null) return;

        AudioSource availableSource = GetAvailableSource();
        if (availableSource != null)
        {
            availableSource.clip = s.clip;
            availableSource.volume = s.volume;
            availableSource.pitch = s.pitch + Random.Range(-s.pitchVariance, s.pitchVariance);

            // [VI] BẬT/TẮT CHẾ ĐỘ LOOP DỰA VÀO INSPECTOR / [EN] TOGGLE LOOP MODE BASED ON INSPECTOR
            availableSource.loop = s.loop;

            availableSource.Play();
        }
    }

    public void Stop(string soundName)
    {
        Sound s = System.Array.Find(sounds, sound => sound.name == soundName);
        if (s == null) return;

        foreach (AudioSource source in audioSourcePool)
        {
            // [VI] Nếu AudioSource này đang phát đúng cái Clip tiếng gió thì tắt nó đi
            // [EN] If this AudioSource is playing exactly the wind Clip, stop it
            if (source.isPlaying && source.clip == s.clip)
            {
                source.Stop();
                return; // [VI] Tắt xong thì thoát hàm luôn / [EN] Exit function once stopped
            }
        }
    }

    // [VI] Vòng lặp tìm kiếm AudioSource nào đang KHÔNG phát nhạc
    // [EN] Loop to find an AudioSource that is NOT playing music
    private AudioSource GetAvailableSource()
    {
        foreach (AudioSource source in audioSourcePool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        return null;
    }
}