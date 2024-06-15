using GangWarsArcade.Properties;
using System.Reflection;

namespace GangWarsArcade.domain;

public class Bullet : IEntity
{
    public const int Damage = 2;
    private const int _maxHP = 1;

    public MoveDirection Direction { get; }
    public Gang Owner { get; }

    public Point Position { get; private set; }
    public int HP { get; private set; }
    public bool IsActive { get; private set; }
    public Bitmap Image { get; set; }

    public Bitmap[,] Sprites { get; }

    public int Speed => 16;

    public Bullet(MoveDirection direction, Point position, Gang owner)
    {
        Direction = direction;
        Position = position;
        Owner = owner;
        HP = _maxHP;
        IsActive = true;

        Sprites = new Bitmap[5, 4];

        var scrImage = (Bitmap)typeof(Resource).GetProperty("FireboltSprites", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        var destRegion = new Rectangle(0, 0, 64, 64);
        for (var i = 0; i < Sprites.GetLength(0); i++) // walking and death sprites
            for (var j = 0; j < Sprites.GetLength(1); j++)
            {
                var scrRegion = new Rectangle(j * 64, i * 64, 64, 64);
                Sprites[i, j] = new Bitmap(64, 64);
                CopyRegionIntoImage(scrImage, scrRegion, Sprites[i, j], destRegion);
            }
        Image = Sprites[0, 0];
    }

    private static void CopyRegionIntoImage(Bitmap srcBitmap, Rectangle srcRegion, Bitmap destBitmap, Rectangle destRegion)
    {
        using (Graphics grD = Graphics.FromImage(destBitmap))
        {
            grD.DrawImage(srcBitmap, destRegion, srcRegion, GraphicsUnit.Pixel);
        }
    }

    public void Move(Map map, Point newPoint)
    {
        Position = newPoint;
    }

    public Point GetNextPoint(Map map)
    {
        if (Direction == 0) throw new ArgumentException("Bullet Direction is null.");

        var newPoint = Position + DirectionExtensions.ConvertDirectionToOffset(Direction);
        if (map.IsPossibleCellToMove(newPoint))
        {
            return newPoint;
        }
        else
        {
            GetHit();
            return Point.Null;
        }
    }

    public void Act(Map map)
    {
    }

    public void Update(IMapWithEntity map)
    {
        if (HP <= 0)
        {
            IsActive = false;
            map.RemoveEntity(this);
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
