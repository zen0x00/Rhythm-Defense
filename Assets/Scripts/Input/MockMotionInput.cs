using UnityEngine;
using System;

public class MockMotionInput : MonoBehaviour, IMotionInput
{
    public event Action OnLeftArmSwing;
    public event Action OnRightArmSwing;
    public event Action<float> OnMarchBeat;

    public ArmSwingDirection CurrentSwingDirection => _currentDirection;
    public float MarchIntensity => _marchIntensity;
    public bool IsLeftArmActive => _isLeftActive;
    public bool IsRightArmActive => _isRightActive;

    [Header("Input Keys")]
    public KeyCode leftArmKey = KeyCode.A;
    public KeyCode rightArmKey = KeyCode.D;
    public KeyCode marchKey = KeyCode.Space;

    [Header("Timing")]
    public float armHoldDuration = 0.3f;
    public float marchCooldown = 0.2f;

    private ArmSwingDirection _currentDirection = ArmSwingDirection.None;
    private float _marchIntensity;
    private bool _isLeftActive;
    private bool _isRightActive;
    private float _leftTimer;
    private float _rightTimer;
    private float _lastMarchTime;

    void Update()
    {
        HandleArmInput();
        HandleMarchInput();
        UpdateTimers();
    }

    void HandleArmInput()
    {
        // Left arm
        if (Input.GetKeyDown(leftArmKey))
        {
            _isLeftActive = true;
            _leftTimer = armHoldDuration;
            OnLeftArmSwing?.Invoke();
            UpdateDirection();
        }

        // Right arm
        if (Input.GetKeyDown(rightArmKey))
        {
            _isRightActive = true;
            _rightTimer = armHoldDuration;
            OnRightArmSwing?.Invoke();
            UpdateDirection();
        }
    }

    void HandleMarchInput()
    {
        if (Input.GetKeyDown(marchKey) && Time.time > _lastMarchTime + marchCooldown)
        {
            _lastMarchTime = Time.time;
            _marchIntensity = 1f; // Perfect for mock
            OnMarchBeat?.Invoke(1f);
        }
    }

    void UpdateTimers()
    {
        if (_isLeftActive)
        {
            _leftTimer -= Time.deltaTime;
            if (_leftTimer <= 0) { _isLeftActive = false; UpdateDirection(); }
        }
        if (_isRightActive)
        {
            _rightTimer -= Time.deltaTime;
            if (_rightTimer <= 0) { _isRightActive = false; UpdateDirection(); }
        }

        _marchIntensity = Mathf.Max(0, _marchIntensity - Time.deltaTime * 2f);
    }

    void UpdateDirection()
    {
        if (_isLeftActive && _isRightActive)
            _currentDirection = ArmSwingDirection.Both;
        else if (_isLeftActive)
            _currentDirection = ArmSwingDirection.Left;
        else if (_isRightActive)
            _currentDirection = ArmSwingDirection.Right;
        else
            _currentDirection = ArmSwingDirection.None;
    }
}
