using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using GXPEngine;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using GXPEngine.Core;
using GXPEngine.OpenGL;
using Rectangle = System.Drawing.Rectangle;

internal class SufficientDungeon : Dungeon
{
    public EasyDraw easyDraw;

    private Font _font;

    public Dictionary<Room, List<Door>> roomDoorsMap = new Dictionary<Room, List<Door>>();

    public List<GenericNode<Room>> rawNodes = new List<GenericNode<Room>>();

    public List<Node> nodes = new List<Node>();

    public List<Point> doorsDirections = new List<Point>();

    public SufficientDungeon(Size pSize) : base(pSize)
    {
        doorPen = Pens.Red;

        _font = new Font("Courier New", 12f, FontStyle.Bold);

        easyDraw = new EasyDraw(game.width, game.height);
        easyDraw.TextFont(_font);
    }

    protected override void generate(int pMinimumRoomSize)
    {
        roomDoorsMap.Clear();
        doorsDirections.Clear();
        rawNodes.Clear();
        nodes.Clear();
        GenericNode<Room>.ResetId();

        var roomsToSplit = new List<Room>();

        roomsToSplit.Add(new Room(new Rectangle(0, 0, size.Width, size.Height)));

        bool allowGenerate = true;

        int infiniteLoopCounter = 0;

        while (roomsToSplit.Count > 0)
        {
            var parentRoom = roomsToSplit.First();

            Console.WriteLine(parentRoom);

            bool roomsGenerated = Generate2Rooms(parentRoom, pMinimumRoomSize, out var moreRooms);

            if (roomsGenerated)
            {
                roomsToSplit.AddRange(moreRooms);
                roomsToSplit.Remove(parentRoom);

                roomDoorsMap.Add(parentRoom, new List<Door>());

                //Check if node exist
                var parentNode = rawNodes.FirstOrDefault(n => n.obj == parentRoom);
                if (parentNode == null)
                {
                    parentNode = new GenericNode<Room>(parentRoom);
                    rawNodes.Add(parentNode);
                }

                var childNode0 = rawNodes.FirstOrDefault(n => n.obj == moreRooms[0]);
                if (childNode0 == null)
                {
                    childNode0 = new GenericNode<Room>(moreRooms[0]);
                    rawNodes.Add(childNode0);
                }

                var childNode1 = rawNodes.FirstOrDefault(n => n.obj == moreRooms[1]);
                if (childNode1 == null)
                {
                    childNode1 = new GenericNode<Room>(moreRooms[1]);
                    rawNodes.Add(childNode1);
                }

                parentNode.connections.Add(childNode0);
                parentNode.connections.Add(childNode1);
            }
            else
            {
                rooms.Add(parentRoom);
                roomsToSplit.Remove(parentRoom);

                roomDoorsMap.Add(parentRoom, new List<Door>());
            }

            infiniteLoopCounter++;
        }

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine($"{string.Join("\r\n", rooms)}");

        Console.WriteLine();
        Console.WriteLine();

        Console.WriteLine(GetNodesTreeText());

        AddDoors();

        Console.WriteLine($"Doors Count: {doors.Count}");
        
        CreateNodes();
    }

    private void CreateNodes()
    {
        for (int i = 0; i < doors.Count; i++)
        {
            var door = doors[i];
            var direction = doorsDirections[i];
            
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
            
            var doorCenter = GetDoorCenter(door);
            var nodeDoor = AddNodeIfNotExists(doorCenter);
            
            nodeNeighborRoom0.connections.Add(nodeDoor);
            nodeNeighborRoom1.connections.Add(nodeDoor);
        }
    }

