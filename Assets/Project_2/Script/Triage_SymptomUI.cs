using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem; // [VI] Dùng để lấy tọa độ chuột / [EN] Used to get mouse coordinates

namespace Portfolio.TriageManagement
{
    public class Triage_SymptomUI : MonoBehaviour
    {
        public static Triage_SymptomUI Instance { get; private set; }

        [Header("Main Panel Settings")]
        [SerializeField] private GameObject uiPanel;
        [SerializeField] private Vector2 offset = new Vector2(20f, -20f); // [VI] Lệch ra khỏi con trỏ / [EN] Offset from cursor
        private RectTransform rectTransform;

        [Header("Patient UI Elements")]
        [SerializeField] private GameObject patientContentContainer; // [VI] Nhóm chứa các thanh Slider / [EN] Group containing Sliders
        [SerializeField] private Slider tempSlider;
        [SerializeField] private Slider hrSlider;
        [SerializeField] private Slider bpSlider;
        [SerializeField] private TextMeshProUGUI tempText;
        [SerializeField] private TextMeshProUGUI hrText;
        [SerializeField] private TextMeshProUGUI bpText;

        [Header("Drug UI Elements")]
        [SerializeField] private GameObject drugContentContainer; // [VI] Nhóm chứa Text thông tin thuốc / [EN] Group containing Drug text
        [SerializeField] private TextMeshProUGUI drugInfoText;

        [Header("Colors")]
        [SerializeField] private Color normalTextColor = Color.white;
        [SerializeField] private Color criticalTextColor = Color.red;
        [SerializeField] private float flashSpeed = 8f;

        private Triage_PatientBiology currentPatient;

        private void Awake()
        {
            if (Instance == null) { Instance = this; }
            else { Destroy(gameObject); }

            rectTransform = uiPanel.GetComponent<RectTransform>();
            HideUI();
        }

        private void Update()
        {
            // [VI] LIÊN TỤC ĐI THEO CON TRỎ CHUỘT NẾU PANEL ĐANG BẬT
            // [EN] CONTINUOUSLY FOLLOW MOUSE CURSOR IF PANEL IS ACTIVE
            if (uiPanel.activeSelf)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                rectTransform.position = mousePos + offset;
            }

            // [VI] Cập nhật thông số bệnh nhân (nếu đang chỉ vào bệnh nhân)
            // [EN] Update patient stats (if pointing at a patient)
            if (currentPatient != null && patientContentContainer.activeSelf)
            {
                tempSlider.value = currentPatient.temperature.currentValue;
                hrSlider.value = currentPatient.heartRate.currentValue;
                bpSlider.value = currentPatient.bloodPressure.currentValue;

                UpdateStatText(tempText, currentPatient.temperature, "°C", false);
                UpdateStatText(hrText, currentPatient.heartRate, " bpm", true);
                UpdateStatText(bpText, currentPatient.bloodPressure, " mmHg", true);
            }
        }

        // ==========================================
        // [VI] HÀM HIỂN THỊ DÀNH CHO BỆNH NHÂN
        // [EN] SHOW FUNCTION FOR PATIENTS
        // ==========================================
        public void ShowPatientUI(Triage_PatientBiology patient)
        {
            currentPatient = patient;

            // [VI] Bật nội dung bệnh nhân, Tắt nội dung thuốc
            // [EN] Enable patient content, Disable drug content
            patientContentContainer.SetActive(true);
            drugContentContainer.SetActive(false);

            uiPanel.SetActive(true);

            SetupSlider(tempSlider, patient.temperature);
            SetupSlider(hrSlider, patient.heartRate);
            SetupSlider(bpSlider, patient.bloodPressure);

            // [VI] Cập nhật ngay lập tức vị trí để không bị giật khung hình đầu
            // [EN] Immediately update position to avoid first-frame stutter
            rectTransform.position = Mouse.current.position.ReadValue() + offset;
        }

        // ==========================================
        // [VI] HÀM HIỂN THỊ DÀNH CHO THUỐC
        // [EN] SHOW FUNCTION FOR DRUGS
        // ==========================================
        public void ShowDrugUI(string drugName, float cost, string effectDescription)
        {
            currentPatient = null; // [VI] Không dính dáng đến bệnh nhân / [EN] Disconnect from patient

            // [VI] Tắt nội dung bệnh nhân, Bật nội dung thuốc
            // [EN] Disable patient content, Enable drug content
            patientContentContainer.SetActive(false);
            drugContentContainer.SetActive(true);

            // [VI] Ghi thông tin vào Text
            // [EN] Write info into Text
            drugInfoText.text = $"<b>{drugName}</b>\nCost: ${cost}\nEffect: {effectDescription}";

            uiPanel.SetActive(true);

            rectTransform.position = Mouse.current.position.ReadValue() + offset;
        }

        public void ShowStaffUI(float wage, float errorChance)
        {
            currentPatient = null;

            patientContentContainer.SetActive(false);
            drugContentContainer.SetActive(true); // [VI] Tận dụng lại khung text của thuốc cho nhanh / [EN] Reuse drug text frame for speed

            float errorPercent = errorChance * 100f;
            drugInfoText.text = $"<b>Nurse/Assistant</b>\nWage: ${wage}/10s\nFatal Error Rate: {errorPercent}%";

            uiPanel.SetActive(true);
            rectTransform.position = Mouse.current.position.ReadValue() + offset;
        }

        public void HideUI()
        {
            currentPatient = null;
            uiPanel.SetActive(false);
        }

        private void SetupSlider(Slider slider, VitalSign sign)
        {
            slider.minValue = sign.minDeathThreshold - 5f;
            slider.maxValue = sign.maxDeathThreshold + 5f;
        }

        private void UpdateStatText(TextMeshProUGUI textUI, VitalSign sign, string unit, bool isInteger)
        {
            if (textUI == null) return;

            if (isInteger) textUI.text = Mathf.RoundToInt(sign.currentValue).ToString() + unit;
            else textUI.text = sign.currentValue.ToString("F1") + unit;

            bool isFatal = sign.currentValue >= sign.maxDeathThreshold || sign.currentValue <= sign.minDeathThreshold;
            if (isFatal)
            {
                float lerp = Mathf.PingPong(Time.time * flashSpeed, 1f);
                textUI.color = Color.Lerp(normalTextColor, criticalTextColor, lerp);
            }
            else
            {
                textUI.color = normalTextColor;
            }
        }
    }
}