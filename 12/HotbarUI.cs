using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// ช่องไอเทมล่างหน้าจอ: เรียงตามลำดับที่เก็บ (ก่อนอยู่ซ้ายสุด) ใช้หมดแล้วช่องถัดไปเลื่อนมาแทน
// กดปุ่ม 1-4 เพื่อใช้ไอเทมในช่องนั้น
// แปะสคริปต์นี้ที่ object "Slot" (ตัวแม่ของ Icon ทั้งหมด)
public class HotbarUI : MonoBehaviour
{
    [System.Serializable]
    public class ItemIcon
    {
        public string itemName;   // ต้องตรงกับ Item Name ใน Pickup
        public Sprite sprite;
    }

    [System.Serializable]
    public class Slot
    {
        public Image icon;           // ลูก Image (ถ้าเว้นว่าง สคริปต์หาให้เอง)
        public TMP_Text countText;   // ลูก Text (TMP) (ถ้าเว้นว่าง สคริปต์หาให้เอง)
        public TMP_Text nameText;    // ไม่บังคับ
        [HideInInspector] public Sprite defaultSprite;   // [เพิ่ม] รูปเดิมของช่อง ใช้ตอนยังไม่ได้ใส่รูปใน Item Icons
    }

    class Entry
    {
        public string itemName;
        public int count;
    }

    public Slot[] slots;
    public ItemIcon[] itemIcons;

    readonly List<Entry> entries = new List<Entry>();
    PlayerController player;

    void Start()
    {
        AutoBind();   // [เพิ่ม] หา Icon / Count Text ให้เองถ้ายังไม่ได้ลาก

        // [เพิ่ม] จำรูปเดิมของแต่ละช่องไว้ก่อนจะถูกล้าง
        foreach (var s in slots)
            if (s.icon != null && s.defaultSprite == null) s.defaultSprite = s.icon.sprite;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.GetComponent<PlayerController>();
            if (player != null)
            {
                player.OnItemAdded += OnItemAdded;
                player.OnItemUsed += OnItemUsed;
            }
        }
        RefreshAll();
    }

    // [เพิ่ม] ผูกอัตโนมัติ: Image ที่ชื่อขึ้นต้นด้วย "Image" และ Text (TMP) ใต้ object นี้ ตามลำดับใน Hierarchy
    // (ช่องที่ลากเองไว้แล้วจะไม่ถูกแตะ)
    void AutoBind()
    {
        var imgs = new List<Image>();
        foreach (var im in GetComponentsInChildren<Image>(true))
            if (im.gameObject.name.StartsWith("Image")) imgs.Add(im);

        var txts = new List<TMP_Text>(GetComponentsInChildren<TMP_Text>(true));

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].icon == null && i < imgs.Count) slots[i].icon = imgs[i];
            if (slots[i].countText == null && i < txts.Count) slots[i].countText = txts[i];
        }
    }

    void OnDestroy()
    {
        if (player != null)
        {
            player.OnItemAdded -= OnItemAdded;
            player.OnItemUsed -= OnItemUsed;
        }
    }

    void Update()
    {
        if (player == null) return;
        for (int i = 0; i < slots.Length && i < 4; i++)
        {
            if (SlotKeyPressed(i)) UseSlot(i);
        }
    }

    void UseSlot(int index)
    {
        if (index >= entries.Count) return;
        player.UseItem(entries[index].itemName);
    }

    void OnItemAdded(string itemName, int amount)
    {
        var e = entries.Find(x => x.itemName == itemName);
        if (e != null) e.count += amount;
        else if (entries.Count < slots.Length)
            entries.Add(new Entry { itemName = itemName, count = amount });

        RefreshAll();
    }

    void OnItemUsed(string itemName, int amount)
    {
        var e = entries.Find(x => x.itemName == itemName);
        if (e == null) return;
        e.count -= amount;
        if (e.count <= 0) entries.Remove(e);
        RefreshAll();
    }

    Sprite FindSprite(string itemName)
    {
        if (itemIcons == null) return null;
        foreach (var i in itemIcons)
            if (i.itemName == itemName) return i.sprite;
        return null;
    }

    void RefreshAll()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            bool has = i < entries.Count;

            Sprite sp = has ? FindSprite(entries[i].itemName) : null;
            if (has && sp == null) sp = s.defaultSprite;   // [เพิ่ม] ยังไม่ได้ใส่รูปใน Item Icons = ใช้รูปเดิมของช่องไปก่อน

            if (s.icon)
            {
                s.icon.sprite = sp;
                s.icon.enabled = has && sp != null;
            }
            if (s.nameText) s.nameText.text = has ? entries[i].itemName : "";
            if (s.countText) s.countText.text = has ? entries[i].count.ToString() : "";
        }
    }

    bool SlotKeyPressed(int i)
    {
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        if (k == null) return false;
        switch (i)
        {
            case 0: return k.digit1Key.wasPressedThisFrame;
            case 1: return k.digit2Key.wasPressedThisFrame;
            case 2: return k.digit3Key.wasPressedThisFrame;
            case 3: return k.digit4Key.wasPressedThisFrame;
        }
        return false;
#else
        return Input.GetKeyDown(KeyCode.Alpha1 + i);
#endif
    }
}