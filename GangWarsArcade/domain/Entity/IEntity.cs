namespace GangWarsArcade.domain;

public interface IEntity
{
    int HP { get; }
    bool IsActive { get; }
    Bitmap Image { get; }
    Point Position { get; }
    void Move(Map map);
    void Act(Map map);
    void Update(IMapWithEntity map);
    void CollisionWith(IEntity rival);
}
