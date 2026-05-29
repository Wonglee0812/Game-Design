using UnityEngine;
using UnityEngine.UI;
using TMPro; // [VI] Thêm thư viện TextMeshPro / [EN] Add TextMeshPro library

namespace Portfolio.TriageManagement
{
    public class Triage_SymptomUI : MonoBehaviour
    {
        public static Triage_SymptomUI Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject uiPanel;

        [Header("Sliders")]
        [SerializeField] private Slider tempSlider;
        [SerializeField] private Slider hrSlider;
        [SerializeField] private Slider bpSlider;

        [Header("Text Displays (TextMeshPro)")]
        [SerializeField] private TextMeshProUGUI tempText; // [VI] Số Nhiệt độ / [EN] Temperature Number
        [SerializeField] private TextMeshProUGUI hrText;   // [VI] Số Nhịp tim / [EN] Heart Rate Number
        [SerializeField] private TextMeshProUGUI bpText;   // [VI] Số Huyết áp / [EN] Blood Pressure Number

        [Header("Colors")]
        [SerializeField] private Color normalTextColor = Color.white;
        [SerializeField] private Color criticalTextColor = Color.red;
        [SerializeField] private float flashSpeed = 8f;

        private Triage_PatientBiology currentPatient;

        private void Awake()
        {
            if (Instance == null) { Instance = this; }
            else { Destroy(gameObject); }

            HideUI();
        }

        public void ShowUI(Triage_PatientBiology patient)
        {
            currentPatient = patient;
            uiPanel.SetActive(true);

            SetupSlider(tempSlider, patient.temperature);
            SetupSlider(hrSlider, patient.heartRate);
            SetupSlider(bpSlider, patient.bloodPressure);
        }

        public void HideUI()
        {
            currentPatient = null;
            uiPanel.SetActive(false);
        }

        private void SetupSlider(Slider slider, VitalSign sign)
        {
            // [VI] Đặt giới hạn Slider / [EN] Set Slider limits
            slider.minValue = sign.minDeathThreshold - 5f;
            slider.maxValue = sign.maxDeathThreshold + 5f;
        }

        private void Update()
        {
            if (currentPatient != null && uiPanel.activeSelf)
            {
                // [VI] Cập nhật thanh Slider / [EN] Update Sliders
                tempSlider.value = currentPatient.temperature.currentValue;
                hrSlider.value = currentPatient.heartRate.currentValue;
                bpSlider.value = currentPatient.bloodPressure.currentValue;

                // [VI] Cập nhật con số và làm chớp đỏ nếu nguy hiểm / [EN] Update numbers and flash red if critical
                UpdateStatText(tempText, currentPatient.temperature, "°C", false);
                UpdateStatText(hrText, currentPatient.heartRate, " bpm", true);
                UpdateStatText(bpText, currentPatient.bloodPressure, " mmHg", true);
            }
        }

        // [VI] Hàm hỗ trợ để format số và đổi màu / [EN] Helper function to format numbers and change colors
        private void UpdateStatText(TextMeshProUGUI textUI, VitalSign sign, string unit, bool isInteger)
        {
            if (textUI == null) return;

            // [VI] Định dạng hiển thị: Nhiệt độ có 1 chữ số thập phân (37.5), Nhịp tim/Huyết áp là số nguyên (120)
            // [EN] Display format: Temp has 1 decimal (37.5), HR/BP are integers (120)
            if (isInteger)
            {
                textUI.text = Mathf.RoundToInt(sign.currentValue).ToString() + unit;
            }
            else
            {
                textUI.text = sign.currentValue.ToString("F1") + unit;
            }

            // [VI] Kiểm tra xem chỉ số này có đang vượt ngưỡng chết không
            // [EN] Check if this specific stat is crossing the death threshold
            bool isFatal = sign.currentValue >= sign.maxDeathThreshold || sign.currentValue <= sign.minDeathThreshold;

            if (isFatal)
            {
                // [VI] Chớp tắt giữa Trắng và Đỏ / [EN] Flash between White and Red
                float lerp = Mathf.PingPong(Time.time * flashSpeed, 1f);
                textUI.color = Color.Lerp(normalTextColor, criticalTextColor, lerp);
            }
            else
            {
                // [VI] Màu bình thường / [EN] Normal color
                textUI.color = normalTextColor;
            }
        }
    }
}