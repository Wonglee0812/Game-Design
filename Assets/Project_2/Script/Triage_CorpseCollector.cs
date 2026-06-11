using UnityEngine;

namespace Portfolio.TriageManagement
{
    public class Triage_CorpseCollector : MonoBehaviour
    {
        public float rewardPerCorpse = 10f;
        public int repPenalty = 5;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Corpse"))
            {
                Triage_GameManager.Instance.currentGold += rewardPerCorpse;
                Triage_GameManager.Instance.OnPatientDied(repPenalty);

                // ==========================================
                // [VI] ÂM THANH: KIẾM TIỀN TỪ BÁN XÁC / [EN] AUDIO: EARN MONEY FROM CORPSE
                // ==========================================
                if (AudioManager.Instance != null) AudioManager.Instance.Play("money");

                Debug.Log("Sold corpse for money!");
                Destroy(other.gameObject);
            }
        }
    }
}