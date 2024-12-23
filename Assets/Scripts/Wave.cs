using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWave", menuName = "Waves/Wave")]
public class Wave : ScriptableObject
{
    public List<EnemySpawnType> enemySpawns = new List<EnemySpawnType>();
}

[System.Serializable] // makes struct visible in editor
public struct EnemySpawnType {
    public GameObject enemyPrefab; // which type of enemy to spawn
    public float spawnInterval; //  how often to spawn in seconds
    public Transform spawnPosition;
    public int amountOfEnemies;

    public EnemySpawnType(GameObject prefab = null, Transform transform = null, float interval = 1f, int amount = 1) { // constructor
        spawnInterval = interval;
        amountOfEnemies = amount;
        enemyPrefab = prefab;
        spawnPosition = transform;
    }
}
