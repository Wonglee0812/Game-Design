// Định nghĩa các loại Input cho phép / Define allowed Input types
using UnityEngine;

public enum SupportedInput { Any, GamepadOnly, KeyboardOnly }

[CreateAssetMenu(fileName = "NewGameLevel", menuName = "PartyGame/LevelData")]
public class GameLevelData : ScriptableObject
{
    [Header("Level Info")]
    public string gameName;          // Tên game hiển thị trên UI / Game name shown on UI
    public string sceneName;         // Tên Scene thật để Load / Real Scene name to Load
    public Sprite levelIcon;         // Hình ảnh 6 ô map / The image for the 6 map tiles

    [Header("Requirements")]
    public int minPlayers = 1;       // Số người tối thiểu / Minimum players
    public int maxPlayers = 2;       // Số người tối đa / Maximum players
    public SupportedInput inputType = SupportedInput.Any; // Yêu cầu tay cầm/phím / Controller/Keyboard requirement
}