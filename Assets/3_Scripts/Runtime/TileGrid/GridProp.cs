using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridProp : GridItem
{
    // Prop data
    public PropData data { get; private set; }

    // Locking pushing
    public bool downLocked = false;
    public bool upLocked = false;
    public bool leftLocked = false;
    public bool rightLocked = false;

    // Set data
    public void LoadData(PropData newData)
    {
        // Set data
        data = newData;
        // Set tile position
        tileIndex = -1;
        tile = null;
        SetTileIndex(data.tileIndex, true);

        // Set variant
        SetVariant(newData.propPrefabVariant);
    }

    // Whether can push from direction
    public override bool CanPush(Direction direction, int startTile)
    {
        // Check each direction
        bool result = false;
        switch (direction)
        {
            case Direction.Down:
                result = !downLocked;
                break;
            case Direction.Up:
                result = !upLocked;
                break;
            case Direction.Left:
                result = !leftLocked;
                break;
            case Direction.Right:
                result = !rightLocked;
                break;
        }
        if (!result)
        {
            return false;
        }

        // Use base
        return base.CanPush(direction, startTile);
    }

    // Pre push
    public override void PrePush(GridItem byItem, Direction inDirection)
    {
        base.PrePush(byItem, inDirection);
        if (data.isWinProp && byItem.GetType() == typeof(GridCharacter))
        {
            GridCharacter c = (GridCharacter)byItem;
            GameManager.instance.WinStage(c.playerIndex);
        }
    }
}
