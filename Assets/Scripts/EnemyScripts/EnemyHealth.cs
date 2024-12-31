using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class EnemyHealth : MonoBehaviour, IDamageable {

    private Enemy _enemyData; // Enemy scriptable object

    private float _currentHealth;

    private float _maxHealth;
    public bool IsAlive { get { return _currentHealth > 0; } }

    public event Action OnDeath;

    public HealthTracker healthTracker;

    public void Start() {
        if (healthTracker != null) // initally invisible
            healthTracker.gameObject.SetActive(false);
    }

    public void SetEnemyData(Enemy data) {
        _enemyData = data;
        _maxHealth = _enemyData.maxHealth;
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(float damage) {
        if (_currentHealth <= 0) return;

        _currentHealth -= damage;

        UpdateHealthUI();

        Debug.Log($"{gameObject.name} took {damage} damage. Health: {_currentHealth}");

        if (!IsAlive) {
            OnDeath?.Invoke();
            Destroy(this.gameObject);
            Debug.Log($"{gameObject.name} has died.");
        }
    }

    private void UpdateHealthUI() {
        healthTracker.gameObject.SetActive(true);

        if (healthTracker != null) {
            healthTracker.UpdateSliderValue(_currentHealth, _maxHealth);
        }
    }
}