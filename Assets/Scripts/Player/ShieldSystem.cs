using UnityEngine;
using System;

public class ShieldSystem : MonoBehaviour
{
    public static ShieldSystem Instance { get; private set; }

    [Header("Shield Visuals")]
    public GameObject leftShieldVisual;
    public GameObject rightShieldVisual;
    public GameObject fullShieldVisual;

    [Header("Shield Settings")]
    public float shieldDuration = 0.5f;
    public float beatMultiplier = 1.5f;

    [Header("Visual Effects")]
    public Color activeColor = new Color(0.3f, 0.7f, 1f, 0.8f);
    public Color inactiveColor = new Color(0.3f, 0.7f, 1f, 0.3f);
    public float beatPulseScale = 1.3f;
    [SerializeField] private float _pulseFrequency = 10f;

    public event Action<ArmSwingDirection> OnShieldActivated;

    private IMotionInput _motionInput;
    private float _leftTimer;
    private float _rightTimer;
    private bool _leftActive;
    private bool _rightActive;
    private float _currentMultiplier = 1f;

    private Material _leftMat;
    private Material _rightMat;
    private Material _fullMat;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        _motionInput = FindObjectOfType<MockMotionInput>() as IMotionInput;
        if (_motionInput == null)
        {
            Debug.LogError("No IMotionInput found in scene!");
            return;
        }

        _motionInput.OnLeftArmSwing += ActivateLeftShield;
        _motionInput.OnRightArmSwing += ActivateRightShield;

        if (BeatManager.Instance != null)
            BeatManager.Instance.OnBeat += HandleBeat;
        else
            Debug.LogError("BeatManager not found!");

        _leftMat = leftShieldVisual != null ? leftShieldVisual.GetComponent<Renderer>()?.material : null;
        _rightMat = rightShieldVisual != null ? rightShieldVisual.GetComponent<Renderer>()?.material : null;
        _fullMat = fullShieldVisual != null ? fullShieldVisual.GetComponent<Renderer>()?.material : null;
    }

    void OnDestroy()
    {
        if (_motionInput != null)
        {
            _motionInput.OnLeftArmSwing -= ActivateLeftShield;
            _motionInput.OnRightArmSwing -= ActivateRightShield;
        }

        if (BeatManager.Instance != null)
            BeatManager.Instance.OnBeat -= HandleBeat;

        if (_leftMat != null) Destroy(_leftMat);
        if (_rightMat != null) Destroy(_rightMat);
        if (_fullMat != null) Destroy(_fullMat);

        if (Instance == this) Instance = null;
    }

    void Update()
    {
        UpdateShieldTimers();
        UpdateVisuals();
    }

    void ActivateLeftShield()
    {
        _leftActive = true;
        _leftTimer = shieldDuration * _currentMultiplier;
        TriggerImpact(_leftMat);
        OnShieldActivated?.Invoke(ArmSwingDirection.Left);
    }

    void ActivateRightShield()
    {
        _rightActive = true;
        _rightTimer = shieldDuration * _currentMultiplier;
        TriggerImpact(_rightMat);
        OnShieldActivated?.Invoke(ArmSwingDirection.Right);
    }

    void TriggerImpact(Material mat)
    {
        if (mat == null) return;
        mat.SetFloat("_ImpactTime", Time.time);
        mat.SetVector("_ImpactPos", new Vector4(0.5f, 0.5f, 0f, 0f));
    }

    void HandleBeat()
    {
        _currentMultiplier = beatMultiplier;
    }

    void UpdateShieldTimers()
    {
        if (_leftActive)
        {
            _leftTimer -= Time.deltaTime;
            if (_leftTimer <= 0) _leftActive = false;
        }
        if (_rightActive)
        {
            _rightTimer -= Time.deltaTime;
            if (_rightTimer <= 0) _rightActive = false;
        }

        _currentMultiplier = Mathf.Lerp(_currentMultiplier, 1f, Time.deltaTime * 2f);
    }

    void UpdateVisuals()
    {
        bool bothActive = _leftActive && _rightActive;

        if (leftShieldVisual != null)
        {
            leftShieldVisual.SetActive(_leftActive && !bothActive);
            if (_leftActive) PulseShield(leftShieldVisual);
        }
        if (rightShieldVisual != null)
        {
            rightShieldVisual.SetActive(_rightActive && !bothActive);
            if (_rightActive) PulseShield(rightShieldVisual);
        }
        if (fullShieldVisual != null)
        {
            fullShieldVisual.SetActive(bothActive);
            if (bothActive) PulseShield(fullShieldVisual);
        }

        UpdateShieldColors();
    }

    void UpdateShieldColors()
    {
        Color targetColor = (_leftActive || _rightActive) ? activeColor : inactiveColor;

        UpdateMaterialColor(_leftMat, targetColor);
        UpdateMaterialColor(_rightMat, targetColor);
        UpdateMaterialColor(_fullMat, targetColor);
    }

    void UpdateMaterialColor(Material mat, Color target)
    {
        if (mat == null) return;
        mat.color = Color.Lerp(mat.color, target, Time.deltaTime * 10f);
    }

    void PulseShield(GameObject shield)
    {
        if (shield == null) return;
        float pulse = 1f + Mathf.Sin(Time.time * _pulseFrequency) * 0.1f;
        shield.transform.localScale = Vector3.one * pulse * beatPulseScale;
    }

    public bool IsDirectionBlocked(ArmSwingDirection direction)
    {
        return direction switch
        {
            ArmSwingDirection.Left => _leftActive,
            ArmSwingDirection.Right => _rightActive,
            ArmSwingDirection.Both => _leftActive && _rightActive,
            _ => false
        };
    }
}
