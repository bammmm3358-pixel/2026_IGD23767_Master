using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyChase : MonoBehaviour
{
    public float moveSpeed = 2.5f;
    public float detectRange = 15f;
    public float attackRange = 1.5f;
    public int attackDamage = 10;
    public float attackCooldown = 1f;
    public int scoreValue = 10;

    [Header("Drop (ไอเทมที่ดรอปตอนตาย)")]
    [Tooltip("เว้นว่างได้ ถ้าเว้นว่างจะใช้ของจาก DropManager ในฉาก")]
    public GameObject dropPrefab;                 // ลากของจาก Scene หรือ Prefab ก็ได้
    [Range(0f, 1f)] public float dropChance = 1f; // 1 = ดรอปทุกตัว, 0.3 = 30%

    Transform player;
    Health playerHealth;
    Health myHealth;
    float nextAttackTime;

    void Start()
    {
        myHealth = GetComponent<Health>();
        myHealth.OnDeath += Die;

        // หลอดเลือดเหนือหัวมอน (เพิ่มให้อัตโนมัติ ไม่ต้องตั้งค่าในแต่ละตัว)
        if (GetComponent<EnemyHealthBar>() == null) gameObject.AddComponent<EnemyHealthBar>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            playerHealth = p.GetComponent<Health>();
        }
    }

    void Update()
    {
        if (player == null || myHealth.IsDead) return;
        if (playerHealth != null && playerHealth.IsDead) return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        float dist = toPlayer.magnitude;

        if (dist > detectRange) return;

        if (dist > attackRange)
        {
            Vector3 dir = toPlayer.normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
        }
        else if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            if (playerHealth != null) playerHealth.TakeDamage(attackDamage);
        }
    }

    void Die()
    {
        if (GameManager.Instance != null) GameManager.Instance.AddScore(scoreValue);

        if (Random.value <= dropChance)
        {
            GameObject source = dropPrefab;
            if (source == null && DropManager.Instance != null) source = DropManager.Instance.dropObject;

            if (source != null)
            {
                GameObject item = Instantiate(source, transform.position, Quaternion.identity);
                item.SetActive(true);   // เผื่อต้นแบบถูกซ่อนไว้
            }
        }

        Destroy(gameObject);
    }
}