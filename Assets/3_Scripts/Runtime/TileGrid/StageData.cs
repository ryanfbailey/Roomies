using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StageData
{
    // Stage id
    public string stageID;
    // Full grid column ammount
    public int columns;
    // Full grid row ammount
    public int rows;

    // Tile data for this grid
    public TileData[] tiles;
    // The prop data for this grid
    public PropData[] props;
    // The player data for this grid
    public PlayerData[] players;
}

[Serializable]
public class TileData
{
    // Tile index
    public int tileIndex;
    // Tile prefab type
    public int tilePrefab;
    // Tile prefab variant
    public int tilePrefabVariant;

    // Connected tile indices
    public int tileDown;
    public int tileUp;
    public int tileRight;
    public int tileLeft;
}

[Serializable]
public class PropData
{
    // Prop tile start index
    public int tileIndex;
    // Starting direction (more for immovable objects)
    public Direction direction;

    // Prop prefab type
    public int propPrefab;
    // Variant material
    public int propPrefabVariant;

    // If touched, you win
    public bool isWinProp;
}

[Serializable]
public class PlayerData
{
    // Player tile start index
    public int tileIndex;
    // Start direction
    public Direction direction;
}

// Direction
public enum Direction
{
    None,
    Down,
    Up,
    Left,
    Right
}