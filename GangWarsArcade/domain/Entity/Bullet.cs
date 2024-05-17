using GangWarsArcade.Properties;

namespace GangWarsArcade.domain;

public class Bullet : IEntity
{
    public const int Damage = 2;
    private const int _maxHP = 1;

    public MoveDirection Direction { get; }
    public Gang Owner { get; }

    public Point Position { get; private set; }
    public int HP { get; private set; }
    public bool IsActive => true;
    public Bitmap Image { get; }

    public Bullet(MoveDirection direction, Point position, Gang owner)
    {
        Direction = direction;
        Position = position;
        Owner = owner;
        HP = _maxHP;

        Image = Resource.FireBolt;
    }

    public void Move(Map map) 
    {
        if (Direction == 0) throw new ArgumentException("Bullet Direction is null.");

        var newPoint = Position + DirectionExtensions.ConvertDirectionToOffset(Direction);
        if (map.IsPossibleCellToMove(newPoint))
        {
            Position = newPoint;
        }
        else
        {
            GetHit();
        }
    }

    public void Act(Map map)
    {
    }

    public void Update(IMapWithEntity map)
    {
        if (HP <= 0)
        {
            map.Entities.Remove(this);
        }
    }

    public void CollisionWith(IEntity rival)
    {
        if (rival is Player player && player.Gang != Owner)
        {
            GetHit();
        }
    }

    private void GetHit() => HP--;
}
