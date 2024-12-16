using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableObject : MonoBehaviour {
    public float moveSpeed = 5f;
    public float acceleration = 2f;
    public float stoppingDistance = 0.5f;

    [SerializeField] public int _gridSize;



    //Data variables to work with internal values.

    private Vector3 _targetPos;
    private bool _targetSet;



    //Unity variable

    private float _gridCellSize;


    //Targeted Pos in grid index,
    private Vector2Int _targetCell;
    private Vector2Int _currentCell;



    private Vector3 finalTargetPosition {
        get {

            //Get World coordinates from index value. This function has not yet been implemented
            Vector3 value = GetWorldCoordinates(_targetCell);


            return new Vector3(value.x, this.transform.position.y, value.z);
        }
    }




    //Method to make setup before gameplay. Use this in `Start()` Function.

    public void OnStart() {
        _targetPos = this.transform.position;
        UpdateCurrentPositionFromPosition(_targetPos);


        // Set each unit Cell Size in World position based in Size of each game unit
        _gridCellSize = (transform.lossyScale.x > transform.lossyScale.z ? transform.lossyScale.x : transform.lossyScale.z) / 10f;

    }

    //Sets a new world location to path to using world coordenates in vector3.
    public void SetNewPos(Vector3 pos) {

        //Gets correct vector using our transform
        UpdateCurrentPositionFromPosition(pos);

        _targetSet = true;

    }

    // Sets a new location from using the grid positions indexes and a 2d index position vector
    public void SetNewGridPos(Vector2Int cellIndex) {


        _targetCell = cellIndex;
        _targetSet = true;

    }


    //Transforms our grid space into world space with offsets
    Vector3 GetWorldCoordinates(Vector2Int cellIndex) {


        //This should change as you change the tile scale on project
        Vector3 value = new Vector3();

        value.x = ((cellIndex.x) * _gridCellSize) + _gridCellSize / 2;
        value.z = ((cellIndex.y) * _gridCellSize) + _gridCellSize / 2;


        return value;


    }


    // Sets an animal new grid positions using a position Vector.3
    void UpdateCurrentPositionFromPosition(Vector3 pos) {


        float cellSize = _gridCellSize;



        int indexX = Mathf.RoundToInt((pos.x) / (cellSize));

        int indexZ = Mathf.RoundToInt((pos.z) / (cellSize));


        _targetCell = new Vector2Int(indexX, indexZ);



    }

    private Vector2Int UpdateCurrentPositionFromIndex() {


        float cellSize = _gridCellSize;

        // Transform world position from game into an cell index value using world position and cell size on a Grid Layout
        int indexX = Mathf.RoundToInt(transform.position.x / (cellSize));
        int indexZ = Mathf.RoundToInt(transform.position.z / cellSize);

        //New grid current cell position, the value must always be between positive or negative index ranges or it might brake our game map layout

        return new Vector2Int(indexX, indexZ);
    }


    //moves object to its current position with acceleration, uses physics.
    private void Move() {


        _currentCell = UpdateCurrentPositionFromIndex();


        Vector3 direction = (finalTargetPosition - this.transform.position).normalized;


        float targetDistance = Vector3.Distance(this.transform.position, finalTargetPosition);


        if (targetDistance <= stoppingDistance) {


            StopMovement();


        } else {

            transform.position += direction * moveSpeed * Time.deltaTime;



        }




    }


    public void StopMovement() {

        _targetSet = false;

    }



    private void Update() {


        if (_targetSet)
            Move();

    }

}