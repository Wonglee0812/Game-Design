using UnityEngine;
using UnityEngine.UI; // [VI] Bắt buộc phải có để dùng Slider / [EN] Required to use Slider
using TMPro; // [VI] Bắt buộc phải có để dùng TextMeshPro / [EN] Required to use TextMeshPro

namespace Portfolio.TriageManagement
{
    public class Triage_GameManager : MonoBehaviour
    {
        public static Triage_GameManager Instance;

        [Header("Economy Data")]
        public float currentGold = 50f;
        public int currentReputation = 50;

        [Header("Passive Income (Lifesaver)")]
        public float incomeInterval = 5f; // [VI] 5 giây cộng tiền 1 lần / [EN] Add gold once every 5 seconds
        private float incomeTimer = 0f;

        [Header("Win Condition")]
        public float levelDuration = 120f; // [VI] 120 giây (2 phút) / [EN] 120 seconds (2 minutes)
        private float timeRemaining;

        [Header("Resource UI Settings")]
        [SerializeField] private float maxGold = 500f; // [VI] Giới hạn thanh Tiền / [EN] Gold slider limit
        [SerializeField] private float maxReputation = 100f; // [VI] Giới hạn thanh Hảo cảm / [EN] Reputation slider limit

        [Header("UI References")]
        [SerializeField] private Slider goldSlider;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private Slider reputationSlider;
        [SerializeField] private TextMeshProUGUI reputationText;
        [SerializeField] private TextMeshProUGUI timerText;
        private int staffCount = 0;
        [Header("Staff Settings")]
        public GameObject staffPrefab;
        public Transform staffSpawnPoint;
        public float hireCost = 50f;
        public float wagePerInterval = 10f;
        public float wageIntervalSeconds = 10f;

        public GameObject gameOverPanel;
        public GameObject winPanel; // [VI] Kéo UI Chiến Thắng vào đây / [EN] Drag Victory UI here

        public bool IsGameOver { get; private set; } = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            // [VI] Gán thời gian bắt đầu / [EN] Set starting time
            timeRemaining = levelDuration;
        }

        private void Start()
        {
            // [VI] Thiết lập giới hạn tối đa cho Slider khi bắt đầu game
            // [EN] Set maximum limits for Sliders when the game starts
            InvokeRepeating(nameof(PayStaffWages), wageIntervalSeconds, wageIntervalSeconds);
            if (goldSlider != null) goldSlider.maxValue = maxGold;
            if (reputationSlider != null) reputationSlider.maxValue = maxReputation;

            UpdateResourceUI();
        }
        public void UpdateResourceUI()
        {
            if (goldSlider != null) goldSlider.value = currentGold;
            if (goldText != null) goldText.text = $"${currentGold}";

            if (reputationSlider != null) reputationSlider.value = currentReputation;
            if (reputationText != null) reputationText.text = $"{currentReputation}/100";
        }
        private void Update()
        {
            if (IsGameOver) return;

            HandlePassiveIncome();
            HandleTimer();
            UpdateUI();
        }

        private void HandlePassiveIncome()
        {
            incomeTimer += Time.deltaTime;
            if (incomeTimer >= incomeInterval)
            {
                // [VI] Công thức: Lấy Hảo cảm chia 20. (VD: 100/20 = 5 vàng, 50/20 = 2 vàng).
                // [EN] Formula: Divide Reputation by 20. (Ex: 100/20 = 5 gold, 50/20 = 2 gold).
                int bonusGold = Mathf.FloorToInt(currentReputation / 20f);
                if (bonusGold > 0)
                {
                    currentGold += bonusGold;
                    UpdateResourceUI();
                }

                // [VI] Reset bộ đếm / [EN] Reset timer
                incomeTimer = 0f;
            }
        }

        private void HandleTimer()
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                TriggerWin();
            }
        }

        private void UpdateUI()
        {
            if (goldText != null) goldText.text = "Gold: " + Mathf.FloorToInt(currentGold).ToString();
            if (reputationText != null) reputationText.text = "Reputation: " + currentReputation.ToString();

            // [VI] Cập nhật đồng hồ theo định dạng Phút:Giây (VD: 01:59)
            // [EN] Update timer in Minute:Second format (Ex: 01:59)
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(timeRemaining / 60);
                int seconds = Mathf.FloorToInt(timeRemaining % 60);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }
        public void HireStaffButton()
        {
            if (SpendGold(hireCost))
            {
                staffCount++;
                Instantiate(staffPrefab, staffSpawnPoint.position, Quaternion.identity);
            }
        }

        private void PayStaffWages() // <--- Hàm này được gọi bởi InvokeRepeating ở trên / This function is called by InvokeRepeating above
        {
            if (staffCount > 0)
            {
                float totalWage = wagePerInterval * staffCount;
                if (!SpendGold(totalWage))
                {
                    currentGold = 0;
                    UpdateResourceUI();
                }
            }
        }
        public void OnPatientCured(int goldReward, int repReward)
        {
            if (IsGameOver) return;
            currentGold += goldReward;
            currentReputation = Mathf.Min(currentReputation + repReward, 100);
            UpdateResourceUI();
        }

        public void OnPatientDied(int repPenalty)
        {
            if (IsGameOver) return;
            currentReputation -= repPenalty;

            if (currentReputation < 0) TriggerGameOver();
            UpdateResourceUI();
        }

        public bool SpendGold(float amount)
        {
            if (currentGold >= amount)
            {
                currentGold -= amount;
                UpdateResourceUI();
                return true;
            }
            return false;
        }

        private void TriggerGameOver()
        {
            IsGameOver = true;
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            Debug.Log("GAME OVER!");
        }

        private void TriggerWin()
        {
            IsGameOver = true;
            if (winPanel != null) winPanel.SetActive(true);
            Debug.Log("YOU WIN!");
        }
    }
}