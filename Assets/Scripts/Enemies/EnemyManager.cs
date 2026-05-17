using UnityEngine;
using System.Collections.Generic;
using System;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [Header("Spawn Points")]
    public Transform leftSpawn;
    public Transform rightSpawn;
    public Transform topSpawn;

    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public float baseSpawnInterval = 2f;
    public float minSpawnInterval = 0.5f;
    public int beatsBetweenSpawn = 2;

    [Header("Difficulty")]
    public float speedIncrease = 0.1f;
    public int waveSize = 5;

    public event Action<Enemy> OnEnemySpawned;
    public event Action<int> OnWaveStart;

    private int _currentBeat;
    private int _waveNumber = 1;
    private List<Enemy> _activeEnemies = new List<Enemy>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (BeatManager.Instance != null)
            BeatManager.Instance.OnBeat += OnBeat;
    }

    void OnDestroy()
    {
        if (BeatManager.Instance != null)
            BeatManager.Instance.OnBeat -= OnBeat;
        if (Instance == this) Instance = null;
    }

    void OnBeat()
    {
        _currentBeat++;

        if (_currentBeat % beatsBetweenSpawn == 0)
            SpawnEnemy();

        if (_currentBeat % (waveSize * beatsBetweenSpawn) == 0)
            StartNewWave();
    }

    void StartNewWave()
    {
        _waveNumber++;
        beatsBetweenSpawn = Mathf.Max(1, beatsBetweenSpawn - 1);
        OnWaveStart?.Invoke(_waveNumber);
        Debug.Log($"Wave {_waveNumber} started!");
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("No enemy prefab assigned!");
            return;
        }

        var directions = new Enemy.MoveDirection[]
        {
            Enemy.MoveDirection.FromLeft,
            Enemy.MoveDirection.FromRight,
            Enemy.MoveDirection.FromTop
        };

        Enemy.MoveDirection dir = directions[UnityEngine.Random.Range(0, directions.Length)];
        Transform spawnPoint = GetSpawnPoint(dir);

        if (spawnPoint == null)
        {
            Debug.LogWarning($"No spawn point assigned for direction {dir}!");
            return;
        }

        GameObject enemyObj = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        Enemy enemy = enemyObj.GetComponent<Enemy>();

        if (enemy == null)
            enemy = enemyObj.AddComponent<Enemy>();

        enemy.direction = dir;
        enemy.speed += _waveNumber * speedIncrease;

        enemy.OnReachedCenter += HandleEnemyReachedCenter;
        _activeEnemies.Add(enemy);
        OnEnemySpawned?.Invoke(enemy);
    }

    Transform GetSpawnPoint(Enemy.MoveDirection dir)
    {
        return dir switch
        {
            Enemy.MoveDirection.FromLeft => leftSpawn,
            Enemy.MoveDirection.FromRight => rightSpawn,
            Enemy.MoveDirection.FromTop => topSpawn,
            _ => leftSpawn
        };
    }

    void HandleEnemyReachedCenter(Enemy enemy, bool wasBlocked)
    {
        _activeEnemies.Remove(enemy);

        if (!wasBlocked)
            GameManager.Instance?.TakeDamage(1);
    }

    public void ClearAllEnemies()
    {
        foreach (var enemy in _activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }
        _activeEnemies.Clear();
    }
}
