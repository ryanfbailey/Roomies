using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Variant of Prop, with different version of CanPush, for example, you can't directly push a chair, but can push a block into the chair to move it
/// </summary>
public class Obstacle : GridProp
{
    [SerializeField]
    private bool pushByChar = false;
    [SerializeField]
    private bool isBeingPushed = false;

    public override void PrePush(GridItem byItem, Direction inDirection)
    {
        //base.PrePush(byItem, inDirection);
        if (byItem.GetComponent<GridCharacter>())
        {
            pushByChar = true;
            Debug.Log("pushed by char");
        }
    }

    public override bool CanPush(Direction direction, int startTile)
    {
        if(pushByChar)
        {
            //Debug.Log(gameObject.name+" Push by char");
            return base.CanPush(direction, startTile);
        }
        else
        {
            // Debug.Log(gameObject.name+"Not directly touching character");
            // return base.CanPush(direction, startTile);
            //use griditem.cs canpush method
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
    }
    
    protected override void LateUpdate()
    {
        //base.LateUpdate();
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
            isBeingPushed = true;
            pushDirection = Direction.None;
        }
        else
        {
            isBeingPushed = false;
            pushByChar = false;
        }

        // Set position with offset
        transform.localPosition = tile.transform.position + (new Vector3(newOffset.x, 0f, newOffset.y) * GridManager.instance.tileSize);
    }

}
