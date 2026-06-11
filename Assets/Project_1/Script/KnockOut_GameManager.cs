using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;
using System.Linq;

public class KnockOut_GameManager : MonoBehaviour
{
    public static KnockOut_GameManager Instance { get; private set; }

    public enum GameState { Warmup, Playing, GameOver }
    public GameState currentState = GameState.Warmup;

    [Header("Game Settings")]
    public float playTime = 180f; // [VI] 3 Phút / [EN] 3 Minutes
    public int pointsPerKill = 50;
    public int pointsPenaltyForFalling = 20;
    public float respawnDelay = 3f;

    [Header("Spawning")]
    public List<Transform> spawnPoints;
    public List<GameObject> randomCarPrefabs; // [VI] Kéo các Prefab xe vào đây / [EN] Drag Car Prefabs here

    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI centerMessageText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI[] playerScoreTexts; // [VI] 4 Text điểm ở 4 góc / [EN] 4 Score Texts at corners

    private Dictionary<PlayerInput, int> playerScores = new Dictionary<PlayerInput, int>();
    private List<PlayerInput> activePlayers = new List<PlayerInput>();
    private float currentTime;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        currentTime = playTime;
        centerMessageText.text = "Waiting for players... (Press any button)";
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // [VI] Khởi tạo xe ngẫu nhiên đầu tiên cho Player 1
        // [EN] Initialize the first random car for Player 1
        if (PlayerInputManager.instance != null && randomCarPrefabs.Count > 0)
        {
            PlayerInputManager.instance.playerPrefab = randomCarPrefabs[Random.Range(0, randomCarPrefabs.Count)];
        }

