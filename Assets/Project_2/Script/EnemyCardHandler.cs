using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // BẮT BUỘC để dùng các lệnh Gom nhóm (GroupBy, OrderBy) / REQUIRED to use Grouping commands
using UnityEngine.UI;
public class EnemyCardHandler : MonoBehaviour
{
    public List<Card> stuckCards = new List<Card>();
    private Coroutine explodeTimer;
    [SerializeField] private float waitTime = 2f;

    [Header("Visual Feedback (UI & VFX)")]
    [SerializeField] private Image[] cardUI_Slots; // Mảng chứa 5 ô Image rỗng trên đầu quái / Array of 5 empty Image slots
    [SerializeField] private GameObject explosionVFX_Prefab; // Particle hiệu ứng nổ / Explosion Particle
    [SerializeField] private Transform cardLayoutParent;

    private void Start()
    {
        UpdateCardUI();
    }


    private void Update()
    {
        if (cardLayoutParent != null)
        {
            cardLayoutParent.rotation = UnityEngine.Camera.main.transform.rotation;
        }
    }
    public void AddCard(Card newCard)
    {
        stuckCards.Add(newCard);

        UpdateCardUI();

        if (explodeTimer != null) StopCoroutine(explodeTimer);

        if (stuckCards.Count >= 5)
        {
            Explode();
        }
        else
        {
            explodeTimer = StartCoroutine(CountdownToExplode());
        }
    }

    IEnumerator CountdownToExplode()
    {
        yield return new WaitForSeconds(waitTime);
        Explode();
    }

    private void Explode()
    {
        if (stuckCards.Count == 0) return;

        // 1. TÌM COMBO (Thám tử soi bài) 
        // 1. FIND COMBO (Detective inspects cards)
        PokerCombo finalCombo = EvaluatePokerCombo();

        // 2. IN LOG DEBUG (Tách riêng cho gọn)
        // 2. PRINT DEBUG LOG (Separated for cleanliness)
        PrintDebugLog(finalCombo);

        // 3. LẤY SÁT THƯƠNG TỪ QUAN TÒA (GameManager_2)
        // 3. GET DAMAGE FROM THE JUDGE (GameManager_2)
        int damage = GameManager_2.instance.GetComboDamage(finalCombo);


        TriggerExplosionVFX();
        // 4. TRỪ MÁU VÀ DỌN DẸP
        // 4. DEDUCT HEALTH AND CLEAN UP
        Enemy e = transform.GetComponent<Enemy>();
        if (e != null)
            e.TakeDamage(damage);

        stuckCards.Clear();
    }


    private void UpdateCardUI()
    {
        // Kiểm tra an toàn / Safety check
        if (cardUI_Slots == null || cardUI_Slots.Length == 0) return;

        for (int i = 0; i < cardUI_Slots.Length; i++)
        {
            if (i < stuckCards.Count)
            {
                // Lấy thông tin bài hiện tại / Get current card info
                Card currentCard = stuckCards[i];

                // Tự động ghép tên (VD: "Hearts_2", "Spades_14") / Auto-generate name
                string spriteName = $"{currentCard.rank}-{currentCard.suit}";

                // Tự động dò tìm ảnh trong thư mục Resources / Auto-load from Resources
                Sprite loadedSprite = Resources.Load<Sprite>($"Cards/{spriteName}");

                if (loadedSprite != null)
                {
                    cardUI_Slots[i].sprite = loadedSprite;
                    cardUI_Slots[i].gameObject.SetActive(true); // Bật ảnh lên / Enable image
                }
                else
                {
                    Debug.LogWarning($"Thiếu ảnh lá bài: {spriteName} trong thư mục Resources/Cards!");
                }
            }
            else
            {
                // Nếu chưa có bài ở vị trí này thì tắt ảnh đi / Disable if no card in this slot
                cardUI_Slots[i].gameObject.SetActive(false);
            }
        }
    }

    // =========================================================================
    // HÀM 2: KÍCH HOẠT HIỆU ỨNG VÀ XÓA BÀI
    // FUNCTION 2: TRIGGER VFX AND CLEAR CARDS
    // =========================================================================
    private void TriggerExplosionVFX()
    {
        // 1. Chạy Particle an toàn / Safe Particle Instantiation
        if (explosionVFX_Prefab != null)
        {
            // Sinh ra hiệu ứng cao hơn quái 1 chút / Spawn slightly above enemy
            GameObject vfx = Instantiate(explosionVFX_Prefab, transform.position + Vector3.up * 1f, Quaternion.identity);
            Destroy(vfx, 1.5f); // Tự dọn dẹp để không nặng máy / Auto cleanup
        }

        // 2. Tắt hết 5 ô Image bài đi / Disable all 5 card Images
        if (cardUI_Slots != null)
        {
            foreach (var img in cardUI_Slots)
            {
                if (img != null) img.gameObject.SetActive(false);
            }
        }
    }


