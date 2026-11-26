using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Slider healthBar;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI scoreText;

    [Header("Weapon Slot (ช่องล่างหน้าจอ)")]
    public Image swordSlotBg;               // พื้นหลังช่อง
    public Image swordIcon;                 // ไอคอนดาบ (ใส่ Sprite ถ้ามี ไม่ใส่ก็ได้)
    public TextMeshProUGUI swordLabel;      // ข้อความในช่อง
    public Color slotEmptyColor = new Color(0f, 0f, 0f, 0.5f);
    public Color slotEquippedColor = new Color(1f, 0.8f, 0.1f, 0.9f);
    public Color iconEmptyColor = new Color(1f, 1f, 1f, 0.2f);
    public Color iconEquippedColor = Color.white;

    Health playerHealth;
    PlayerController playerCtrl;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHealth;
                UpdateHealth(playerHealth.CurrentHealth, playerHealth.maxHealth);
            }

            playerCtrl = player.GetComponent<PlayerController>();
            if (playerCtrl != null)
            {
                playerCtrl.OnSwordChanged += UpdateSword;
                UpdateSword(playerCtrl.hasSword);
            }
        }
        else
        {
            UpdateSword(false);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScore;
            UpdateScore(GameManager.Instance.Score);
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null) playerHealth.OnHealthChanged -= UpdateHealth;
        if (playerCtrl != null) playerCtrl.OnSwordChanged -= UpdateSword;
        if (GameManager.Instance != null) GameManager.Instance.OnScoreChanged -= UpdateScore;
    }

    void UpdateHealth(float current, float max)
    {
        if (healthBar) { healthBar.maxValue = max; healthBar.value = current; }
        if (healthText) healthText.text = Mathf.CeilToInt(current) + " / " + Mathf.CeilToInt(max);
    }

    void UpdateScore(int score)
    {
        if (scoreText) scoreText.text = "Score: " + score;
    }

    void UpdateSword(bool has)
    {
        if (swordSlotBg) swordSlotBg.color = has ? slotEquippedColor : slotEmptyColor;
        if (swordIcon) swordIcon.color = has ? iconEquippedColor : iconEmptyColor;
        if (swordLabel) swordLabel.text = has ? "SWORD" : "EMPTY";
    }
}