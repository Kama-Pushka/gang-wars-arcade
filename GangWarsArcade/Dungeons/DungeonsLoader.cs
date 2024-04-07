using System.ComponentModel;
using System.IO;

namespace GangWarsArcade.Dungeons;

public static class DungeonsLoader
{
	public static string Load(DungeonsName dungeonName)
	{
		return File.ReadAllText("Dungeons/" + dungeonName.ToDescriptionString());
	}
}

public enum DungeonsName
{
	[Description("D_BigTestDungeon.txt")] D_BigDungeon,
	[Description("D_Dungeon1.txt")] D_Dungeon1,
	[Description("D_Dungeon2.txt")] D_Dungeon2,
	[Description("D_Dungeon3.txt")] D_Dungeon3,
	[Description("D_Dungeon4.txt")] D_Dungeon4,

    [Description("R_Dungeon1.txt")] R_Dungeon1,
    [Description("R_Dungeon2.txt")] R_Dungeon2,
    [Description("R_Dungeon3.txt")] R_Dungeon3,
    [Description("R_Dungeon4.txt")] R_Dungeon4,
    [Description("R_Dungeon5.txt")] R_Dungeon5,
}

public static class DungeonsNameExtensions
{
	public static string ToDescriptionString(this DungeonsName val)
	{
		var attributes = (DescriptionAttribute[])val
			.GetType()
			.GetField(val.ToString())
			.GetCustomAttributes(typeof(DescriptionAttribute), false);
		return attributes.Length > 0 ? attributes[0].Description : string.Empty;
	}
}