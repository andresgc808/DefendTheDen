using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundDistance = 0.4f;
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private float _rotationSpeed = 15f;
    private Matrix4x4 matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));

    private Vector3 _input;
    private Vector3 _velocity;
    private bool _isGrounded;

    private void Awake() {
        if (_rb == null) {
            _rb = GetComponent<Rigidbody>();
            if (_rb == null) {
                _rb = gameObject.AddComponent<Rigidbody>();
            }
        }
        _rb.freezeRotation = true;
    }

    void Update() {
        GetPlayerInput();
        //CheckGround();
    }

    void FixedUpdate() {
        Move();
        RotatePlayer();
        //ApplyGravity();
    }

    void GetPlayerInput() {
        _input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    }

    void CheckGround() {
        _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundDistance, _groundMask);

        if (_isGrounded && _velocity.y < 0) {
            _velocity.y = -2f;
        }
    }

    void Move() {
        //Vector3 moveDirection = _input.normalized;
        //Vector3 horizontalMove = moveDirection * _moveSpeed;
        //_rb.velocity = horizontalMove + _velocity;

        // not sure if input needs to be normalized
        // look into fixed delta time vs time 
        
        var skewedInput = matrix.MultiplyPoint3x4(_input);
        _rb.MovePosition(transform.position + (skewedInput.normalized * skewedInput.magnitude) * _moveSpeed * Time.deltaTime);
    }

    void RotatePlayer() {
        if (_input != Vector3.zero) {
            // update look direction for isometic camera distortion (45 degrees)
            
            var skewedInput = matrix.MultiplyPoint3x4(_input);

            Quaternion targetRotation = Quaternion.LookRotation(skewedInput);

            // TODO: Look into Quaternion.RotateTowards vs Slerp
            // Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
    }

        void ApplyGravity() {
        _velocity.y += _gravity * Time.fixedDeltaTime;
    }
}