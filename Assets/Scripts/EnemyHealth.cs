using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class EnemyHealth : MonoBehaviour, IDamageable {

    private Enemy _enemyData; // Enemy scriptable object

    private float _currentHealth;
    public bool IsAlive { get { return _currentHealth > 0; } }

    public event Action OnDeath;

    public void SetEnemyData(Enemy data) {
        _enemyData = data;
        _currentHealth = _enemyData.maxHealth;
    }

    public void TakeDamage(float damage) {
        if (_currentHealth <= 0) return;

        _currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {_currentHealth}");
        if (!IsAlive) {
            OnDeath?.Invoke();
            Destroy(this.gameObject);
            Debug.Log($"{gameObject.name} has died.");
        }
    }
}