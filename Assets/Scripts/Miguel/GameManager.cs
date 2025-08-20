using UnityEngine;

/// <summary>
/// GameManager controls spawning enemies from ScriptableObject data,
/// manages spawn points, and tracks enemy kills and player scores.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Enemy Spawning")]
    [Tooltip("Prefab for the basic enemy (with EnemyAI script attached).")]
    public EnemyAI basicEnemyPrefab;

    [Tooltip("Data for the basic enemy type.")]
    public EnemyTypeData basicEnemyData;

    [Tooltip("Prefab for the fast enemy (with EnemyAI script attached).")]
    public EnemyAI fastEnemyPrefab;

    [Tooltip("Data for the fast enemy type.")]
    public EnemyTypeData fastEnemyData;

    [Tooltip("Prefab for the heavy enemy (with EnemyAI script attached).")]
    public EnemyAI heavyEnemyPrefab;

    [Tooltip("Data for the heavy enemy type.")]
    public EnemyTypeData heavyEnemyData;

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
    /// Spawns enemies at each spawn point.
    /// Chooses between basic, fast, and heavy enemies, and assigns matching type data.
    /// </summary>
    public void SpawnEnemies()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("GameManager: No spawn points assigned!");
            return;
        }

        if (basicEnemyPrefab == null || fastEnemyPrefab == null || heavyEnemyPrefab == null)
        {
            Debug.LogError("GameManager: One or more enemy prefabs are not assigned!");
            return;
        }

        if (basicEnemyData == null || fastEnemyData == null || heavyEnemyData == null)
        {
            Debug.LogError("GameManager: One or more enemy type data objects are not assigned!");
            return;
        }

        foreach (Transform spawn in spawnPoints)
        {
            // Randomly pick which enemy type to spawn
            int choice = Random.Range(0, 3); // 0 = basic, 1 = fast, 2 = heavy

            EnemyAI chosenPrefab = null;
            EnemyTypeData chosenData = null;

            switch (choice)
            {
                case 0:
                    chosenPrefab = basicEnemyPrefab;
                    chosenData = basicEnemyData;
                    break;
                case 1:
                    chosenPrefab = fastEnemyPrefab;
                    chosenData = fastEnemyData;
                    break;
                case 2:
                    chosenPrefab = heavyEnemyPrefab;
                    chosenData = heavyEnemyData;
                    break;
            }

            if (chosenPrefab == null || chosenData == null)
            {
                Debug.LogWarning("GameManager: Missing prefab or type data. Skipping spawn.");
                continue;
            }

            // Instantiate enemy
            EnemyAI newEnemy = Instantiate(chosenPrefab, spawn.position, Quaternion.identity);

            if (newEnemy == null)
            {
                Debug.LogError("GameManager: Failed to instantiate enemy prefab.");
                continue;
            }

            // Assign the correct type data
            newEnemy.SetEnemyTypeData(chosenData);
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
