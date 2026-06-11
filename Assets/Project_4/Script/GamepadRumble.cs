using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

// =========================================================
// [VI] CLASS GAMEPAD RUMBLE: Quản lý tính năng rung tay cầm
// [EN] GAMEPAD RUMBLE CLASS: Manages gamepad vibration
// =========================================================
public class GamepadRumble : MonoBehaviour
{
    public static GamepadRumble Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // [VI] HÀM GỌI RUNG: Truyền vào độ rung trầm (trái), độ rung thanh (phải) và thời gian
    // [EN] RUMBLE CALL: Pass in low frequency (left), high frequency (right), and duration
    public void Rumble(float lowFrequency, float highFrequency, float duration)
    {
        // [VI] Lấy tay cầm đang được sử dụng hiện tại
        // [EN] Get the currently used gamepad
        Gamepad gamepad = Gamepad.current;

        if (gamepad != null)
        {
            // [VI] Kích hoạt 2 mô-tơ (giá trị từ 0.0f đến 1.0f)
            // [EN] Activate the 2 motors (values from 0.0f to 1.0f)
            gamepad.SetMotorSpeeds(lowFrequency, highFrequency);

            // [VI] Dừng Coroutine cũ (nếu có) để tránh lỗi đè thời gian
            // [EN] Stop old Coroutine (if any) to avoid time overlap errors
            StopAllCoroutines();
            StartCoroutine(StopRumbleAfterTime(gamepad, duration));
        }
    }

    private IEnumerator StopRumbleAfterTime(Gamepad gamepad, float duration)
    {
        yield return new WaitForSeconds(duration);

        // [VI] Trả rung về 0
        // [EN] Reset rumble to 0
        if (gamepad != null)
        {
            gamepad.SetMotorSpeeds(0f, 0f);
        }
    }

    private void OnDisable()
    {
        // [VI] QUAN TRỌNG: Đảm bảo tắt rung khi thoát game/tắt script, nếu không tay cầm sẽ rung mãi
        // [EN] IMPORTANT: Ensure rumble stops when exiting game/disabling script, else controller vibrates forever
        if (Gamepad.current != null)
        {
            Gamepad.current.PauseHaptics();
            Gamepad.current.ResetHaptics();
        }
    }
}