using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyTargeting : MonoBehaviour {
    //[SerializeField] private float _aggroRange = 10f;  // remove
    [SerializeField] private LayerMask _targetMask;
    [SerializeField] private IDamageable _currentTarget;
    private EnemyMovement _enemyMovement;
    private EnemyCombat _enemyCombat;
    private EnemyStateMachine _enemyStateMachine;

    void Start() {
        _enemyMovement = GetComponent<EnemyMovement>();
        _enemyCombat = GetComponent<EnemyCombat>();
        _enemyStateMachine = GetComponent<EnemyStateMachine>();
    }

    private void Update() {
        FindTarget();
    }

    private void FindTarget() {
        // find all the targets in the scene in a given mask, then sort by distance.
        AnimalObject closestTarget = FindClosestAnimal();

        if (closestTarget == null) {
            if (_currentTarget != null) { // if we have a target and lost it, lets set target to null
                Debug.Log("Lost target: " + _currentTarget.ToString());
                _enemyStateMachine.ClearTarget();
                ClearTarget();
            }
            return; // exit early if no target found
        }

        // select the closest target.
        _currentTarget = closestTarget.GetComponent<IDamageable>();
        if (Vector3.Distance(transform.position, ((MonoBehaviour)_currentTarget).transform.position) <= _enemyCombat.AttackRange) {
            _enemyMovement.ClearTarget();
            _enemyCombat.SelectTarget(_currentTarget);
        } else {
            _enemyCombat.ClearTarget();
            _enemyMovement.SetTarget(((MonoBehaviour)_currentTarget).transform);
        }
    }

    private AnimalObject FindClosestAnimal() {
        var allTargets = FindObjectsOfType<AnimalObject>().ToList();

        AnimalObject closestTarget = null;
        float minDistance = float.MaxValue;

        if (allTargets.Count == 0) {
            Debug.Log("No targets found");
            return null; // exit early if no target is found

        }
        foreach (var target in allTargets) {
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (target != null && damageable.IsAlive && !target.Equals(this.GetComponent<AnimalObject>())) {
                float distance = Vector3.Distance(transform.position, target.transform.position);
                if (distance < minDistance) {
                    minDistance = distance;
                    closestTarget = target;
                }
            }
        }
        return closestTarget;
    }

    private void ClearTarget() {
        _currentTarget = null;
    }
}