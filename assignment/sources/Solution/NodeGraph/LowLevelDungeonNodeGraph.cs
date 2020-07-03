using System.Collections.Generic;
using System.Drawing;
using System.Linq;

internal class LowLevelDungeonNodeGraph : NodeGraph
{
    public List<Point> floors = new List<Point>();

    private Dictionary<Point, Node> nodesLocationMap = new Dictionary<Point, Node>();

    private Dungeon _dungeon;

    private Point[] _directions = new Point[]
    {
        new Point(1, 0),
        new Point(-1, 0),
        new Point(0, 1),
        new Point(0, -1),
    };

    public LowLevelDungeonNodeGraph(Dungeon pDungeon) : base((int) (pDungeon.size.Width * pDungeon.scale),
        (int) (pDungeon.size.Height * pDungeon.scale), (int) pDungeon.scale / 3)
    {
        _dungeon = pDungeon;
    }

    protected override void generate()
    {
        SetFloors();

        //Add nodes
        foreach (var floor in floors)
        {
            var floorCenter = GetPointCenter(floor, _dungeon);
            var node = new Node(floorCenter);

            nodes.Add(node);

            nodesLocationMap.Add(floor, node);
        }

        //Create connections
        foreach (var floor in floors)
        {
            var floorNode = nodesLocationMap[floor];

            for (int i = 0; i < _directions.Length; i++)
            {
                var dir = _directions[i];

                var neighbourFloor = new Point(floor.X + dir.X, floor.Y + dir.Y);

                //Check if it's a Node/Floor
                if (nodesLocationMap.TryGetValue(neighbourFloor, out var node))
                {
                    //Connect
                    AddConnection(floorNode, node);
                }
            }
        }
    }

    void SetFloors()
    {
        foreach (var room in _dungeon.rooms)
        {
            floors.AddRange(room.GetFloors());
        }

        foreach (var door in _dungeon.doors)
        {
            floors.Add(door.location);
        }

        floors = floors.Distinct().ToList();
    }

    protected Point GetPointCenter(Point pLocation, Dungeon pDungeon)
    {
        float centerX = (pLocation.X + 0.5f) * pDungeon.scale;
        float centerY = (pLocation.Y + 0.5f) * pDungeon.scale;
        return new Point((int) centerX, (int) centerY);
    }
}