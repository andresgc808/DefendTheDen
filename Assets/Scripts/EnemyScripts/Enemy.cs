using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Enemies/Enemy")]
public class Enemy : ScriptableObject {
    public string enemyName;
    public float moveSpeed;
    public float rotationSpeed;
    public float attackRange;
    public float attackPower;
    public float fireRate;
    public float maxHealth;
    public AttackType attackType;
    public GameObject projectilePrefab; // optional if every enemy has a unique projectile
}