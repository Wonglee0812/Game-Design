using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RacismProject
{
    public class Racism_GameManager : MonoBehaviour
    {
        public static Racism_GameManager Instance;

        [Header("Player Prefabs")]
        public GameObject whitePlayerPrefab;
        public GameObject blackPlayerPrefab;
        public Transform spawnPoint;

        [Header("Economy Settings")]
        public float currentMoney = 0f;
        public float targetToWin = 100f;
        private bool isBlackPlayer = false;
        private bool gameActive = false;

        [Header("Reward Values (Chỉnh sửa tại đây)")]
        [Tooltip("Số tiền khối Trắng nhận được khi bán 1 vật phẩm")]
        public float whitePlayerSellReward = 5f;
        [Tooltip("Số tiền khối Đen nhận được khi bán 1 vật phẩm")]
        public float blackPlayerSellReward = 1f;

        [Header("UI References")]
        public TMP_Text moneyText;
        public GameObject winScreen;
        public GameObject loseScreen;
        public GameObject characterSelectionScreen;

        private float drainTimer = 0f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        public void SpawnBlackPlayer()
        {
            Instantiate(blackPlayerPrefab, spawnPoint.position, Quaternion.identity);
            isBlackPlayer = true;
            currentMoney = 10f;
            StartGame();
        }

        public void SpawnWhitePlayer()
        {
            Instantiate(whitePlayerPrefab, spawnPoint.position, Quaternion.identity);
            isBlackPlayer = false;
            currentMoney = 95f;
            StartGame();
        }

        private void StartGame()
        {
            characterSelectionScreen.SetActive(false);
            gameActive = true;
            UpdateUI();
        }

        private void Update()
        {
            if (!gameActive) return;

            if (isBlackPlayer)
            {
                drainTimer += Time.deltaTime;
                if (drainTimer >= 1f)
                {
                    currentMoney -= 1f;
                    drainTimer = 0f;
                    UpdateUI();
                    CheckWinLose();
                }
            }
        }

        public void AddMoneyFromSelling()
        {
            // Thay vì dùng số cứng, hệ thống bây giờ lấy trực tiếp từ ô cấu hình trên Inspector
            if (isBlackPlayer)
            {
                currentMoney += blackPlayerSellReward;
            }
            else
            {
                currentMoney += whitePlayerSellReward;
            }

            UpdateUI();
            CheckWinLose();
        }

        private void UpdateUI()
        {
            if (moneyText != null)
                moneyText.text = "Money: " + currentMoney.ToString("F0") + " / " + targetToWin;
        }

        private void CheckWinLose()
        {
            if (currentMoney >= targetToWin)
            {
                gameActive = false;
                winScreen.SetActive(true);
            }
            else if (currentMoney <= 0 && isBlackPlayer)
            {
                gameActive = false;
                loseScreen.SetActive(true);
            }
        }
    }
}