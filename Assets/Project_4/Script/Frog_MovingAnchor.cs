using UnityEngine;

public class Frog_MovingAnchor : MonoBehaviour
{
    public enum MoveAxis { HorizontalX, VerticalY, DepthZ }

    [Header("Movement Axis")]
    public MoveAxis chooseAxis = MoveAxis.VerticalY;

    [Header("Parameters")]
    public float speed = 2f;
    [Tooltip("Khoảng cách tối đa vật di chuyển tính từ vạch xuất phát")]
    public float moveDistance = 5f;

    private Vector3 startPosition;

    private void Start()
    {
        // [VI] Lưu lại vị trí xuất phát gốc để làm mốc toán học
        // [EN] Save the original starting position as a mathematical reference
        startPosition = transform.position;
    }

    private void Update()
    {
        Vector3 offset = Vector3.zero;

        // [VI] Tạo ra giá trị chạy tuần hoàn từ 0 đến moveDistance dựa trên thời gian
        // [EN] Create a periodic value running from 0 to moveDistance based on time
        float movement = Mathf.PingPong(Time.time * speed, moveDistance);

        switch (chooseAxis)
        {
            case MoveAxis.HorizontalX:
                offset = new Vector3(movement, 0, 0);
                break;
            case MoveAxis.VerticalY:
                offset = new Vector3(0, movement, 0);
                break;
            case MoveAxis.DepthZ:
                offset = new Vector3(0, 0, movement);
                break;
        }

        transform.position = startPosition + offset;
    }
}