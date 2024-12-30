using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;

public class AnimalHealth : MonoBehaviour, IDamageable {
    public float Health { get; set; }
    public float Armor { get; set; }
    public bool IsAlive { get { return Health > 0; } }

    public event Action OnDeath;

    public void TakeDamage(float damage) {
        if (Health <= 0) return;

        float damageReceived = damage - Armor;
        Health -= damageReceived; // armor can be changed later to be more complex

        Debug.Log($"{gameObject.name} took {damageReceived} damage. Health: {Health}");

        if (!IsAlive) {
            OnDeath?.Invoke();
            Debug.Log($"{gameObject.name} has died.");
        }
    }
}
