using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager_1 : MonoBehaviour
{
    public static GameManager_1 Instance { get; private set; }

    [Header("References")]
    [SerializeField] private List<Transform> spawnPoints; // ĐẢM BẢO KÉO 4 ĐIỂM VÀO ĐÂY / ENSURE YOU DRAG 4 POINTS HERE
    [SerializeField] private Transform cam;
    [SerializeField] private TextMeshProUGUI vicWord;

    [Header("Health & Lives Settings")]
    [SerializeField] private int startingLives = 3;
    [SerializeField] private TextMeshProUGUI[] playerHealthTexts; // 4 UI Texts

    [Header("Camera Settings")]
    [SerializeField] private float minZoomIn = 5f;
    [SerializeField] private float maxZoomOut = 20f;
    [SerializeField] private float camMoveSpeed = 10f;
    [SerializeField] private Vector3 lookAtOffset = new Vector3(0, 0, 0);

    private List<PlayerInput> activePlayers = new List<PlayerInput>();
    private Dictionary<PlayerInput, int> playerLives = new Dictionary<PlayerInput, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (vicWord != null) vicWord.gameObject.SetActive(false);
    }

    private void Update()
    {
        CameraUpdate();
    }

    // [VI] Hàm này được gọi từ VehicleSelectionManager
    // [EN] This is called by VehicleSelectionManager
    public void StartGameWithSelections(List<VehicleDataSO> selections)
    {
        var globalManager = GlobalGameManager.Instance;

        for (int i = 0; i < selections.Count; i++)
        {
            if (i >= spawnPoints.Count) break;

            InputDevice device = globalManager.joinedPlayerDevices[i];
            GameObject selectedPrefab = selections[i].vehiclePrefab;

            // Spawn xe thật / Spawn real car
            PlayerInput newPlayer = PlayerInput.Instantiate(
                selectedPrefab,
                playerIndex: i,
                splitScreenIndex: -1,
                controlScheme: null,
                pairWithDevice: device
            );

            activePlayers.Add(newPlayer);
            playerLives.Add(newPlayer, startingLives);
            UpdateHealthUI(i, startingLives);

            RespawnPlayer(newPlayer, true);
        }
    }

    private void UpdateHealthUI(int playerIndex, int currentLives)
    {
        if (playerIndex < playerHealthTexts.Length && playerHealthTexts[playerIndex] != null)
            playerHealthTexts[playerIndex].text = $"{currentLives}";
    }

    public void OnPlayerFall(PlayerInput player)
    {
        if (!activePlayers.Contains(player)) return;

        // [VI] Gọi Kill Effect (Mới thêm) / [EN] Call Kill Effect (Newly added)
        if (VFXManager.Instance != null)
            VFXManager.Instance.PlayKillEffect(player.transform.position, 0.5f);

        int pIndex = player.playerIndex;
        playerLives[player]--;
        UpdateHealthUI(pIndex, playerLives[player]);

        if (playerLives[player] > 0)
        {
            RespawnPlayer(player, false);
        }
        else
        {
            activePlayers.Remove(player);
            player.gameObject.SetActive(false);


            CheckWinCondition();
        }
    }

    private void RespawnPlayer(PlayerInput player, bool isFirstSpawn)
    {
        CarVisual visual = player.GetComponent<CarVisual>();
        if (visual != null) visual.RepairCar();

        int id = player.playerIndex;
        var rb = player.GetComponent<Rigidbody>();

        if (rb != null && !isFirstSpawn)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        player.transform.position = spawnPoints[id].position;
        player.transform.rotation = spawnPoints[id].rotation;

        if (VFXManager.Instance != null)
            VFXManager.Instance.PlaySpawnEffect(spawnPoints[id].position, 0.5f);

        // [VI] 3. Đợi 0.3 giây, sau đó phóng to chiếc xe ra một cách "nhún nhảy" (OutBack)
        // [EN] 3. Wait 0.3 seconds, then scale the car up with a bouncy feel (OutBack)
        player.transform.DOScale(Vector3.one, 0.5f).SetDelay(0.3f).SetEase(Ease.OutBack);

        if (rb != null) rb.isKinematic = false;
    }

    private void CameraUpdate()
    {
        // Giữ nguyên logic Camera của bạn (Tạm thời không đổi để tránh rối)
        // Keep your Camera logic unchanged for now to avoid confusion
        if (activePlayers.Count == 0) return;

        if (activePlayers.Count >= 2)
        {
            // Note: Cần nâng cấp Camera để follow 4 player sau, hiện tại follow 2 người đầu
            // Note: Will need to upgrade Camera to follow 4 players later, currently follows first 2
            Vector3 p1 = activePlayers[0].transform.position;
            Vector3 p2 = activePlayers[1].transform.position;
            Vector3 midPoint = (p1 + p2) / 2f;

            float dis = Vector3.Distance(p1, p2);
            float finalZ = -Mathf.Clamp(dis * 0.8f, minZoomIn, maxZoomOut);
            float finalY = Mathf.Clamp(dis * 0.7f, minZoomIn, maxZoomOut);

            Vector3 targetPos = new Vector3(midPoint.x, midPoint.y + finalY, midPoint.z + finalZ);
            cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, Time.deltaTime * camMoveSpeed);
            cam.LookAt(midPoint + lookAtOffset);
        }
    }

    private void CheckWinCondition() { /* Giữ nguyên / Keep as is */ }
}