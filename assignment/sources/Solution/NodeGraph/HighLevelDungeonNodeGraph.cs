using System.Diagnostics;
using System.Drawing;

class HighLevelDungeonNodeGraph : NodeGraph
{
    private Dungeon _dungeon;

    public HighLevelDungeonNodeGraph(Dungeon pDungeon) : base((int)(pDungeon.size.Width * pDungeon.scale), (int)(pDungeon.size.Height * pDungeon.scale), (int)pDungeon.scale/3)
    {
        Debug.Assert(pDungeon != null, "Please pass in a dungeon.");

        _dungeon = pDungeon;
    }

    protected override void generate()
    {
        for (int i = 0; i < _dungeon.rooms.Count; i++)
        {
            nodes.Add(new Node(getRoomCenter(_dungeon.rooms[i])));
        }
    }
    
    /**
	 * A helper method for your convenience so you don't have to meddle with coordinate transformations.
	 * @return the location of the center of the given room you can use for your nodes in this class
	 */
    protected Point getRoomCenter(Room pRoom)
    {
        float centerX = ((pRoom.area.Left + pRoom.area.Right) / 2.0f) * _dungeon.scale;
        float centerY = ((pRoom.area.Top + pRoom.area.Bottom) / 2.0f) * _dungeon.scale;
        return new Point((int)centerX, (int)centerY);
    }
}