using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour
{
    // Tile data
    public TileData data;// { get; private set; }
    // Variants
    public GridVariant[] variants;
    // Object on tile
    public GridItem movable;

    // Load data
    public void LoadData(TileData newData)
    {
        // Set data & name
        data = newData;
        gameObject.name = "TILE_" + data.tileIndex.ToString("00000");
        movable = null;

        // Set variants
        if (variants != null)
        {
            for (int v = 0; v < variants.Length; v++)
            {
                GridVariant variant = variants[v];
                variant.SetVariant(v == data.tilePrefabVariant);
            }
        }
    }

    // Get tile in direction
    public GridTile GetAdjacentTile(Direction direction)
    {
        GridTile result = null;
        if (data != null)
        {
            switch (direction)
            {
                case Direction.Down:
                    result = GridManager.instance.GetTile(data.tileDown);
                    break;
                case Direction.Up:
                    result = GridManager.instance.GetTile(data.tileUp);
                    break;
                case Direction.Left:
                    result = GridManager.instance.GetTile(data.tileLeft);
                    break;
                case Direction.Right:
                    result = GridManager.instance.GetTile(data.tileRight);
                    break;
            }
        }
        return result;
    }
}