        UpdateScoreUI();
    }

    private void Update()
    {
        if (currentState == GameState.Warmup)
        {
            foreach (var gamepad in Gamepad.all)
            {
                if (gamepad.buttonSouth.wasPressedThisFrame)
                {
                    bool isAlreadyJoined = false;
                    foreach (var player in activePlayers)
                    {
                        if (player.devices.Contains(gamepad)) isAlreadyJoined = true;
                    }

                    if (!isAlreadyJoined && PlayerInputManager.instance != null && randomCarPrefabs.Count > 0)
                    {
                        PlayerInputManager.instance.playerPrefab = randomCarPrefabs[Random.Range(0, randomCarPrefabs.Count)];

                        // ==========================================
                        // [VI] FIX 1: Đổi thành "Controller" theo đúng tên trong Input Actions của bạn
                        // [EN] FIX 1: Changed to "Controller" matching your Input Actions file exactly
                        // ==========================================
                        PlayerInputManager.instance.JoinPlayer(-1, -1, "Controller", gamepad);
                    }
                }
            }

            bool startButtonPressed = false;

            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) startButtonPressed = true;

            foreach (var gamepad in Gamepad.all)
            {
                if (gamepad.startButton.wasPressedThisFrame) startButtonPressed = true;
            }

            if (activePlayers.Count >= 2 && startButtonPressed)
            {
                StartCoroutine(StartGameCountdown());
            }
        }
        else if (currentState == GameState.Playing)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerUI();

            if (currentTime <= 0) EndGame();
        }
    }

    public void AddScore(PlayerInput player, int amount)
    {
        if (!playerScores.ContainsKey(player)) return;

        playerScores[player] += amount;
        if (playerScores[player] < 0) playerScores[player] = 0;

        UpdateScoreUI();
        if (amount > 0 && AudioManager.Instance != null)
        {
            AudioManager.Instance.Play("ping");
        }
        int index = player.playerIndex;

        // ==========================================
        // [VI] FIX 2: Thêm điều kiện "index >= 0" để chặn tuyệt đối lỗi IndexOutOfRangeException
        // [EN] FIX 2: Added "index >= 0" condition to absolutely block IndexOutOfRangeException
        // ==========================================
        if (index >= 0 && index < playerScoreTexts.Length && playerScoreTexts[index] != null)
        {
            playerScoreTexts[index].transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 10, 1f);
        }
    }

    // [VI] HÀM NHẬN NGƯỜI CHƠI TỪ PLAYER INPUT MANAGER
    // [EN] FUNCTION TO RECEIVE PLAYERS FROM PLAYER INPUT MANAGER
    public void OnPlayerJoined(PlayerInput newPlayer)
    {
        if (currentState == GameState.GameOver) return;

        activePlayers.Add(newPlayer);
        playerScores.Add(newPlayer, 0);

        RespawnPlayer(newPlayer, true);
        UpdateScoreUI();

        if (currentState == GameState.Warmup && activePlayers.Count >= 2)
        {
            centerMessageText.text = "Press ENTER or SPACE to begin!";
        }

        // [VI] Đổi sẵn một Prefab xe ngẫu nhiên khác cho người tiếp theo (Player 2, 3, 4...)
        // [EN] Pre-swap another random Car Prefab for the next person (Player 2, 3, 4...)
        if (PlayerInputManager.instance != null && randomCarPrefabs.Count > 0)
        {
            PlayerInputManager.instance.playerPrefab = randomCarPrefabs[Random.Range(0, randomCarPrefabs.Count)];
        }
    }
    private void SetPlayerActiveState(PlayerInput player, bool state)
    {
        // 1. Tắt/Bật hình ảnh (Mesh, Trail, Particles) / Toggle visuals
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        foreach (Renderer ren in renderers) ren.enabled = state;

        // 2. Tắt/Bật va chạm / Toggle collisions
        Collider[] colliders = player.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders) col.enabled = state;

        // 3. Tắt/Bật script vật lý / Toggle physics script
        CarPhysics physics = player.GetComponent<CarPhysics>();
        if (physics != null) physics.enabled = state;

        // 4. Khóa/Mở luồng nhận Input / Lock/Unlock Input stream
        if (state) player.ActivateInput();
        else player.DeactivateInput();
    }
    // [VI] HÀM XỬ LÝ RỚT ĐÀI & TÍNH ĐIỂM
    // [EN] FALL HANDLING & SCORING FUNCTION
    public void OnPlayerFall(PlayerInput victim)
    {
        if (!activePlayers.Contains(victim)) return;

        if (VFXManager.Instance != null)
            VFXManager.Instance.PlayKillEffect(victim.transform.position, 0.5f);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play("KO");
        }

        if (currentState == GameState.Playing)
        {
            CarPhysics victimPhysics = victim.GetComponent<CarPhysics>();

            // [VI] 1. Cộng điểm cho người húc (Nếu có bị húc trong 5 giây qua)
            // [EN] 1. Add score to the attacker (If hit within the last 5 seconds)
            if (victimPhysics != null && victimPhysics.lastAttacker != null)
            {
                if (Time.time - victimPhysics.lastAttackTime <= 5f)
                {
                    AddScore(victimPhysics.lastAttacker, pointsPerKill);
                    Debug.Log($"Player {victimPhysics.lastAttacker.playerIndex + 1} knocked out Player {victim.playerIndex + 1}!");
                }
            }

            // [VI] 2. Phạt trừ điểm người bị rớt
            // [EN] 2. Deduct points from the falling player
            AddScore(victim, -pointsPenaltyForFalling);
        }

        StartCoroutine(RespawnRoutine(victim));
    }

    private IEnumerator RespawnRoutine(PlayerInput player)
    {
        // [VI] Tắt vật lý / [EN] Turn off physics
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        SetPlayerActiveState(player, false);

        yield return new WaitForSeconds(respawnDelay);

        if (currentState != GameState.GameOver)
        {
            SetPlayerActiveState(player, true);
            RespawnPlayer(player, false);
        }
    }

    private void RespawnPlayer(PlayerInput player, bool isFirstSpawn)
    {
        CarVisual visual = player.GetComponent<CarVisual>();
        if (visual != null) visual.RepairCar();

        int id = player.playerIndex % spawnPoints.Count;
        var rb = player.GetComponent<Rigidbody>();

        if (rb != null && !isFirstSpawn)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        player.transform.position = spawnPoints[id].position;
        player.transform.rotation = spawnPoints[id].rotation;

        if (VFXManager.Instance != null && !isFirstSpawn)
            VFXManager.Instance.PlaySpawnEffect(spawnPoints[id].position, 0.5f);

        player.transform.localScale = Vector3.zero;
        player.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

        if (rb != null) rb.isKinematic = false;

        // [VI] Xóa trí nhớ về người húc trước đó để tránh cộng điểm lặp
        // [EN] Clear memory of the previous attacker to avoid double scoring
        CarPhysics phys = player.GetComponent<CarPhysics>();
        if (phys != null) phys.lastAttacker = null;
    }

    private IEnumerator StartGameCountdown()
    {
        centerMessageText.text = "3";
        yield return new WaitForSeconds(1f);
        centerMessageText.text = "2";
        yield return new WaitForSeconds(1f);
        centerMessageText.text = "1";
        yield return new WaitForSeconds(1f);
        centerMessageText.text = "GO!";

        currentState = GameState.Playing;

        yield return new WaitForSeconds(1f);
        centerMessageText.text = "";
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void UpdateScoreUI()
    {
        for (int i = 0; i < playerScoreTexts.Length; i++)
        {
            if (playerScoreTexts[i] == null) continue;

            PlayerInput p = null;
            foreach (var kvp in playerScores)
            {
                if (kvp.Key.playerIndex == i) { p = kvp.Key; break; }
            }

            if (p != null) playerScoreTexts[i].text = $"P{i + 1}: {playerScores[p]} Pts";
            else playerScoreTexts[i].text = "Press to Join";
        }
    }

    private void EndGame()
    {
        currentState = GameState.GameOver;
        centerMessageText.text = "TIME'S UP!";
        Time.timeScale = 0.5f;

        PlayerInput winner = null;
        int highestScore = -1;

        foreach (var kvp in playerScores)
        {
            if (kvp.Value > highestScore)
            {
                highestScore = kvp.Value;
                winner = kvp.Key;
            }
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (winnerText != null)
        {
            winnerText.text = winner != null ? $"PLAYER {winner.playerIndex + 1} WINS!" : "DRAW!";
        }
    }
}