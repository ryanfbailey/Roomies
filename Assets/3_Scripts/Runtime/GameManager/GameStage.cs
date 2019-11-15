using System;
using UnityEngine;

[Serializable]
public class GameStage
{
    // Used for display
    public string stageName;
    // Used for display pt. 2
    public string stageGrouping;
    // Stage id (used for loading)
    public string stageID;
    // Stage variant
    public int stageVariant;
    // Complexity
    public int stageComplexity;
    // Stage icon
    public string stageIcon;
    public Texture2D stageIconSprite;
}
