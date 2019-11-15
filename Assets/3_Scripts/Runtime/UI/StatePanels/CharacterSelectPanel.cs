using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectPanel : GameStatePanel
{
    #region SETUP
    [Header("Player Setup UI")]
    public RoomiesButton homeButton;
    public RoomiesButton startButton;
    public TextMeshProUGUI startLabel;

    // Awake
    protected override void Awake()
    {
        base.Awake();
        GameManager.onPlayersUpdated += OnPlayersChanged;
        homeButton.onSubmit += OnHomeClick;
        startButton.onSubmit += OnStartClick;
    }
    // Load
    private void OnEnable()
    {
        ReloadTable();
    }
    // On Disable
    protected override void OnDestroy()
    {
        base.OnDestroy();
        GameManager.onPlayersUpdated -= OnPlayersChanged;
        homeButton.onSubmit -= OnHomeClick;
        startButton.onSubmit -= OnStartClick;
    }
    // Show if player setup panel
    protected override bool ShouldShow()
    {
        return GameManager.instance.gameState == GameState.PlayerSetup;
    }
    // Home click
    private void OnHomeClick()
    {
        GoToTitle();
    }
    // Start click
    private void OnStartClick()
    {
        GoToGame();
    }
    // Update all
    private void OnPlayersChanged(List<GamePlayer> players)
    {
        ReloadTable();
    }
    // Go to title
    public void GoToTitle()
    {
        while (GameManager.instance.players.Count > 0)
        {
            GameManager.instance.RemovePlayer(0);
        }
        GameManager.instance.SetState(GameState.Title);
    }
    // Start game
    public void GoToGame()
    {
        if (IsEveryoneReady() && GameManager.instance.gameState == GameState.PlayerSetup)
        {
            GameManager.instance.PlayNewMatch();
        }
    }
    // Is everyone ready
    public bool IsEveryoneReady()
    {
        // Not enough players
        if (GameManager.instance.players.Count < GameManager.instance.gameData.minPlayers)
        {
            return false;
        }

        // Someone is not ready
        for (int p = 0; p < GameManager.instance.players.Count; p++)
        {
            if (!GetPlayerReady(p))
            {
                return false;
            }
        }

        // Success!
        return true;
    }
    // Update start button
    private PlayerCell.PlayerInputButton _btnType = (PlayerCell.PlayerInputButton)(-1);
    private void Update()
    {
        // Should disable
        bool shouldDisable = !IsEveryoneReady();
        if (startButton.isDisabled != shouldDisable)
        {
            SetDisabled(shouldDisable);
        }
        // Get child
        if (playerTable.childCount > 0)
        {
            Transform child = playerTable.GetChild(0);
            PlayerCell cell = child.GetComponent<PlayerCell>();
            if (_btnType != cell.inputButton)
            {
                _btnType = cell.inputButton;
                homeButton.SetInputPlayer(_btnType == PlayerCell.PlayerInputButton.Home ? 0 : -1);
                startButton.SetInputPlayer(_btnType == PlayerCell.PlayerInputButton.Start ? 0 : -1);
            }
        }
    }
    // Set disabled
    private void SetDisabled(bool toDisabled)
    {
        startButton.SetDisabled(toDisabled);
        startLabel.text = toDisabled ? "WAITING ON PLAYERS..." : "START GAME";
        startLabel.fontSize = toDisabled ? 30 : 36;
    }
    #endregion

    #region TABLE
    [Header("Table Layout")]
    // Column Margin
    public float columnMargin = 5f;
    // Row Margin
    public float rowMargin = 5f;
    // Total per row
    public int playersPerRow = 4;
    // CTA
    public RectTransform cta;
    // Player cell
    public PlayerCell playerCell;
    // Player table
    public RectTransform playerTable;

    // Load table
    private void ReloadTable()
    {
        // Unload all cells
        for (int c = 0; c < playerTable.childCount; c++)
        {
            Transform child = playerTable.GetChild(c);
            child.gameObject.SetActive(false);
        }

        // Get player count
        int players = GameManager.instance.players.Count;
        int cells = players + (players != GameManager.instance.gameData.maxPlayers ? 1 : 0);

        // Get rows & columns
        int rows = Mathf.CeilToInt((float)cells / (float)playersPerRow);
        int columns = Mathf.Min(playersPerRow, cells);

        // Get cell size
        float cellHeight = (playerTable.rect.height - (rows > 1 ? (rows - 1) * rowMargin : 0f)) / rows;
        float cellWidth = (playerTable.rect.width - (columns > 1 ? (columns - 1) * columnMargin : 0f)) / columns;

        // Final row offset
        float header = 0f;
        int open = (cells % columns);
        if (cells > columns && open > 0)
        {
            header = open * cellWidth + (open > 1 ? (open - 1) * columnMargin : 0f);
            header = (playerTable.rect.width - header) / 2f;
        }

        // Load a cell for each player
        int column = 0; int row = 0;
        for (int p = 0; p < players; p++)
        {
            // Get child transform
            RectTransform child = p < playerTable.childCount ? playerTable.GetChild(p).GetComponent<RectTransform>() : null;
            if (child == null)
            {
                child = Instantiate<GameObject>(playerCell.gameObject).GetComponent<RectTransform>();
                child.SetParent(playerTable);
                child.localPosition = Vector3.zero;
                child.localRotation = Quaternion.identity;
                child.localScale = Vector3.one;
            }
            child.gameObject.SetActive(true);

            // Adjust position & size
            child.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, (row == rows - 1 ? header : 0f) + column * (cellWidth + columnMargin), cellWidth);
            child.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, row * (cellHeight + rowMargin), cellHeight);
            column++;
            if (column >= columns)
            {
                column = 0;
                row++;
            }

            // Setup cell
            ReloadCell(p);
        }

        // Adjust cta
        if (cta != null)
        {
            // Hit max players
            if (players == GameManager.instance.gameData.maxPlayers)
            {
                cta.gameObject.SetActive(false);
            }
            // Use cta
            else
            {
                // Enable
                cta.gameObject.SetActive(true);

                // Same as cells
                cta.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, (row == rows - 1 ? header : 0f) + column * (cellWidth + columnMargin), cellWidth);
                cta.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, row * (cellHeight + rowMargin), cellHeight);
            }
        }

        // Set disabled
        _btnType = (PlayerCell.PlayerInputButton)(-1);
        SetDisabled(true);
    }
    // Reload cell
    private void ReloadCell(int playerIndex)
    {
        Transform child = playerTable.GetChild(playerIndex);
        PlayerCell cell = child.GetComponent<PlayerCell>();
        cell.SetPlayer(playerIndex);
    }
    // Check if ready
    private bool GetPlayerReady(int playerIndex)
    {
        GamePlayer player = GameManager.instance.GetPlayer(playerIndex);
        return player.ready;
    }
    // Player pause
    protected override void OnPlayerPause(int playerIndex)
    {
        PlayerClick(playerIndex);
    }
    // Player select
    protected override void OnPlayerSelect(int playerIndex)
    {
        PlayerClick(playerIndex);
    }
    // Player hit okay
    private void PlayerClick(int playerIndex)
    {
        if (playerIndex == 0)
        {
            Transform child = playerTable.GetChild(playerIndex);
            PlayerCell cell = child.GetComponent<PlayerCell>();
            if (cell.inputButton == PlayerCell.PlayerInputButton.Home)
            {
                GoToTitle();
            }
            else if (cell.inputButton == PlayerCell.PlayerInputButton.Start)
            {
                GoToGame();
            }
        }
    }
    /*
    // Player cancel
    protected override void OnPlayerCancel(int playerIndex)
    {
        // Not ready
        if (GetPlayerReady(playerIndex))
        {
            SetPlayerReady(playerIndex, false);
        }
        // Escape
        else if (playerIndex == 0)
        {
            GoToTitle();
        }
    }
    // Player direction
    protected override void OnPlayerDirection(int playerIndex, Direction direction)
    {
        // No player
        GamePlayer player = GameManager.instance.GetPlayer(playerIndex);
        if (player == null)
        {
            return;
        }
        if (GetPlayerReady(playerIndex))
        {
            return;
        }

        // Left
        if (direction == Direction.Left)
        {
            player.characterIndex--;
            if (player.characterIndex < 0)
            {
                player.characterIndex = GameManager.instance.gameData.characters.Length - 1;
            }
            ReloadCell(playerIndex);
        }
        // Right
        else if (direction == Direction.Right)
        {
            player.characterIndex++;
            if (player.characterIndex >= GameManager.instance.gameData.characters.Length)
            {
                player.characterIndex = 0;
            }
            ReloadCell(playerIndex);
        }
    }
    */
    #endregion
}
