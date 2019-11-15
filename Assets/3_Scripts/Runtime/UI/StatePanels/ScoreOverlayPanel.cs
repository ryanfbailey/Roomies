using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreOverlayPanel : Panel
{
    [Header("Player Score UI")]
    // Offset
    public float playerPaddingX = 10f;
    public float playerMarginY = 10f;
    // Player ui
    public PlayerScoreCell playerScorePrefab;
    public RectTransform playerScoreContainer;

    // Add delegates
    protected override void Awake()
    {
        base.Awake();
        GameManager.onGameStateChange += OnGameStateChanged;
    }
    // Remove delegates
    protected override void OnDestroy()
    {
        base.OnDestroy();
        GameManager.onGameStateChange -= OnGameStateChanged;
    }

    // State changed
    protected virtual void OnGameStateChanged(GameState newState, bool immediately)
    {
        // Hide
        if (newState == GameState.Title || newState == GameState.PlayerSetup)
        {
            Transition(false, immediately);
        }
        // Refresh characters & hide
        else if (newState == GameState.GameLoad && GameManager.instance.currentRound == 0)
        {
            LayoutCharacters(true);
            Transition(false, true);
        }
        else if (newState == GameState.GameIntro)
        {
            // Fade in & animate score
            if (GameManager.instance.currentRound == 0)
            {
                Transition(true, immediately);
                Animate(immediately);
            }
            // Animate score
            else
            {
                Animate(immediately);
            }
        }
    }

    // Load character info
    private void LayoutCharacters(bool immediately)
    {
        // Unload player data
        UnloadPlayerData();

        // Load players
        if (GameManager.instance.players != null && GameManager.instance.players.Count > 0)
        {
            int leftCount = Mathf.CeilToInt((float)GameManager.instance.players.Count / 2f);
            float height = leftCount * playerScorePrefab.GetComponent<RectTransform>().rect.height + Mathf.Max(0f, leftCount - 1f) * playerMarginY;
            float startY = (playerScoreContainer.rect.height - height) / 2f;
            float y = startY;
            for (int p = 0; p < GameManager.instance.players.Count; p++)
            {
                // Get cell
                RectTransform playerScoreRect = Pool.instance.Load(playerScorePrefab.gameObject).GetComponent<RectTransform>();
                playerScoreRect.gameObject.name = "PLAYER_" + (p + 1).ToString("00");
                playerScoreRect.gameObject.SetActive(true);

                // Set transform
                playerScoreRect.transform.SetParent(playerScoreContainer);
                playerScoreRect.transform.localPosition = Vector3.zero;
                playerScoreRect.transform.localRotation = Quaternion.identity;
                playerScoreRect.transform.localScale = Vector3.one;
                if (p < leftCount)
                {
                    playerScoreRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, playerPaddingX, playerScoreRect.rect.width);
                    playerScoreRect.localScale = Vector3.one;
                }
                else
                {
                    playerScoreRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, playerPaddingX - playerScoreRect.rect.width, playerScoreRect.rect.width);
                    playerScoreRect.localScale = new Vector3(-1f, 1f, 1f);
                }
                playerScoreRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, y, playerScoreRect.rect.height);
                y += playerScoreRect.rect.height + playerMarginY;
                if (p + 1 == leftCount)
                {
                    y = startY;
                }

                // Set player items
                PlayerScoreCell cell = playerScoreRect.GetComponent<PlayerScoreCell>();
                cell.characterName.transform.localScale = playerScoreRect.localScale;
                cell.scoreMaxLabel.transform.parent.localScale = playerScoreRect.localScale;
                cell.LayoutInterface(p);
            }
        }
    }
    // Refresh score
    public void Animate(bool immediately)
    {
        if (GameManager.instance.players != null && GameManager.instance.players.Count > 0 && GameManager.instance.players.Count <= playerScoreContainer.childCount)
        {
            for (int p = 0; p < GameManager.instance.players.Count; p++)
            {
                PlayerScoreCell cell = playerScoreContainer.GetChild(p).GetComponent<PlayerScoreCell>();
                cell.Animate(immediately);
            }
        }
    }

    // Unload all cells
    private void UnloadPlayerData()
    {
        while (playerScoreContainer.childCount > 0)
        {
            Pool.instance.Unload(playerScoreContainer.GetChild(0).gameObject);
        }
    }
}
