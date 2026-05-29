using UnityEngine;

namespace Portfolio.TriageManagement
{
    public class Triage_CorpseCollector : MonoBehaviour
    {
        public float rewardPerCorpse = 10f; // [VI] Tiền bán xác / [EN] Cash per corpse
        public int repPenalty = 5;          // [VI] Hảo cảm bị trừ vì... làm chuyện mờ ám / [EN] Reputation penalty for... shady business

        private void OnTriggerEnter(Collider other)
        {
            // [VI] Nếu vật thể đi vào là một Xác chết / [EN] If the entering object is a Corpse
            if (other.gameObject.layer == LayerMask.NameToLayer("Corpse"))
            {
                // [VI] Cộng tiền và trừ hảo cảm thông qua GameManager / [EN] Add money and deduct rep via GameManager
                Triage_GameManager.Instance.currentGold += rewardPerCorpse;
                Triage_GameManager.Instance.OnPatientDied(repPenalty); // [VI] Dùng hàm trừ hảo cảm có sẵn / [EN] Use existing rep deduction function

                Debug.Log("Sold corpse for money!");

                // [VI] Xóa xác chết sau khi bán / [EN] Destroy corpse after selling
                Destroy(other.gameObject, 10f);
            }
        }
    }
}