using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWave", menuName = "Waves/Wave")]
public class Wave : ScriptableObject
{
    public int waveCoinBudget;
    public List<EnemySpawnType> enemySpawns = new List<EnemySpawnType>();
}

[System.Serializable] // makes struct visible in editor
public struct EnemySpawnType {
    public GameObject enemyPrefab; // which type of enemy to spawn
    public List<SpawnLocationType> preferredSpawnLocations;

    public EnemySpawnType(GameObject prefab = null, List<SpawnLocationType> locations = null) { // constructor
        enemyPrefab = prefab;
        preferredSpawnLocations = locations;
    }
}
