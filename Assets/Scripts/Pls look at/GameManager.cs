using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Central game management system that controls the overall game flow, timing, win conditions,
/// and scene management for the 2D platformer combat adventure game.
/// Handles unified adventure mode with 5-minute time limit and multiple win conditions.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public float gameTimeLimit = 300f;
    public int enemyCount = 10;
    public int platformCount = 20;
    public int movingPlatformCount = 6;
    public float finishLineX = 3000f;

    [Header("UI References")]
    public Text timeText;
    public Text player1HealthText;
    public Text player2HealthText;
    public Text player1KillsText;
    public Text player2KillsText;
    public GameObject gameOverPanel;
    public Text winnerText;

    [Header("Prefabs")]
    public GameObject enemyPrefab;
    public GameObject platformPrefab;
    public GameObject movingPlatformPrefab;
    public GameObject powerupPrefab;
    public GameObject trapPrefab;

    public static GameManager Instance { get; private set; }

    private float gameTime = 0f;
    private bool gameActive = false;
    private Player1Controller player1;
    private Player2Controller player2;
    private List<EnemyAI> enemies = new List<EnemyAI>();

    public enum GamePhase { Menu, CharacterSelection, Adventure, GameOver }
    public GamePhase currentPhase = GamePhase.Menu;

    public enum WinCondition { FinishLine, MostKills, TimeUp }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        player1 = FindObjectOfType<Player1Controller>();
        player2 = FindObjectOfType<Player2Controller>();
        InitializeGame();
    }

    private void Update()
    {
        if (gameActive && currentPhase == GamePhase.Adventure)
        {
            UpdateGameTimer();
            CheckWinConditions();
            UpdateUI();
        }
    }

    public void StartAdventure()
    {
        Debug.Log("GameManager: Starting adventure mode...");

        currentPhase = GamePhase.Adventure;
        gameTime = 0f;

        SpawnEnemies();
        SpawnPlatforms();
        SpawnPowerups();

        if (player1 != null) player1.ResetPlayer();
        if (player2 != null) player2.ResetPlayer();

        gameActive = true;

        Debug.Log($"GameManager: Adventure started! {enemies.Count} enemies spawned, game is now active.");
    }

    private void UpdateGameTimer()
    {
        gameTime += Time.deltaTime;

        if (gameTime >= gameTimeLimit)
        {
            EndGame(DetermineWinnerByScore(), WinCondition.TimeUp);
        }
    }

    private void CheckWinConditions()
    {
        if (player1 != null && player1.transform.position.x >= finishLineX && player1.IsAlive())
        {
            EndGame(player1, WinCondition.FinishLine);
            return;
        }

        if (player2 != null && player2.transform.position.x >= finishLineX && player2.IsAlive())
        {
            EndGame(player2, WinCondition.FinishLine);
            return;
        }

        if (enemies.Count > 0 && enemies.All(e => !e.IsAlive()))
        {
            var topKiller = GetTopKiller();
            EndGame(topKiller, WinCondition.MostKills);
            return;
        }

        bool player1Dead = player1 == null || !player1.IsAlive();
        bool player2Dead = player2 == null || !player2.IsAlive();

        if (player1Dead && player2Dead)
        {
            EndGame(null, WinCondition.MostKills);
        }
    }

    private object DetermineWinnerByScore()
    {
        bool player1Alive = player1 != null && player1.IsAlive();
        bool player2Alive = player2 != null && player2.IsAlive();

        if (!player1Alive && !player2Alive) return null;
        if (player1Alive && !player2Alive) return player1;
        if (!player1Alive && player2Alive) return player2;

        int player1Kills = player1.GetKills();
        int player2Kills = player2.GetKills();

        if (player1Kills > player2Kills) return player1;
        if (player2Kills > player1Kills) return player2;

        float player1Progress = player1.transform.position.x;
        float player2Progress = player2.transform.position.x;

        return player1Progress >= player2Progress ? player1 : player2;
    }

    private object GetTopKiller()
    {
        if (player1 == null && player2 == null) return null;
        if (player1 == null) return player2;
        if (player2 == null) return player1;

        return player1.GetKills() >= player2.GetKills() ? player1 : player2;
    }

    private void EndGame(object winner, WinCondition condition)
    {
        gameActive = false;
        currentPhase = GamePhase.GameOver;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        string winMessage = winner == null
            ? "ADVENTURE COMPLETE - NO WINNER"
            : GetWinMessage(winner, condition);

        if (winnerText != null)
            winnerText.text = winMessage;
    }

    private string GetWinMessage(object winner, WinCondition condition)
    {
        string playerName = GetPlayerName(winner);

        switch (condition)
        {
            case WinCondition.FinishLine:
                return $"{playerName} WINS BY REACHING THE FINISH!";
            case WinCondition.MostKills:
                return $"{playerName} WINS WITH MOST KILLS!";
            case WinCondition.TimeUp:
                return $"{playerName} WINS BY SUPERIOR PERFORMANCE!";
            default:
                return $"{playerName} WINS!";
        }
    }

    private string GetPlayerName(object player)
    {
        if (player is Player1Controller p1) return p1.GetPlayerName();
        if (player is Player2Controller p2) return p2.GetPlayerName();
        return "Unknown Player";
    }

    private void SpawnEnemies()
    {
        enemies.Clear();

        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPos = new Vector3(300 + (i * 200) + Random.Range(0, 100), 0, 0);
            GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            EnemyAI enemy = enemyObj.GetComponent<EnemyAI>();

            EnemyAI.EnemyType[] types = { EnemyAI.EnemyType.Basic, EnemyAI.EnemyType.Fast, EnemyAI.EnemyType.Heavy };
            enemy.SetEnemyType(types[Random.Range(0, types.Length)]);

            enemies.Add(enemy);
        }

        Debug.Log($"GameManager: Spawned {enemies.Count} enemies for the adventure");
    }

    private void SpawnPlatforms()
    {
        for (int i = 0; i < platformCount; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(200, finishLineX - 200),
                Random.Range(50, 300),
                0
            );
            Instantiate(platformPrefab, pos, Quaternion.identity);
        }

        for (int i = 0; i < movingPlatformCount; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(800, finishLineX - 500),
                Random.Range(100, 250),
                0
            );
            Instantiate(movingPlatformPrefab, pos, Quaternion.identity);
        }
    }

    private void SpawnPowerups()
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(400, finishLineX - 400),
                Random.Range(50, 200),
                0
            );
            Instantiate(powerupPrefab, pos, Quaternion.identity);
        }
    }

    private void InitializeGame()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        gameActive = false;
        currentPhase = GamePhase.Menu;
        gameTime = 0f;
        enemies.Clear();

        Debug.Log("GameManager: Initialized to clean state - ready for adventure start");
    }

    private void UpdateUI()
    {
        float remainingTime = Mathf.Max(0, gameTimeLimit - gameTime);
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        if (timeText != null)
            timeText.text = $"{minutes:00}:{seconds:00}";

        if (player1 != null)
        {
            player1HealthText.text = $"Health: {player1.GetHealth()}/100";
            player1KillsText.text = $"Kills: {player1.GetKills()}";
        }

        if (player2 != null)
        {
            player2HealthText.text = $"Health: {player2.GetHealth()}/100";
            player2KillsText.text = $"Kills: {player2.GetKills()}";
        }
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void AddEnemyKill(object player)
    {
        if (player is Player1Controller p1) p1.AddKill();
        if (player is Player2Controller p2) p2.AddKill();
    }

    public bool IsGameActive() => gameActive;
}
