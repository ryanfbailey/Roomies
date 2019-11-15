using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitlePanel : GameStatePanel
{
    [Header("Title UI")]
    // Start button
    public RoomiesButton setupButton;

    // On Enable
    private void OnEnable()
    {
        setupButton.onSubmit += OnStartClick;
    }
    // On Disable
    private void OnDisable()
    {
        setupButton.onSubmit -= OnStartClick;
    }

    // Show if title panel
    protected override bool ShouldShow()
    {
        return GameManager.instance.gameState == GameState.Title;
    }
    // Player pause
    protected override void OnPlayerPause(int playerIndex)
    {
        base.OnPlayerPause(playerIndex);
        OnStartClick();
    }
    // Player select
    protected override void OnPlayerSelect(int playerIndex)
    {
        base.OnPlayerSelect(playerIndex);
        OnStartClick();
    }
    // Start game
    private void OnStartClick()
    {
        GameManager.instance.SetState(GameState.PlayerSetup);
    }

    // Quit
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
