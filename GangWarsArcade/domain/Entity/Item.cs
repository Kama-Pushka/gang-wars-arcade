using GangWarsArcade.Properties;

namespace GangWarsArcade.domain;

 public class Item : IEntity
{
    public ItemType ItemType;

    public int HP { get; private set; }

    public bool IsActive { get; private set; }

    public Bitmap Image { get; }

    public Point Position { get; }

    public Item(ItemType itemType, Point position)
    {
        ItemType = itemType;
        Position = position;

        HP = 1;
        IsActive = true;

        Image = IdentifyImage();
    }

    private Bitmap IdentifyImage()
    {
        switch (ItemType)
        {
            case ItemType.FireBolt:
                return Resource.FireBolt;
            case ItemType.HPRegeneration:
                return Resource.PotionRed;
            case ItemType.Trap:
                return Resource.Trap;
        }
        return Resource.Chest;
    }

    public void Move(Map map)
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
        if (rival is Player)
        {
            GetHit();
        }
    }

    private void GetHit()
    {
        HP--;
    }

    public void Act(Map map)
    {
        
    }
}
