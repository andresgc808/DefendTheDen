using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePosition3D : MonoBehaviour {
    [SerializeField] private Camera _camera;

    private GameObject _selectedAnimal;

    private MoveableObject _currentMovement;

    private bool _selectionEnabled = true;

    private PauseMenu _pauseMenu;

    void Start() {
        // get pause menu instance
        _pauseMenu = FindObjectOfType<PauseMenu>();
    }

    void Update() {

        if (!PauseMenu.isPaused) { 
            if (_selectionEnabled == false)
                return;

            if (Input.GetMouseButtonDown(0)) {
                HandleRaycastSelection();
                Debug.Log("Mouse Clicked");
            }

            HandleObjectMovement();
        }
    }

    private void HandleRaycastSelection() {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;


        if (Physics.Raycast(ray, out hit)) {
            GameObject clickedObject = hit.collider.gameObject;

            AnimalObject animalObject = clickedObject.GetComponent<AnimalObject>();

            if (animalObject != null) {
                Debug.Log($"Hit object with AnimalObject: {clickedObject.name}");
                if (_selectedAnimal != clickedObject) {
                    
                    SelectNewAnimal(clickedObject);

                } else {

                    DeselectCurrentAnimal();


                }
            } else {
                Debug.Log($"Hit object without AnimalObject: {clickedObject.name}");
                DeselectCurrentAnimal();

            }
        }


    }


    private void HandleObjectMovement() {


        if (_selectedAnimal != null) {

            if (Input.GetMouseButtonDown(1)) {

                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100f)) {


                    Debug.Log("Starting movement.");
                    _currentMovement.StartMovement(hit.point);

                }
                Debug.Log("Right click registered in nothingness.");
            }
        }

    }


    void SelectNewAnimal(GameObject animal) {

        if (_selectedAnimal != null) {

            _currentMovement.StopMovement();

        }

        _selectedAnimal = animal;


        _currentMovement = animal.GetComponent<MoveableObject>();

    }

    void DeselectCurrentAnimal() {


        if (_selectedAnimal != null) {
            _currentMovement.StopMovement();

        }

        _selectedAnimal = null;
        _currentMovement = null;

    }
}