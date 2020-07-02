using GXPEngine;

internal class TiledDungeonView : TiledView
{
    private Dungeon _dungeon;

    public TiledDungeonView(Dungeon pDungeon, TileType pDefaultTileType) : base(pDungeon.size.Width,
        pDungeon.size.Height, (int) pDungeon.scale, pDefaultTileType)
    {
        _dungeon = pDungeon;
    }

    protected override void generate()
    {
        //Set Walls
        for (int r = 0; r < _dungeon.rooms.Count; r++)
        {
            var room = _dungeon.rooms[r];
            var walls = room.GetWalls();
            for (int w = 0; w < walls.Count; w++)
            {
                var wall = walls[w];
                SetTileType(wall.X, wall.Y, TileType.WALL);
            }
        }

        //Set Doors
        for (int d = 0; d < _dungeon.doors.Count; d++)
        {
            var door = _dungeon.doors[d];
            SetTileType(door.location.X, door.location.Y, TileType.GROUND);
        }
    }
}