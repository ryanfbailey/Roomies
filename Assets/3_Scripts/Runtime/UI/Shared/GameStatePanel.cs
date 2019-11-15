using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameStatePanel : Panel
{
    // Add delegates
    protected override void Awake()
    {
        base.Awake();
        GameManager.onGameStateChange += OnGameStateChanged;
        GameManager.onPlayerButtonClick += OnButtonClicked;
    }
    // Remove delegates
    protected override void OnDestroy()
    {
        base.OnDestroy();
        GameManager.onGameStateChange -= OnGameStateChanged;
        GameManager.onPlayerButtonClick -= OnButtonClicked;
    }
    // Game state changed
    protected virtual void OnGameStateChanged(GameState newState, bool immediately)
    {
        // Check whether should show
        bool shouldShow = ShouldShow();

        // Transition
        if (isShowing != shouldShow)
        {
            Transition(shouldShow, immediately);
        }
    }
    // Player clicked a button
    private void OnButtonClicked(int playerIndex, string buttonID)
    {
        if (ShouldShow() && !isTransitioning)
        {
            if (string.Equals(buttonID, GameManager.SUBMIT_INPUT_KEY))
            {
                OnPlayerSelect(playerIndex);
            }
            else if (string.Equals(buttonID, GameManager.CANCEL_INPUT_KEY))
            {
                OnPlayerCancel(playerIndex);
            }
            else if (string.Equals(buttonID, GameManager.PAUSE_INPUT_KEY))
            {
                OnPlayerPause(playerIndex);
            }
            else if (string.Equals(buttonID, GameManager.HORIZONTAL_POS_INPUT_KEY))
            {
                OnPlayerDirection(playerIndex, Direction.Right);
            }
            else if (string.Equals(buttonID, GameManager.HORIZONTAL_NEG_INPUT_KEY))
            {
                OnPlayerDirection(playerIndex, Direction.Left);
            }
            else if (string.Equals(buttonID, GameManager.VERTICAL_POS_INPUT_KEY))
            {
                OnPlayerDirection(playerIndex, Direction.Up);
            }
            else if (string.Equals(buttonID, GameManager.VERTICAL_NEG_INPUT_KEY))
            {
                OnPlayerDirection(playerIndex, Direction.Down);
            }
            else
            {
                Debug.Log("UNKNOWN BUTTON ID: " + buttonID);
            }
        }
    }

    // Whether or not should show
    protected abstract bool ShouldShow();

    // Player pressed select
    protected virtual void OnPlayerSelect(int playerIndex)
    {

    }
    // Player pressed cancel
    protected virtual void OnPlayerCancel(int playerIndex)
    {

    }
    // Player pressed pause
    protected virtual void OnPlayerPause(int playerIndex)
    {

    }
    // Player pressed direction
    protected virtual void OnPlayerDirection(int playerIndex, Direction direction)
    {

    }
}
