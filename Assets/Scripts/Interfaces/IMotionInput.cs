using System;

public enum ArmSwingDirection
{
    None,
    Left,
    Right,
    Both
}

public interface IMotionInput
{
    event Action OnLeftArmSwing;
    event Action OnRightArmSwing;
    event Action<float> OnMarchBeat; // Beat accuracy (0-1, 1 = perfect)
    ArmSwingDirection CurrentSwingDirection { get; }
    float MarchIntensity { get; }
    bool IsLeftArmActive { get; }
    bool IsRightArmActive { get; }
}
