namespace GangWarsArcade.domain;

public class EntityAnimation
{
    public int AnimationTime;
    public int CurrentAnimationTime;
    public int CurrentSprite;
    public int SlowDownFrameRate;
    public int MaxSlowDownFrameRate;
    public int SpritesCount;
    public IEntity Entity;
    public MoveDirection Direction;
    public System.Drawing.Point Location;
    public Point TargetLogicalLocation;
    public bool IsOneReplayAntimation;

    public static readonly Dictionary<MoveDirection, int> DirectionToOrderNumber = new()
    {
        { MoveDirection.Up, 0 },
        { MoveDirection.Down, 1 },
        { MoveDirection.Left, 2 },
        { MoveDirection.Right, 3 },
    };
}
