using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine;

using UnityEngine.Events;



public class Health : MonoBehaviour

{
    public float maxHealth = 100f;
    public float current;
    public UnityEvent onDeath = new UnityEvent();   
    public event Action OnDeath;                    
    public event Action<float, float> OnHealthChanged; 

    public float CurrentHealth => current;
    public bool IsDead => current <= 0f;
    void Awake() { current = maxHealth; }
    void Start() { OnHealthChanged?.Invoke(current, maxHealth); }
    public void TakeDamage(float amount)

    {

        if (IsDead) return;
        current = Mathf.Max(0f, current - amount);
        OnHealthChanged?.Invoke(current, maxHealth);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayHurt();
        if (IsDead)
        {

            onDeath.Invoke();

            OnDeath?.Invoke();

        }

    }

    public void Heal(float amount)

    {
    
        if (IsDead) return;
        current = Mathf.Min(maxHealth, current + amount);
        OnHealthChanged?.Invoke(current, maxHealth);
    }


}