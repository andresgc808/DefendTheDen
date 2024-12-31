using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEditor;

public enum SpawnLocationType {
    Forest,
    Hill,
    Mountain
}

public class WaveManager : MonoBehaviour {
    public float holdDuration = 5f;
    [SerializeField] private float _holdTimer = 0f;
    [SerializeField] private bool _isHolding = false;
    public Slider slider;
    public TextMeshProUGUI timerText;
    public event Action OnWaveStart;
    public bool CanStartWave { get { return _holdTimer >= holdDuration; } }

    public List<Wave> waves; // list of scriptable objects
    private int _currentWaveIndex = 0;
    private List<GameObject> _currentEnemies = new List<GameObject>();
    public bool isWaveActive { get; private set; } = false;
    private bool _isStartingWave = false;

    public static WaveManager Instance { get; private set; }

    //Serialized lists for each spawn location type
    public List<Transform> forestSpawnLocations = new List<Transform>();
    public List<Transform> hillSpawnLocations = new List<Transform>();
    public List<Transform> mountainSpawnLocations = new List<Transform>();

    private Dictionary<SpawnLocationType, List<Transform>> _spawnLocations = new Dictionary<SpawnLocationType, List<Transform>>();
    public Dictionary<SpawnLocationType, List<Transform>> spawnLocations { get { return _spawnLocations; } private set { _spawnLocations = value; } }


    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
        InitializeSpawnLocations();
    }
    private void InitializeSpawnLocations() {
        spawnLocations = new Dictionary<SpawnLocationType, List<Transform>>();
        spawnLocations.Add(SpawnLocationType.Forest, forestSpawnLocations);
        spawnLocations.Add(SpawnLocationType.Hill, hillSpawnLocations);
        spawnLocations.Add(SpawnLocationType.Mountain, mountainSpawnLocations);
    }



    private void Update() {
        if (Input.GetKey(KeyCode.Space)) {
            if (!_isHolding) {
                _isHolding = true;
            }

            _holdTimer += Time.deltaTime;

            if (CanStartWave) {
                _holdTimer = 0f;
                StartWave();
                _isHolding = false;
                return;
            }
        }

        if (_isHolding && !Input.GetKey(KeyCode.Space)) {
            _holdTimer = 0f;
            _isHolding = false;
        }
        UpdateUI();
    }

    private void UpdateUI() {
        if (timerText == null) return;

        // show /hide UI elements based on whether a wave is active
        timerText.gameObject.SetActive(!isWaveActive);
        slider.gameObject.SetActive(!isWaveActive);

        float sliderPercentage = _holdTimer / holdDuration;

        if (slider != null)
            slider.value = sliderPercentage;
        if (!isWaveActive) {
            timerText.text = $"Hold space to start wave"; // reset text based on game cycle
        }
    }

    public void StartWave() {
        if (isWaveActive) {
            Debug.LogError("StartWave was called while a wave was already active!");
            return;
        }

        if (_isStartingWave) {
            Debug.LogError("StartWave was called while a wave was already starting!");
            return;
        }

        if (waves == null || waves.Count == 0) {
            Debug.LogError("No waves are configured in the wave manager!");
            return;  // exit if there are no waves
        }

        _isStartingWave = true;

        _currentWaveIndex = (_currentWaveIndex + 1) % waves.Count;
        isWaveActive = true;
        Debug.Log($"Wave Start triggered. Current Wave: {_currentWaveIndex}!");
        StartCoroutine(SpawnWave(waves[_currentWaveIndex]));
        OnWaveStart?.Invoke();
    }

    private IEnumerator SpawnWave(Wave wave) {
        int remainingCoins = wave.waveCoinBudget;
        List<EnemySpawnType> availableSpawns = new List<EnemySpawnType>(wave.enemySpawns); // make a copy to work with

        while (remainingCoins > 0 && availableSpawns.Count > 0) {
            // Step 1: Filter out enemies that cannot be afforded.
            List<EnemySpawnType> affordableSpawns = new List<EnemySpawnType>();
            foreach (EnemySpawnType spawnType in availableSpawns) {
                EnemyStateMachine enemyStateMachine = spawnType.enemyPrefab.GetComponent<EnemyStateMachine>();

                if (enemyStateMachine != null && enemyStateMachine.EnemyData != null && enemyStateMachine.EnemyData.cost <= remainingCoins) {
                    affordableSpawns.Add(spawnType);
                } else if (enemyStateMachine == null || enemyStateMachine.EnemyData == null) {
                    Debug.LogWarning($"Enemy prefab: {spawnType.enemyPrefab.name} does not have EnemyStateMachine or Enemy Data attached!");
                }
            }
            // Step 2: If there are no affordable enemies, exit
            if (affordableSpawns.Count == 0) {
                break;
            }
            // Step 3: Select a random enemy from affordable spawns
            int randomIndex = Random.Range(0, affordableSpawns.Count);
            EnemySpawnType selectedSpawnType = affordableSpawns[randomIndex];
            Enemy selectedEnemy = selectedSpawnType.enemyPrefab.GetComponent<EnemyStateMachine>().EnemyData;

            // Step 4: Try to spawn the enemy while there are coins remaining.
            if (TrySpawnEnemy(selectedSpawnType, selectedEnemy, ref remainingCoins)) {
                Debug.Log($"Spawning enemy: {selectedEnemy.enemyName}");
            } else {
                Debug.LogWarning($"Failed to spawn any of enemy {selectedEnemy.name}, removing from list!");
                availableSpawns.Remove(selectedSpawnType); // remove from potential spawns list
            }

            yield return new WaitForSeconds(0.5f);
        }

        _isStartingWave = false;
        isWaveActive = false;
        yield break;
    }

    private bool TrySpawnEnemy(EnemySpawnType spawnType, Enemy enemy, ref int remainingCoins) {
        // Step 1: Get preferred spawn locations
        List<SpawnLocationType> preferredLocations = spawnType.preferredSpawnLocations;
        if (preferredLocations == null || preferredLocations.Count == 0) {
            Debug.LogError($"Enemy {enemy.enemyName} has no preferred spawn locations!");
            return false;
        }
        // Step 2: find available locations
        List<SpawnLocationType> availableLocations = new List<SpawnLocationType>();
        foreach (SpawnLocationType locationType in preferredLocations) {
            if (spawnLocations.ContainsKey(locationType)) {
                availableLocations.Add(locationType);
            }
        }
        // step 3: No valid spawn locations
        if (availableLocations.Count == 0) {
            Debug.LogWarning($"No valid spawn locations for enemy type {enemy.enemyName}!");
            return false;
        }

        // Step 4: Randomly select one of the available locations
        int randomLocationIndex = Random.Range(0, availableLocations.Count);
        SpawnLocationType selectedLocationType = availableLocations[randomLocationIndex];

        //step 5: Randomly select one of the spawn points within that location type
        List<Transform> possibleSpawnPoints = spawnLocations[selectedLocationType];
        if (possibleSpawnPoints == null || possibleSpawnPoints.Count == 0) {
            Debug.LogError($"No valid spawn points for location {selectedLocationType}!");
            return false;
        }

        int randomSpawnIndex = Random.Range(0, possibleSpawnPoints.Count);
        Transform spawnLocation = possibleSpawnPoints[randomSpawnIndex];

        if (spawnLocation == null) {
            Debug.LogError("Failed to find a valid spawn location from dictionary! (This should never happen)");
            return false;
        }
        // Step 6: Check if we can afford
        if (remainingCoins < enemy.cost) {
            return false;
        }

        // Step 7: Instantiate enemy and deduct coins
        GameObject enemyInstance = Instantiate(spawnType.enemyPrefab, spawnLocation.position, Quaternion.identity);
        var obj = enemyInstance.GetComponent<EnemyStateMachine>();
        if (obj != null) {
            obj.SetEnemyData(enemy);
        } else {
            Debug.LogWarning($"Enemy Prefab: {enemy.name} doesn't have EnemyStateMachine attached!");
        }
        _currentEnemies.Add(enemyInstance);
        remainingCoins -= enemy.cost;
        return true;
    }
}