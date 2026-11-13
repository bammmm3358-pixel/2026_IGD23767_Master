using UnityEngine;

public class Pickup : MonoBehaviour
{
    public string itemName = "Item";
    public int amount = 1;
    public float healAmount = 0f;     // ใส่ค่า > 0 ถ้าอยากให้ของชิ้นนี้เพิ่มเลือด
    public float spinSpeed = 90f;     // หมุนโชว์ของ (0 = ไม่หมุน)

    void Update()
    {
        if (spinSpeed != 0f)
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
    }

    public void Collect(PlayerController player)
    {
        player.AddItem(itemName, amount);
        if (healAmount > 0f)
        {
            var hp = player.GetComponent<Health>();
            if (hp != null) hp.Heal(healAmount);
        }
        Destroy(gameObject);
    }
}
