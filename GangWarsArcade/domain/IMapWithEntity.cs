namespace GangWarsArcade.domain;

public interface IMapWithEntity
{
    HashSet<IEntity> Entities { get; }
    void RemoveEntity(IEntity entity);
}
