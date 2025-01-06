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
    public event Action OnWaveEnd;
    public bool CanStartWave { get { return _holdTimer >= holdDuration; } }

    public List<Wave> waves; // list of scriptable objects
    private int _currentWaveIndex = 0;
    public int currentWaveIndex { get { return _currentWaveIndex; } private set { _currentWaveIndex = value; } }
    private List<GameObject> _currentEnemies = new List<GameObject>();
    public bool isWaveActive { get; private set; } = false;
    private bool _isStartingWave = false;

    [SerializeField] private TextMeshProUGUI _waveCounterText;

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
        _waveCounterText.gameObject.SetActive(true);
        UpdateWaveCounterUI();
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

        if (isWaveActive && _currentEnemies.Count == 0) {
            isWaveActive = false;
            UpdateWaveCounterUI();
            OnWaveEnd?.Invoke();
        }

        UpdateUI();
    }

    private void UpdateWaveCounterUI() {
        if (_waveCounterText == null) {
            Debug.Log("wave counter is null");
            return;
        }
        _waveCounterText.text = $"Wave: {_currentWaveIndex + 1}";
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
        
        isWaveActive = true;
        Debug.Log($"Wave Start triggered. Current Wave: {_currentWaveIndex + 1}!");
        StartCoroutine(SpawnWave(waves[_currentWaveIndex]));
        _currentWaveIndex = (_currentWaveIndex + 1) % waves.Count;
        OnWaveStart?.Invoke();
    }

    private IEnumerator SpawnWave(Wave wave) {
        int remainingCoins = wave.waveCoinBudget;
        List<EnemySpawnType> availableSpawns = new List<EnemySpawnType>(wave.enemySpawns);

        // Step 1: Filter out enemies that cannot be afforded.
        List<EnemySpawnType> affordableSpawns = new List<EnemySpawnType>();
        foreach (EnemySpawnType spawnType in availableSpawns) {

            if (spawnType.enemyPrefab != null && Resources.Load<Enemy>($"Enemies/{spawnType.enemyPrefab.name}") != null) {
                    affordableSpawns.Add(spawnType);
            } else {
                Debug.LogWarning($"Enemy Prefab: {spawnType.enemyPrefab} is NULL! remove prefab from list!");
                yield return null;
            }
        }

        while (remainingCoins > 0 && availableSpawns.Count > 0) {
            
            // Step 2: If there are no affordable enemies, exit
            if (affordableSpawns.Count == 0) {
                break;
            }
            // Step 3: Select a random enemy from affordable spawns
            int randomIndex = Random.Range(0, affordableSpawns.Count);
            EnemySpawnType selectedSpawnType = affordableSpawns[randomIndex];
            Enemy enemyData = Resources.Load<Enemy>($"Enemies/{selectedSpawnType.enemyPrefab.name}");

            
            if (remainingCoins < enemyData.cost) {
                // continue and remove from list
                Debug.LogWarning($"Not enough coins to spawn {enemyData.enemyName}, removing from list!");
                availableSpawns.Remove(selectedSpawnType); // remove from potential spawns list
                continue;
            }

            // Step 4: Try to spawn the enemy while there are coins remaining.
            if (TrySpawnEnemy(selectedSpawnType, ref remainingCoins, enemyData)) {
                Debug.Log($"Spawning enemy: {enemyData.enemyName}");
            } else {
                Debug.LogWarning($"Failed to spawn any of enemy {enemyData.name}, removing from list!");
                availableSpawns.Remove(selectedSpawnType); // remove from potential spawns list
            }

            yield return new WaitForSeconds(0.5f);
        }
        _isStartingWave = false;
        yield break;
    }

    private bool TrySpawnEnemy(EnemySpawnType spawnType, ref int remainingCoins, Enemy enemyData) {
        // Step 1: Get preferred spawn locations
        List<SpawnLocationType> preferredLocations = spawnType.preferredSpawnLocations;
        if (preferredLocations == null || preferredLocations.Count == 0) {
            Debug.LogError($"Enemy {spawnType.enemyPrefab.name} has no preferred spawn locations!");
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
            Debug.LogWarning($"No valid spawn locations for enemy type {spawnType.enemyPrefab.name}!");
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

        // Step 6: Instantiate enemy and deduct coins, using spawn data for configuration.
        GameObject enemyInstance = GenerateEnemy(spawnType.enemyPrefab, spawnLocation.position, Quaternion.identity, spawnType, enemyData);


        if (enemyInstance == null) return false;

        _currentEnemies.Add(enemyInstance); // add for memory management.
        remainingCoins -= enemyData.cost; // decrement the coins for this object.

        return true;
    }

    private GameObject GenerateEnemy(GameObject prefab, Vector3 position, Quaternion rotation, EnemySpawnType spawnType, Enemy enemyData) {  // method to get specific game object based on Enemy type data

        GameObject enemyInstance = Instantiate(prefab, position, rotation);  // create enemy

        if (enemyInstance == null) {
            Debug.LogWarning($"Error Instantiating prefab {prefab.name} from resources!");
            return null;
        }

        EnemyStateMachine obj = enemyInstance.GetComponent<EnemyStateMachine>(); // getting the state machine to pass all data at start.

        if (obj == null) {
            Debug.LogWarning($"Enemy Prefab: {prefab.name} doesn't have EnemyStateMachine attached!");
            return null;
        }

        var clone = Instantiate(enemyData);

        obj.EnemyData = DetermineEnemyVariation(clone); // setting the data to the state machine

        EnemyHealth health = enemyInstance.GetComponent<EnemyHealth>();
        if (health != null)
            health.OnDeath += () => RemoveEnemy(enemyInstance);

        return enemyInstance;
    }

    private Enemy DetermineEnemyVariation(Enemy baseEnemyData) {
        Enemy clonedEnemyData = Instantiate(baseEnemyData); // creates a new instance rather than setting properties directly, also avoiding shared data

        // Apply variation with controlled randomness, using the already loaded data.
        clonedEnemyData.moveSpeed = ApplyVariation(baseEnemyData.moveSpeed, 0.15f, 1, 20); // 15% range of original value
        clonedEnemyData.attackRange = ApplyVariation(baseEnemyData.attackRange, 0.15f, 1, 10);
        clonedEnemyData.attackPower = ApplyVariation(baseEnemyData.attackPower, 0.15f, 1, 20);
        clonedEnemyData.fireRate = ApplyVariation(baseEnemyData.fireRate, 0.15f, 0.1f, 10); // fire rate is more sensitive so smaller variation.
        clonedEnemyData.maxHealth = ApplyVariation(baseEnemyData.maxHealth, 0.15f, 10, 500);

        return clonedEnemyData;
    }

    private float ApplyVariation(float baseValue, float rangePercentage, float minLimit, float maxLimit) {

        float variation = baseValue * Random.Range(-rangePercentage, rangePercentage); // applies a percentage for a max range
        float newValue = baseValue + variation;
        return Mathf.Clamp(newValue, minLimit, maxLimit); // applies limits.
    }

    public void RemoveEnemy(GameObject enemy) {
        _currentEnemies.Remove(enemy);
        Debug.Log($"removed object {enemy.name}. Current count of active enemies is {_currentEnemies.Count}."); // just to make a debug.
    }
}