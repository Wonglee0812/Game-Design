using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("Reference Setup")]
    [SerializeField]private Transform rotatePart;
    [SerializeField]private Transform firePoint;
    [SerializeField]private GameObject bulletPrefab;
    [Header("Shoot Settings")]
    [SerializeField]private float range;
    [SerializeField]private float fireRate;
    [SerializeField] private LayerMask enemyLayer;
    [HideInInspector]
    public Transform target;
    private float fireCountDown;
    [Header("Poker Setup")]
    // Lá bài mặc định khi vừa xây xong / Default card upon building
    public Card currentCard = new Card { rank = 2, suit = "Hearts" };

    private void Start()
    {
        InvokeRepeating("updateTarget", 0f, 0.5f);
    }
    private void Update()
    {
        updateVisual();
        updateShoot();
    }

    private void updateVisual()
    {
        if (target != null)
        {
            Vector3 dir = target.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            Vector3 rotation = Quaternion.Lerp(rotatePart.rotation, lookRotation, Time.deltaTime * 5f).eulerAngles;
            rotatePart.rotation = Quaternion.Euler(0, rotation.y, 0);
        }    
    }    
    private void updateTarget()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, range, enemyLayer);
        float shortestDis = Mathf.Infinity;
        GameObject nearestE = null;
        foreach (Collider e in enemies)
        {
            float disToE = Vector3.Distance(transform.position, e.transform.position);
            if(disToE < shortestDis)
            {
                shortestDis = disToE;
                nearestE = e.transform.gameObject;
            }    
        }    
        target = nearestE != null ? nearestE.transform : null;
    }
    private void updateShoot()
    {
        if (target == null)
            return;
        if (fireCountDown <= 0)
        {
            Shoot();
            fireCountDown = 1f / fireRate;
        }

        fireCountDown -= Time.deltaTime;
    }
    private void Shoot()
    {
        //Debug.Log("SHOOT");
        if (bulletPrefab == null) return;
        GameObject bulletGo = (GameObject)Instantiate(bulletPrefab, firePoint.position, rotatePart.rotation);
        Bullet bullet = bulletGo.GetComponent<Bullet>();

        if (bullet != null)
        {
            // Truyền luôn lá bài của Turret cho viên đạn!
            // Pass the Turret's card directly to the bullet!
            bullet.Seek(target, currentCard);
        }
    }

    public void SetCardRank(int newRank)
    {
        currentCard.rank = newRank;
        Debug.Log($"Turret Rank updated: {newRank}");
    }

    // Hàm đổi Chất / Function to change Suit
    public void SetCardSuit(string newSuit)
    {
        currentCard.suit = newSuit;
        Debug.Log($"Turret Suit updated: {newSuit}");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }

}
