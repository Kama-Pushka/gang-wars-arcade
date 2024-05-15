﻿using GangWarsArcade.domain;
using Point = GangWarsArcade.domain.Point;

namespace GangWarsArcade.AI;

public class RivalsTask
{
    public static IEnumerable<OwnedLocation> AssignOwners(Map map)
    {
        var visited = new Dictionary<Point, (Gang Owner, int Distance)>();
        var queue = new Queue<Point>();
        for (var i = 0; i < map.Players.Count; i++)
        {
            visited[map.Players[(Gang)i].Position] = ((Gang)i, 0);
            queue.Enqueue(map.Players[(Gang)i].Position);
            yield return new OwnedLocation((Gang)i, map.Players[(Gang)i].Position, 0);
        }

        var d = new Point[]
        {
            new Point(0, 1),
            new Point(0, -1),
            new Point(1, 0),
            new Point(-1, 0)
        };
        while (queue.Count != 0)
        {
            var player = queue.Dequeue();
            var incidentPoints = d
                .Select(p => new Point(player.X + p.X, player.Y + p.Y))
                .Where((Func<Point, bool>)(p => map.InBounds(p) && map.Maze[p.X, p.Y] == MapCell.Empty));

            foreach (var point in incidentPoints)
            {
                if (visited.ContainsKey(point)) continue;
                visited[point] = (visited[player].Owner, visited[player].Distance + 1);
                queue.Enqueue(point);
                yield return new OwnedLocation(visited[point].Owner, point, visited[point].Distance);
            }
        }
    }
}