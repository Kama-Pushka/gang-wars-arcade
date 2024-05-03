namespace GangWarsArcade.domain;

public interface IEntity
{
    int HP { get; }
    Type Type { get; }
    Bitmap Image { get; }
    Point Position { get; }
    void Act(Map map);
    void Update(IMapWithEntity map);
    void CollisionWith(IEntity rival);
}
