using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridItem : MonoBehaviour
{
    // Tile data
    public int tileIndex { get; protected set; }
    public GridTile tile { get; protected set; }
    // Push data
    public Vector2 pushOffset { get; protected set; }
    public Direction pushDirection { get; protected set; }
    protected const float PUSH_MAX = 0.5f;
    // Slide data
    protected const float SLIDE_LERP = 0.2f;
    protected const float VEL_MIN = 0.01f;

    // Variants
    public GridVariant[] variants;
    // Set variant
    public void SetVariant(int variantIndex)
    {
        if (variants != null)
        {
            for (int v = 0; v < variants.Length; v++)
            {
                GridVariant variant = variants[v];
                variant.SetVariant(v == variantIndex);
            }
        }
    }

    // Set tile index
    public void SetTileIndex(int newIndex, bool reset = false)
    {
        // Remove from old tile
        if (tile != null && tile.movable == this)
        {
            tile.movable = null;
        }

        // Set index
        tileIndex = newIndex;
        tile = GridManager.instance.GetTile(tileIndex);

        // Set to new tile
        if (tile != null)
        {
            tile.movable = this;
        }

        // Set position
        if (reset)
        {
            pushOffset = Vector2.zero;
            pushDirection = Direction.None;
        }
    }

    // Whether can push from direction
    public virtual bool CanPush(Direction direction, int startTile)
    {
        // Get next tile
        GridTile nextTile = tile.GetAdjacentTile(direction);
        if (nextTile == null)
        {
            return false;
        }

        // Cannot push start tile
        if (nextTile.data.tileIndex == startTile)
        {
            return false;
        }
        // Iterate through
        if (nextTile.movable != null)
        {
            return nextTile.movable.CanPush(direction, startTile);
        }

        // Return true
        return true;
    }

    // Push was attempted
    public virtual void PrePush(GridItem byItem, Direction inDirection)
    {
        //Debug.Log(gameObject.name.ToUpper() + " PRE PUSH\nDIRECTION: " + inDirection.ToString());
    }

    // Perform push somehow
    protected void Push(Direction direction, float offset, bool allowPush = true)
    {
        // Get next tile
        GridTile next = tile.GetAdjacentTile(direction);
        if (next == null)
        {
            return;
        }

        // Push next tile
        if (next.movable != null && allowPush)
        {
            next.movable.Push(direction, offset);
        }

        // Move if next movable is free
        if (Mathf.Abs(offset) > PUSH_MAX)
        {
            if (next.movable == null)
            {
                SetTileIndex(next.data.tileIndex);
                offset += (offset > 0f ? -1f : 1f) * PUSH_MAX * 2f;
            }
            else
            {
                offset = 0f;
            }
        }

        // Set push direction
        pushDirection = direction;

        // Set push offset
        Vector2 newOffset = pushOffset;
        switch (direction)
        {
            case Direction.Left:
            case Direction.Right:
                newOffset.x = offset;
                break;
            case Direction.Down:
            case Direction.Up:
                newOffset.y = offset;
                break;
        }
        pushOffset = newOffset;
    }

    // Adjust push offset
    protected virtual void Update()
    {
        // Only move if tile exists
        if (tile == null)
        {
            return;
        }

        // Slide if not pushing
        Vector2 newOffset = pushOffset;
        if (newOffset.x != 0f && pushDirection != Direction.Left && pushDirection != Direction.Right)
        {
            newOffset.x = Mathf.Lerp(newOffset.x, 0f, SLIDE_LERP);
            if (Mathf.Abs(newOffset.x) < VEL_MIN)
            {
                newOffset.x = 0f;
            }
        }
        if (newOffset.y != 0f && pushDirection != Direction.Up && pushDirection != Direction.Down)
        {
            newOffset.y = Mathf.Lerp(newOffset.y, 0f, SLIDE_LERP);
            if (Mathf.Abs(newOffset.y) < VEL_MIN)
            {
                newOffset.y = 0f;
            }
        }
        pushOffset = newOffset;

        // Done pushing for this frame
        if (pushDirection != Direction.None)
        {
            pushDirection = Direction.None;
        }
    }


    // Set position with offset
    protected virtual void LateUpdate()
    {
        transform.localPosition = tile.transform.position + (new Vector3(pushOffset.x, 0f, pushOffset.y) * GridManager.instance.tileSize);
    }
}
