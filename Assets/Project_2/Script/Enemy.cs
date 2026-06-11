using UnityEngine;
using UnityEngine.UI;
public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private int damageToPlayer = 1; // Số máu sẽ trừ khi quái đi hết đường
    [SerializeField] private int goldReward = 10; // Chỉnh trong Inspector
    public float speed = 2f;

    [Header("Health Settings")]
    public float startHealth = 10f; // Máu gốc lúc mới đẻ / Max health at spawn
    private float health;

    [Header("UI Component")]
    public Slider healthBar;

    private Transform target;
    private int wavepointIndex = 0;
    private void Start()
    {
        target = WayPoint.Points[0];
        // Thiết lập máu gốc và cập nhật thanh UI
        health = startHealth;
        if (healthBar != null)
        {
            healthBar.maxValue = startHealth;
            healthBar.value = health;
        }
    }

    private void Update()
    {
        Movement();
        if (healthBar != null)
        {
            // Ép thanh máu quay cùng hướng với góc nhìn của Camera
            // Force the health bar to match the Camera's rotation
            healthBar.transform.rotation = UnityEngine.Camera.main.transform.rotation;
        }
    }

    private void Movement()
    {
        Vector3 dir = target.position - transform.position;
        transform.Translate(dir.normalized * speed * Time.deltaTime, Space.World);
        if(Vector3.Distance(transform.position, target.position) <= 0.1f)
        {
            GetNextWaypoint();
        }
    }

    private void GetNextWaypoint()
    {

        if (wavepointIndex >= WayPoint.Points.Length - 1)
        {
            if (GameManager_2.instance != null)
            {
                GameManager_2.instance.TakeDamage(damageToPlayer);
            }

            Destroy(transform.gameObject);
            return;
        }
        wavepointIndex++;
        target = WayPoint.Points[wavepointIndex];
    }

    public void TakeDamage(int damge)
    {
        health -= damge;
        if (healthBar != null)
        {
            healthBar.value = health;
        }
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        GameManager_2.instance.AddGold(goldReward);
        Destroy(gameObject);
    }
}
