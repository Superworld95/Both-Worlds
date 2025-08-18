using UnityEngine;

/// <summary>
/// GameManager controls spawning enemies from ScriptableObject data,
/// manages spawn points, and tracks enemy kills and player scores.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Enemy Spawning")]
    [Tooltip("Prefab that has the EnemyAI script attached.")]
    public EnemyAI enemyPrefab;

    [Tooltip("All possible enemy types stored as ScriptableObjects.")]
    public EnemyTypeData[] enemyTypes;

    [Tooltip("All spawn points where enemies can be placed.")]
    public Transform[] spawnPoints;

    [Header("Gameplay Settings")]
    [Tooltip("If true, enemies will be spawned automatically on Start.")]
    public bool autoSpawnOnStart = true;

    // Player kill counts
    private int player1Kills = 0;
    private int player2Kills = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (autoSpawnOnStart)
        {
            SpawnEnemies();
        }
    }

    /// <summary>
    /// Spawns one enemy at each spawn point, assigning a random type from the enemyTypes array.
    /// Ensures enemy prefab and type are valid before initializing.
    /// </summary>
    public void SpawnEnemies()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("GameManager: No spawn points assigned!");
            return;
        }

        if (enemyTypes == null || enemyTypes.Length == 0)
        {
            Debug.LogWarning("GameManager: No enemy types assigned!");
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogError("GameManager: Enemy prefab is not assigned!");
            return;
        }

        foreach (Transform spawn in spawnPoints)
        {
            // Choose a random type for this enemy
            EnemyTypeData chosenType = enemyTypes[Random.Range(0, enemyTypes.Length)];

            if (chosenType == null)
            {
                Debug.LogWarning("GameManager: Chosen EnemyTypeData is null. Skipping spawn.");
                continue;
            }

            // Instantiate enemy
            EnemyAI newEnemy = Instantiate(enemyPrefab, spawn.position, Quaternion.identity);

            if (newEnemy == null)
            {
                Debug.LogError("GameManager: Failed to instantiate enemy prefab.");
                continue;
            }

            // Assign type data safely
            newEnemy.SetEnemyTypeData(chosenType);
        }
    }

    /// <summary>
    /// Adds a kill to the specified player and logs the new score.
    /// </summary>
    /// <param name="player">Player object who killed the enemy</param>
    public void AddEnemyKill(object player)
    {
        if (player is Player1Controller)
        {
            player1Kills++;
            Debug.Log($"Player 1 scored! Total kills: {player1Kills}");
        }
        else if (player is Player2Controller)
        {
            player2Kills++;
            Debug.Log($"Player 2 scored! Total kills: {player2Kills}");
        }
        else
        {
            Debug.LogWarning("Unknown player tried to add kill.");
        }
    }

    /// <summary>
    /// Returns player 1's kill count.
    /// </summary>
    public int GetPlayer1Kills()
    {
        return player1Kills;
    }

    /// <summary>
    /// Returns player 2's kill count.
    /// </summary>
    public int GetPlayer2Kills()
    {
        return player2Kills;
    }

    /// <summary>
    /// Returns total kills (sum of both players).
    /// </summary>
    public int GetTotalKills()
    {
        return player1Kills + player2Kills;
    }
}
