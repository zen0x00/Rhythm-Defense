using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public int maxLives = 3;
    public int currentLives;
    public int score;
    public int combo;
    public int maxCombo;

    public event Action<int> OnLivesChanged;
    public event Action<int> OnScoreChanged;
    public event Action<int> OnComboChanged;
    public event Action OnGameOver;
    public event Action OnGameStart;

    private bool _isGameActive;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        currentLives = maxLives;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void StartGame()
    {
        score = 0;
        combo = 0;
        currentLives = maxLives;
        _isGameActive = true;

        OnGameStart?.Invoke();
        OnScoreChanged?.Invoke(score);
        OnLivesChanged?.Invoke(currentLives);
        OnComboChanged?.Invoke(combo);

        if (BeatManager.Instance != null && BeatManager.Instance.audioSource != null)
            BeatManager.Instance.audioSource.Play();
    }

    public void TakeDamage(int damage)
    {
        if (!_isGameActive) return;

        currentLives -= damage;
        combo = 0;
        OnLivesChanged?.Invoke(currentLives);
        OnComboChanged?.Invoke(combo);

        if (currentLives <= 0)
            GameOver();
    }

    public void AddScore(int points)
    {
        if (!_isGameActive) return;

        combo++;
        if (combo > maxCombo) maxCombo = combo;

        int multiplier = Mathf.Min(combo / 5 + 1, 5);
        score += points * multiplier;

        OnScoreChanged?.Invoke(score);
        OnComboChanged?.Invoke(combo);
    }

    void GameOver()
    {
        _isGameActive = false;
        OnGameOver?.Invoke();

        if (BeatManager.Instance != null && BeatManager.Instance.audioSource != null)
            BeatManager.Instance.audioSource.Stop();
    }

    public void RestartGame()
    {
        EnemyManager.Instance?.ClearAllEnemies();
        StartGame();
    }

    public bool IsGameActive => _isGameActive;
}
