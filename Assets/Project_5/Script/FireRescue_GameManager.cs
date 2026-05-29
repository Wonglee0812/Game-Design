using UnityEngine;
using TMPro;

public class FireRescue_GameManager : MonoBehaviour
{
    public static FireRescue_GameManager Instance;

    [Header("Game Settings")]
    public float timeLimit = 60f; // [VI] Thời gian cho phép (giây) / [EN] Allowed time (seconds)
    private float timeRemaining;

    private int totalObjectsToCollect = 0;
    private int currentCollected = 0;
    public bool isGameOver = false;

    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI progressText;
    public GameObject winPanel;
    public GameObject losePanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        timeRemaining = timeLimit;
    }

    private void Start()
    {
        UpdateUI();
    }

    private void Update()
    {
        if (isGameOver) return;

        // [VI] Đếm lùi thời gian / [EN] Count down the time
        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            TriggerLoss();
        }

        UpdateUI();
    }

    // [VI] Hàm này được gọi bởi các vật thể lúc mới sinh ra để báo danh
    // [EN] This function is called by objects when spawned to register themselves
    public void RegisterCollectible()
    {
        totalObjectsToCollect++;
        UpdateUI();
    }

    // [VI] Hàm này được gọi khi Player chạm vào vật thể
    // [EN] This function is called when the Player touches an object
    public void CollectObject()
    {
        if (isGameOver) return;
        currentCollected++;
        UpdateUI();
    }

    // [VI] Hàm này được gọi khi Player bước vào Điểm Tập Kết
    // [EN] This function is called when the Player enters the Extraction Zone
    public void CheckWinCondition()
    {
        if (isGameOver) return;

        if (currentCollected >= totalObjectsToCollect && totalObjectsToCollect > 0)
        {
            TriggerWin();
        }
        else
        {
            Debug.Log("Chưa nhặt đủ đồ! / Not enough items collected!");
        }
    }

    private void UpdateUI()
    {
        if (progressText != null) progressText.text = $"Saved: {currentCollected}/{totalObjectsToCollect}";

        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    private void TriggerWin()
    {
        isGameOver = true;
        if (winPanel != null) winPanel.SetActive(true);
        Debug.Log("MISSION ACCOMPLISHED!");
    }

    private void TriggerLoss()
    {
        isGameOver = true;
        if (losePanel != null) losePanel.SetActive(true);
        Debug.Log("TIME IS UP! MISSION FAILED!");
    }
}