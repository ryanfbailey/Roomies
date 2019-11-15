using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatchCompletePanel : GameStatePanel
{
    #region SETUP
    // Add btn delegates
    private void OnEnable()
    {
        homeButton.onSubmit += HomeClick;
        charSelButton.onSubmit += CharSelClick;
        restartButton.onSubmit += Restart;
        SetInterface(GameManager.instance.lastWinner);
    }
    // Remove btn delegates
    private void OnDisable()
    {
        homeButton.onSubmit -= HomeClick;
        charSelButton.onSubmit -= CharSelClick;
        restartButton.onSubmit -= Restart;
    }
    // Show if match complete panel
    protected override bool ShouldShow()
    {
        return GameManager.instance.gameState == GameState.MatchComplete;
    }
    #endregion

    #region LAYOUT
    [Header("Match Complete UI")]
    // Player label
    public TextMeshProUGUI playerLabel;
    // Player border tint
    [Range(0f, 1f)]
    public float playerBorderTint = 0.8706f;

    // Character label
    public TextMeshProUGUI characterLabel;
    // Character icon
    public RawImage characterIcon;
    // Tintables
    public Graphic[] tintables;
    // Borders
    public RawImage[] borders;

    // Refresh interface
    protected virtual void SetInterface(int playerIndex)
    {
        // Set player
        GamePlayer player = GameManager.instance.GetPlayer(playerIndex);
        playerLabel.text = "P" + (playerIndex + 1);

        // Set character
        GameCharacter character = GameManager.instance.gameData.characters[player.characterIndex];
        characterLabel.text = character.characterName.ToUpper();
        PlayerCell.SetCharacterIcon(characterIcon, character.characterIconTexture);

        // Set tints
        Color tint = GameManager.instance.GetPlayerColor(playerIndex);
        foreach (Graphic t in tintables)
        {
            t.color = tint;
        }

        // Set borders
        Texture2D texture = GameManager.instance.GetPlayerBorder(playerIndex);
        foreach (RawImage border in borders)
        {
            border.texture = texture;
            if (texture != null)
            {
                border.color = Color.white;
                border.uvRect = new Rect(0f, 0f, border.rectTransform.rect.width / (float)texture.width, border.rectTransform.rect.height / (float)texture.height);
            }
            else
            {
                border.color = Color.black;
                border.uvRect = new Rect(0f, 0f, 1f, 1f);
            }
        }

        // Set text outline
        tint.r *= playerBorderTint;
        tint.g *= playerBorderTint;
        tint.b *= playerBorderTint;
        playerLabel.fontMaterial.SetColor("_OutlineColor", tint);

        // Set match button
        _player = GameManager.instance.lastWinner;
        SetInputButton(2);
    }
    #endregion

    #region BUTTONS
    [Header("Buttons")]
    // Home button
    public RoomiesButton homeButton;
    // Restart button
    public RoomiesButton charSelButton;
    // Restart button
    public RoomiesButton restartButton;

    // Current button
    private int _player = 0;
    private int _button = 2;
    private void SetInputButton(int newButton)
    {
        _button = newButton;
        homeButton.SetInputPlayer(_button == 0 ? _player : -1);
        charSelButton.SetInputPlayer(_button == 1 ? _player : -1);
        restartButton.SetInputPlayer(_button == 2 ? _player : -1);
    }
    // Play next
    protected override void OnPlayerDirection(int playerIndex, Direction direction)
    {
        if (playerIndex == _player)
        {
            if (direction == Direction.Left)
            {
                int b = _button-1;
                if (b < 0)
                {
                    b = 2;
                }
                SetInputButton(b);
            }
            else if (direction == Direction.Right)
            {
                int b = _button+1;
                if (b > 2)
                {
                    b = 0;
                }
                SetInputButton(b);
            }
        }
    }
    // Play next
    protected override void OnPlayerSelect(int playerIndex)
    {
        if (playerIndex == _player)
        {
            switch (_button)
            {
                case 0:
                    Home();
                    break;
                case 1:
                    CharSelect();
                    break;
                case 2:
                    Restart();
                    break;
            }
        }
    }
    // Home button click
    private void HomeClick()
    {
        Home();
    }
    // Char Sel click
    private void CharSelClick()
    {
        CharSelect();
    }
    // Restart click
    private void RestartClick()
    {
        Restart();
    }
    // Home
    public void Home()
    {
        GameManager.instance.SetState(GameState.Title);
    }
    // Character Select
    public void CharSelect()
    {
        GameManager.instance.SetState(GameState.PlayerSetup);
    }
    // Restart button
    public void Restart()
    {
        GameManager.instance.PlayNewMatch();
    }
    #endregion
}
