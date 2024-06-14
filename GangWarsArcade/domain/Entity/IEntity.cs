namespace GangWarsArcade.domain;

public interface IEntity
{
    int HP { get; }
    bool IsActive { get; }
    Bitmap Image { get; set; }
    Point Position { get; }
    MoveDirection Direction { get; }
    void Move(Map map, Point newPosition);
    void Act(Map map);
    void Update(IMapWithEntity map);
    void CollisionWith(IEntity rival);

    Point GetNextPoint(Map map);
    Bitmap[,] Sprites { get; }
    int Speed { get; }
}
