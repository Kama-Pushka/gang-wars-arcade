namespace GangWarsArcade.domain;

public enum MoveDirection
{
    Up = Keys.W,
    Down = Keys.S,
    Right = Keys.D,
    Left = Keys.A
}

public class Direction
{
    public static readonly Dictionary<MoveDirection, Point> directionToOffset = new()
    {
        { MoveDirection.Up, new Point(0, -1) },
        { MoveDirection.Down, new Point(0, 1) },
        { MoveDirection.Left, new Point(-1, 0) },
        { MoveDirection.Right, new Point(1, 0) }
    };

    private static readonly Dictionary<Point, MoveDirection> offsetToDirection = new()
    {
        { new Point(0, -1), MoveDirection.Up },
        { new Point(0, 1), MoveDirection.Down },
        { new Point(-1, 0), MoveDirection.Left },
        { new Point(1, 0), MoveDirection.Right }
    };

    public static readonly IReadOnlyList<Point> PossibleDirections = offsetToDirection.Keys.ToList();

    public static MoveDirection ConvertOffsetToDirection(Point offset)
    {
        return offsetToDirection[offset];
    }

    public static Point ConvertDirectionToOffset(MoveDirection direction) 
    {
        return directionToOffset[direction];
    }
}