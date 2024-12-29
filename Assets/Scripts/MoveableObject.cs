using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableObject : MonoBehaviour {
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 15f;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundDistance = 0.4f;
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _movementTolerance = 0.2f;

    private Vector3 _moveTarget;
    private Vector3 _velocity;
    private bool _isGrounded;
    [SerializeField] private bool _isMoving = false;
    private Matrix4x4 matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));

    private void Awake() {
        if (_rb == null) {
            _rb = GetComponent<Rigidbody>();
            if (_rb == null) {
                _rb = gameObject.AddComponent<Rigidbody>();
            }
        }
        _rb.freezeRotation = true;
    }

    private void Update() {
        HandleMouseClick();
        //CheckGround();
    }

    private void FixedUpdate() {
        if (_isMoving) {
            Move();
            RotatePlayer();
        }

        //ApplyGravity();
    }

    private void HandleMouseClick() {
        if (Input.GetMouseButtonDown(0)) { // Left mouse button
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _groundMask)) {
                StartMovement(hit.point);
            }
        }
    }

    public void StartMovement(Vector3 targetPosition) {
        _moveTarget = targetPosition;
        _moveTarget.y = transform.position.y; // Maintain Y level
        _isMoving = true;

        if (_animator != null) {
            Debug.Log("MovingFromStart");
            _animator.SetBool("isMoving", true);
        }
    }

    public void StopMovement() {
        _isMoving = false;

        if (_animator != null) {
            Debug.Log("StopingMovement");
            _animator.SetBool("isMoving", false);
        }

        _rb.velocity = Vector3.zero; // Stop Rigidbody movement
    }

    private void Move() {
        Vector3 direction = (_moveTarget - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, _moveTarget);

        // check horizontal distance then raycast to check if movement should stop
        Vector2 currentPosXZ = new Vector2(transform.position.x, transform.position.z);
        Vector2 targetPosXZ = new Vector2(_moveTarget.x, _moveTarget.z);

        float horizontalDistance = Vector2.Distance(currentPosXZ, targetPosXZ);
        if (horizontalDistance < _movementTolerance) {

            RaycastHit hit;
            Vector3 origin = _moveTarget + Vector3.up * 0.1f;
            if (Physics.Raycast(origin, Vector3.down, out hit, _groundDistance, _groundMask)) {

                Vector2 currentPos = new Vector2(transform.position.x, transform.position.z);
                Vector2 targetPos = new Vector2(hit.point.x, hit.point.z);

                // check if we are also close enough to the destination on the y
                if (Vector2.Distance(currentPos, targetPos) < _movementTolerance) {
                    StopMovement();
                    return;
                }
            }

            StopMovement();
            return;
        }


        if (distance < 0.1f) {
            StopMovement();
            return;
        }

        if (_animator != null) {
            Debug.Log("Moving");
            _animator.SetBool("isMoving", true);
        }

        //Vector3 skewedDirection = matrix.MultiplyPoint3x4(direction);
        _rb.MovePosition(transform.position + direction * _moveSpeed * Time.fixedDeltaTime);
    }

    private void RotatePlayer() {
        Vector3 direction = _moveTarget - transform.position;
        direction.y = 0; // Ignore vertical differences

        if (direction.magnitude > 0.1f) {
            //Vector3 skewedDirection = matrix.MultiplyPoint3x4(direction.normalized);
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void CheckGround() {
        _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundDistance, _groundMask);

        if (_isGrounded && _velocity.y < 0) {
            _velocity.y = -2f;
        }
    }

    private void ApplyGravity() {
        _velocity.y += _gravity * Time.fixedDeltaTime;
        _rb.velocity = new Vector3(_rb.velocity.x, _velocity.y, _rb.velocity.z);
    }
}
