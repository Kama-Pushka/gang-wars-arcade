namespace GangWarsArcade.domain;

public enum MapCellEnum
{
    Wall,
    Empty
}

public class MapCell
{
    public Bitmap Image { get; set; }
    public MapCellEnum Cell { get; set; }
}