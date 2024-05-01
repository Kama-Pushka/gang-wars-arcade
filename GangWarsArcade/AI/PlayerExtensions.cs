using GangWarsArcade.domain;

namespace GangWarsArcade.AI;

public static class PlayerExtensions
{
    private static readonly Random random = new();

    public static MoveDirection GetRandomMoveDirection(this Player player)
    {
        ArgumentNullException.ThrowIfNull(player); // TODO ??

        var ran = random.Next(4);

        if (ran >= 3) return MoveDirection.Right; 
        if (ran >= 2) return MoveDirection.Left; 
        if (ran >= 1) return MoveDirection.Down; 
        return MoveDirection.Up; 
    }
}
