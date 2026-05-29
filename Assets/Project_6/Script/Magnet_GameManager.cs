using UnityEngine;

public class Magnet_GameManager : MonoBehaviour
{
    public static Magnet_GameManager Instance;

    [Header("UI References")]
    public GameObject winPanel;
    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void TriggerWin()
    {
        if (isGameOver) return;

        isGameOver = true;

        if (winPanel != null) winPanel.SetActive(true);

        Debug.Log("MAGNET PLATFORMER: YOU WIN!");

        // [VI] (Tùy chọn) Dừng thời gian lại khi thắng / [EN] (Optional) Freeze time on win
        Time.timeScale = 0f;
    }
}