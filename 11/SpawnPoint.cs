using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SpawnPoint : MonoBehaviour
{
    [Header("Enemy")]
    public GameObject enemyPrefab;

    [Header("Activation (ผู้เล่นเข้าใกล้ถึงจะเริ่มเกิด)")]
    public float activationRange = 20f;     // ระยะที่ผู้เล่นต้องเข้ามา (วงสีแดงใน Scene)
    public bool stopWhenPlayerFar = true;   // ผู้เล่นออกนอกระยะ = หยุดเกิด

    [Header("Spawning")]
    public float spawnInterval = 4f;        // เวลาระหว่างการเกิดแต่ละตัว
    public int maxAlive = 3;                // มีชีวิตพร้อมกันได้กี่ตัว
    public int maxTotalSpawns = 3;          // เกิดรวมต่อรอบได้กี่ตัว (0 = ไม่จำกัด และจะไม่มีการรีเซ็ตรอบ)
    public float spawnRadius = 3f;          // รัศมีสุ่มตำแหน่งเกิด (วงสีเหลือง)
    public float minDistanceFromPlayer = 6f; // ไม่เกิดใกล้ตัวผู้เล่นเกินไป

    [Header("Respawn (ฆ่าหมดทั้งรอบแล้วรอเกิดใหม่)")]
    public float respawnDelay = 300f;       // 300 วินาที = 5 นาที

    readonly List<GameObject> alive = new List<GameObject>();
    Transform player;
    float timer;
    int totalSpawned;
    bool wasActive;
    float respawnAt = -1f;

    void Start()
    {
        FindPlayer();
        timer = spawnInterval; // เข้าระยะแล้วเกิดตัวแรกทันที
    }

    void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (enemyPrefab == null) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        if (player == null) { FindPlayer(); return; }

        alive.RemoveAll(e => e == null);

        // ฆ่าหมดทั้งรอบแล้ว -> นับถอยหลัง 5 นาที (นับต่อแม้ผู้เล่นอยู่ไกล) แล้วเริ่มรอบใหม่
        bool batchDone = maxTotalSpawns > 0 && totalSpawned >= maxTotalSpawns;
        if (batchDone && alive.Count == 0)
        {
            if (respawnAt < 0f) respawnAt = Time.time + respawnDelay;
            if (Time.time < respawnAt) return;

            totalSpawned = 0;
            respawnAt = -1f;
            timer = spawnInterval;
        }

        // ผู้เล่นอยู่ในระยะหรือไม่ (วัดแนวราบ ไม่สนความสูง)
        Vector3 d = player.position - transform.position;
        d.y = 0f;
        bool active = d.magnitude <= activationRange;

        if (!active && stopWhenPlayerFar) { wasActive = false; return; }
        if (active && !wasActive) { wasActive = true; timer = spawnInterval; }

        if (maxTotalSpawns > 0 && totalSpawned >= maxTotalSpawns) return;

        timer += Time.deltaTime;
        if (timer < spawnInterval) return;

        if (alive.Count >= maxAlive) return;

        if (TryGetSpawnPosition(out Vector3 pos))
        {
            alive.Add(Instantiate(enemyPrefab, pos, Quaternion.identity));
            totalSpawned++;
            timer = 0f;
        }
    }

    bool TryGetSpawnPosition(out Vector3 pos)
    {
        // ลองสุ่มหลายครั้งให้ได้จุดที่ไม่ใกล้ผู้เล่นเกินไป
        for (int i = 0; i < 10; i++)
        {
            Vector2 r = Random.insideUnitCircle * spawnRadius;
            pos = transform.position + new Vector3(r.x, 0f, r.y);

            Vector3 dp = player.position - pos;
            dp.y = 0f;

            if (dp.magnitude >= minDistanceFromPlayer) return true;
        }

        pos = transform.position;
        return false;
    }

    // วาดวงในหน้า Scene (ต้องเปิดปุ่ม Gizmos)
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 1f);
        DrawFlatCircle(activationRange);

        Gizmos.color = Color.yellow;
        DrawFlatCircle(spawnRadius);

#if UNITY_EDITOR
        Handles.Label(transform.position + Vector3.up * 2f,
            "SpawnPoint\nแดง = ระยะที่ผู้เล่นต้องเข้ามา (" + activationRange + ")\nเหลือง = จุดที่ศัตรูเกิด (" + spawnRadius + ")");
#endif
    }

    void DrawFlatCircle(float radius)
    {
        const int seg = 48;
        Vector3 prev = transform.position + new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= seg; i++)
        {
            float a = i * Mathf.PI * 2f / seg;
            Vector3 next = transform.position + new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}