using UnityEngine;

[CreateAssetMenu(fileName = "EnemyTypeData", menuName = "Scriptable Objects/EnemyTypeData")]
public class EnemyTypeData : ScriptableObject
{
    public string enemyName;
    public int maxHealth;
    public float attackCooldown;
    public float moveSpeed;
    public float patrolRange;
    public int attackDamage;
    public Color enemyColor;
}

