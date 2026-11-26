using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                 // เว้นว่างได้ จะหาตัวที่มี Tag "Player" เอง
    public Vector3 lookOffset = new Vector3(0f, 1f, 0f);   // จุดที่กล้องมอง (เหนือเท้าตัวละคร)

    [Header("Position")]
    public float distance = 6f;              // ระยะห่างจากตัวละคร
    public float height = 3f;                // ความสูงของกล้อง
    public float yaw = 0f;                   // มุมหันของกล้องรอบตัวละคร (0 = อยู่ด้านหลังแกน Z-)
    public float smoothTime = 0.15f;         // ยิ่งมากยิ่งหน่วง

    [Header("Orbit (กดเมาส์ขวาค้างแล้วลาก)")]
    public bool allowOrbit = true;
    public float orbitSpeed = 0.2f;

    Vector3 velocity;

    void Start()
    {
        if (target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }
        if (target != null) SnapToTarget();
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (allowOrbit && RightMouseHeld())
            yaw += MouseDeltaX() * orbitSpeed;

        Vector3 desired = DesiredPosition();
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        transform.LookAt(target.position + lookOffset);
    }

    Vector3 DesiredPosition()
    {
        Quaternion rot = Quaternion.Euler(0f, yaw, 0f);
        Vector3 back = rot * Vector3.back * distance;
        return target.position + back + Vector3.up * height;
    }

    void SnapToTarget()
    {
        transform.position = DesiredPosition();
        transform.LookAt(target.position + lookOffset);
    }

    bool RightMouseHeld()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.rightButton.isPressed;
#else
        return Input.GetMouseButton(1);
#endif
    }

    float MouseDeltaX()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.delta.x.ReadValue() : 0f;
#else
        return Input.GetAxis("Mouse X") * 5f;
#endif
    }
}