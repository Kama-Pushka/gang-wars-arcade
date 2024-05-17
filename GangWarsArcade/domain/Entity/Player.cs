using GangWarsArcade.AI;
using GangWarsArcade.Properties;
using Timer = System.Timers.Timer;

namespace GangWarsArcade.domain;

public class Player : IEntity
{
    private const int _maxHP = 5;

    public event Action<Player> Updated;
    public event Action<IEntity> Respawned;

    public event Action<Player> HumanPlayerWasted;
    public event Action<int> HumanPlayerShoted; 

    public Gang Gang { get; }
    public ItemType Weapon { get; private set; }
    public ItemType Inventory { get; private set; }
    public MoveDirection Direction { get; private set; }
    public int OwnedBuildingsCount => OwnedBuildings.Count;
    public readonly HashSet<Building> OwnedBuildings;
    public bool IsAlive { get; private set; }
    public bool IsHumanPlayer => AI == null;
    private Bot AI;
    public SolidBrush DamageColor { get; set; }

    public Point Position { get; private set; }
    public int HP { get; private set; }
    public bool IsActive => OwnedBuildingsCount != 0 || IsAlive;
    public Bitmap Image { get; private set; }

    public Player(Point position, Gang gang)
    {
        Position = position;
        Gang = gang;
        HP = _maxHP;
        IsAlive = true;
        OwnedBuildings = new HashSet<Building>();

        // Initialize timer for respawn
        respawnTimer = new() { Interval = 5000 };
        respawnTimer.Elapsed += Respawn;

        // Initialize timer for cooldown
        shotCooldown = new() { Interval = 1000 };
        shotCooldown.Elapsed += (_, __) => shotCooldown.Stop();

        Image = IdentifyImage();
    }

    private Bitmap IdentifyImage()
    {
        return Gang switch
        {
            Gang.Green => Resource.Peon,
            Gang.Blue => Resource.Peasant,
            Gang.Yellow => Resource.SkeletonWarrior,
            Gang.Pink => Resource.Priest,
            _ => Resource.Peasant,
        };
    }

    public void CreateAI() => AI = new Bot();

    public void Reset(Point newPosition)
    {
        OwnedBuildings.Clear();
        Weapon = 0;
        Inventory = 0;
        HP = _maxHP;
        IsAlive = true;

        Position = newPosition;
    }

    #region Move
    public void Move(Map map)
    {
        if (!IsHumanPlayer) SetNewDirection(AI.GetMove(map, this));

        WalkInDirection(map);
    }

    public void SetNewDirection(MoveDirection direction)
    {
        if (direction == Direction) return;

        Direction = direction;
        var image = IdentifyImage();
        switch (direction)
        {
            case MoveDirection.Up:
                image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                break;
            case MoveDirection.Down:
                image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                break;
            case MoveDirection.Right:
                break;
            case MoveDirection.Left:
                image.RotateFlip(RotateFlipType.Rotate180FlipY);
                break;
        }
        Image = image;
    }

    private void WalkInDirection(Map map)
    {
        if (Direction == 0) return;

        var newPoint = Position + DirectionExtensions.ConvertDirectionToOffset(Direction);
        if (map.IsPossibleCellToMove(newPoint))
        {
            Position = newPoint;
            if (map.OwnedLocations.TryGetValue(newPoint, out OwnedLocation? value))
            {
                value.Owner = Gang;
            }
            else
            {
                map.OwnedLocations[newPoint] = new OwnedLocation(Gang, newPoint);
            }
        }
    }
    #endregion

    public void Act(Map map)
    {
        if (!IsHumanPlayer) AI.Act(map, this);
    }

    public void Update(IMapWithEntity map)
    {
        foreach (var rival in map.Entities.Where(e => e.Position == Position).ToList())
        {
            CollisionWith(rival);
            rival.CollisionWith(this);
        }

        if (HP <= 0)
        {
            Wasted(map);
        }

        Updated(this);
    }

    public void CollisionWith(IEntity rival)
    {
        switch (rival)
        {
            case Item item:
                TakeItem(item);
                break;
            case Bullet bullet:
                if (bullet.Owner != Gang)
                    GetHit(rival);
                break;
            case Trap trap:
                if (trap.Owner != Gang)
                    GetHit(rival);
                break;
        }
    }

    public void GetHit(IEntity rival)
    {
        switch (rival)
        {
            case Bullet:
                HP -= Bullet.Damage;
                break;
            case Trap:
                HP -= Trap.Damage;
                break;
        }
        DamageColor = new SolidBrush(Color.Red);
    }

    #region Wasted and Respawn
    private void Wasted(IMapWithEntity map)
    {
        IsAlive = false;
        Weapon = 0;
        Inventory = 0;
        DamageColor = null;
        if (IsActive) respawnTimer.Start();

        map.Entities.Remove(this);

        if (IsHumanPlayer) HumanPlayerWasted(this);
    }

    private readonly Timer respawnTimer;
    private void Respawn(object? _, EventArgs __)
    {
        respawnTimer.Stop();
        if (!IsActive) return;

        IsAlive = true;
        HP = _maxHP;
        Position = OwnedBuildings.First().GetRandonPointToRespawn();

        Respawned(this);
        Updated(this);
    }
    #endregion

    #region Inventory
    private void TakeItem(Item item)
    {
        switch (item.ItemType)
        {
            case ItemType.FireBolt:
                Weapon = item.ItemType;
                break;
            case ItemType.HPRegeneration:
                HP = _maxHP;
                break;
            case ItemType.Trap:
                Inventory = item.ItemType;
                break;
        }
    }

    public void UseInventoryItem(Map map)
    {
        if (Inventory == 0) return;

        switch (Inventory)
        {
            case ItemType.Trap:
                map.Entities.Add(new Trap(Position, Gang));
                break;
        }
        Inventory = 0;

        Updated(this);
    }
    #endregion

    #region Shot
    private readonly Timer shotCooldown;

    public void Shot(Map map)
    {
        if (shotCooldown.Enabled) return;

        if (Direction != 0 && Weapon != 0)
        {
            var bullet = new Bullet(Direction, Position, Gang);
            bullet.Move(map);
            map.Entities.Add(bullet); 
            shotCooldown.Start();
        }

        Updated(this);
        if (IsHumanPlayer) HumanPlayerShoted((int)shotCooldown.Interval);
    }
    #endregion
}