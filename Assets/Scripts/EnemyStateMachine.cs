using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine : MonoBehaviour {


    private StateMachine _stateMachine;
    private EnemyMovement _enemyMovement;
    private EnemyCombat _enemyCombat;
    private EnemyHealth _enemyHealth;
    [SerializeField] public Enemy EnemyData;

    private Animator _animator;

    public void SetEnemyData(Enemy data) {
        EnemyData = data;
        _enemyMovement.SetEnemyData(data);
        _enemyCombat.SetEnemyData(data);
        _enemyHealth.SetEnemyData(data);
    }

    private void Awake() {
        _enemyMovement = GetComponent<EnemyMovement>();
        _enemyCombat = GetComponent<EnemyCombat>();
        _enemyHealth = GetComponent<EnemyHealth>();
        _animator = GetComponentInChildren<Animator>(); // loading cached animator object
        _stateMachine = new StateMachine();

        if (EnemyData != null) {
            Debug.Log("Enemy Data is not null");
            Debug.Log("Enemy Data is: " + EnemyData);
            SetEnemyData(EnemyData);
        }   
    }

    private void Start() {
        var idle = new State();
        var moving = new State();
        var attacking = new State();
        var dead = new State();

        _stateMachine.AddTransition(idle, moving, () => { return _enemyMovement.IsMoving; });
        _stateMachine.AddTransition(moving, idle, () => { return !_enemyMovement.IsMoving; }); // if we are not moving transition to idle
        _stateMachine.AddTransition(idle, attacking, () => { return _enemyCombat.IsAttacking; });
        _stateMachine.AddTransition(attacking, idle, () => { return !_enemyCombat.IsAttacking; });
        _stateMachine.AddTransition(idle, dead, () => { return !_enemyHealth.IsAlive; }); // if dead transition to the dead state.
        _stateMachine.AddTransition(moving, dead, () => { return !_enemyHealth.IsAlive; });
        _stateMachine.AddTransition(attacking, dead, () => { return !_enemyHealth.IsAlive; });

        _stateMachine.SetState(idle);
        _enemyHealth.OnDeath += OnDeath;
    }
    private void Update() {
        _stateMachine.Update();

        if (_stateMachine.CurrentState == _stateMachine.states[0]) { // idle
            Debug.Log($"{gameObject.name}: Switched to IDLE state");
            if (_animator != null) {
                _animator.SetBool("isMoving", false);
                _animator.SetBool("isAttacking", false);
            }
        }
        if (_stateMachine.CurrentState == _stateMachine.states[1]) {  // moving
            Debug.Log($"{gameObject.name}: Switched to MOVING state");
            if (_animator != null) {
                _animator.SetBool("isMoving", true);
                _animator.SetBool("isAttacking", false);
            }
        }
        if (_stateMachine.CurrentState == _stateMachine.states[2]) { //attacking
            Debug.Log($"{gameObject.name}: Switched to ATTACKING state");
            if (_animator != null) {
                _animator.SetBool("isMoving", false);
                _animator.SetBool("isAttacking", true);
            }
        }
        if (_stateMachine.CurrentState == _stateMachine.states[3]) { // dead
            Debug.Log($"{gameObject.name}: Switched to DEAD state");
            if (_animator != null) {
                _animator.SetBool("isMoving", false);
                _animator.SetBool("isAttacking", false);
            }
        }

        if (_stateMachine.CurrentState == _stateMachine.states[3] && _animator != null) {
            Destroy(gameObject);
        }
    }
    private void OnDeath() {
        _stateMachine.Update(); // update the state machine to transition to death
    }
    public void SetTarget(Transform target) {
        _enemyMovement.SetTarget(target);
        _enemyCombat.ClearTarget();
    }
    public void ClearTarget() {
        _enemyMovement.ClearTarget();
        _enemyCombat.ClearTarget();
    }

}