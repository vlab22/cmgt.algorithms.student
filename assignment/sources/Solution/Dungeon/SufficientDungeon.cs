using System;
using System.Collections.Generic;
using GXPEngine;
using System.Drawing;
using System.Linq;
using GXPEngine.OpenGL;
using Rectangle = System.Drawing.Rectangle;

internal class SufficientDungeon : Dungeon
{
    public Dictionary<Room, List<Room>> roomHierarchyTree = new Dictionary<Room,  List<Room>>();

    private int _stepCreationIndex = 1;
    
    public SufficientDungeon(Size pSize) : base(pSize)
    {
        doorPen = Pens.Red;
    }

    protected override void generate(int pMinimumRoomSize)
    {
        roomHierarchyTree.Clear();
        _stepCreationIndex = 1;

        var roomsToSplit = new List<Room>();

        roomsToSplit.Add(new Room(new Rectangle(0, 0, size.Width, size.Height)));

        bool allowGenerate = true;

        while (roomsToSplit.Count > 0)
        {
            var parentRoom = roomsToSplit.First();

            Console.WriteLine(parentRoom);

            bool roomsGenerated = Generate2Rooms(parentRoom, pMinimumRoomSize, out var moreRooms);

            if (roomsGenerated)
            {
                roomsToSplit.AddRange(moreRooms);
                roomsToSplit.Remove(parentRoom);

                roomHierarchyTree.Add(parentRoom, moreRooms);
            }
            else
            {
                rooms.Add(parentRoom);
                roomsToSplit.Remove(parentRoom);

                roomHierarchyTree.Add(parentRoom, new List<Room>(0));
            }
        }

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine($"{string.Join("\r\n", rooms)}");

        Console.WriteLine();
        Console.WriteLine();

        AddDoors();

        Console.WriteLine($"Doors Count: {doors.Count}");
    }

    bool Generate2Rooms(Room parent, int pMinimumRoomSize, out List<Room> pRooms)
    {
        pRooms = new List<Room>(2);

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
        var allRooms = roomHierarchyTree.Keys.ToArray();
        for (int i = allRooms.Length - 1; i > 0; i -= 2)
        {
            Console.WriteLine($"Door[{i}] {allRooms[i - 1]} <=> {allRooms[i]}");

            AddDoorBetweenRooms(allRooms[i - 1], allRooms[i]);
        }
    }

    public void AddDoorBetweenRooms(Room room0, Room room1)
    {
        var roomDirection = RoomDirection(room0, room1); // (1, 0) or (0, 1)

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

        int newDoorOffset = 1;
        var doorPosCorner = doorPos;
        while (IsInRoomCorner(doorPosCorner))
        {
            doorPosCorner.X = doorPos.X + roomDirection.Y * newDoorOffset;
            doorPosCorner.Y = doorPos.Y + roomDirection.X * newDoorOffset;

            newDoorOffset *= -1;

            Console.WriteLine($"doorPos: {doorPos} RELOCATED");
        }

        doorPos = doorPosCorner;

        var door = new Door(doorPos)
        {
            horizontal = roomDirection.X != 0
        };

        doors.Add(door);

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

    bool IsInRoomCorner(Point p)
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

    private void Update()
    {
        var mouseX = Input.mouseX;
        var mouseY = Input.mouseY;
        var mouseTileX = (int) (mouseX / scale);
        var mouseTileY = (int) (mouseY / scale);

        var roomAtPoint = GetRoomAtPoint(new Point(mouseTileX, mouseTileY));

        GL.glfwSetWindowTitle($"MouseTileX: {mouseTileX:00} | MouseTileY: {mouseTileY:00} | {roomAtPoint}");
    }

    public void DrawRoomsByStep(int dir)
    {
        _stepCreationIndex = ArrayTools.GetCircularArrayIndex(_stepCreationIndex + dir, roomHierarchyTree.Count + 1);

        var roomsToDraw = roomHierarchyTree.Keys.Take(_stepCreationIndex);

        graphics.Clear(Color.Transparent);
        drawRooms(roomsToDraw, wallPen);
        drawDoors(doors, doorPen);
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
}