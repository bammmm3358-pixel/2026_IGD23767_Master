using UnityEngine;

// สคริปต์ตรวจสอบชั่วคราว: แปะที่ Player แล้วกด Play ดูข้อความใน Console
// ใช้เสร็จแล้วลบทิ้งได้เลย
public class DebugInventory : MonoBehaviour
{
    PlayerController player;

    void Start()
    {
        player = GetComponent<PlayerController>();
        if (player == null) { Debug.LogError("[DEBUG] สคริปต์นี้ต้องแปะที่ Player (ตัวที่มี PlayerController)"); return; }

        // ---- ดาบในมือ ----
        if (player.swordInHand == null)
            Debug.LogError("[DEBUG] Sword In Hand = ว่าง (ยังไม่ได้ลาก)");
        else
            Debug.Log("[DEBUG] Sword In Hand = " + player.swordInHand.name
                + " | เปิดอยู่=" + player.swordInHand.activeSelf
                + " | อยู่ในฉาก=" + player.swordInHand.scene.IsValid()
                + " (ถ้า false แปลว่าลากจาก Assets ผิดตัว)");

        // ---- HotbarUI ----
        var hb = FindFirstObjectByType<HotbarUI>(FindObjectsInactive.Include);
        if (hb == null)
        {
            Debug.LogError("[DEBUG] ไม่พบ HotbarUI ในฉากเลย (ยังไม่ได้ Add Component หรือ object ถูกลบ)");
        }
        else
        {
            Debug.Log("[DEBUG] พบ HotbarUI ที่ object: " + hb.name
                + " | เปิดอยู่=" + hb.gameObject.activeInHierarchy
                + " | enabled=" + hb.enabled
                + " | จำนวน Slots=" + (hb.slots == null ? 0 : hb.slots.Length)
                + " | จำนวน Item Icons=" + (hb.itemIcons == null ? 0 : hb.itemIcons.Length));

            if (hb.slots != null)
            {
                for (int i = 0; i < hb.slots.Length; i++)
                {
                    var s = hb.slots[i];
                    Debug.Log("[DEBUG] Slot " + i + ": Icon=" + (s.icon ? s.icon.name : "ว่าง")
                        + " | Count Text=" + (s.countText ? s.countText.name : "ว่าง"));
                }
            }

            if (hb.itemIcons != null)
            {
                foreach (var ic in hb.itemIcons)
                    Debug.Log("[DEBUG] Item Icon: ชื่อ='" + ic.itemName + "' sprite=" + (ic.sprite ? ic.sprite.name : "ว่าง"));
            }
        }

        player.OnItemAdded += OnItemAdded;
    }

    void OnDestroy()
    {
        if (player != null) player.OnItemAdded -= OnItemAdded;
    }

    void OnItemAdded(string itemName, int amount)
    {
        Debug.Log("[DEBUG] เก็บไอเทมชื่อ '" + itemName + "' จำนวน " + amount);
        Invoke(nameof(LogSword), 0.1f);
    }

    void LogSword()
    {
        Debug.Log("[DEBUG] หลังเก็บ: Has Sword=" + player.hasSword
            + " | ดาบในมือเปิดอยู่=" + (player.swordInHand ? player.swordInHand.activeSelf.ToString() : "ไม่มี"));
    }
}