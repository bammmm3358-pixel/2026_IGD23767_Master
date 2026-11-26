using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ใส่ที่บอส: ผู้เล่นเข้าใกล้ถึงระยะ แล้วจะมีหลอดเลือดใหญ่ขึ้นกลางบนหน้าจอ
[RequireComponent(typeof(Health))]
public class BossHealthBar : MonoBehaviour
{
    public string bossName = "BOSS";
    public float showRange = 15f;          // เข้าใกล้ระยะนี้หลอดใหญ่ถึงจะขึ้น
    public Color barColor = new Color(0.85f, 0.1f, 0.1f, 1f);

    [Header("UI ของตัวเอง (ไม่บังคับ)")]
    [Tooltip("ลากรูปกรอบ เช่น Boss / Alien จาก Assets/Ui มาใส่ (ต้องเป็น Sprite)")]
    public Sprite frameSprite;
    public float panelWidth = 900f;        // ความกว้างกรอบบนหน้าจอ (ความสูงคำนวณตามสัดส่วนรูป)
    [Tooltip("ถ้าช่องหลอดในรูปเป็นสีทึบ ให้ติ๊ก เพื่อวางเลือดไว้หน้ากรอบ")]
    public bool fillAboveFrame = false;
    [Header("ตำแหน่งช่องหลอดในรูป (สัดส่วน 0-1 จากขอบรูป)")]
    [Range(0f, 1f)] public float fillLeft = 0.124f;
    [Range(0f, 1f)] public float fillRight = 0.195f;
    [Range(0f, 1f)] public float fillTop = 0.463f;
    [Range(0f, 1f)] public float fillBottom = 0.297f;

    Health health;
    Transform player;
    GameObject panel;
    RectTransform fill;
    TextMeshProUGUI label;

    void Start()
    {
        health = GetComponent<Health>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        BuildUI();
        health.OnHealthChanged += UpdateBar;
        UpdateBar(health.current, health.maxHealth);
        panel.SetActive(false);
    }

    void BuildUI()
    {
        var cgo = new GameObject("BossHealthCanvas");
        var canvas = cgo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = cgo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        bool hasFrame = frameSprite != null;
        float h = 40f;
        float w = hasFrame ? panelWidth : 900f;
        if (hasFrame) h = w * frameSprite.rect.height / frameSprite.rect.width;

        panel = new GameObject("Panel", typeof(RectTransform));
        panel.transform.SetParent(cgo.transform, false);
        var prt = panel.GetComponent<RectTransform>();
        prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 1f);
        prt.pivot = new Vector2(0.5f, 1f);
        prt.anchoredPosition = new Vector2(0f, -30f);
        prt.sizeDelta = new Vector2(w, h);

        if (!hasFrame)
        {
            var bg = NewImage("BG", new Color(0f, 0f, 0f, 0.75f));
            StretchFull(bg.rectTransform);
        }

        if (!(hasFrame && fillAboveFrame)) CreateFill();
        if (hasFrame)
        {
            var frame = NewImage("Frame", Color.white);
            frame.sprite = frameSprite;
            frame.raycastTarget = false;
            StretchFull(frame.rectTransform);
        }
        if (hasFrame && fillAboveFrame) CreateFill();

        var tgo = new GameObject("Label");
        tgo.transform.SetParent(panel.transform, false);
        label = tgo.AddComponent<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 24;
        label.color = Color.white;
        // ข้อความอยู่กลางช่องหลอด (ไม่ใช่กลางทั้งกรอบ)
        var lrt = label.rectTransform;
        lrt.anchorMin = hasFrame ? new Vector2(fillLeft, fillBottom) : Vector2.zero;
        lrt.anchorMax = hasFrame ? new Vector2(1f - fillRight, 1f - fillTop) : Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
    }

    void CreateFill()
    {
        var f = NewImage("Fill", barColor);
        fill = f.rectTransform;
        fill.pivot = new Vector2(0f, 0.5f);
        fill.offsetMin = fill.offsetMax = Vector2.zero;
    }

    Image NewImage(string n, Color c)
    {
        var img = new GameObject(n).AddComponent<Image>();
        img.transform.SetParent(panel.transform, false);
        img.color = c;
        return img;
    }

    static void StretchFull(RectTransform r)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    void UpdateBar(float current, float max)
    {
        if (fill == null) return;
        float t = max > 0f ? Mathf.Clamp01(current / max) : 0f;

        bool hasFrame = frameSprite != null;
        float l = hasFrame ? fillLeft : 0.005f;
        float r = hasFrame ? fillRight : 0.005f;
        float tp = hasFrame ? fillTop : 0.1f;
        float b = hasFrame ? fillBottom : 0.1f;

        fill.anchorMin = new Vector2(l, b);
        fill.anchorMax = new Vector2(l + (1f - l - r) * t, 1f - tp);
        label.text = bossName + "  " + Mathf.CeilToInt(current) + " / " + Mathf.CeilToInt(max);
    }

    void OnValidate()
    {
        if (fill != null && health != null) UpdateBar(health.current, health.maxHealth);
    }

    void Update()
    {
        if (panel == null || player == null) return;
        Vector3 d = player.position - transform.position;
        d.y = 0f;
        bool show = !health.IsDead && d.magnitude <= showRange;
        if (panel.activeSelf != show) panel.SetActive(show);
    }

    void OnDestroy()
    {
        if (health != null) health.OnHealthChanged -= UpdateBar;
        if (panel != null) Destroy(panel.transform.root.gameObject);
    }
}