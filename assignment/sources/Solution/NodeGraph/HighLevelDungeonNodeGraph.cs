using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

class HighLevelDungeonNodeGraph : NodeGraph
{
    private Dungeon _dungeon;
    
    public HighLevelDungeonNodeGraph(Dungeon pDungeon) : base((int)(pDungeon.size.Width * pDungeon.scale), (int)(pDungeon.size.Height * pDungeon.scale), (int)pDungeon.scale/3)
    {
        Debug.Assert(pDungeon != null, "Please pass in a dungeon.");

        _dungeon = (SufficientDungeon)pDungeon;
    }

    protected override void generate()
    {
        nodes.Clear();
        
        CreateNodes();
    }

    private void CreateNodes()
    {
        for (int i = 0; i < _dungeon.doors.Count; i++)
        {
            var door = _dungeon.doors[i];
            var direction = door.horizontal ? new Point(1, 0) : new Point(0, 1);

            //Get the point at direction +1

            var neighborPoint0 = new Point(door.location.X + direction.X, door.location.Y + direction.Y);
            var neighborRoom0 = GetRoomAtPoint(neighborPoint0);

            //Get the point at direction -1
            var neighborPoint1 = new Point(door.location.X - direction.X, door.location.Y - direction.Y);
            var neighborRoom1 = GetRoomAtPoint(neighborPoint1);

            //Create nodes if not exists

            var neighborRoom0Center = GetRoomCenter(neighborRoom0);
            var nodeNeighborRoom0 = AddNodeIfNotExists(neighborRoom0Center);

            var neighborRoom1Center = GetRoomCenter(neighborRoom1);
            var nodeNeighborRoom1 = AddNodeIfNotExists(neighborRoom1Center);

            var doorCenter = GetPointCenter(door.location);
            var nodeDoor = AddNodeIfNotExists(doorCenter);

            nodeNeighborRoom0.connections.Add(nodeDoor);
            nodeNeighborRoom1.connections.Add(nodeDoor);
            nodeDoor.connections.Add(nodeNeighborRoom0);
            nodeDoor.connections.Add(nodeNeighborRoom1);
        }
    }
    
    Node AddNodeIfNotExists(Point location)
    {
        var node = nodes.FirstOrDefault(n => n.location == location);
        if (node == null)
        {
            node = new Node(location);
            nodes.Add(node);
        }

        return node;
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
    
    string GetNodesTreeText()
    {
        string result = "Nodes:\r\n";

        foreach (var node in nodes)
        {
            result += $"Node[{node.id}] => {node.location}\r\n";

            if (node.connections.Count == 2)
            {
                result += $"\tNode[{node.connections[0].id}] => {node.connections[0].location}\r\n";
                result += $"\tNode[{node.connections[1].id}] => {node.connections[1].location}\r\n";
            }

            result += "\r\n";
        }

        return result;
    }
}