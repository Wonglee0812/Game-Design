using UnityEngine;

public class FireSource : MonoBehaviour
{
    public float health = 100f;
    public float damageToPlayerPerSecond = 15f;
    public ParticleSystem fireVFX;

    private bool isExtinguished = false;

    public void TakeDamage(float amount)
    {
        if (isExtinguished) return;

        health -= amount;

        if (health <= 0)
        {
            isExtinguished = true;
            if (fireVFX != null) fireVFX.Stop();

            GetComponent<Collider>().enabled = false;
            Destroy(gameObject, 3f); // [VI] Đợi 3s cho hạt lửa bay hết rồi xóa / [EN] Wait 3s for particles to clear then destroy
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isExtinguished) return;

        FireRescue_PlayerController firefighter = other.GetComponent<FireRescue_PlayerController>();
        if (firefighter != null)
        {
            firefighter.TakeDamage(damageToPlayerPerSecond); // Hoặc bất kỳ lượng sát thương nào bạn thiết lập / Or whatever damage amount you set
        }
    }
}