using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private float _speed;
    private Vector3 _input; 

    void Update() {
        GetPlayerInput();
    }

    void FixedUpdate() {
        Move();
    }

    void GetPlayerInput() {
        _input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    }

    void Move() {
        _rb.MovePosition(transform.position + transform.forward * _speed * Time.deltaTime);
    }
}
