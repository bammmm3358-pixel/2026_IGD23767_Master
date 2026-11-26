using UnityEngine;

// วางไว้ใน Scene 1 ตัว แล้วลาก Potion จาก Hierarchy ใส่ช่อง Drop Object
// ศัตรูทุกตัว (รวมที่เกิดจาก SpawnPoint) จะมาใช้ต้นแบบนี้ตอนตาย
public class DropManager : MonoBehaviour
{
    public static DropManager Instance { get; private set; }

    [Tooltip("ลาก Potion จาก Hierarchy มาใส่ (ตัวนี้จะถูกซ่อนตอนเริ่มเกม)")]
    public GameObject dropObject;

    void Awake()
    {
        Instance = this;
        if (dropObject != null) dropObject.SetActive(false); // ซ่อนต้นแบบ ไม่ให้โผล่ในฉาก
    }

    public GameObject Spawn(GameObject source, Vector3 position)
    {
        if (source == null) return null;
        GameObject go = Instantiate(source, position, Quaternion.identity);
        go.SetActive(true);
        return go;
    }
}