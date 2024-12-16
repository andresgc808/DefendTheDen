using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePosition3D : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    private GameObject _selectedAnimal;
    private MoveableObject _currentMovement;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        //if (Physics.Raycast(ray, out RaycastHit hit)) {
        //    Debug.Log(hit.point);
        //    transform.position = hit.point;

        //}

        if (Input.GetMouseButtonDown(0)) {
            Debug.Log("Mouse Clicked");
            HandleMouseClick(ray);

        }
    }

    private void HandleMouseClick(Ray ray) {

        RaycastHit hit;

        // See if the mouse position hits any object.
        if (Physics.Raycast(ray, out hit)) {
            GameObject clickedObject = hit.collider.gameObject;


            //check if any Animal Tower, in that case. If selected it's now this target.

            if (clickedObject.TryGetComponent(out AnimalTower animal)) {

                // if its not this game object
                if (_selectedAnimal != clickedObject) {

                    //new object selected. If you click an animal it's now a new reference and not our previous selected
                    SelectNewAnimal(clickedObject);

                } else {

                    _currentMovement.StopMovement();
                    DeselectCurrentAnimal();
                }


            }
        } //If there was a current animal target it now needs a move command, if theres movement components in that new target object move.
                else if (_selectedAnimal != null && _currentMovement != null) {
            _currentMovement.SetNewPos(hit.point);
        }
    }

    // Set as selected the animal.

    void SelectNewAnimal(GameObject animal) {
        if (_selectedAnimal != null)
            DeselectCurrentAnimal();

        _selectedAnimal = animal;

        if (_selectedAnimal.TryGetComponent(out MoveableObject movement)) {

            _currentMovement = movement;

        }


        Debug.Log("Selected" + _selectedAnimal.name);

    }


    // sets to deselect the current target animal and its selected variables
    void DeselectCurrentAnimal() {


        _selectedAnimal = null;
        _currentMovement = null;

        Debug.Log("Deselected Animal");


    }

}
