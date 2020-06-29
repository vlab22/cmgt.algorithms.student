using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using GXPEngine;
using System.Drawing;
using System.Linq;
using GXPEngine.Core;
using GXPEngine.OpenGL;
using Rectangle = System.Drawing.Rectangle;

internal class SufficientDungeon : Dungeon
{
    public readonly List<GenericNode<Room>> nodes = new List<GenericNode<Room>>();

    public Dictionary<Room, GenericNode<Room>> roomNodeMap = new Dictionary<Room, GenericNode<Room>>();

    public EasyDraw easyDraw;

    private Font _font;

    public SufficientDungeon(Size pSize) : base(pSize)
    {
        doorPen = Pens.Red;

        _font = new Font("Times New Roman", 4f, FontStyle.Regular);

        easyDraw = new EasyDraw(game.width, game.height);
    }

    protected override void generate(int pMinimumRoomSize)
    {
        roomNodeMap.Clear();
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

                var parentNode = AddNode(parentRoom);
                var child0 = AddNode(moreRooms[0]);
                var child1 = AddNode(moreRooms[1]);

                parentNode.connections.Add(child0);
                parentNode.connections.Add(child1);
            }
            else
            {
                rooms.Add(parentRoom);
                roomsToSplit.Remove(parentRoom);
            }

            infiniteLoopCounter++;
        }

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine($"{string.Join("\r\n", rooms)}");

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine(GetNodes());

        AddDoors();
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
    protected Point getRoomCenter(Room pRoom)
    {
        float centerX = ((pRoom.area.Left + pRoom.area.Right) / 2.0f) * this.scale;
        float centerY = ((pRoom.area.Top + pRoom.area.Bottom) / 2.0f) * this.scale;
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
        var invertedNodes = roomNodeMap.Values.ToArray();
        for (int i = invertedNodes.Length - 1; i > 0; i -= 2)
        {
            Console.WriteLine($"Door[{i}] {invertedNodes[i - 1].id} <=> {invertedNodes[i].id}");

            AddDoorBetweenRooms(invertedNodes[i - 1].obj, invertedNodes[i].obj);
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

        Console.WriteLine($"Room[{room0.area.Location} - {room0.area.Width} / {room0.area.Height}]: corners: {string.Join("|", GetRoomsCorners(room0))}");

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
            throw new Exception($"Infinite Loop in door creation between '{room0}' and '{room1}'. Door at pos {doorPos}");
        }

        doorPos = doorPosCorner;
        
        var door = new Door(doorPos);
        doors.Add(door);

        Console.WriteLine($"doorPos: {doorPos}");
    }

    public Point RoomDirection(Room room0, Room room1)
    {
        var center0 = getRoomCenter(room0);
        var center1 = getRoomCenter(room1);

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
            new Point(room.area.Right-1, room.area.Top),
            new Point(room.area.Left, room.area.Bottom-1),
            new Point(room.area.Right-1, room.area.Bottom-1),
        };

        return pts;
    }

    public GenericNode<Room> AddNode(Room room)
    {
        var roomLocation = getRoomCenter(room);
        // for (int i = 0; i < nodes.Count; i++)
        // {
        //     var n = nodes[i];
        //
        //     var otherLocation = getRoomCenter(n.obj);
        //
        //     if (roomLocation == otherLocation)
        //     {
        //         return n;
        //     }
        // }

        var node = new GenericNode<Room>(room);

        nodes.Add(node);

        if (!roomNodeMap.ContainsKey(room))
            roomNodeMap.Add(room, node);

        return node;
    }

    string GetNodes()
    {
        string result = "Nodes: \r\n";

        for (int i = 0; i < nodes.Count; i++)
        {
            result += nodes[i] + "\r\n";

            if (nodes[i].connections.Count == 2)
            {
                result += "\t" + nodes[i].connections[0] + "\r\n";
                result += "\t" + nodes[i].connections[1] + "\r\n";
            }

            result += "\r\n";
        }

        return result;
    }

    public int creationIndex = 1;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DrawNodesRooms(1);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            DrawNodesRooms(-1);
        }

        var mouseX = Input.mouseX;
        var mouseY = Input.mouseY;
        var mouseTileX = (int) (mouseX / scale);
        var mouseTileY = (int) (mouseY / scale);
        GL.glfwSetWindowTitle($"MouseTileX: {mouseTileX:00} | MouseTileY: {mouseTileY:00}");
    }

    void DrawNodesRooms(int dir)
    {
        creationIndex = ArrayTools.GetCircularArrayIndex(creationIndex + dir, roomNodeMap.Count + 1);

        var nodesToDraw = roomNodeMap.Values.Take(creationIndex);
        var newRooms = nodesToDraw.Select(gn => gn.obj);

        graphics.Clear(Color.Transparent);
        drawRooms(newRooms, wallPen);
        drawDoors(doors, doorPen);

        DrawNodesLabels(nodesToDraw);
    }

    void DrawNodeLabel(GenericNode<Room> roomNode)
    {
        easyDraw.TextAlign(CenterMode.Center, CenterMode.Center);

        var room = roomNode.obj;
        var point = getRoomCenter(room);

        var pRoom = room.area;

        easyDraw.Fill(Color.Cyan);
        easyDraw.Text($"Node: {roomNode.id}", point.X, point.Y);
    }

    void DrawNodesLabels(IEnumerable<GenericNode<Room>> pRoomNodes)
    {
        easyDraw.Clear(Color.Transparent);
        foreach (var roomNode in pRoomNodes)
        {
            DrawNodeLabel(roomNode);
        }
    }
}