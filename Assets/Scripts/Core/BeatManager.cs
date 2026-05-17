using UnityEngine;
using System;
using System.Collections.Generic;

public class BeatManager : MonoBehaviour
{
    public static BeatManager Instance { get; private set; }

    [Header("Audio")]
    public AudioSource audioSource;
    public float bpm = 120f;

    [Header("Beat Timing")]
    public float perfectWindow = 0.05f;
    public float goodWindow = 0.1f;
    public float okayWindow = 0.15f;

    public event Action OnBeat;
    public event Action<BeatAccuracy> OnBeatWithAccuracy;
    public event Action<float> OnBeatPredict;

    public float BeatInterval => 60f / bpm;
    public float CurrentBeatTime { get; private set; }
    public int CurrentBeatNumber { get; private set; }
    public float NextBeatETA => BeatInterval - (CurrentBeatTime % BeatInterval);

    private float _nextBeatTime;
    private Queue<float> _beatQueue = new Queue<float>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
            _nextBeatTime = Time.time + BeatInterval;
        }
    }

    void Update()
    {
        if (audioSource == null || !audioSource.isPlaying) return;

        CurrentBeatTime = audioSource.time;

        if (Time.time >= _nextBeatTime)
        {
            _nextBeatTime += BeatInterval;
            CurrentBeatNumber++;
            OnBeat?.Invoke();
        }

        float eta = NextBeatETA;
        if (eta < 0.2f)
            OnBeatPredict?.Invoke(eta);
    }

    public BeatAccuracy GetAccuracy(float inputTime)
    {
        float nearestBeat = Mathf.Round(inputTime / BeatInterval) * BeatInterval;
        float diff = Mathf.Abs(inputTime - nearestBeat);

        if (diff <= perfectWindow) return BeatAccuracy.Perfect;
        if (diff <= goodWindow) return BeatAccuracy.Good;
        if (diff <= okayWindow) return BeatAccuracy.Okay;
        return BeatAccuracy.Miss;
    }

    public void TriggerBeat(float accuracy = 1f)
    {
        OnBeat?.Invoke();
        OnBeatWithAccuracy?.Invoke(GetAccuracy(CurrentBeatTime));
    }
}

public enum BeatAccuracy
{
    Perfect,
    Good,
    Okay,
    Miss
}
