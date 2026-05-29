using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GlobalGameManager : MonoBehaviour
{
    public static GlobalGameManager Instance;

    [Header("Transition Settings")]
    [SerializeField] private float countdownDelay = 3f;

    // [VI] Thêm biến thời gian chờ sau khi hiện Banner (2s)
    // [EN] Added wait time after showing the Banner (2s)
    [SerializeField] private float bannerDisplayDuration = 2f;

    // [VI] Đổi từ Prefab thành GameObject có sẵn trong Scene
    // [EN] Changed from Prefab to an existing GameObject in the Scene
    [SerializeField] private GameObject transitionBanner;
    [SerializeField] private string animationStateName = "Play";

    [HideInInspector]
    public GameLevelData currentLevelData;
    [HideInInspector]
    public List<InputDevice> joinedPlayerDevices = new List<InputDevice>();
    private bool isLoadingScene = false;

    // [VI] Biến lưu trữ Coroutine đếm ngược để có thể reset
    // [EN] Variable to store the countdown Coroutine so it can be reset
    private Coroutine countdownCoroutine;

    [Header("Black Screen Transition")]
    [SerializeField] private RectTransform blackScreen; // [VI] Kéo BlackScreen vào đây / [EN] Drag BlackScreen here
    [SerializeField] private float transitionDuration = 0.5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // [VI] Đăng ký sự kiện khi load xong Scene mới
            // [EN] Subscribe to the event when a new Scene finishes loading
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // [VI] Đảm bảo màn hình đen nằm ngoài rìa trái lúc mới bắt đầu game
        // [EN] Ensure the black screen is off the left edge when the game starts
        if (blackScreen != null)
        {
            blackScreen.anchoredPosition = new Vector2(-Screen.width * 2, 0);
        }
    }

    public void StartGameChecker()
    {
        if (currentLevelData == null || isLoadingScene) return;

        // [VI] Đổi thành >= minPlayers để kích hoạt đếm ngược
        // [EN] Changed to >= minPlayers to trigger the countdown
        if (joinedPlayerDevices.Count >= currentLevelData.minPlayers)
        {
            // [VI] Nếu đang đếm ngược mà có người vào, DỪNG đếm ngược cũ để reset về 0
            // [EN] If already counting down and someone joins, STOP the old countdown to reset to 0
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
            }

            Debug.Log($"Đạt {joinedPlayerDevices.Count} người! Chờ {countdownDelay}s xem có ai vào thêm không...");

            // [VI] Bắt đầu lượt đếm ngược mới
            // [EN] Start a new countdown cycle
            countdownCoroutine = StartCoroutine(CountdownAndLoadRoutine());
        }
    }

    private IEnumerator CountdownAndLoadRoutine()
    {
        // 1. [VI] Chờ xem có người chơi mới không (thời gian này sẽ bị reset nếu StartGameChecker được gọi lại)
        // 1. [EN] Wait to see if new players join (this time resets if StartGameChecker is called again)
        yield return new WaitForSeconds(countdownDelay);

        // 2. [VI] Hết thời gian chờ, KHÓA hàm lại không cho người khác vào làm reset nữa
        // 2. [EN] Countdown finished, LOCK the function so new joins won't reset it anymore
        isLoadingScene = true;

        // 3. [VI] Hiện Banner
        // 3. [EN] Show Banner
        if (transitionBanner != null)
        {
            transitionBanner.SetActive(true);
            Animator anim = transitionBanner.GetComponentInChildren<Animator>();

            if (anim != null && !string.IsNullOrEmpty(animationStateName))
            {
                anim.Update(0);
                anim.Play(animationStateName, -1, 0f);
            }
        }

        // 4. [VI] Chờ Banner chạy (2s)
        // 4. [EN] Wait for Banner to play (2s)
        yield return new WaitForSeconds(bannerDisplayDuration);

        // 5. [VI] Kéo màn hình đen và Load Scene
        // 5. [EN] Pull black screen and Load Scene
        if (blackScreen != null)
        {
            blackScreen.anchoredPosition = new Vector2(-Screen.width * 2, 0);
            blackScreen.DOAnchorPosX(0, transitionDuration).SetEase(Ease.InOutSine);

            yield return new WaitForSeconds(transitionDuration);
        }

        SceneManager.LoadScene(currentLevelData.sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (blackScreen != null)
        {
            // [VI] Kéo màn đen từ Giữa (0) sang Phải (Screen.width) để mở màn
            // [EN] Pull black screen from Center (0) to Right (Screen.width) to reveal
            blackScreen.anchoredPosition = new Vector2(0, 0);
            blackScreen.DOAnchorPosX(Screen.width * 2, transitionDuration).SetEase(Ease.InOutSine);
        }
    }

    private void OnDestroy()
    {
        // [VI] Hủy đăng ký sự kiện để tránh lỗi memory leak
        // [EN] Unsubscribe from the event to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void AddPlayer(PlayerInput playerInput)
    {
        if (playerInput.devices.Count == 0)
        {
            Debug.LogWarning("Player joined but no device was found!");
            return;
        }

        InputDevice newDevice = playerInput.devices[0];

        if (!joinedPlayerDevices.Contains(newDevice))
        {
            joinedPlayerDevices.Add(newDevice);
            Debug.Log($"Player {joinedPlayerDevices.Count} Joined with {newDevice.name}");
        }

        // [VI] Gọi hàm check mỗi khi có người mới vào để chạy hoặc reset đếm ngược
        // [EN] Call the check function every time someone joins to start or reset the countdown
        StartGameChecker();
    }

    public int GetPlayerCount()
    {
        return joinedPlayerDevices.Count;
    }
}