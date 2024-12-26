using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour {
    private Enemy _enemyData; // Enemy scriptable object
    [SerializeField] private float _groundCheckDistance = 0.5f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _movementTolerance = 0.2f;

    private Rigidbody _rb;
    private Transform _targetTransform;
    public bool IsMoving { get; private set; } = false;


    public void SetEnemyData(Enemy data) {
        _enemyData = data;
    }

    private void Awake() {
        if (_rb == null) {
            _rb = GetComponent<Rigidbody>();
            if (_rb == null) {
                _rb = gameObject.AddComponent<Rigidbody>();
            }
        }
        _rb.freezeRotation = true;
    }

    public void SetTarget(Transform target) {
        _targetTransform = target;
        IsMoving = true;
    }
    public void ClearTarget() {
        _targetTransform = null;
        IsMoving = false;
    }

    private void FixedUpdate() {
        if (!IsMoving) return;
        MoveTowardsTarget();
    }


    private void MoveTowardsTarget() {
        if (_targetTransform == null)
            return; // exit early to not run any further code, since we dont have a target

        Vector3 direction = (_targetTransform.position - transform.position).normalized;
        // Move towards target using kinematic rigidbody, we still calculate direction in same way as moveable object.

        // only check horizontal distance

        Vector2 currentPosXZ = new Vector2(transform.position.x, transform.position.z);
        Vector2 targetPosXZ = new Vector2(_targetTransform.position.x, _targetTransform.position.z);

        float horizontalDistance = Vector2.Distance(currentPosXZ, targetPosXZ);

        if (horizontalDistance > _movementTolerance) {
            _rb.MovePosition(transform.position + direction * _enemyData.moveSpeed * Time.fixedDeltaTime);
            RotateTowardsTarget(direction);
            return;

        }

        // only stop if we are within both horizontal tolerance and y tolerance
        RaycastHit hit;
        Vector3 origin = _targetTransform.position + Vector3.up * 0.1f;
        if (Physics.Raycast(origin, Vector3.down, out hit, _groundCheckDistance, _groundLayer)) {

            Vector2 currentPos = new Vector2(transform.position.x, transform.position.z);
            Vector2 targetPos = new Vector2(hit.point.x, hit.point.z);

            // check if we are also close enough to the destination on the y
            if (Vector2.Distance(currentPos, targetPos) < _movementTolerance) {
                ClearTarget(); // stop movement, since we are in range
                return;
            }
        }
    }

    private void RotateTowardsTarget(Vector3 direction) {

        if (direction.magnitude <= 0.1f)
            return; // return early if direction is not big enough
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _enemyData.rotationSpeed * Time.fixedDeltaTime);
    }
}