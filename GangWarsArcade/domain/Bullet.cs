using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GangWarsArcade.domain;

public class Bullet
{
    public MoveDirection Direction { get; }
    public Point Position { get; set; }
    public Gang Owner { get; }

    public Bullet(MoveDirection direction, Point position, Gang owner)
    {
        Direction = direction;
        Position = position;
        Owner = owner;
    }

    public void Update(Map map)
    {
        WalkInDirection(map);
    }

    private void WalkInDirection(Map map)
    {
        if (Direction != 0)
        {
            var newPoint = Position + directionToOffset[Direction];
            if (map.InBounds(newPoint) && map.Maze[newPoint.X, newPoint.Y] != MapCell.Wall)
            {
                Position = newPoint;
            }
            else 
            {
                map.Bullets.Remove(this);
            }
        }
    }

    private static readonly Dictionary<MoveDirection, Point> directionToOffset = new()
    {
        { MoveDirection.Up, new Point(0, -1) },
        { MoveDirection.Down, new Point(0, 1) },
        { MoveDirection.Left, new Point(-1, 0) },
        { MoveDirection.Right, new Point(1, 0) }
    };
}
