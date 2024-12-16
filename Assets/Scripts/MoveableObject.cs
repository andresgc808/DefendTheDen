using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableObject : MonoBehaviour {

    public float movementSpeed = 5f;

    [SerializeField] public bool isMoving;
    private Vector3 _moveTarget;
    private bool useHorizontalGrid;


    public void Awake() {
        movementSpeed = 5;
        isMoving = false;
        useHorizontalGrid = true;

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

    }

    public void StopMovement() {
        isMoving = false;

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


    }

}