    bool Generate2Rooms(Room parent, int pMinimumRoomSize, out List<Room> pRooms)
    {
        pRooms = new List<Room>();

        var parentArea = parent.area;

        Rectangle rect0;
        Rectangle rect1;

        if (!AllowMoreRooms(parent, pMinimumRoomSize))
        {
            return false;
        }

        if (parentArea.Height > parentArea.Width)
        {
            //Slice in Height
            int randY = Utils.Random(pMinimumRoomSize, parentArea.Height - pMinimumRoomSize);

            rect0 = new Rectangle(parentArea.X, parentArea.Y, parentArea.Width, randY);
            rect1 = new Rectangle(parentArea.X, parentArea.Y + randY - 1, parentArea.Width,
                parentArea.Height - randY + 1);
        }
        else
        {
            //Slice in Width
            int randX = Utils.Random(pMinimumRoomSize, parentArea.Width - pMinimumRoomSize);

            rect0 = new Rectangle(parentArea.X, parentArea.Y, randX, parentArea.Height);
            rect1 = new Rectangle(parentArea.X + randX - 1, parentArea.Y, parentArea.Width - randX + 1,
                parentArea.Height);
        }

        var room0 = new Room(rect0);
        var room1 = new Room(rect1);

        pRooms.Add(room0);
        pRooms.Add(room1);

        return true;
    }

    bool AllowMoreRooms(Room room, int pMinimumRoomSize)
    {
        Rectangle rect = room.area;
        return (rect.Width > pMinimumRoomSize * 2 || rect.Height > pMinimumRoomSize * 2);
    }

    /**
	 * A helper method for your convenience so you don't have to meddle with coordinate transformations.
	 * @return the location of the center of the given room you can use for your nodes in this class
	 */
    protected Point GetRoomCenter(Room pRoom)
    {
        float centerX = ((pRoom.area.Left + pRoom.area.Right) / 2.0f) * this.scale;
        float centerY = ((pRoom.area.Top + pRoom.area.Bottom) / 2.0f) * this.scale;
        return new Point((int) centerX, (int) centerY);
    }

    protected Point GetDoorCenter(Door pDoor)
    {
        return GetPointCenter(pDoor.location);
    }

    protected Point GetPointCenter(Point pLocation)
    {
        float centerX = (pLocation.X + 0.5f) * scale;
        float centerY = (pLocation.Y + 0.5f) * scale;
        return new Point((int) centerX, (int) centerY);
    }

    protected Point GetRoomCenterCoord(Room pRoom)
    {
        float centerX = ((pRoom.area.Left + pRoom.area.Right) / 2.0f);
        float centerY = ((pRoom.area.Top + pRoom.area.Bottom) / 2.0f);
        return new Point((int) centerX, (int) centerY);
    }

    public void AddDoors()
    {
        var allRooms = roomDoorsMap.Keys.ToArray();
        for (int i = allRooms.Length - 1; i > 0; i -= 2)
        {
            Console.WriteLine($"Door[{i}] {allRooms[i - 1]} <=> {allRooms[i]}");

            AddDoorBetweenRooms(allRooms[i - 1], allRooms[i]);
        }
    }

    public void AddDoorBetweenRooms(Room room0, Room room1)
    {
        var roomDirection = RoomDirection(room0, room1);

        var roomCenterCoord = GetRoomCenterCoord(room0);
        var doorPosX = roomCenterCoord.X + roomDirection.X * room0.area.Width / 2;
        var doorPosY = roomCenterCoord.Y + roomDirection.Y * room0.area.Height / 2;

        var doorPos = new Point(doorPosX, doorPosY);

        if (room0.area.Width % 2 == 0)
        {
            doorPos.X--;
        }

        if (room0.area.Height % 2 == 0)
        {
            doorPos.Y--;
        }

        Console.WriteLine(
            $"Room[{room0.area.Location} - {room0.area.Width} / {room0.area.Height}]: corners: {string.Join("|", GetRoomsCorners(room0))}");

        int infloop = 0;
        int newDoorOffset = 1;
        var doorPosCorner = doorPos;
        while (IsARoomCorner(doorPosCorner) && infloop < 1000)
        {
            doorPosCorner.X = doorPos.X + roomDirection.Y * newDoorOffset;
            doorPosCorner.Y = doorPos.Y + roomDirection.X * newDoorOffset;

            newDoorOffset *= -1;

            infloop++;

            Console.WriteLine($"infloop: {infloop} | doorPos: {doorPos}");
        }

        if (infloop == 999)
        {
            throw new Exception(
                $"Infinite Loop in door creation between '{room0}' and '{room1}'. Door at pos {doorPos}");
        }

        doorPos = doorPosCorner;

        var door = new Door(doorPos);
        doors.Add(door);
        doorsDirections.Add(roomDirection);

        Console.WriteLine($"doorPos: {doorPos}");
    }

