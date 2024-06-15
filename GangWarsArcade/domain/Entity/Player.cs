using GangWarsArcade.AI;
using GangWarsArcade.Properties;
using System.Reflection;
using Timer = System.Timers.Timer;

namespace GangWarsArcade.domain;

public class Player : IEntity
{
    private const int _maxHP = 5;

    public event Action<Player> Updated;
    public event Action<IEntity> Respawned;

    public event Action<Player> HumanPlayerWasted;
    public event Action<int> HumanPlayerShoted;

    public Bitmap[,] Sprites { get; }

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
    public Bitmap Image { get; set; }

    public int Speed => 8;

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

        Sprites = new Bitmap[4, 5];
        var scrImage = (Bitmap)typeof(Resource).GetProperty(IdentifySprites() + "Sprites", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        var destRegion = new Rectangle(0, 0, 64, 64);
        for (var i = 0; i < Sprites.GetLength(0); i++) // walking sprites
            for (var j = 0; j < Sprites.GetLength(1); j++)
            {
                var scrRegion = new Rectangle(j * 64, i * 64, 64, 64);
                Sprites[i, j] = new Bitmap(64, 64);
                CopyRegionIntoImage(scrImage, scrRegion, Sprites[i, j], destRegion);
            }
        Image = Sprites[0, 0];
    }

    public static void CopyRegionIntoImage(Bitmap srcBitmap, Rectangle srcRegion, Bitmap destBitmap, Rectangle destRegion)
    {
        using (Graphics grD = Graphics.FromImage(destBitmap))
        {
            grD.DrawImage(srcBitmap, destRegion, srcRegion, GraphicsUnit.Pixel);
        }
    }

    private string IdentifySprites()
    {
        return Gang switch
        {
            Gang.Green => "Peon",
            Gang.Blue => "Peasant",
            Gang.Yellow => "Skeleton",
            Gang.Pink => "Elf"
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
    public void Move(Map map, Point newPoint)
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

    public void SetNewDirection(MoveDirection direction)
    {
        if (direction == Direction) return;

        Direction = direction;
    }

    public Point GetNextPoint(Map map)
    {
        if (Direction == 0) return new Point();

        var newPoint = Position + DirectionExtensions.ConvertDirectionToOffset(Direction);
        if (map.IsPossibleCellToMove(newPoint)) return newPoint;
        return Position;
    }
    #endregion

    public void Act(Map map)
    {
        if (!IsHumanPlayer) SetNewDirection(AI.GetMove(map, this));
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
    }

    #region Wasted and Respawn
    private void Wasted(IMapWithEntity map)
    {
        IsAlive = false;
        Weapon = 0;
        Inventory = 0;
        if (IsActive) respawnTimer.Start();

        map.RemoveEntity(this);

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
                map.AddEntity(new Trap(Position, Gang));
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
            //bullet.Move(map);
            map.AddEntity(bullet);
            shotCooldown.Start();
        }

        Updated(this);
        if (IsHumanPlayer) HumanPlayerShoted((int)shotCooldown.Interval);
    }
    #endregion
}