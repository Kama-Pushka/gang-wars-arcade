namespace GangWarsArcade.domain;

public class Building
{
    public readonly Point Location;

    public Building(Point point)
    {
        Location = point;
    }

    public void Update(Map map)
    {
        var result = IsBuildingCaptured(map);
        if (result.Item1 == true)
        {
            if (result.Item2 != 0 && (!map.OwnedLocations.ContainsKey(Location) || map.OwnedLocations[Location].Owner != result.Item2))
            {
                map.OwnedLocations[Location] = new OwnedLocation(result.Item2, Location, 0);
                map.Players[result.Item2].AddRespawnBuilding(this);
            }
        }
        else if (map.OwnedLocations.TryGetValue(Location, out var value) && value != null)
        {
            map.Players[value.Owner].RemoveRespawnBuilding(this);

            map.OwnedLocations.Remove(Location);
        }
    }

    public Point GetRandonPointToRespawn()
    {
        var random = new Random();
        var index = random.Next(offsetToSurroundingRoads.Length);
        return Location + offsetToSurroundingRoads[index];
    }

    private (bool, Gang) IsBuildingCaptured(Map map)
    {
        var ownerId = (Gang)0;
        foreach (var offset in offsetToSurroundingRoads)
        {
            var newPoint = Location + offset;
            if (!map.OwnedLocations.TryGetValue(newPoint, out var owner)) return (false, 0);

            if (ownerId == 0) ownerId = owner.Owner;
            else if (ownerId != owner.Owner) return (false, 0);
        }
        return (true, ownerId);
    }

    private static readonly Point[] offsetToSurroundingRoads = new[] 
    {
        new Point(0, -1),
        new Point(1, -1),
        new Point(2, 0),
        new Point(2, 1),
        new Point(1, 2),
        new Point(0, 2),
        new Point(-1, 1),
        new Point(-1, 0)
    };
}
