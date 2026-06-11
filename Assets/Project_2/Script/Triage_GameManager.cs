using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Portfolio.TriageManagement
{
    public class Triage_GameManager : MonoBehaviour
    {
        public static Triage_GameManager Instance;

        [Header("Economy Data")]
        public float currentGold = 50f;
        public int currentReputation = 50;

        [Header("Passive Income (Lifesaver)")]
        public float incomeInterval = 5f;
        private float incomeTimer = 0f;

        [Header("Win Condition")]
        public float levelDuration = 120f;
        private float timeRemaining;

        [Header("Resource UI Settings")]
        [SerializeField] private float maxGold = 500f;
        [SerializeField] private float maxReputation = 100f;

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
        public GameObject winPanel;

        public bool IsGameOver { get; private set; } = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            timeRemaining = levelDuration;
        }

        private void Start()
        {
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
                int bonusGold = Mathf.FloorToInt(currentReputation / 20f);
                if (bonusGold > 0)
                {
                    currentGold += bonusGold;
                    UpdateResourceUI();

                    // ==========================================
                    // [VI] ÂM THANH: CỘNG TIỀN THEO THỜI GIAN / [EN] AUDIO: PASSIVE INCOME ADDED
                    // ==========================================
                    if (AudioManager.Instance != null) AudioManager.Instance.Play("money");
                }
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

        private void PayStaffWages()
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

            // ==========================================
            // [VI] ÂM THANH: NHẬN TIỀN THƯỞNG / [EN] AUDIO: RECEIVE REWARD MONEY
            // ==========================================
            if (AudioManager.Instance != null) AudioManager.Instance.Play("money");
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

                // ==========================================
                // [VI] ÂM THANH: TRỪ TIỀN MUA SẮM / [EN] AUDIO: SPEND MONEY
                // ==========================================
                if (AudioManager.Instance != null) AudioManager.Instance.Play("money");

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