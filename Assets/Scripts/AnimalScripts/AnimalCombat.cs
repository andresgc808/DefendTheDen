using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;

public class AnimalCombat : MonoBehaviour, IAttacker
{
    [SerializeField] public float AttackRange { get; set; }
    [SerializeField]  public float AttackPower { get; set; }
    [SerializeField]  public float FireRate { get; set; }
    [SerializeField]  public AttackType attackType { get; set; }

    [SerializeField]  private float _timeSinceLastAttack;

    [SerializeField] public LayerMask whatIsEnemy;

    private BaseAttack _currentAttack;
    private bool _isInitialized = false;
    private void Start() {
        _currentAttack = this.GetComponent<BaseAttack>();
    }

    private void Update() {
        if (!_isInitialized) {
            _currentAttack = this.GetComponent<BaseAttack>();
            if (_currentAttack != null) {
                _isInitialized = true;
            }
            return;
        }
        if (_currentAttack == null) return;

        _timeSinceLastAttack += Time.deltaTime;

        if(_timeSinceLastAttack >= FireRate) {
            IDamageable target = SelectTarget(); // add later

            if (target != null) {
                // log self and target
                Debug.Log($"{this.gameObject.name} is attacking {target}");
                Attack(target);
                _timeSinceLastAttack = 0;
            } else {

                _timeSinceLastAttack = 0; // to prevent infinte logs, remove later
            }
        }
    }

    public void Attack(IDamageable target) {
        if (target == null) return;
        _currentAttack.PerformAttack(this.transform, target, AttackPower);
    }

    IDamageable SelectTarget() {
        Collider[] colliders = Physics.OverlapSphere(this.transform.position, AttackRange, whatIsEnemy);

        float minDistance = float.MaxValue;
        IDamageable closestTarget = null;

        foreach (var hitCollider in colliders) {
            IDamageable damageable = hitCollider.GetComponent<IDamageable>();

            if (damageable != null && damageable.IsAlive && !damageable.Equals(this.GetComponent<IDamageable>())) {
                float distance = Vector3.Distance(this.transform.position, hitCollider.transform.position);

                if (distance < minDistance) {
                    minDistance = distance;
                    closestTarget = damageable;
                }
            }
        }
        return closestTarget;
    }
}
