using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;
using System;

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

    private bool _isAttacking = false; // variable state
    public bool IsAttacking { get { return _isAttacking; } } // read only property.

    public event Action OnAttackStart;
    public event Action OnAttackEnd;

    public Trait appliedTrait;

    private UnitMovement _unitMovement;

    private void Awake() {
        _currentAttack = this.GetComponent<BaseAttack>();
        _unitMovement = GetComponent<UnitMovement>();
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

        if (_unitMovement != null && !_unitMovement.IsMoving) {
            if (_timeSinceLastAttack >= FireRate) {
                IDamageable target = SelectTarget();

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
    }

    private IEnumerator AttackSequence(IDamageable target) {

        yield return new WaitForSeconds(1.5f); // wait time

        if (target == null) yield break;

        _currentAttack.PerformAttack(this.transform, target, AttackPower, appliedTrait);



        _isAttacking = false; // sets flag as attack has finished.
        OnAttackEnd?.Invoke();  // send event to subscribers.
    }

    public void Attack(IDamageable target) {
        if (target == null) return;
        _isAttacking = true; // set state to attack
        OnAttackStart?.Invoke();  // trigger event that state is now attacking

        if (attackType == AttackType.Projectile) {
            Debug.Log("Projectile Attack");
            StartCoroutine(AttackSequence(target));
        } else {
            // only projectile attacks have traits for now
            _currentAttack.PerformAttack(this.transform, target, AttackPower, null);
            _isAttacking = false; // sets flag as attack has finished.
            OnAttackEnd?.Invoke();  // send event to subscribers.
        }
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
