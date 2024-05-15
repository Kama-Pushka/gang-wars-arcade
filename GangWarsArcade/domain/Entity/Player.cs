using GangWarsArcade.AI;
using GangWarsArcade.Properties;
using Timer = System.Timers.Timer;

namespace GangWarsArcade.domain;

 public class Player : IEntity {
    private const int _maxHP = 5;

    public event Action<Player> PlayerUpdated;     public event Action<IEntity> PlayerRespawned; 
    public event Action<Player> PlayerWasted;     public event Action<int> OnPlayerShoted; 
    public Gang Gang { get; }     public Point Position { get; private set; }
    public MoveDirection Direction { get; set; }     public Bitmap Image { get; private set; }

    public int HP { get; private set; }
    public ItemType Weapon { get; private set; }
    public ItemType Inventory { get; private set; }

    public bool IsAlive { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsHumanPlayer { get; set; } 
    private readonly Bot AI;

    public SolidBrush DamageColor { get; set; } 
    public Player(Point position, Gang gang)
    {
        Position = position;
        Gang = gang;
        HP = _maxHP;
        IsAlive = true;         IsActive = true;

        _respawnBuildings = new HashSet<Building>();

                respawnTimer = new() { Interval = 5000 };
        respawnTimer.Elapsed += Respawn;

                shotCooldown = new() { Interval = 1000 };
        shotCooldown.Elapsed += StopShotCooldown;

        Image = IdentifyImage(); 
        if (!IsHumanPlayer) AI = new Bot();
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

                                                    
    public int OwnedBuildings => _respawnBuildings.Count;     private readonly HashSet<Building> _respawnBuildings; 
    public void AddRespawnBuilding(Building building)     {
        _respawnBuildings.Add(building);
    }

    public void RemoveRespawnBuilding(Building building) 
    {
        _respawnBuildings.Remove(building);
    }

    public void Move(Map map)
    {
        if (!IsHumanPlayer) SetNewDirection(AI.GetMove(map, this));
        
        WalkInDirection(map);
    }

    public void Act(Map map)
    {
        if (!IsHumanPlayer) AI.Act(map, this);     }

    public void Update(IMapWithEntity map)
    {
        foreach (var rival in map.Entities.Where(e => e.Position == Position))         {
            CollisionWith(rival);
            rival.CollisionWith(this);
        }

        if (HP <= 0)         {
            Wasted(map);
        }

        PlayerUpdated(this);     }

    public void CollisionWith(IEntity rival)
    {
        if (rival is Item item)
        {
            TakeItem(item);
        }
        else if (rival is Bullet bullet)
        {
            if (bullet.Owner != Gang)
                GetHit(rival);
        }
        else if (rival is Trap trap) 
        {
            if (trap.Owner != Gang)
                GetHit(rival);
        }
    }

    public void GetHit(IEntity rival)
    {
        if (rival is Bullet)
        {
            HP--;         }
        else if (rival is Trap)
        {
            HP -= 2;
        }
        DamageColor = new SolidBrush(Color.Red);     }

    public void ResetPlayer(Point newPosition)
    {
        _respawnBuildings.Clear();
        Weapon = 0;
        HP = _maxHP;
        IsAlive = true;
        IsActive = true;

        Position = newPosition;

        PlayerUpdated(this);             }

    #region Death and Respawn
    private void Wasted(IMapWithEntity map)
    {
        IsAlive = false;
        Weapon = 0;
        DamageColor = null;         respawnTimer.Start();

        map.Entities.Remove(this);

        if (IsHumanPlayer) PlayerWasted(this);
    }

    private readonly Timer respawnTimer;     private void Respawn(object sender, EventArgs e)     {
        respawnTimer.Stop();
        if (OwnedBuildings == 0)
        {
            IsActive = false;
            return;
        }

        IsAlive = true;
        HP = _maxHP;

                Position = _respawnBuildings.First().Location - new Point(1, 0); 
        PlayerRespawned(this);
        PlayerUpdated(this);
    }
    #endregion

    #region Inventory
    private void TakeItem(Item item)     {
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

    public void UseInventoryItem(Map map)     {
        if (Inventory == 0) return;

        if (Inventory == ItemType.Trap)          {
            map.Entities.Add(new Trap(Position, Gang));
        }
        Inventory = 0;

        PlayerUpdated(this);
    }
    #endregion

    #region Shot (Act)
    private readonly Timer shotCooldown;

    public void Shot(Map map)      {
        if (shotCooldown.Enabled) return;

        if (Direction != 0 && Weapon != 0)          {
            var bullet = new Bullet(Direction, Position, Gang);
            bullet.Move(map);              bullet.Move(map);
            map.Entities.Add(bullet);              shotCooldown.Start();
        }

        PlayerUpdated(this);
        if (IsHumanPlayer) OnPlayerShoted((int)shotCooldown.Interval);     }

    private void StopShotCooldown(object sender, EventArgs e)
    {
        shotCooldown.Stop();
    }
    #endregion

        #region Move
    private void WalkInDirection(Map map)    {
        if (Direction != 0)
        {
            var newPoint = Position + domain.Direction.directionToOffset[Direction];
            if (map.InBounds(newPoint) && map.Maze[newPoint.X, newPoint.Y] != MapCell.Wall)             {
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
        if (direction == Direction) return;
        
        Direction = direction;
        var image = IdentifyImage();         switch (direction)
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
    #endregion
}