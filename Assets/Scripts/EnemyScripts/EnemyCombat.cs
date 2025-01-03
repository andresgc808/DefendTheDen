using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : MonoBehaviour, IAttacker {
    private Enemy _enemyData; // Enemy scriptable object
    public float AttackRange { get; set; }
    public float AttackPower { get; set; }
    public float FireRate { get; set; }

    [SerializeField] private float _timeSinceLastAttack;
    [SerializeField] public LayerMask whatIsTarget;

    private IDamageable _currentTarget;

    public bool IsAttacking { get; private set; } = false;

    private Animator _animator;

    private BaseAttack _currentAttack;

    public void SetEnemyData(Enemy data) {
        _enemyData = data;
        AttackRange = _enemyData.attackRange;
        AttackPower = _enemyData.attackPower;
        FireRate = _enemyData.fireRate;
    }

    void Start() {
        _animator = GetComponentInChildren<Animator>();
        var attackComponent = this.gameObject.AddComponent(System.Type.GetType(_enemyData.attackType.ToString() + "Attack")) as BaseAttack;
        if (attackComponent is ProjectileAttack projectileAttack && _enemyData.attackType == AttackType.Projectile) {
            // we will not load a projectile this way but it can be done.
             projectileAttack.LoadProjectilePrefab($"{_enemyData.enemyName}Projectile");
        }
        _currentAttack = attackComponent;
    }

    void Update() {
        if (_currentTarget == null) return;
            //SelectTarget();
        //if (_currentAttack == null) return;

        _timeSinceLastAttack += Time.deltaTime;

        if (_timeSinceLastAttack >= FireRate) {
            Attack(_currentTarget);
            _timeSinceLastAttack = 0;
        } 
    }

    public void ClearTarget() {
        _currentTarget = null;
        IsAttacking = false;
    }

    public void Attack(IDamageable target) {
        if (_currentTarget == null) {
            if (_animator != null)
                _animator.SetBool("isAttacking", false);
            IsAttacking = false;
            return;
        }
        _timeSinceLastAttack += Time.deltaTime;

        if (_animator != null) {
            _animator.SetBool("isAttacking", true);
            StartCoroutine(AttackSequence());
        } else {
            // performing attack without animation
            Debug.Log("Performing attack without animation");
            PerformAttack();
        }
    }


    public void SelectTarget(IDamageable target) { // method to set target from targeting component
        _currentTarget = target;
        IsAttacking = true;
    }

    //IDamageable SelectTarget() {
    //    Collider[] colliders = Physics.OverlapSphere(transform.position, AttackRange, whatIsTarget);

    //    if (colliders.Length == 0) {
    //        _currentTarget = null;
    //        IsAttacking = false;
    //        return null;
    //    } // exit early if no target found

    //    // select the first target. In later steps we can implement a better target selection algorithm
    //    IDamageable target = colliders[0].GetComponent<IDamageable>();
    //    _currentTarget = target;
    //    IsAttacking = true;
    //    return target;
    //}


    private void Attack() {
        if (_currentTarget == null) {
            if (_animator != null)
                _animator.SetBool("isAttacking", false);
            IsAttacking = false;
            return;
        }

        _timeSinceLastAttack += Time.deltaTime;
        if (_timeSinceLastAttack >= (1f / FireRate)) {
            if (_animator != null) {
                _animator.SetBool("isAttacking", true);
                // wait for attack to finish before we attack again
                StartCoroutine(AttackSequence());
            } else {
                PerformAttack();
            }
            _timeSinceLastAttack = 0f;
        }
    }

    private IEnumerator AttackSequence() {
        yield return new WaitForSecondsRealtime(0.5f);
        PerformAttack();
        if (_animator != null)
            _animator.SetBool("isAttacking", false);
        IsAttacking = false;
        ClearTarget();
    }

    private void PerformAttack() {
        if (_currentTarget == null) return;
        _currentAttack.PerformAttack(this.transform, _currentTarget, AttackPower, null);
    }
}