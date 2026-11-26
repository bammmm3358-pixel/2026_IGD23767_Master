using UnityEngine;
using UnityEngine.UI;

// หลอดเลือดลอยเหนือหัวมอนเตอร์ สร้างอัตโนมัติตอนรันเกม (EnemyChase จะเพิ่มสคริปต์นี้ให้เอง)
[RequireComponent(typeof(Health))]
public class EnemyHealthBar : MonoBehaviour
{
    public float barWidth = 1.2f;          // ความกว้างหลอด (หน่วยโลก)
    public float barHeight = 0.15f;        // ความสูงหลอด (ใช้เมื่อไม่มีรูปกรอบ)
    public float heightOffset = 0.3f;      // ลอยเหนือหัวเพิ่มอีกเท่าไหร่
    public Color fullColor = Color.green;
    public Color lowColor = Color.red;

    [Header("UI ของตัวเอง (ไม่บังคับ)")]
    [Tooltip("ลากรูปกรอบจาก Assets/Ui มาใส่ (ต้องเป็น Sprite) ใส่ที่ Prefab ศัตรู")]
    public Sprite frameSprite;
    public bool fillAboveFrame = false;
    [Range(0f, 1f)] public float fillLeft = 0.22f;
    [Range(0f, 1f)] public float fillRight = 0.20f;
    [Range(0f, 1f)] public float fillTop = 0.38f;
    [Range(0f, 1f)] public float fillBottom = 0.38f;

    Health health;
    Transform barRoot;
    RectTransform fill;
    Image fillImage;
    float headY = 2f;

    void Start()
    {
        health = GetComponent<Health>();

        // หาความสูงหัวจาก Renderer ของตัวมอนเตอร์
        var rends = GetComponentsInChildren<Renderer>();
        if (rends.Length > 0)
        {
            Bounds b = rends[0].bounds;
            foreach (var r in rends) b.Encapsulate(r.bounds);
            headY = b.max.y - transform.position.y;
        }

        BuildBar();
        health.OnHealthChanged += UpdateBar;
        UpdateBar(health.current, health.maxHealth);
    }

    void BuildBar()
    {
        bool hasFrame = frameSprite != null;
        float h = hasFrame ? 100f * frameSprite.rect.height / frameSprite.rect.width
                           : 100f * barHeight / barWidth;

        var root = new GameObject("EnemyHealthBar");   // ไม่เป็นลูก จะได้ไม่โดน scale/หมุนของมอน
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        var rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100f, h);
        root.transform.localScale = Vector3.one * (barWidth / 100f);
        barRoot = root.transform;

        if (!hasFrame)
        {
            var bg = NewImage("BG", new Color(0f, 0f, 0f, 0.7f));
            Stretch(bg.rectTransform);
        }

        if (!(hasFrame && fillAboveFrame)) CreateFill();
        if (hasFrame)
        {
            var frame = NewImage("Frame", Color.white);
            frame.sprite = frameSprite;
            Stretch(frame.rectTransform);
        }
        if (hasFrame && fillAboveFrame) CreateFill();
    }

    void CreateFill()
    {
        fillImage = NewImage("Fill", fullColor);
        fill = fillImage.rectTransform;
        fill.pivot = new Vector2(0f, 0.5f);
        fill.offsetMin = fill.offsetMax = Vector2.zero;
    }

    Image NewImage(string n, Color c)
    {
        var img = new GameObject(n).AddComponent<Image>();
        img.transform.SetParent(barRoot, false);
        img.color = c;
        return img;
    }

    static void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }

    void UpdateBar(float current, float max)
    {
        if (fill == null) return;
        float t = max > 0f ? Mathf.Clamp01(current / max) : 0f;

        bool hasFrame = frameSprite != null;
        float l = hasFrame ? fillLeft : 0.01f;
        float r = hasFrame ? fillRight : 0.01f;
        float tp = hasFrame ? fillTop : 0.08f;
        float b = hasFrame ? fillBottom : 0.08f;

        fill.anchorMin = new Vector2(l, b);
        fill.anchorMax = new Vector2(l + (1f - l - r) * t, 1f - tp);
        fillImage.color = Color.Lerp(lowColor, fullColor, t);
    }

    void LateUpdate()
    {
        if (barRoot == null) return;
        barRoot.position = transform.position + Vector3.up * (headY + heightOffset);
        Camera cam = Camera.main;
        if (cam != null) barRoot.rotation = cam.transform.rotation;   // หันเข้ากล้อง
    }

    void OnDestroy()
    {
        if (health != null) health.OnHealthChanged -= UpdateBar;
        if (barRoot != null) Destroy(barRoot.gameObject);
    }
}