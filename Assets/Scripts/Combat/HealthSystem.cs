using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public event EventHandler OnDie;
    public event EventHandler OnHealthChanged;

    [SerializeField] private float maxHealth = 10f;

    public bool IsAlive { get; private set; }

    public float MaxHealth
    {
        get => maxHealth;
        set => maxHealth = Mathf.Max(0, value);
    }

    private float currentHealth;
    public float CurrentHealth
    {
        get => currentHealth;
        private set
        {
            var newHealth = Mathf.Clamp(value, 0, MaxHealth);
            if (currentHealth != newHealth)
            {

                currentHealth = newHealth;
                OnHealthChanged?.Invoke(this, EventArgs.Empty);

                if (currentHealth <= 0)
                {
                    IsAlive = false;
                    OnDie?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    IsAlive = true;                    
                }
            }
        }
    }

    private void Start()
    {
        CurrentHealth = MaxHealth;
        IsAlive = true;
    }

    public void ApplyDamage(float damage)
    {
        if (damage < 0)
        {
            Debug.LogError("Damage cannot be negative.");
            return;
        }
        CurrentHealth -= damage;
    }

    public void Heal(float amount)
    {
        if (amount < 0)
        {
            Debug.LogError("Heal amount cannot be negative.");
            return;
        }
        CurrentHealth += amount;
    }

    public void HealToFull()
    {
        CurrentHealth = MaxHealth;
    }

    public void IncreaseMaxHealth(int addHealth)
    {
        MaxHealth += addHealth;
        HealToFull();
    }
    public float GetNormalizedHealth()
    {
        return CurrentHealth / MaxHealth;
    }
}
