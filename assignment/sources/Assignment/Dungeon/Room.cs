﻿using System.Collections.Generic;
using System.Drawing;

/**
 * This class represents (the data for) a Room, at this moment only a rectangle in the dungeon.
 */
class Room
{
    public Rectangle area;

    public Room(Rectangle pArea)
    {
        area = pArea;
    }

    public List<Point> GetWalls()
    {
        var walls = new List<Point>();

        //Left and Right walls
        for (int i = area.Top; i < area.Bottom; i++)
        {
            walls.Add(new Point(area.Left, i));
            walls.Add(new Point(area.Right - 1, i));
        }

        //Top and Bottom walls
        for (int i = area.Left; i < area.Right; i++)
        {
            walls.Add(new Point(i, area.Top));
            walls.Add(new Point(i, area.Bottom - 1));
        }

        return walls;
    }

    public List<Point> GetFloors()
    {
        var floors = new List<Point>();

        for (int i = area.Left + 1; i < area.Right - 1; i++)
        {
            for (int j = area.Top + 1; j < area.Bottom - 1; j++)
            {
                floors.Add(new Point(i, j));
            }
        }

        return floors;
    }

    public override string ToString()
    {
        return area.ToString() + $" | Left: {area.Left}, Top: {area.Top}, Right: {area.Right}, Bottom: {area.Bottom}";
    }
}