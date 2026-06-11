using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Transform effect;
    [SerializeField] private Card cardProperty;
    private Transform target;

    private void Update()
    {
        followTarget();
    }
    public void Seek(Transform _target, Card card)
    {
        target = _target;
        cardProperty = card;
    }

    private void followTarget()
    {
        if (target == null)
        {
            // Tự dọn dẹp bản thân nếu mục tiêu đã biến mất
            // Clean itself up if the target has disappeared
            Destroy(gameObject);
            return;
        }
        Vector3 dir = target.position - transform.position;
        Quaternion lookRotate = Quaternion.LookRotation(dir);
        Vector3 roation = Quaternion.Lerp(transform.rotation, target.rotation, Time.deltaTime * 5f).eulerAngles;
        transform.rotation = Quaternion.Euler(0, roation.y, 0);

        float disThisFrame = speed * Time.deltaTime;
        if(dir.magnitude < disThisFrame)
        {
            HitTarget();
            return;
        }    
        transform.Translate(dir.normalized * speed * Time.deltaTime, Space.World);
    }

    private void HitTarget()
    {
        //Debug.Log("Hit");
        target.GetComponent<Enemy>().TakeDamage(1);
        EnemyCardHandler handler = target.GetComponent<EnemyCardHandler>();
        if (handler != null)
        {
            handler.AddCard(this.cardProperty); // cardData là biến Long đã gán từ Turret
        }
        Transform eff = Instantiate(effect, transform.position, Quaternion.identity);
        Destroy(eff.gameObject, 1f);
        Destroy(transform.gameObject);
    }    
}
