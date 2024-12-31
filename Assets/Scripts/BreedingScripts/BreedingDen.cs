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

    // list of transforms of where the animals will spawn
    public List<Transform> spawnPoints = new List<Transform>();

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
            _waveManager.OnWaveStart += TimerFinished;
        }

        if (waveStartTimerText != null) {
            waveStartTimerText.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other) {
        
        if (_waveManager.isWaveActive) return; // break instantly to prevent breeding during wave

        Debug.Log("Object with name: " + other.gameObject.name +" entered. isBreeding = " + _isBreeding + ". On Cooldown = "+ _isOnCooldown + ". Breeding attempt: " + _breedingCount);

        if (_isBreeding || _isOnCooldown || _breedingCount >= maxBreedingAttempts) return;

        var animalObject = other.gameObject.GetComponent<AnimalObject>();

        if (animalObject == null) return;

        if (_animalsInDen.Contains(animalObject)) return;
        if (_isBreeding) return;

        UnitSelectionManager.Instance.DeselectObject(animalObject.gameObject);
        animalObject.gameObject.SetActive(false);

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


        // spawn and set animals to active
        offspring.transform.position = spawnPoints[0].position;
        _animalsInDen[0].transform.position = spawnPoints[1].position;
        _animalsInDen[1].transform.position = spawnPoints[2].position;

        for (int i = 0; i < _animalsInDen.Count; i++) {
            AnimalObject animalObject = _animalsInDen[i]; // activate again
            animalObject.gameObject.SetActive(true);
            var stateMachine = animalObject.GetComponent<AnimalStateMachine>();
            if (stateMachine != null)
                stateMachine.SetStateToIdle();
        }

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
        if (!_waveManager.isWaveActive) {
            _waveManager.StartWave();
        }

        _breedingCount = 0;
        _waveCountdown = 15;
        _timerEnabled = false;
        waveStartTimerText?.gameObject.SetActive(false);
        UpdateTimerUI();
    }

    private void UpdateTimerUI() {
        if (waveStartTimerText == null) return;
        waveStartTimerText.text = $"Wave incoming: {_waveCountdown}";
    }

    public void ResetBreedSystem() //method that provides object behaviours reset cleanup parameters before method operation or any other actions happens
      { if (CountAnimalsInsideDen == 0) return; _isBreeding = false; StopAllCoroutines(); _animalsInDen.Clear(); }  //reset  settings, by clear states and memory. When needed or called
}