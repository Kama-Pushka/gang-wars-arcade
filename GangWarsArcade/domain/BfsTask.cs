using GangWarsArcade.domain;
using System.Collections.Generic;
using System.Linq;

namespace GangWarsArcade.domain;

public class BfsTask
{
    public static IEnumerable<SinglyLinkedList<Point>> FindPaths(Map map, Point start, Point[] chests)
    {
        var uncollectedChests = new HashSet<Point>(chests);

        var track = new Dictionary<Point, SinglyLinkedList<Point>>
        {
            [start] = new SinglyLinkedList<Point>(start)
        };
        var queue = new Queue<Point>();
        queue.Enqueue(start);

        var d = Direction.PossibleDirections;
        while (queue.Count != 0)
        {
            var point = queue.Dequeue();
            var incidentPoints = d
                .Select(p => new Point(point.X + p.X, point.Y + p.Y))
                .Where(p => map.InBounds(p) && map.Maze[p.X, p.Y] == MapCell.Empty);
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