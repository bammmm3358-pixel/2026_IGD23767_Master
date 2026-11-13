using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Health))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float turnSpeed = 720f;
    public float jumpHeight = 1.2f;
    public float gravity = -20f;

    [Header("Attack")]
    public float attackRange = 1.5f;
    public float attackDamage = 25f;
    public float attackHitDelay = 0.25f;   // เวลาก่อนดาเมจเข้า (ให้ตรงกับจังหวะฟันในอนิเมชัน)
    public float attackDuration = 0.6f;    // เวลารวมของอนิเมชันตี
    public LayerMask hitLayers = ~0;

    [Header("Pickup")]
    public float pickupRange = 2f;
    public float pickupDuration = 0.8f;

    [Header("Animation state names (ต้องตรงกับชื่อ State ใน Animator)")]
    public Animator animator;
    public string idleState = "idle";
    public string walkState = "walk";
    public string sprintState = "sprint";
    public string jumpState = "jump";
    public string attackStateA = "attack-melee-right";
    public string attackStateB = "attack-melee-left";
    public string pickupState = "pick-up";
    public string dieState = "die";

    [Header("Inventory (ดูค่าได้ตอนเล่น)")]
    public int totalItems;
    public List<string> inventory = new List<string>();

    CharacterController cc;
    Health health;
    float velocityY;
    bool busy;
    bool useAttackA = true;
    string currentState;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        health = GetComponent<Health>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        health.onDeath.AddListener(OnDeath);
    }

    void Update()
    {
        if (health.IsDead) return;

        // ---------- Gravity / Jump ----------
        bool grounded = cc.isGrounded;
        if (grounded && velocityY < 0f) velocityY = -2f;

        Vector2 input = busy ? Vector2.zero : ReadMove();
        bool sprint = ReadSprint();

        if (!busy && grounded && ReadJumpPressed())
        {
            velocityY = Mathf.Sqrt(jumpHeight * -2f * gravity);
            Play(jumpState);
        }
        velocityY += gravity * Time.deltaTime;

        // ---------- Move (อิงทิศของกล้อง) ----------
        Vector3 dir = Vector3.zero;
        if (input.sqrMagnitude > 0.01f)
        {
            Transform cam = Camera.main != null ? Camera.main.transform : transform;
            Vector3 fwd = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
            dir = (fwd * input.y + right * input.x).normalized;

            Quaternion target = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.deltaTime);
        }

        float speed = sprint ? sprintSpeed : walkSpeed;
        Vector3 move = dir * speed;
        move.y = velocityY;
        cc.Move(move * Time.deltaTime);

        // ---------- Animation ----------
        if (!busy)
        {
            if (!grounded) { /* ปล่อยให้อนิเมชันกระโดดเล่นต่อ */ }
            else if (dir != Vector3.zero) Play(sprint ? sprintState : walkState);
            else Play(idleState);
        }

        // ---------- Actions ----------
        if (!busy && grounded)
        {
            if (ReadAttackPressed()) StartCoroutine(AttackRoutine());
            else if (ReadInteractPressed()) TryPickup();
        }

        // ปุ่มทดสอบ: K = โดนดาเมจ 25
        if (ReadTestDamagePressed()) health.TakeDamage(25f);
    }

    // ================= Actions =================
    IEnumerator AttackRoutine()
    {
        busy = true;
        Play(useAttackA ? attackStateA : attackStateB, true);
        useAttackA = !useAttackA;

        yield return new WaitForSeconds(attackHitDelay);

        if (!health.IsDead)
        {
            Vector3 center = transform.position + Vector3.up * 0.5f + transform.forward * (attackRange * 0.5f);
            Collider[] hits = Physics.OverlapSphere(center, attackRange, hitLayers);
            var already = new HashSet<Health>();
            foreach (var h in hits)
            {
                Health target = h.GetComponentInParent<Health>();
                if (target != null && target != health && already.Add(target))
                    target.TakeDamage(attackDamage);
            }
        }

        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - attackHitDelay));
        busy = false;
    }

    void TryPickup()
    {
        Pickup nearest = null;
        float best = float.MaxValue;
        foreach (var c in Physics.OverlapSphere(transform.position, pickupRange))
        {
            Pickup p = c.GetComponentInParent<Pickup>();
            if (p == null) continue;
            float d = (p.transform.position - transform.position).sqrMagnitude;
            if (d < best) { best = d; nearest = p; }
        }
        if (nearest != null) StartCoroutine(PickupRoutine(nearest));
    }

    IEnumerator PickupRoutine(Pickup item)
    {
        busy = true;
        Vector3 look = item.transform.position - transform.position;
        look.y = 0f;
        if (look.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(look);

        Play(pickupState, true);
        yield return new WaitForSeconds(pickupDuration * 0.5f);
        if (item != null) item.Collect(this);
        yield return new WaitForSeconds(pickupDuration * 0.5f);
        busy = false;
    }

    public void AddItem(string itemName, int amount)
    {
        totalItems += amount;
        for (int i = 0; i < amount; i++) inventory.Add(itemName);
    }

    void OnDeath()
    {
        StopAllCoroutines();
        busy = true;
        Play(dieState, true);
    }

    // ================= Animation helper =================
    void Play(string state, bool force = false)
    {
        if (animator == null || string.IsNullOrEmpty(state)) return;
        if (!force && currentState == state) return;
        currentState = state;
        animator.CrossFadeInFixedTime(state, 0.1f);
    }

    // ================= Input (รองรับทั้ง Input System ใหม่และเก่า) =================
    Vector2 ReadMove()
    {
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        if (k == null) return Vector2.zero;
        float x = (k.dKey.isPressed ? 1f : 0f) - (k.aKey.isPressed ? 1f : 0f);
        float y = (k.wKey.isPressed ? 1f : 0f) - (k.sKey.isPressed ? 1f : 0f);
        return new Vector2(x, y);
#else
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
    }

    bool ReadSprint()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
#else
        return Input.GetKey(KeyCode.LeftShift);
#endif
    }

    bool ReadJumpPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Space);
#endif
    }

    bool ReadAttackPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    bool ReadInteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }

    bool ReadTestDamagePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.K);
#endif
    }

    // ================= Simple HUD =================
    void OnGUI()
    {
        GUI.Label(new Rect(15, 10, 300, 25), "HP: " + Mathf.CeilToInt(health.current) + " / " + health.maxHealth);
        GUI.Label(new Rect(15, 30, 300, 25), "Items: " + totalItems);
        if (health.IsDead) GUI.Label(new Rect(15, 50, 300, 25), "YOU DIED");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f + transform.forward * (attackRange * 0.5f), attackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}