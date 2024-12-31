// AnimalStateMachine.cs
using UnityEngine;
using System;

public class AnimalStateMachine : MonoBehaviour {

    private StateMachine _stateMachine;
    private UnitMovement _unitMovement;
    private AnimalCombat _animalCombat;
    private AnimalHealth _animalHealth;
    [SerializeField] public AnimalTower AnimalData;


    private Animator _animator;
    public void SetAnimalData(AnimalTower data) {
        AnimalData = data;
    }

    private void Awake() {
        _unitMovement = GetComponent<UnitMovement>();
        _animalCombat = GetComponent<AnimalCombat>();
        _animalHealth = GetComponent<AnimalHealth>();
        _animator = GetComponentInChildren<Animator>();
        _stateMachine = new StateMachine();
        if (AnimalData != null) {
            SetAnimalData(AnimalData);
        }
    }
    private void Start() {

        var idle = new State();
        var moving = new State();
        var attacking = new State();
        var dead = new State();

        _stateMachine.AddTransition(idle, moving, () => { return _unitMovement.IsMoving; });
        _stateMachine.AddTransition(moving, idle, () => { return !_unitMovement.IsMoving; });
        _stateMachine.AddTransition(idle, attacking, () => { return _animalCombat.IsAttacking; });
        _stateMachine.AddTransition(attacking, idle, () => { return !_animalCombat.IsAttacking; });
        _stateMachine.AddTransition(idle, dead, () => { return !_animalHealth.IsAlive; });
        _stateMachine.AddTransition(moving, dead, () => { return !_animalHealth.IsAlive; });
        _stateMachine.AddTransition(attacking, dead, () => { return !_animalHealth.IsAlive; });

        _stateMachine.SetState(idle); // set idle as default state for any other behaviour.
        _animalHealth.OnDeath += OnDeath; // action used for checking death.
        _animalCombat.OnAttackStart += OnAttackStart; 
        _animalCombat.OnAttackEnd += OnAttackEnd;
    }
    private void Update() {
        _stateMachine.Update();

        if (_stateMachine.CurrentState == _stateMachine.states[0]) // idle
        {
            Debug.Log($"{gameObject.name}: Switched to IDLE state");
            if (_animator != null) {
                _animator.SetBool("isMoving", false);
                _animator.SetBool("isAttacking", false);
            }
        }
        if (_stateMachine.CurrentState == _stateMachine.states[1])   // moving
        {
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
        if (_stateMachine.CurrentState == _stateMachine.states[3])  // dead
       {
            Debug.Log($"{gameObject.name}: Switched to DEAD state");
            if (_animator != null) {
                _animator.SetBool("isMoving", false);
                _animator.SetBool("isAttacking", false);
            }
        }
        if (_stateMachine.CurrentState == _stateMachine.states[3] && _animator != null) // handle death and destroy.
       {
            Destroy(gameObject);
        }
    }

    private void OnDeath() {
        _stateMachine.Update();
    }

    private void OnAttackStart() { // this event method simply calls animation parameter update
        if (_animator != null)
            _animator.SetBool("isAttacking", true);
    }
    private void OnAttackEnd() {
        if (_animator != null)
            _animator.SetBool("isAttacking", false);
    }

    public void SetStateToIdle() {
        _stateMachine.SetState(_stateMachine.states[0]); // manually set states.
    }
}