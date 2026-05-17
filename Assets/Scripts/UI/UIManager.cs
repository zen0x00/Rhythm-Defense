using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI waveText;
    public GameObject gameOverPanel;
    public GameObject startPanel;
    public TextMeshProUGUI comboMultiplierText;

    [Header("Beat Indicator")]
    public RectTransform beatIndicator;
    public float beatScale = 1.3f;
    public float beatLerpSpeed = 10f;

    private Vector3 _originalIndicatorScale;
    private bool _isPulsing;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (startPanel != null) startPanel.SetActive(true);
    }

    void Start()
    {
        if (beatIndicator != null)
            _originalIndicatorScale = beatIndicator.localScale;

        SubscribeToEvents();
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnLivesChanged -= UpdateLives;
            GameManager.Instance.OnComboChanged -= UpdateCombo;
            GameManager.Instance.OnGameOver -= ShowGameOver;
            GameManager.Instance.OnGameStart -= HideStartPanel;
        }

        if (EnemyManager.Instance != null)
            EnemyManager.Instance.OnWaveStart -= UpdateWave;

        if (BeatManager.Instance != null)
            BeatManager.Instance.OnBeat -= PulseBeatIndicator;

        if (Instance == this) Instance = null;
    }

    void SubscribeToEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnLivesChanged += UpdateLives;
            GameManager.Instance.OnComboChanged += UpdateCombo;
            GameManager.Instance.OnGameOver += ShowGameOver;
            GameManager.Instance.OnGameStart += HideStartPanel;
        }

        if (EnemyManager.Instance != null)
            EnemyManager.Instance.OnWaveStart += UpdateWave;

        if (BeatManager.Instance != null)
            BeatManager.Instance.OnBeat += PulseBeatIndicator;
    }

    void Update()
    {
        if (beatIndicator != null && _isPulsing)
        {
            beatIndicator.localScale = Vector3.Lerp(
                beatIndicator.localScale,
                _originalIndicatorScale,
                Time.deltaTime * beatLerpSpeed
            );

            if (Vector3.Distance(beatIndicator.localScale, _originalIndicatorScale) < 0.01f)
                _isPulsing = false;
        }
    }

    void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    void UpdateLives(int lives)
    {
        if (livesText != null)
            livesText.text = $"Lives: {lives}";
    }

    void UpdateCombo(int combo)
    {
        if (comboText != null)
        {
            comboText.text = combo > 0 ? $"Combo: {combo}x" : "";
            comboText.gameObject.SetActive(combo > 0);
        }

        if (comboMultiplierText != null)
        {
            int multiplier = Mathf.Min(combo / 5 + 1, 5);
            comboMultiplierText.text = multiplier > 1 ? $"{multiplier}x" : "";
        }
    }

    void UpdateWave(int wave)
    {
        if (waveText != null)
            waveText.text = $"Wave: {wave}";
    }

    void PulseBeatIndicator()
    {
        if (beatIndicator != null)
        {
            beatIndicator.localScale = _originalIndicatorScale * beatScale;
            _isPulsing = true;
        }
    }

    void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    void HideStartPanel()
    {
        if (startPanel != null)
            startPanel.SetActive(false);
    }

    public void OnStartButtonPressed()
    {
        GameManager.Instance?.StartGame();
    }

    public void OnRestartButtonPressed()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        GameManager.Instance?.RestartGame();
    }
}
