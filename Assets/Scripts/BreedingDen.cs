using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class BreedingDen : MonoBehaviour
{
    [SerializeField] private int breedingTimeInSeconds = 5;

    [SerializeField] private BreedingManager _breedingManager;

    [SerializeField] private float breedingCooldownTime = 3f; // Cooldown after breeding

    private int _breedingCount = 0; // Current breeding count

    [SerializeField] private int maxBreedingAttempts = 2;

    private int animalsNeededToBreed = 2;

    private bool _isBreeding = false;

    private bool _isOnCooldown = false;

    private List<AnimalObject> _animalsInDen = new List<AnimalObject>();

    public int CountAnimalsInsideDen {
        get { return _animalsInDen.Count; }
    }

    private Vector3[] _spawnOffsets = new Vector3[]
      {
        new Vector3(0, 0, -3.5f),       // South
         new Vector3(-3f, 0, -3),    // South-West
       new Vector3(3, 0, -3),   // South-East
    };

    private AnimalTower _offspring;

    public event Action OnAnimalArrived;

    public event Action OnBreedingEnd;

    private WaveManager _waveManager;

    private int _waveCountdown = 15;

    public TextMeshProUGUI waveStartTimerText;

    private bool _timerEnabled = false;

    void Start() {

        _breedingManager = GameObject.FindObjectOfType<BreedingManager>();
        _waveManager = FindObjectOfType<WaveManager>();

        if (_waveManager != null) {
            _waveManager.OnWaveStart += TimerOverrideEvent;
        }

        if (waveStartTimerText != null) {
            waveStartTimerText.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other) {
        
        
        Debug.Log("Object with name: " + other.gameObject.name +" entered. isBreeding = " + _isBreeding + ". On Cooldown = "+ _isOnCooldown + ". Breeding attempt: " + _breedingCount);

        if (_isBreeding || _isOnCooldown || _breedingCount >= maxBreedingAttempts) return;

        var animalObject = other.gameObject.GetComponent<AnimalObject>();

        if (animalObject == null) return;

        animalObject.Deactivate();

        if (_animalsInDen.Contains(animalObject)) return;
        if (_isBreeding) return;

        _animalsInDen.Add(animalObject);

        Debug.Log("Animal entered den and has movement component");
        Debug.Log($"Animals in den: {_animalsInDen.Count}");


        OnAnimalArrived?.Invoke();

        if (CountAnimalsInsideDen == animalsNeededToBreed) { 
            BeginBreeding(); 
        }
    }

    private void BeginBreeding() {


        if (_isBreeding) return;

        // return control to unity since we are spreading the breeding over multiple frames
        // see: https://docs.unity3d.com/6000.0/Documentation/Manual/coroutines.html
        StartCoroutine(BreedSequence());
    }



    private IEnumerator BreedSequence() {

        _isBreeding = true;

        Debug.Log("Breeding has begun!");

        // yield allows us to return control to Unity for the amount of seconds specified
        // when the time has passed, Unity will resume the coroutine at TimerExpiredEvent
        yield return new WaitForSecondsRealtime(breedingTimeInSeconds); TimerExpiredEvent();
        
        _isOnCooldown = true;

        StartCoroutine(CooldownSequence());

    }

    private IEnumerator CooldownSequence() {

        yield return new WaitForSecondsRealtime(breedingCooldownTime);

        _isOnCooldown = false;

        Debug.Log("End cooldown" + Time.time.ToString());

    }

    private void TimerExpiredEvent() {


        Debug.Log("Timer expired!");


        if (_breedingManager == null) { Debug.LogWarning("No manager loaded,  fix that now!"); return; }


        if (_animalsInDen.Count < 2) { Debug.LogError($"failed required  amount {animalsNeededToBreed}"); return; }   // no need for code execution. Invalid parameters


        // get the offspring from the breeding manager
        GameObject offspring = _breedingManager.BreedAnimals(_animalsInDen[0].AnimalData, _animalsInDen[1].AnimalData);


        if (offspring == null) return;

        var moveableChild = offspring.GetComponent<MoveableObject>();

        Vector3 basePosition = new Vector3(this.transform.position.x, 1, this.transform.position.z); // Use the den's position
     

        for (int i = 0; i < _animalsInDen.Count; i++) { // move animals to exit position

            // Calculate exit position using relative offsets
            Vector3 spawnOffset = _spawnOffsets[i % _spawnOffsets.Length];
            //Vector3 exitOffset = Offsets[i % Offsets.Length];
            Vector3 exitPosition = basePosition + spawnOffset;

            // need to activate animal
            AnimalObject animalObject = _animalsInDen[i];

            animalObject.Activate();

            _animalsInDen[i].transform.position = exitPosition;

            var movable = _animalsInDen[i].gameObject.GetComponent<MoveableObject>();

            movable?.StartMovement(exitPosition + (spawnOffset.normalized) * 1f);  
        }

        Vector3 offspringPosition = basePosition + _spawnOffsets[_animalsInDen.Count % _spawnOffsets.Length];
        offspring.transform.position = offspringPosition;

        moveableChild.StartMovement(offspringPosition + (_spawnOffsets[_animalsInDen.Count % _spawnOffsets.Length].normalized) * 1f); 

        _breedingCount++; // increment breeding count

        ResetBreedSystem();    // system property used for memory safety + data + values + states setup cleanup

        // Check if there are any subscribers to the OnBreedingEnd event
        // If there are, invoke the event to notify all subscribers that the breeding process has ended
        OnBreedingEnd?.Invoke();  // triggers new callbacks events

        if (_breedingCount >= maxBreedingAttempts && !_timerEnabled) {
            // start wave countdown after all breeding has been done
            _timerEnabled = true;
            StartCoroutine(StartWaveTimer());
            waveStartTimerText?.gameObject.SetActive(true); // shows counter after max breeding has been reached
        }
    }

    private IEnumerator StartWaveTimer() {
        while (_waveCountdown > 0) {
            UpdateTimerUI();
            yield return new WaitForSecondsRealtime(1f);

            _waveCountdown--;
        }
        _timerEnabled = false;
        TimerFinished();
    }

    private void TimerFinished() {
        _waveManager.StartWave();
        _waveCountdown = 15;
        _timerEnabled = false;
        waveStartTimerText?.gameObject.SetActive(false);
        UpdateTimerUI();

    }

    private void UpdateTimerUI() {
        if (waveStartTimerText == null) return;
        waveStartTimerText.text = $"Wave incoming: {_waveCountdown}";
    }

    private void TimerOverrideEvent() {
        _waveCountdown = 0;
        TimerFinished();
    }

    public void ResetBreedSystem() //method that provides object behaviours reset cleanup parameters before method operation or any other actions happens
      { if (CountAnimalsInsideDen == 0) return; _isBreeding = false; StopAllCoroutines(); _animalsInDen.Clear(); }  //reset  settings, by clear states and memory. When needed or called
}