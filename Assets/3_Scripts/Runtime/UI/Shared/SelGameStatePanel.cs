using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelGameStatePanel : GameStatePanel
{
    // Allowed states
    public List<GameState> states;

    // Show
    protected override bool ShouldShow()
    {
        if (states.Contains(GameManager.instance.gameState))
        {
            return true;
        }
        return false;
    }
}
