using GangWarsArcade.Properties;

namespace GangWarsArcade.domain;

public class Trap : IEntity
{
    public const int Damage = 3;
    private const int _maxHP = 1;
    
    public Gang Owner { get; }

    public int HP { get; private set; }
    public Point Position { get; }
    public bool IsActive => true;
    public Bitmap Image { get; }

    public Trap(Point position, Gang owner)
    {
        Position = position;
        Owner = owner;
        HP = _maxHP;

        Image = Resource.Trap;
    }

    public void Move(Map map)
    {
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