    // =========================================================================
    // HÀM TÌM COMBO POKER (THE POKER BRAIN)
    // =========================================================================
    private PokerCombo EvaluatePokerCombo()
    {
        // GOM NHÓM BẰNG LINQ / GROUPING WITH LINQ:
        // GroupBy(c => c.rank): Gom các lá bài có cùng Số lại với nhau. / Group cards with the same Rank together.
        // OrderByDescending(g => g.Count()): Sắp xếp nhóm đông nhất lên đầu. / Sort so the largest group is first.
        var rankGroups = stuckCards.GroupBy(c => c.rank).OrderByDescending(g => g.Count()).ToList();

        // GroupBy(c => c.suit): Gom các lá bài cùng Chất (Cơ, Rô...) / Group cards by Suit (Hearts, Diamonds...)
        var suitGroups = stuckCards.GroupBy(c => c.suit).ToList();

        // KIỂM TRA ĐIỀU KIỆN ĐẶC BIỆT / CHECK SPECIAL CONDITIONS:
        // isFlush (Đồng chất): Phải có đủ 5 lá VÀ tất cả nằm chung 1 nhóm Chất.
        // isFlush: Must have exactly 5 cards AND all fall into 1 Suit group.
        bool isFlush = (stuckCards.Count == 5) && (suitGroups.Count == 1);
        bool isStraight = false;

        // isStraight (Sảnh): Phải có 5 lá, chia làm 5 nhóm Số khác nhau (không trùng).
        // isStraight: Must have exactly 5 cards, split into 5 different Rank groups (no duplicates).
        if (stuckCards.Count == 5 && rankGroups.Count == 5)
        {
            // Lấy ra các con số và xếp từ nhỏ đến lớn / Extract numbers and sort ascending (e.g., 4, 5, 6, 7, 8)
            var orderedRanks = stuckCards.Select(c => c.rank).OrderBy(r => r).ToList();

            // Nếu (Số lớn nhất - Số nhỏ nhất == 4) thì chắc chắn là chuỗi liên tiếp!
            // If (Highest rank - Lowest rank == 4) then it's definitely a consecutive sequence!
            if (orderedRanks.Last() - orderedRanks.First() == 4)
            {
                isStraight = true;
            }
        }

        // TIẾN HÀNH SO SÁNH TỪ MẠNH XUỐNG YẾU / BEGIN COMPARING FROM STRONGEST TO WEAKEST:

        // 1. Straight Flush: Vừa Sảnh vừa Đồng Chất / Both Straight and Flush
        if (isFlush && isStraight) return PokerCombo.StraightFlush;

        // 2. Four of a Kind: Nhóm đông nhất có 4 lá / Largest group has 4 cards
        if (rankGroups[0].Count() == 4) return PokerCombo.FourOfAKind;

        // 3. Full House: Nhóm 1 có 3 lá VÀ nhóm 2 có 2 lá / Group 1 has 3 cards AND Group 2 has 2 cards
        if (rankGroups.Count >= 2 && rankGroups[0].Count() == 3 && rankGroups[1].Count() == 2) return PokerCombo.FullHouse;

        // 4. Flush: 5 lá đồng chất / 5 cards same suit
        if (isFlush) return PokerCombo.Flush;

        // 5. Straight: 5 lá liên tiếp / 5 consecutive cards
        if (isStraight) return PokerCombo.Straight;

        // 6. Three of a Kind: Nhóm đông nhất có 3 lá / Largest group has 3 cards
        if (rankGroups[0].Count() == 3) return PokerCombo.ThreeOfAKind;

        // 7. Two Pair: Nhóm 1 có 2 lá VÀ nhóm 2 có 2 lá / Group 1 has 2 cards AND Group 2 has 2 cards
        if (rankGroups.Count >= 2 && rankGroups[0].Count() == 2 && rankGroups[1].Count() == 2) return PokerCombo.TwoPair;

        // 8. Pair: Nhóm đông nhất có 2 lá / Largest group has 2 cards
        if (rankGroups[0].Count() == 2) return PokerCombo.Pair;

        // 9. High Card: Rác (Mặc định) / Default garbage
        return PokerCombo.HighCard;
    }

    // =========================================================================
    // HÀM IN CONSOLE / CONSOLE PRINT FUNCTION
    // =========================================================================
    private void PrintDebugLog(PokerCombo combo)
    {
        string color = "white";
        switch (combo)
        {
            case PokerCombo.StraightFlush: color = "red"; break;
            case PokerCombo.FourOfAKind: color = "magenta"; break;
            case PokerCombo.FullHouse: color = "orange"; break;
            case PokerCombo.Flush: color = "cyan"; break;
            case PokerCombo.Straight: color = "green"; break;
            case PokerCombo.ThreeOfAKind: color = "yellow"; break;
            case PokerCombo.TwoPair: color = "lightblue"; break;
            case PokerCombo.Pair: color = "lightblue"; break;
        }

        Debug.Log($"<color={color}>Enemy Exploded with: {combo.ToString().ToUpper()}!</color>");
    }
}