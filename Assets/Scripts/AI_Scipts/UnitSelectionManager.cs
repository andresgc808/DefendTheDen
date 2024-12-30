using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionManager : MonoBehaviour {
    public static UnitSelectionManager Instance { get; set; }

    public List<GameObject> allUnitsList = new List<GameObject>();
    public List<GameObject> selectedUnitsList = new List<GameObject>();

    [SerializeField] private Camera _camera;

    public LayerMask animal;
    public LayerMask ground;
    public GameObject groundMarker;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
        } else {
            Instance = this;
        }
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, animal)) {

                if (Input.GetKey(KeyCode.LeftShift)) {
                    MultiSelect(hit.collider.gameObject);
                } else {
                    Debug.Log(hit.collider.gameObject);
                    SelectObject(hit.collider.gameObject);
                }
            } else {
                // deselect all units
                DeselectAll();
            }
        } 

        if (Input.GetMouseButtonDown(1) && selectedUnitsList.Count > 0) {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground)) {
                groundMarker.transform.position = hit.point;

                // trick to make animation later
                groundMarker.SetActive(false);
                groundMarker.SetActive(true);
            }
        }
    }

    private void SelectObject(GameObject obj) {
        DeselectAll();
        selectedUnitsList.Add(obj);
        TriggerSelectionIndicator(obj, true);
        EnableUnitMovement(obj, true);

    }

    public void DeselectObject(GameObject obj) {
        selectedUnitsList.Remove(obj);
        TriggerSelectionIndicator(obj, false);
        EnableUnitMovement(obj, false);
    }

    private void DeselectAll() {
        foreach (GameObject obj in selectedUnitsList) {
            EnableUnitMovement(obj, false);
            TriggerSelectionIndicator(obj, false);
        }

        groundMarker.SetActive(false);
        selectedUnitsList.Clear();
    }

    private void EnableUnitMovement(GameObject obj, bool shouldMove) {
        obj.GetComponent<UnitMovement>().enabled = shouldMove;
    }

    private void TriggerSelectionIndicator(GameObject obj, bool isSelected) {
        obj.transform.GetChild(0).gameObject.SetActive(isSelected);
    }

    private void MultiSelect(GameObject obj) {
        if (selectedUnitsList.Contains(obj)) {
            selectedUnitsList.Remove(obj);
            TriggerSelectionIndicator(obj, false);
            EnableUnitMovement(obj, false);
        } else {
            selectedUnitsList.Add(obj);
            TriggerSelectionIndicator(obj, true);
            EnableUnitMovement(obj, true);
        }
    }
}