    public Point RoomDirection(Room room0, Room room1)
    {
        var center0 = GetRoomCenter(room0);
        var center1 = GetRoomCenter(room1);

        var dx = center1.X - center0.X;
        var dy = center1.Y - center0.Y;

        var mag = Mathf.Sqrt(dx * dx + dy * dy);

        var xU = Mathf.Round(dx / mag);
        var yU = Mathf.Round(dy / mag);

        return new Point(xU, yU);
    }

    bool IsARoomCorner(Point p)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            var corners = GetRoomsCorners(rooms[i]);
            for (int j = 0; j < corners.Length; j++)
            {
                if (p == corners[j])
                {
                    return true;
                }
            }
        }

        return false;
    }

    Point[] GetRoomsCorners(Room room)
    {
        var pts = new Point[]
        {
            new Point(room.area.Left, room.area.Top),
            new Point(room.area.Right - 1, room.area.Top),
            new Point(room.area.Left, room.area.Bottom - 1),
            new Point(room.area.Right - 1, room.area.Bottom - 1),
        };

        return pts;
    }

    public int creationIndex = 1;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DrawRoomsByStep(1);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            DrawRoomsByStep(-1);
        }

        var mouseX = Input.mouseX;
        var mouseY = Input.mouseY;
        var mouseTileX = (int) (mouseX / scale);
        var mouseTileY = (int) (mouseY / scale);

        var roomAtPoint = GetRoomAtPoint(new Point(mouseTileX, mouseTileY));

        GL.glfwSetWindowTitle($"MouseTileX: {mouseTileX:00} | MouseTileY: {mouseTileY:00} | {roomAtPoint}");
    }

    void DrawRoomsByStep(int dir)
    {
        creationIndex = ArrayTools.GetCircularArrayIndex(creationIndex + dir, roomDoorsMap.Count + 1);

        var roomsToDraw = roomDoorsMap.Keys.Take(creationIndex);

        graphics.Clear(Color.Transparent);
        drawRooms(roomsToDraw, wallPen);
        drawDoors(doors, doorPen);

        DrawRoomsLabels(roomsToDraw);
    }

    void DrawRoomsLabels(IEnumerable<Room> rooms)
    {
        easyDraw.Clear(Color.Transparent);
        int roomCounter = 0;
        foreach (var room in rooms)
        {
            DrawRoomLabel(room, roomCounter);
            roomCounter++;
        }
    }

    void DrawRoomLabel(Room room, int roomCounter)
    {
        easyDraw.TextAlign(CenterMode.Center, CenterMode.Center);

        var point = GetRoomCenter(room);

        var pRoom = room.area;

        easyDraw.Fill(Color.DimGray);
        easyDraw.TextSize(12.2f);
        easyDraw.Text(
            $"Room[{roomCounter}]:\r\n{room.area.Left},{room.area.Top} | {room.area.Right - 1}, {room.area.Bottom - 1}",
            point.X, point.Y);

        easyDraw.Fill(Color.Cyan);
        easyDraw.TextSize(12f);
        easyDraw.Text(
            $"Room[{roomCounter}]:\r\n{room.area.Left},{room.area.Top} | {room.area.Right - 1}, {room.area.Bottom - 1}",
            point.X, point.Y);
    }

    Room GetRoomAtPoint(Point p)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            var room = rooms[i];
            if (room.area.Contains(p))
            {
                return room;
            }
        }

        return null;
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