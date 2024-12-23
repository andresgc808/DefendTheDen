using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public float holdDuration = 5f;
    [SerializeField] private float _holdTimer = 0f;
    [SerializeField] private bool _isHolding = false;
    public Slider slider;
    public TextMeshProUGUI timerText;
    public event Action OnWaveStart;
    public bool CanStartWave {  get {  return _holdTimer >= holdDuration; } }

    public List<Wave> waves; // list of scriptable objects
    private int _currentWaveIndex = 0;
    private List<GameObject> _currentEnemies = new List<GameObject>();
    public bool isWaveActive { get; private set; } = false;

    public static WaveManager Instance { get; private set; }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
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
        if(waves == null || waves.Count == 0) {
            Debug.Log("No waves are configured in the wave manager!");
            return;  // exit if there are no waves
        }
        _currentWaveIndex = (_currentWaveIndex + 1) % waves.Count;
        isWaveActive = true;
        Debug.Log($"Wave Start triggered. Current Wave: {_currentWaveIndex}!");
        StartCoroutine(SpawnWave(waves[_currentWaveIndex]));
        OnWaveStart?.Invoke();
    }

    private IEnumerator SpawnWave(Wave wave) {

        foreach (EnemySpawnType spawnType in wave.enemySpawns) {
            for (int i = 0; i < spawnType.amountOfEnemies; i++) {
                GameObject enemy = Instantiate(spawnType.enemyPrefab, spawnType.spawnPosition.position, Quaternion.identity);
                _currentEnemies.Add(enemy);
                yield return new WaitForSeconds(spawnType.spawnInterval);
            }
        }
        isWaveActive = false;
        yield break;
    }
}
