namespace GangWarsArcade.domain;

public class DirectionExtensions
{
    private static readonly Dictionary<MoveDirection, Point> _directionToOffset = new()
    {
        { MoveDirection.Up, new Point(0, -1) },
        { MoveDirection.Down, new Point(0, 1) },
        { MoveDirection.Left, new Point(-1, 0) },
        { MoveDirection.Right, new Point(1, 0) }
    };

    private static readonly Dictionary<Point, MoveDirection> _offsetToDirection = new()
    {
        { new Point(0, -1), MoveDirection.Up },
        { new Point(0, 1), MoveDirection.Down },
        { new Point(-1, 0), MoveDirection.Left },
        { new Point(1, 0), MoveDirection.Right }
    };

    public static readonly IReadOnlyList<Point> PossibleDirections = _offsetToDirection.Keys.ToList();

    public static MoveDirection ConvertOffsetToDirection(Point offset) => _offsetToDirection[offset];

    public static Point ConvertDirectionToOffset(MoveDirection direction) => _directionToOffset[direction];
}