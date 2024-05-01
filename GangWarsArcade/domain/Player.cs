using GangWarsArcade.AI;
using GangWarsArcade.Properties;
using GangWarsArcade.views;
using Timer = System.Windows.Forms.Timer;

namespace GangWarsArcade.domain;

public class Player
{
    private const int maxHP = 5;
    
    public event Action<Player> OnPlayerWasted;
    public event Action<Player> OnPlayerRespawned;

    public Gang Gang { get; }
    public Point Position { get; private set; }
    public MoveDirection Direction { get; set; }
    public Bitmap Image { get; private set; }

    public int OwnedBuildings => RespawnLocations.Count;
    public HashSet<OwnedLocation> RespawnLocations { get; }
    
    public ItemType Weapon { get; private set; }

    public int HP { get; set; }
    public bool IsAlive { get; private set; }

    public bool IsHumanPlayer { get; set; }

    public ItemType Inventory { get; private set; }

    public SolidBrush DamageColor { get; set; }

    public Player(Point position, Gang gang)
    {
        Position = position;
        Gang = gang;
        HP = maxHP;
        IsAlive = true;

        RespawnLocations = new HashSet<OwnedLocation>();

        // Initialize timer for respawn
        respawnTimer = new() { Interval = 5000 };
        respawnTimer.Tick += Respawn;

        // Initialize timer for cooldown
        shotCooldown = new() { Interval = 1000 };
        shotCooldown.Tick += StopShotCooldown;

        Image = Resource.Peasant;
    }

    public void Update(Map map)
    {
        if (!IsHumanPlayer) SetNewDirection(this.GetRandomMoveDirection());
        WalkInDirection(map);

        if (map.Items.TryGetValue(Position, out var item))
        {
            TakeItem(item);
            map.Items.Remove(Position);
        }
        if (map.Bullets.Where(b => b.Position == Position && b.Owner != Gang).Any())
        {
            HP--;
            DamageColor = new SolidBrush(Color.Red);
            map.Bullets.Remove(map.Bullets.Where(b => b.Position == Position && b.Owner != Gang).First());
        }
        if (map.Traps.TryGetValue(Position, out var trap) && trap.Item1 != Gang)
        {
            HP -= 2;
            DamageColor = new SolidBrush(Color.Red);
            map.Traps.Remove(Position);
        }
        if (HP <= 0)
        {
            IsAlive = false;
            Weapon = 0;
            DamageColor = null;
            if (OwnedBuildings > 0) respawnTimer.Start();
            OnPlayerWasted(this);
        }
    }

    public void ResetPlayer(Point position)
    {
        Position = position;
        RespawnLocations.Clear();
        Weapon = 0;
        HP = 1;
        IsAlive = true;
    }

    

    #region Respawn
    private readonly Timer respawnTimer;

    private void Respawn(object sender, EventArgs e)
    {
        IsAlive = true;
        HP = 1;
        respawnTimer.Stop();

        Position = RespawnLocations.First().Location - new Point(1, 0);
    }
    #endregion

    public void TakeItem(ItemType item)
    {
        if (item == ItemType.Pistol)
        {
            Weapon = item;
        }
        else if (item == ItemType.HPRegeneration)
        {
            HP = maxHP;
        }
        else if (item == ItemType.Trap)
        {
            Inventory = item;
        }
    }

    public void UseInventoryItem(Map map)
    {
        if (Inventory == 0) return;

        if (Inventory == ItemType.Trap)
        {
            map.Traps[Position] = (Gang.Blue, ItemType.Trap);
        }
        Inventory = 0;
    }

    #region Shot
    private readonly Timer shotCooldown;

    public void Shot(object sender, MouseEventArgs e)
    {
        if (shotCooldown.Enabled) return;

        var gameplay = (GameplayControl)sender;
        var map = gameplay.GameMap;
        if (Direction != 0 && Weapon != 0)
        {
            var bullet = new Bullet(Direction, Position, Gang);
            map.Bullets.Add(bullet);
            shotCooldown.Start();
        }
    }

    private void StopShotCooldown(object sender, EventArgs e)
    {
        shotCooldown.Stop();
    }
    #endregion

    public void WalkInDirection(Map map)
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
        var image = Resource.Peasant;
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
}