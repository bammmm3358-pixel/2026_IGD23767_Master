using TMPro;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    [Header("Item")]
    public string itemName = "Item";
    public int amount = 1;
    public float healAmount = 0f;     // ใส่ค่า > 0 ถ้าอยากให้ของชิ้นนี้เพิ่มเลือด
    public bool isSword = false;      // ติ๊กถ้าเป็นดาบ: เก็บแล้วผู้เล่นถือดาบ

    [Header("Spin / Float (หมุนและลอย)")]
    public float spinSpeed = 90f;     // หมุนโชว์ของ (0 = ไม่หมุน)
    public float bobHeight = 0.15f;   // ลอยขึ้นลงสูงแค่ไหน (0 = ไม่ลอย)
    public float bobSpeed = 2f;
    public float hoverHeight = 0f;    // ยกไอเทมให้ลอยสูงจากจุดเกิด (ยาที่ดรอปแนะนำ 0.5)

    [Header("Lifetime (เวลาให้เก็บ)")]
    public float lifetime = 0f;       // วินาทีที่ไอเทมอยู่ได้ (0 = อยู่ตลอด เช่นดาบที่วางใน Scene)

    [Header("Label UI (ข้อความลอยเหนือไอเทม)")]
    public bool showLabel = true;
    public string labelText = "[E] PICK UP";
    public Vector3 labelOffset = new Vector3(0f, 1.2f, 0f);
    public float labelSize = 4f;
    public Color labelColor = Color.yellow;
    public Color labelWarnColor = Color.red;   // สีตอนเหลือ 2 วิสุดท้าย
    public TMP_FontAsset labelFont;            // ใส่ฟอนต์ไทยได้ถ้าอยากเปลี่ยนข้อความเป็นไทย

    float baseY;
    float spawnTime;
    TextMeshPro label;

    void Start()
    {
        baseY = transform.position.y + hoverHeight;
        spawnTime = Time.time;

        if (showLabel) CreateLabel();
    }

    void CreateLabel()
    {
        var go = new GameObject("PickupLabel");   // ไม่เป็นลูก เพื่อไม่ให้หมุนตามไอเทม
        label = go.AddComponent<TextMeshPro>();
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = labelSize;
        label.color = labelColor;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        if (labelFont != null) label.font = labelFont;
        label.text = labelText;
        UpdateLabelTransform();
    }

    void Update()
    {
        // หมุน
        if (spinSpeed != 0f)
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);

        // ลอยขึ้นลง
        Vector3 p = transform.position;
        p.y = baseY + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = p;

        // นับเวลาหมดอายุ
        float remaining = float.MaxValue;
        if (lifetime > 0f)
        {
            remaining = lifetime - (Time.time - spawnTime);
            if (remaining <= 0f)
            {
                Destroy(gameObject);
                return;
            }
        }

        if (label != null)
        {
            if (lifetime > 0f)
            {
                label.text = labelText + "\n" + Mathf.CeilToInt(remaining) + "s";
                label.color = remaining <= 2f ? labelWarnColor : labelColor;
            }
            UpdateLabelTransform();
        }
    }

    void UpdateLabelTransform()
    {
        label.transform.position = transform.position + labelOffset;
        Camera cam = Camera.main;
        if (cam != null) label.transform.rotation = cam.transform.rotation;   // หันหน้าเข้ากล้อง
    }

    void OnDestroy()
    {
        if (label != null) Destroy(label.gameObject);
    }

    public void Collect(PlayerController player)
    {
        player.AddItem(itemName, amount);
        if (isSword || itemName == "Sword") player.EquipSword();   // [แก้] ชื่อ Sword ก็นับเป็นดาบ ไม่ต้องพึ่งช่อง Is Sword
        if (healAmount > 0f)
        {
            var hp = player.GetComponent<Health>();
            if (hp != null) hp.Heal(healAmount);
        }
        if (AudioManager.Instance != null) AudioManager.Instance.PlayPickup();
        Destroy(gameObject);
    }
}