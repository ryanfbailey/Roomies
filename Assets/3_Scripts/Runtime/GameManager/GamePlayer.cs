using System;
using UnityEngine;

[Serializable]
public class GamePlayer
{
    // Player user name
    public string userName;
    // Color index
    public int colorIndex;
    // Character selection
    public int characterIndex;
    // Input tracking
    public int inputIndex;
    // Whether ready or not
    public bool ready;
    // Player score
    public int score;
}
