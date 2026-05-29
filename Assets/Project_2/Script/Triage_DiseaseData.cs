using UnityEngine;

namespace Portfolio.TriageManagement
{
    // [VI] Danh sách các loại thuốc / [EN] List of drug types
    public enum DrugType
    {
        None,
        Paracetamol, // Hạ sốt / Reduces fever
        Adrenaline,  // Tăng nhịp tim / Increases heart rate
        Saline       // Tăng huyết áp / Increases blood pressure
    }

    // [VI] Cấu trúc của 1 chỉ số sinh tồn / [EN] Structure of 1 vital sign
    [System.Serializable]
    public class VitalSign
    {
        public float currentValue;
        public float normalValue;
        public float maxDeathThreshold; // Mức cao gây tử vong / High fatal threshold
        public float minDeathThreshold; // Mức thấp gây tử vong / Low fatal threshold
    }

    // [VI] Tạo menu click chuột phải để tạo Bệnh mới / [EN] Create right-click menu to make new Disease
    [CreateAssetMenu(fileName = "NewDisease", menuName = "Triage/Disease Data")]
    public class Triage_DiseaseData : ScriptableObject
    {
        public string diseaseName;

        [Header("Stat Modifiers (Per Second)")]
        public float tempModifier; // [VI] Lực làm thay đổi nhiệt độ / [EN] Force changing temperature
        public float hrModifier;   // [VI] Lực làm thay đổi nhịp tim / [EN] Force changing heart rate
        public float bpModifier;   // [VI] Lực làm thay đổi huyết áp / [EN] Force changing blood pressure

        [Header("Cure Condition")]
        public DrugType correctCure; // [VI] Thuốc đúng để chữa / [EN] The correct drug to cure
    }
}