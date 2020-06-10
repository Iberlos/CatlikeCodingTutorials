
public enum ShapeBehaviorType
{
    Movement = 0,
    Rotation,
    Oscillation
}

public static class ShapeBehaviorTypeMethods //extension methods
{
    public static ShapeBehavior GetInstance(this ShapeBehaviorType type) //adding the this keyword makes this an extension method. making it possible to call ShapeBehaviorType.Movement.GetInstance() as if the movement enum was a class. weird as hell.
    {
        switch(type)
        {
            case ShapeBehaviorType.Movement:
                return ShapeBehaviorPool<MovementShapeBehavior>.Get();
            case ShapeBehaviorType.Rotation:
                return ShapeBehaviorPool<RotationShapeBehavior>.Get();
            case ShapeBehaviorType.Oscillation:
                return ShapeBehaviorPool<OscillationShapeBehavior>.Get();
        }
        UnityEngine.Debug.Log("Forgot to Support " + type);
        return null;
    }
}
