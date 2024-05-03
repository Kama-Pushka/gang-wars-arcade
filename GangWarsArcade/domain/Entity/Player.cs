using GangWarsArcade.AI;
using GangWarsArcade.Properties;
using GangWarsArcade.views;
using Timer = System.Windows.Forms.Timer;

namespace GangWarsArcade.domain;

public class Player : IEntity
{
    private const int _maxHP = 5;

    public event Action<Player> PlayerUpdated;
    public event Action<IEntity> PlayerRespawned;

    public event Action<Player> OnPlayerWasted;
    public event Action<int> OnPlayerShoted;

    public Type Type => typeof(Player);

    public Gang Gang { get; }
    public Point Position { get; private set; }
    public MoveDirection Direction { get; set; }
    public Bitmap Image { get; private set; }

    public int HP { get; private set; }
    public ItemType Weapon { get; private set; }
    public ItemType Inventory { get; private set; }

    public bool IsAlive { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsHumanPlayer { get; set; }

    public SolidBrush DamageColor { get; set; }

    public Player(Point position, Gang gang)
    {
        Position = position;
        Gang = gang;
        HP = _maxHP;
        IsAlive = true;
        IsActive = true;

        _respawnBuildings = new HashSet<Building>();

        // Initialize timer for respawn
        respawnTimer = new() { Interval = 5000 };
        respawnTimer.Tick += Respawn;

        // Initialize timer for cooldown
        shotCooldown = new() { Interval = 1000 };
        shotCooldown.Tick += StopShotCooldown;

        Image = IdentifyImage();
    }

    private Bitmap IdentifyImage()
    {
        switch (Gang)
        {
            case Gang.Green:
                return Resource.Peon;
            case Gang.Blue:
                return Resource.Peasant;
            case Gang.Yellow:
                return Resource.SkeletonWarrior;
            case Gang.Pink:
                return Resource.Priest;
        }
        return Resource.Peasant;
    }

    public int OwnedBuildings => _respawnBuildings.Count;
    private readonly HashSet<Building> _respawnBuildings;

    public void AddRespawnBuilding(Building building)
    {
        _respawnBuildings.Add(building);
    }

    public void RemoveRespawnBuilding(Building building) 
    {
        _respawnBuildings.Remove(building);
    }

    public void Act(Map map)
    {
        if (!IsHumanPlayer) SetNewDirection(this.GetRandomMoveDirection());
        WalkInDirection(map);
    }

    public void Update(IMapWithEntity map)
    {
        foreach (var rival in map.Entities.Where(e => e.Position == Position))
        {
            CollisionWith(rival);
            rival.CollisionWith(this);
        }

        if (HP <= 0)
        {
            Wasted(map);
        }

        PlayerUpdated(this);
    }

    public void CollisionWith(IEntity rival)
    {
        switch (rival.Type.Name)
        {
            case "Item":
                TakeItem((Item)rival);
                break;
            case "Bullet":
                var bullet = (Bullet)rival;
                if (bullet.Owner != Gang)
                    GetHit(rival);
                break;
            case "Trap":
                var trap = (Trap)rival;
                if (trap.Owner != Gang)
                    GetHit(rival);
                break;
        }
    }

    public void GetHit(IEntity rival)
    {
        switch (rival.Type.Name)
        {
            case "Bullet":
                HP--;
                break;
            case "Trap":
                HP -= 2;
                break;
        }
        DamageColor = new SolidBrush(Color.Red);
    }

    public void ResetPlayer(Point newPosition)
    {
        _respawnBuildings.Clear();
        Weapon = 0;
        HP = _maxHP;
        IsAlive = true;
        IsActive = true;

        Position = newPosition;

        PlayerUpdated(this);
    }

    #region Death and Respawn
    private void Wasted(IMapWithEntity map)
    {
        IsAlive = false;
        Weapon = 0;
        DamageColor = null; // чтобы не было запоздалой анимации
        if (OwnedBuildings > 0) respawnTimer.Start();
        else IsActive = false;

        map.Entities.Remove(this);

        if (IsHumanPlayer) OnPlayerWasted(this);
    }

    private readonly Timer respawnTimer;
    private void Respawn(object sender, EventArgs e)
    {
        IsAlive = true;
        HP = _maxHP;
        respawnTimer.Stop();

        Position = _respawnBuildings.First().Location - new Point(1, 0);

        PlayerRespawned(this);
        PlayerUpdated(this);
    }
    #endregion

    #region Inventory
    private void TakeItem(Item item)
    {
        if (item.ItemType == ItemType.FireBolt)
        {
            Weapon = item.ItemType;
        }
        else if (item.ItemType == ItemType.HPRegeneration)
        {
            HP = _maxHP;
        }
        else if (item.ItemType == ItemType.Trap)
        {
            Inventory = item.ItemType;
        }
    }

    public void UseInventoryItem(Map map)
    {
        if (Inventory == 0) return;

        if (Inventory == ItemType.Trap)
        {
            map.Entities.Add(new Trap(Position, Gang));
        }
        Inventory = 0;

        PlayerUpdated(this);
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
            bullet.Act(map);
            bullet.Act(map);
            map.Entities.Add(bullet);
            shotCooldown.Start();
        }

        PlayerUpdated(this);
        OnPlayerShoted(shotCooldown.Interval);
    }

    private void StopShotCooldown(object sender, EventArgs e)
    {
        shotCooldown.Stop();
    }
    #endregion

    #region Act
    private void WalkInDirection(Map map)
    {
        if (Direction != 0)
        {
            var newPoint = Position + domain.Direction.directionToOffset[Direction];
            if (map.InBounds(newPoint) && map.Maze[newPoint.X, newPoint.Y] != MapCell.Wall)
            {
                Position = newPoint;
                if (map.OwnedLocations.TryGetValue(newPoint, out OwnedLocation? value))
                {
                    value.Owner = Gang;
                }
                else
                {
                    map.OwnedLocations[newPoint] = new OwnedLocation(Gang, newPoint, 0);
                }
            }
        }
    }

    public void SetNewDirection(MoveDirection direction)
    {
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
                //image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                break;
            case MoveDirection.Left:
                image.RotateFlip(RotateFlipType.Rotate180FlipY);
                break;
        }
        Image = image;
    }
    #endregion
}