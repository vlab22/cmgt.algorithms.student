using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

class HighLevelDungeonNodeGraph : NodeGraph
{
    private Dungeon _dungeon;

    public List<Point> doorsDirections = new List<Point>();
    
    public HighLevelDungeonNodeGraph(Dungeon pDungeon) : base((int)(pDungeon.size.Width * pDungeon.scale), (int)(pDungeon.size.Height * pDungeon.scale), (int)pDungeon.scale/3)
    {
        Debug.Assert(pDungeon != null, "Please pass in a dungeon.");

        _dungeon = (SufficientDungeon)pDungeon;
        
    }

    protected override void generate()
    {
        var sDundegon = _dungeon as SufficientDungeon;

        nodes.Clear();
        nodes.AddRange(sDundegon.nodes);
    }
    
    /**
	 * A helper method for your convenience so you don't have to meddle with coordinate transformations.
	 * @return the location of the center of the given room you can use for your nodes in this class
	 */
    protected Point GetRoomCenter(Room pRoom)
    {
        float centerX = ((pRoom.area.Left + pRoom.area.Right) / 2.0f) * _dungeon.scale;
        float centerY = ((pRoom.area.Top + pRoom.area.Bottom) / 2.0f) * _dungeon.scale;
        return new Point((int)centerX, (int)centerY);
    }
    
    /**
	 * A helper method for your convenience so you don't have to meddle with coordinate transformations.
	 * @return the location of the center of the given point you can use for your nodes in this class
	 */
    protected Point GetPointCenter(Point pLocation)
    {
        float centerX = (pLocation.X + 0.5f) * _dungeon.scale;
        float centerY = (pLocation.Y + 0.5f) * _dungeon.scale;
        return new Point((int)centerX, (int)centerY);
    }
    
    public Room GetRoomAtPoint(Point p)
    {
        for (int i = 0; i < _dungeon.rooms.Count; i++)
        {
            var room = _dungeon.rooms[i];
            if (room.area.Contains(p))
            {
                return room;
            }
        }
   
        return null;
    }
}