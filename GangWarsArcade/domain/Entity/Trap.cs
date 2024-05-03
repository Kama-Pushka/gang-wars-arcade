using GangWarsArcade.Properties;

namespace GangWarsArcade.domain;

public class Trap : IEntity
{
    public readonly Gang Owner;

    public int HP { get; private set; }

    public Type Type => typeof(Trap);

    public bool IsActive { get; private set; }

    public Bitmap Image { get; }

    public Point Position { get; }

    public Trap(Point position, Gang owner)
    {
        Position = position;
        Owner = owner;

        HP = 1;
        IsActive = true;

        Image = Resource.Trap;
    }

    public void Act(Map map)
    {
    }

    public void Update(IMapWithEntity map)
    {
        if (HP <= 0)
        {
            map.Entities.Remove(this);
            IsActive = false;
        }
    }

    public void CollisionWith(IEntity rival)
    {
        switch (rival.Type.Name)
        {
            case "Player":
                var player = (Player)rival;
                if (player.Gang != Owner)
                    GetHit();
                break;
        }
    }

    private void GetHit()
    {
        HP--;
    }
}
