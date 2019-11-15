using System;
using UnityEngine;

[Serializable]
public class GameCharacter
{
    // Character display names
    public string characterName;
    // Character grouping (usually ignored)
    public string characterGrouping;
    // Character id (used for loading)
    public string characterID;
    // Character variant (used for minor differences)
    public int characterVariant;
    // Character icon
    public string characterIconPath;
    public Texture2D characterIconTexture;
}
