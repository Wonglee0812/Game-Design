using UnityEngine;
using TMPro; // Dùng cho UI sau này / For later UI
using UnityEngine.SceneManagement;
using System.Collections.Generic;

// 1. Enum định nghĩa 9 loại Combo từ yếu đến mạnh
public enum PokerCombo
{
    HighCard, Pair, TwoPair, ThreeOfAKind, Straight, Flush, FullHouse, FourOfAKind, StraightFlush
}

// 2. Struct dùng để hiển thị trên Inspector cho bạn nhập số
[System.Serializable]
public struct ComboDamageData
{
    public PokerCombo comboType;
    public int damage;
}


public class GameManager_2 : MonoBehaviour
{
    // Singleton Pattern
    public static GameManager_2 instance;

    [Header("Player Stats")]
    public int health = 20;            // Máu của người chơi / Player Health
    public int gold = 100;             // Vàng khởi đầu / Starting Gold
    public int maxTurrets = 5;         // Giới hạn số tháp / Max Turret Limit
    public int currentTurrets = 0;     // Số tháp hiện có / Current Turrets on field

    [Header("UI References (Optional for now)")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI turretLimitText;
    [SerializeField] private GameObject gameOverUI; // Kéo bảng Lose Panel vào đây / Drag Lose Panel here

    [Header("Poker Combo Setup")]
    // List này để bạn chỉnh sửa trong Inspector / This List is for you to edit in Inspector
    public List<ComboDamageData> comboDamageList;

    // Từ điển dùng để code tra cứu tốc độ cao / Dictionary used for high-speed code lookup
    private Dictionary<PokerCombo, int> comboDamageDict;



    private void Awake()
    {
        // Khởi tạo Singleton / Initialize Singleton
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        UpdateUI();
        SetupDamageDictionary();
    }

    // --- CÁC HÀM XỬ LÝ POKER / POKER HANDLING FUNCTIONS ---
    private void SetupDamageDictionary()
    {
        comboDamageDict = new Dictionary<PokerCombo, int>();

        foreach (var item in comboDamageList)
        {
            if (!comboDamageDict.ContainsKey(item.comboType))
            {
                comboDamageDict.Add(item.comboType, item.damage);
            }
        }
        Debug.Log("Đã nạp xong Từ Điển Sát Thương! / Damage Dictionary Loaded!");
    }

    public int GetComboDamage(PokerCombo combo)
    {
        if (comboDamageDict != null && comboDamageDict.ContainsKey(combo))
        {
            return comboDamageDict[combo];
        }

        Debug.LogWarning("Không tìm thấy Combo trong List! Trả về sát thương mặc định. / Combo not found in List! Returning default damage.");
        return 2;
    }

    // Hàm trừ máu khi quái lọt lưới / Function to lose health when enemy leaks
    public void TakeDamage(int amount)
    {
        health -= amount;
        UpdateUI();

        if (health <= 0)
        {
            health = 0;
            GameOver();
        }
    }

    // Hàm quản lý tiền tệ / Function to manage gold
    public void AddGold(int amount)
    {
        gold += amount;
        UpdateUI();
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }

    // Cập nhật hiển thị UI / Update UI display
    public void UpdateUI()
    {
        if (healthText != null) healthText.text = "Health: " + health;
        if (goldText != null) goldText.text = "Gold: $" + gold;
        if (turretLimitText != null) turretLimitText.text = $"Turrets: {currentTurrets}/{maxTurrets}";
    }



    //Turret Update
    public void AddTurret()
    {
        currentTurrets++;
        UpdateUI();
    }
    public void RemoveTurret()
    {
        currentTurrets--;
        if (currentTurrets < 0) currentTurrets = 0;
        UpdateUI();
    }
    public bool CanBuild()
    {
        return currentTurrets < maxTurrets;
    }


    //Win condition
    private void GameOver()
    {
        Debug.Log("<color=red>GAME OVER</color>");
        // Sẽ thêm logic hiện bảng Lose tại đây / Will add Lose Panel logic here
        Time.timeScale = 0f;

        // 2. Hiện bảng thông báo thua
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
    }

    public void Restart()
    {
        // QUAN TRỌNG: Phải trả lại thời gian về 1 trước khi load cảnh mới
        // IMPORTANT: Must reset time scale to 1 before loading a new scene
        Time.timeScale = 1f;

        // Tải lại Scene hiện tại / Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}