using UnityEngine;
using System;

public class Enemy : MonoBehaviour
{
    public enum MoveDirection
    {
        FromLeft,
        FromRight,
        FromTop
    }

    [Header("Movement")]
    public MoveDirection direction;
    public float speed = 3f;
    public Transform targetPoint;

    [Header("Scoring")]
    public int pointsOnBlock = 10;
    public int pointsOnMiss = 5;

    private bool _isBlocked;
    private bool _hasReachedCenter;

    public event Action<Enemy, bool> OnReachedCenter;

    void Start()
    {
        if (targetPoint == null)
            targetPoint = GameObject.Find("PlayerCenter")?.transform;
    }

    void Update()
    {
        if (targetPoint == null || _hasReachedCenter) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.5f)
        {
            HandleReachedCenter();
        }
    }

    void HandleReachedCenter()
    {
        _hasReachedCenter = true;

        ArmSwingDirection requiredDirection = DirectionToArmSwing(direction);
        _isBlocked = ShieldSystem.Instance != null &&
                     ShieldSystem.Instance.IsDirectionBlocked(requiredDirection);

        OnReachedCenter?.Invoke(this, _isBlocked);
        Destroy(gameObject);
    }

    ArmSwingDirection DirectionToArmSwing(MoveDirection dir)
    {
        return dir switch
        {
            MoveDirection.FromLeft => ArmSwingDirection.Left,
            MoveDirection.FromRight => ArmSwingDirection.Right,
            _ => ArmSwingDirection.Both
        };
    }

    public bool IsBlocked => _isBlocked;
}

[System.Serializable]
public class EnemySpawnData
{
    public GameObject enemyPrefab;
    public Enemy.MoveDirection direction;
    public float spawnDelay = 0f;
}
