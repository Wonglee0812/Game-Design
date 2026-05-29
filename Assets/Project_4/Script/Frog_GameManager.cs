using UnityEngine;
using UnityEngine.SceneManagement; // [VI] Thư viện bắt buộc để chuyển/load Scene / [EN] Mandatory library for changing/reloading Scenes
using TMPro;

public class Frog_GameManager : MonoBehaviour
{
    public static Frog_GameManager Instance;

    [Header("Level Settings")]
    public float timeLimit = 60f;
    private float timeRemaining;
    public bool isGameOver = false;

    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public GameObject winPanel;
    // [VI] Đã xóa biến losePanel vì không cần hiện bảng nữa / [EN] Removed losePanel variable since we don't need to show the board anymore

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        timeRemaining = timeLimit;
    }

    private void Update()
    {
        if (isGameOver) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            TriggerLose();
        }

        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void TriggerWin()
    {
        if (isGameOver) return;
        isGameOver = true;
        if (winPanel != null) winPanel.SetActive(true);
        Debug.Log("FROG SURVIVED! YOU WIN!");
    }

    public void TriggerLose()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("FROG DIED! RELOADING SCENE...");

        // [VI] Lấy tên (hoặc Index) của Scene đang chơi hiện tại
        // [EN] Get the name (or Index) of the currently playing Scene
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // [VI] Bắt đầu load lại chính Scene đó
        // [EN] Start reloading that exact Scene
        SceneManager.LoadScene(currentSceneIndex);
    }
}