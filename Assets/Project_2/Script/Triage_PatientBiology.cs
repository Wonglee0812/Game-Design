using UnityEngine;
using System.Collections.Generic;

namespace Portfolio.TriageManagement
{
    public class Triage_PatientBiology : MonoBehaviour
    {
        [Header("Disease Configuration")]
        public List<Triage_DiseaseData> possibleDiseases;

        [HideInInspector]
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

        private bool isDiseaseActive = false;

        private void Start()
        {
            if (possibleDiseases != null && possibleDiseases.Count > 0)
            {
                int randomIndex = Random.Range(0, possibleDiseases.Count);
                currentDisease = possibleDiseases[randomIndex];
            }
        }

        public void StartDisease()
        {
            isDiseaseActive = true;
        }

        private void Update()
        {
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

        public void AdministerDrug(DrugType appliedDrug)
        {
            if (isDead || isCured) return;

            if (currentDisease != null && appliedDrug == currentDisease.correctCure)
            {
                Cured();
            }
            else
            {
                Debug.Log("Wrong Drug! Patient suffered Anaphylactic Shock!");
                Die();
            }
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;

            // ==========================================
            // [VI] ÂM THANH: BỆNH NHÂN CHẾT / [EN] AUDIO: PATIENT DEATH
            // ==========================================
            if (AudioManager.Instance != null) AudioManager.Instance.Play("pain");

            Triage_GameManager.Instance?.OnPatientDied(punishment);
            GetComponent<Triage_PatientMovement>()?.BecomeCorpse();
        }

        private void Cured()
        {
            if (isCured) return;
            isCured = true;

            // ==========================================
            // [VI] ÂM THANH: CHỮA KHỎI / [EN] AUDIO: CURED
            // ==========================================
            if (AudioManager.Instance != null) AudioManager.Instance.Play("ping");

            Triage_GameManager.Instance?.OnPatientCured(reward, reputation);
            GetComponent<Triage_PatientMovement>()?.GoToExit();
        }
    }
}