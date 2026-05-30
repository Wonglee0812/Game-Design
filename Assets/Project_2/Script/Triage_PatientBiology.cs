using UnityEngine;

namespace Portfolio.TriageManagement
{
    public class Triage_PatientBiology : MonoBehaviour
    {
        [Header("Disease Configuration")]
        public Triage_DiseaseData currentDisease;

        [Header("Vital Signs")]
        public VitalSign temperature = new VitalSign { currentValue = 36.5f, normalValue = 36.5f, maxDeathThreshold = 41.0f, minDeathThreshold = 33.0f };
        public VitalSign heartRate = new VitalSign { currentValue = 80f, normalValue = 80f, maxDeathThreshold = 150f, minDeathThreshold = 40f };
        public VitalSign bloodPressure = new VitalSign { currentValue = 120f, normalValue = 120f, maxDeathThreshold = 180f, minDeathThreshold = 70f };
        public int reward, reputation, punishment;

        [Header("Homeostasis & Death Rules")]
        public float recoveryRate = 0.1f;
        public float deathGracePeriod = 3.0f;

        private float criticalTimer = 0f;
        private bool isDead = false;
        private bool isCured = false;

        // [VI] Biến mới: Kiểm soát xem bệnh đã bắt đầu phát tác chưa
        // [EN] New variable: Controls whether the disease has started acting up
        private bool isDiseaseActive = false;

        // [VI] Hàm public để Movement Script gọi khi đi vào Vùng Chờ
        // [EN] Public function for Movement Script to call when entering the Waiting Zone
        public void StartDisease()
        {
            isDiseaseActive = true;
        }
        private void Update()
        {
            // [VI] Chặn không cho trừ máu nếu bệnh chưa phát tác
            // [EN] Block health drain if the disease is not yet active
            if (isDead || isCured || !isDiseaseActive) return;

            UpdateVitals();
            CheckDeathConditions();
        }

        private void UpdateVitals()
        {
            float tMod = currentDisease != null ? currentDisease.tempModifier : 0f;
            float hMod = currentDisease != null ? currentDisease.hrModifier : 0f;
            float bMod = currentDisease != null ? currentDisease.bpModifier : 0f;

            temperature.currentValue += (tMod * Time.deltaTime) + ((temperature.normalValue - temperature.currentValue) * recoveryRate * Time.deltaTime);
            heartRate.currentValue += (hMod * Time.deltaTime) + ((heartRate.normalValue - heartRate.currentValue) * recoveryRate * Time.deltaTime);
            bloodPressure.currentValue += (bMod * Time.deltaTime) + ((bloodPressure.normalValue - bloodPressure.currentValue) * recoveryRate * Time.deltaTime);
        }

        private void CheckDeathConditions()
        {
            if (IsFatal(temperature) || IsFatal(heartRate) || IsFatal(bloodPressure))
            {
                criticalTimer += Time.deltaTime;
                if (criticalTimer >= deathGracePeriod) Die();
            }
            else
            {
                criticalTimer = 0f;
            }
        }

        private bool IsFatal(VitalSign sign)
        {
            return sign.currentValue >= sign.maxDeathThreshold || sign.currentValue <= sign.minDeathThreshold;
        }

        public bool IsCritical() => criticalTimer > 0f;
        // [VI] Trong script Triage_PatientBiology.cs / [EN] In script Triage_PatientBiology.cs

        public void AdministerDrug(DrugType appliedDrug)
        {
            if (isDead || isCured) return;

            if (currentDisease != null && appliedDrug == currentDisease.correctCure)
            {
                Cured();
            }
            else
            {
                // [VI] SAI THUỐC -> ĐỘT TỬ NGAY LẬP TỨC (Bỏ qua thời gian ân hạn)
                // [EN] WRONG DRUG -> INSTANT SUDDEN DEATH (Bypass grace period)
                Debug.Log("Wrong Drug! Patient suffered Anaphylactic Shock!");
                Die();
            }
        }

        private void Die()
        {
            if (isDead) return; // [VI] Tránh gọi nhiều lần / [EN] Prevent multiple calls
            isDead = true;

            // [VI] Phạt 10 Hảo cảm khi có người chết / [EN] Deduct 10 Reputation on death
            Triage_GameManager.Instance?.OnPatientDied(punishment);

            GetComponent<Triage_PatientMovement>()?.BecomeCorpse();
        }

        private void Cured()
        {
            if (isCured) return;
            isCured = true;

            // [VI] Thưởng 20 Vàng và 5 Hảo cảm / [EN] Reward 20 Gold and 5 Reputation
            Triage_GameManager.Instance?.OnPatientCured(reward, reputation);

            GetComponent<Triage_PatientMovement>()?.GoToExit();
        }
    }
}