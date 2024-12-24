using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableObject : MonoBehaviour {

    public float movementSpeed = 5f;

    [SerializeField] public bool isMoving = false;
    private Vector3 _moveTarget;
    private bool useHorizontalGrid = true;
    //private Animator _animator;
    [SerializeField] private float _rotationSpeed = 15f;
    private Matrix4x4 matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    [SerializeField] private GameObject _forwardDirectionObject;
    private Transform _forwardDirection;
    [SerializeField] private Animator _animator;

    private void Start() {
        //_animator = GetComponent<Animator>();
        if (_forwardDirectionObject == null)
            _forwardDirection = this.transform;
        else
            _forwardDirection = _forwardDirectionObject.transform; // sets forward if null
    }

    public void StartMovement(Vector3 targetPosition) {
        if (isMoving == true) {
            return;
        }
        _moveTarget = targetPosition;

        //we set our target position Y, to same that of original position
        if (useHorizontalGrid)
            _moveTarget.y = transform.position.y;

        isMoving = true;

        if (_animator != null)
            _animator.SetBool("isMoving", true);

    }

    public void StopMovement() {
        isMoving = false;

        if (_animator != null)
            _animator.SetBool("isMoving", false);

        var body = GetComponent<Rigidbody>();

        if (body != null) {
            body.velocity = Vector3.zero;
        }
    }

    private void FixedUpdate() {

        // we only move when in a movement state
        if (!isMoving) { return; }

        // calculate direction
        var dir = _moveTarget - transform.position;


        //If is to small distance stop move
        if (dir.magnitude < 0.1f) {

            StopMovement();

            return;

        }



        dir = dir.normalized; // normalize for only directional


        transform.position += (dir * Time.fixedDeltaTime) * movementSpeed;

        //var skewedDir = matrix.MultiplyPoint3x4(dir);
        //var skewedInput = matrix.MultiplyPoint3x4(_forwardDirection.up);
        //Rotation code start here:
        Quaternion targetRotation = Quaternion.LookRotation(dir, _forwardDirection.up); // calculates new rotation to look at that direction
        
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);


    }

}