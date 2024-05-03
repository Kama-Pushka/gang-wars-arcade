using System.Linq;
using System.Collections.Generic;
using GangWarsArcade.domain;

namespace GangWarsArcade.domain;

public class DungeonTask
{
    public static MoveDirection[] FindShortestPath(Map map)
    {
        // пока так
        //var pathFromEndToStart = BfsTask.FindPaths(map, map.Exit, new Point[] { map.InitialPosition }); // оставить только два поиска!!
        //if (!pathFromEndToStart.Any()) return System.Array.Empty<MoveDirection>();

        //var pathsFromEndToChests = BfsTask.FindPaths(map, map.Exit, map.Items.Keys.ToArray());
        //if (!pathsFromEndToChests.Any()) return ConvertOffsetsToDirections(pathFromEndToStart.First().ToList());

        //var pathsFromStartToChests = BfsTask.FindPaths(map, map.InitialPosition, map.Items.Keys.ToArray());

        //var allPaths = pathsFromStartToChests
        //    .Join(pathsFromEndToChests,
        //        s => s.Value,
        //        e => e.Value,
        //        (s, e) => (s, e));

        //var path = allPaths.MinBy(p => p.s.Length + p.e.Length);

        //return ConvertOffsetsToDirections(path.s.Reverse().Concat(path.e.Skip(1)).ToList());
        return System.Array.Empty<MoveDirection>();
    }

    public static MoveDirection[] ConvertOffsetsToDirections(List<Point> path) // Zip
    {
        var result = new MoveDirection[path.Count - 1];
        for (var i = 0; i < result.Length; i++)
        {
            result[i] = Direction.ConvertOffsetToDirection(path[i + 1] - path[i]);
        }
        return result;
    }
}