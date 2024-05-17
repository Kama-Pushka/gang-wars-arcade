using GangWarsArcade.domain;
using Point = GangWarsArcade.domain.Point;

namespace GangWarsArcade.AI;

public class Bot
{
    private Queue<MoveDirection> _moveDirections = new();
    private Random _rand = new Random();

    public MoveDirection GetMove(Map map, Player player)
    {
        if (_moveDirections.Count != 0) return _moveDirections.Dequeue();

        var position = player.Position;
        _moveDirections = new Queue<MoveDirection>();

        var items = map.Entities.Where(e => e is Item).Select(e => (Item)e).ToArray();
        if (items.Length != 0)
        {
            var guns = items.Where(e => e.ItemType == ItemType.FireBolt).Select(g => g.Position).ToArray();
            if (guns.Length != 0)
            {
                _moveDirections = FindShortestPath(map, position, guns);
                return _moveDirections.Dequeue();
            }
            else
            {
                _moveDirections = FindShortestPath(map, position, items.Select(i => i.Position));
                return _moveDirections.Dequeue();
            }
        }
        else
        {
            var index = _rand.Next(0, map.Buildings.Length);
            var requiredPoints = map.Buildings[index].GetSurroundingRoads();

            _moveDirections = FindShortestPath(map, position, [requiredPoints[0]]);
            for (var i = 1; i < requiredPoints.Length; i++)
            {
                _moveDirections.Enqueue(DirectionExtensions.ConvertOffsetToDirection(requiredPoints[i] - requiredPoints[i - 1]));
            }
            return _moveDirections.Dequeue();
        }
    }

    public void Act(Map map, Player ai)
    {
        if (ai.Weapon == 0 && ai.Inventory == 0) return;
        
        var rivals = map.Entities.Where(e => e is Player player && player != ai && ((player.Position - ai.Position).X == 0 || (player.Position - ai.Position).Y == 0)).ToList();
        if (rivals.Count != 0)
        { 
            var rival = rivals.First();
            var offset = rival.Position - ai.Position;

            var x = offset.X != 0 ? offset.X / Math.Abs(offset.X) : 0;
            var y = offset.Y != 0 ? offset.Y / Math.Abs(offset.Y) : 0;
            if (new Point(x, y) == DirectionExtensions.ConvertDirectionToOffset(ai.Direction))
            {
                ai.Shot(map);
                ai.UseInventoryItem(map);
            }        
        }
    }

    public static Queue<MoveDirection> FindShortestPath(Map map, Point position, IEnumerable<Point> targets)
    {
        var paths = FindPaths(map, position, targets);
        var path = paths.MinBy(p => p.Length);
        return ConvertOffsetsToDirections(path.Reverse().ToList());
    }

    private static Queue<MoveDirection> ConvertOffsetsToDirections(List<Point> path)
    {
        var result = new Queue<MoveDirection>();
        for (var i = 1; i < path.Count; i++)
        {
            result.Enqueue(DirectionExtensions.ConvertOffsetToDirection(path[i] - path[i - 1]));
        }
        return result;
    }

    public static IEnumerable<SinglyLinkedList<Point>> FindPaths(Map map, Point start, IEnumerable<Point> targets)
    {
        var uncollectedChests = new HashSet<Point>(targets);

        var track = new Dictionary<Point, SinglyLinkedList<Point>>
        {
            [start] = new SinglyLinkedList<Point>(start)
        };
        var queue = new Queue<Point>();
        queue.Enqueue(start);

        var d = DirectionExtensions.PossibleDirections;
        while (queue.Count != 0)
        {
            var point = queue.Dequeue();
            var incidentPoints = d
                .Select(p => new Point(point.X + p.X, point.Y + p.Y))
                .Where(p => map.IsPossibleCellToMove(p));
            foreach (var nextPoints in incidentPoints)
            {
                if (track.ContainsKey(nextPoints)) continue;
                track[nextPoints] = new SinglyLinkedList<Point>(nextPoints, track[point]);
                queue.Enqueue(nextPoints);
            }
        }

        foreach (var chest in uncollectedChests)
            if (track.ContainsKey(chest))
            {
                yield return track[chest];
                uncollectedChests.Remove(chest);
            }
    }
}