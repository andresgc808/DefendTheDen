using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;

public class AnimalHealth : MonoBehaviour, IDamageable {
    public float MaxHealth;
    public float Health { get; set; }
    public float Armor { get; set; }
    public bool IsAlive { get { return Health > 0; } }

    public event Action OnDeath;

    public HealthTracker healthTracker;

    public void Start() {
        MaxHealth = Health;

        if (healthTracker != null) // initally invisible
            healthTracker.gameObject.SetActive(false);
    }

    public void TakeDamage(float damage) {
        if (Health <= 0) return;

        float damageReceived = damage - Armor;
        Health -= damageReceived; // armor can be changed later to be more complex

        Debug.Log($"{gameObject.name} took {damageReceived} damage. Health: {Health}");

        UpdateHealthUI();

        if (!IsAlive) {
            OnDeath?.Invoke();
            Debug.Log($"{gameObject.name} has died.");
        }
    }

    private void UpdateHealthUI() {
        healthTracker.gameObject.SetActive(true);

        if (healthTracker != null) {
            healthTracker.UpdateSliderValue(Health, MaxHealth);
        }
    }
}
