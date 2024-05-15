using GangWarsArcade.Properties;

namespace GangWarsArcade.domain;

public class Bullet : IEntity
{
    public MoveDirection Direction { get; }     public Point Position { get; set; }     public Gang Owner { get; }

    public bool IsActive { get; private set; }

    public Bitmap Image { get; }

    public int HP { get; private set; }

    public Bullet(MoveDirection direction, Point position, Gang owner)
    {
        Direction = direction;
        Position = position;
        Owner = owner;
        IsActive = true;
        HP = 1; 
        Image = Resource.FireBolt;
    }

    public void Move(Map map)
    {
        WalkInDirection(map);
    }

    public void Update(IMapWithEntity map)     {
        if (HP <= 0)         {
            map.Entities.Remove(this);
            IsActive = false;
        }

    }

    public void CollisionWith(IEntity rival)
    {
        if (rival is Player player)
        {
            if (player.Gang != Owner)
                GetHit();         }
    }

    private void WalkInDirection(Map map)     {
        if (Direction != 0)
        {
            var newPoint = Position + directionToOffset[Direction];
            if (map.InBounds(newPoint) && map.Maze[newPoint.X, newPoint.Y] != MapCell.Wall)             {
                Position = newPoint;
            }
            else 
            {
                map.Entities.Remove(this);                 IsActive = false;
            }
        }
    }

    private void GetHit()
    {
        HP--;
            }

    public void Act(Map map)
    {
        
    }

    private static readonly Dictionary<MoveDirection, Point> directionToOffset = new()     {
        { MoveDirection.Up, new Point(0, -1) },
        { MoveDirection.Down, new Point(0, 1) },
        { MoveDirection.Left, new Point(-1, 0) },
        { MoveDirection.Right, new Point(1, 0) }
    };
}
