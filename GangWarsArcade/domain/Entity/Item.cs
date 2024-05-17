using GangWarsArcade.Properties;

namespace GangWarsArcade.domain;

public class Item : IEntity
{
    private const int _maxHP = 1;

    public ItemType ItemType { get; }

    public Point Position { get; }
    public int HP { get; private set; }
    public bool IsActive => true;
    public Bitmap Image { get; }

    public Item(ItemType itemType, Point position)
    {
        ItemType = itemType;
        Position = position;

        HP = _maxHP;

        Image = IdentifyImage();
    }

    private Bitmap IdentifyImage()
    {
        return ItemType switch
        {
            ItemType.FireBolt => Resource.FireBolt,
            ItemType.HPRegeneration => Resource.PotionRed,
            ItemType.Trap => Resource.Trap,
            _ => Resource.Chest,
        };
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
        if (rival is Player)
        {
            GetHit();
        }
    }

    private void GetHit() => HP--;
}
