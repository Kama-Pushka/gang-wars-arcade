using System;
using System.Collections.Generic;

namespace GangWarsArcade.domain;

public class Map
{
    public readonly MapCell[,] Maze;
    public readonly Point InitialPosition;
    public readonly Point Exit;
    public readonly Point[] Chests;
    public readonly Point[] Players;

    private Map(MapCell[,] maze, Point initialPosition, Point exit, Point[] chests, Point[] players = null)
    {
        Maze = maze;
        InitialPosition = initialPosition;
        Exit = exit;
        Chests = chests;
        Players = players;
    }

    public static Map FromText(string text)
    {
        var lines = text.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries); // new[] { Environment.NewLine }
        return FromLines(lines);
    }

    public static Map FromLines(string[] lines)
    {
        var dungeon = new MapCell[lines[0].Length, lines.Length];
        var players = new List<Point>();
        var initialPosition = new Point();
        var exit = new Point();
        var chests = new List<Point>();
        for (var y = 0; y < lines.Length; y++)
        {
            for (var x = 0; x < lines[0].Length; x++)
            {
                switch (lines[y][x])
                {
                    case '#':
                        dungeon[x, y] = MapCell.Wall;
                        break;
                    case 'P':
                        dungeon[x, y] = MapCell.Empty;
                        players.Add(new Point(x, y));
                        initialPosition = new Point(x, y); // TODO
                        break;
                    case 'C':
                        dungeon[x, y] = MapCell.Empty;
                        chests.Add(new Point(x, y));
                        break;
                    case 'E':
                        dungeon[x, y] = MapCell.Empty;
                        exit = new Point(x, y);
                        break;
                    default:
                        dungeon[x, y] = MapCell.Empty;
                        break;
                }
            }
        }

        return new Map(dungeon, initialPosition, exit, chests.ToArray(), players.ToArray());
    }

    public bool InBounds(Point point)
        => point is { X: >= 0, Y: >= 0 }
           && Maze.GetLength(0) > point.X
           && Maze.GetLength(1) > point.Y;
}