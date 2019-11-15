using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePausePanel : GameStatePanel
{
    [Header("Pause UI")]
    public RoomiesButton homeButton;
    public RoomiesButton playerButton;
    public RoomiesButton restartButton;
    public RoomiesButton resumeButton;

    // Add btn delegates
    protected override void Awake()
    {
        base.Awake();
        homeButton.onSubmit += HomeClick;
        playerButton.onSubmit += PlayerClick;
        restartButton.onSubmit += RestartClick;
        resumeButton.onSubmit += ResumeClick;
    }
    // Remove btn delegates
    protected override void OnDestroy()
    {
        base.OnDestroy();
        homeButton.onSubmit -= HomeClick;
        playerButton.onSubmit -= PlayerClick;
        restartButton.onSubmit -= RestartClick;
        resumeButton.onSubmit -= ResumeClick;
    }
    // Show if pause panel
    protected override bool ShouldShow()
    {
        return GameManager.instance.gameState == GameState.GamePause;
    }
    // Only reverse if going to play
    protected override void OnTransitionBegin()
    {
        _reverse = GameManager.instance.gameState == GameState.GamePlay;
        base.OnTransitionBegin();
    }

    #region MOUSE
    // Home click
    private void HomeClick()
    {
        Home();
    }
    // Go to player select
    private void PlayerClick()
    {
        PlayerSelect();
    }
    // Restart click
    private void RestartClick()
    {
        Restart();
    }
    // Resume
    private void ResumeClick()
    {
        Resume();
    }
    #endregion

    #region INPUT
    // Player index
    public static int playerIndex = -1;

    // Player index
    private int _playerIndex = -1;
    // Input index
    private int _inputIndex = -1;
    // Update
    protected virtual void Update()
    {
        // Player set
        if (_playerIndex != playerIndex)
        {
            _playerIndex = playerIndex;
            _inputIndex = 3;
            UpdateButtons();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Resume();
        }
    }
    // Update buttons
    private void UpdateButtons()
    {
        homeButton.SetInputPlayer(_inputIndex == 0 ? _playerIndex : -1);
        playerButton.SetInputPlayer(_inputIndex == 1 ? _playerIndex : -1);
        restartButton.SetInputPlayer(_inputIndex == 2 ? _playerIndex : -1);
        resumeButton.SetInputPlayer(_inputIndex == 3 ? _playerIndex : -1);
    }

    // Resume game
    protected override void OnPlayerPause(int playerIndex)
    {
        if (playerIndex == _playerIndex)
        {
            Resume();
        }
    }
    // Move button
    protected override void OnPlayerDirection(int playerIndex, Direction direction)
    {
        if (playerIndex == _playerIndex)
        {
            switch (direction)
            {
                case Direction.Left:
                    _inputIndex--;
                    if (_inputIndex == -1)
                    {
                        _inputIndex = 3;
                    }
                    UpdateButtons();
                    break;
                case Direction.Right:
                    _inputIndex++;
                    if (_inputIndex == 4)
                    {
                        _inputIndex = 0;
                    }
                    UpdateButtons();
                    break;
            }
        }
    }
    // Confirm button
    protected override void OnPlayerSelect(int playerIndex)
    {
        if (playerIndex == _playerIndex)
        {
            switch (_inputIndex)
            {
                case 0:
                    Home();
                    break;
                case 1:
                    PlayerSelect();
                    break;
                case 2:
                    Restart();
                    break;
                case 3:
                    Resume();
                    break;
            }
        }
    }
    #endregion

    #region ACTIONS
    // Home button
    private void Home()
    {
        playerIndex = -1;
        GameManager.instance.SetState(GameState.Title);
    }
    // Player select
    private void PlayerSelect()
    {
        playerIndex = -1;
        GameManager.instance.SetState(GameState.PlayerSetup);
    }
    // Restart button
    private void Restart()
    {
        playerIndex = -1;
        GameManager.instance.PlayNewMatch();
    }
    // Resume button
    private void Resume()
    {
        playerIndex = -1;
        GameManager.instance.SetState(GameState.GamePlay);
    }
    #endregion
}
