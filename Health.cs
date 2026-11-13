using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float current;
    public UnityEvent onDeath;
    public UnityEvent onHurt;

    public bool IsDead { get; private set; }

    void Awake()
    {
        current = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        current = Mathf.Max(0f, current - amount);
        onHurt?.Invoke();
        if (current <= 0f)
        {
            IsDead = true;
            onDeath?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        current = Mathf.Min(maxHealth, current + amount);
    }
}
