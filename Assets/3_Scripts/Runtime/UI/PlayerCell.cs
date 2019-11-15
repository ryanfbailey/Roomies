using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCell : MonoBehaviour
{
    #region SETUP
    // Player UI
    [Header("Player UI")]
    public Image playerLabelFrame;
    public TextMeshProUGUI playerLabel;
    private Color _playerNameDefaultColor = new Color(101f / 255f, 135f / 255f, 158f / 255f, 1f);
    private Color _playerNameReadyColor = new Color(1f, 1f, 1f, 0.75f);
    public Image playerReadyIcon;

    // Character
    [Header("Character UI")]
    public TextMeshProUGUI characterNameLabel;
    public Image[] characterArrows;
    public RawImage characterIcon;
    private Color _characterIconDefaultTint = Color.white;
    private Color _characterIconReadyTint = new Color(1f, 1f, 1f, 0.5f);

    // Buttons
    [Header("Buttons")]
    // Select button
    public RoomiesButton characterSelectButton;
    // Remove player button
    public RoomiesButton playerRemoveButton;
    // Ready player button
    public RoomiesButton playerReadyButton;
    // Reselect button
    public RoomiesButton playerBackButton;

    // Shadows
    [Header("Shadows")]
    // View
    public Image shadowView;
    // One
    public Sprite shadowOneBtn;
    // Two
    public Sprite shadowTwoBtn;
    // Ready
    public Sprite shadowReady;

    // Add delegates
    private void Awake()
    {
        GameManager.onPlayerButtonClick += ButtonClicked;
        playerRemoveButton.onSubmit += RemoveClicked;
        playerReadyButton.onSubmit += ReadyClicked;
        playerBackButton.onSubmit += ChangeClicked;
    }
    // Reset
    private void Start()
    {
        SetReady(false);
        SetInputButton(PlayerInputButton.CharSelect);
    }
    // Remove delegates
    private void OnDestroy()
    {
        GameManager.onPlayerButtonClick -= ButtonClicked;
        playerRemoveButton.onSubmit -= RemoveClicked;
        playerReadyButton.onSubmit -= ReadyClicked;
        playerBackButton.onSubmit -= ChangeClicked;
    }
    #endregion

    #region READY
    //
    private bool _isReady = false;

    // Ready
    private void ReadyClicked()
    {
        SetReady(true);
    }
    // Not ready
    private void ChangeClicked()
    {
        SetReady(false);
    }
    // Set ready
    public void SetReady(bool toReady)
    {
        if (playerIndex >= 0 && GameManager.instance.gameState == GameState.PlayerSetup)
        {
            // Set ready
            _isReady = toReady;
            GameManager.instance.players[playerIndex].ready = _isReady;

            // Set frame
            if (playerLabelFrame != null)
            {
                playerLabelFrame.color = _isReady ? GameManager.instance.GetPlayerColor(playerIndex) : Color.white;
            }
            // Set player color
            if (playerLabel != null)
            {
                playerLabel.color = _isReady ? _playerNameReadyColor : _playerNameDefaultColor;
            }
            // Set player ready icon
            if (playerReadyIcon != null)
            {
                playerReadyIcon.gameObject.SetActive(_isReady);
            }
            // Set character icon tint
            if (characterIcon != null)
            {
                characterIcon.color = _isReady ? _characterIconReadyTint : _characterIconDefaultTint;
            }
            // Set arrows
            if (characterArrows != null)
            {
                foreach (Image characterArrow in characterArrows)
                {
                    characterArrow.gameObject.SetActive(!_isReady);
                }
            }
            // Set button state
            if (characterSelectButton != null)
            {
                characterSelectButton.SetDisabled(_isReady);
            }
            // Adjust remove button
            if (playerRemoveButton != null)
            {
                playerRemoveButton.gameObject.SetActive(playerIndex > 0 && !_isReady);
            }
            // Adjust ready button
            if (playerReadyButton != null)
            {
                playerReadyButton.gameObject.SetActive(!_isReady);
                Vector2 pos = playerReadyButton.rectTransform.anchoredPosition;
                pos.x = playerRemoveButton.rectTransform.anchoredPosition.x + (playerIndex == 0 ? 0f : playerRemoveButton.rectTransform.rect.width);
                playerReadyButton.rectTransform.anchoredPosition = pos;
                float width = playerBackButton.rectTransform.rect.width + (playerIndex == 0 ? 0f : -playerRemoveButton.rectTransform.rect.width);
                playerReadyButton.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
            // Adjust back button
            if (playerBackButton != null)
            {
                playerBackButton.gameObject.SetActive(_isReady);
            }
            // Adjust shadow
            if (shadowView != null)
            {
                if (_isReady)
                {
                    shadowView.sprite = shadowReady;
                }
                else if (playerIndex == 0)
                {
                    shadowView.sprite = shadowOneBtn;
                }
                else
                {
                    shadowView.sprite = shadowTwoBtn;
                }
            }

            // Back to character select
            if (!_isReady)
            {
                SetInputButton(PlayerInputButton.CharSelect);
            }
            else if (playerIndex == 0)
            {
                SetInputButton(PlayerInputButton.Start);
            }
            else
            {
                SetInputButton(PlayerInputButton.PlayerBack);
            }
        }
    }
    #endregion

    #region PLAYER
    // Player index
    public int playerIndex { get; private set; }

    // Remove player
    private void RemoveClicked()
    {
        if (playerIndex >= 1)
        {
            GameManager.instance.RemovePlayer(playerIndex);
        }
    }
    // Refresh interface
    public void SetPlayer(int newPlayerIndex)
    {
        // Set player index
        playerIndex = newPlayerIndex;

        // Set player name
        GamePlayer player = GameManager.instance.GetPlayer(playerIndex);
        if (player == null)
        {
            return;
        }

        // Set name
        if (playerLabel != null)
        {
            playerLabel.text = "PLAYER " + (playerIndex + 1);
        }

        // Set character
        int charIndex = player.characterIndex;
        if (charIndex < 0 || charIndex >= GameManager.instance.gameData.characters.Length)
        {
            charIndex = playerIndex % GameManager.instance.gameData.characters.Length;
        }
        SetCharacter(player, charIndex);

        // Set ready
        SetReady(player.ready);
    }
    // Set character
    protected virtual void SetCharacter(GamePlayer player, int toIndex)
    {
        // Ignore invalid player
        if (player == null)
        {
            return;
        }
        // Ignore invalid character
        if (toIndex < 0 || toIndex >= GameManager.instance.gameData.characters.Length)
        {
            return;
        }

        // Get character & set index
        GameCharacter character = GameManager.instance.gameData.characters[toIndex];
        player.characterIndex = toIndex;

        // Set character name
        if (characterNameLabel != null)
        {
            characterNameLabel.text = character.characterName;
        }
        // Set character texture
        if (characterIcon != null)
        {
            SetCharacterIcon(characterIcon, character.characterIconTexture);
        }
    }
    // Set icon
    public static void SetCharacterIcon(RawImage imageView, Texture2D charSprite)
    {
        // Set icon
        imageView.texture = charSprite;

        // Adjust layout
        if (charSprite != null)
        {
            Rect parentSize = imageView.transform.parent.GetComponent<RectTransform>().rect;
            float ratio = parentSize.width / parentSize.height;
            float newRatio = charSprite.width / charSprite.height;
            if (ratio >= newRatio)
            {
                imageView.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentSize.width);
                imageView.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parentSize.width / newRatio);
            }
            else
            {
                imageView.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parentSize.height);
                imageView.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentSize.height * newRatio);
            }
        }
    }
    #endregion

    #region INPUT
    // Current input row
    public PlayerInputButton inputButton { get; private set; }
    // Input button
    public enum PlayerInputButton
    {
        CharSelect,
        PlayerRemove,
        PlayerReady,
        PlayerBack,
        Home,
        Start
    }

    // Input
    protected void ResetInput()
    {
        if (!_isReady)
        {
            SetInputButton(PlayerInputButton.CharSelect);
        }
        else
        {
            SetInputButton(PlayerInputButton.PlayerBack);
        }
    }
    // Set input button
    protected virtual void SetInputButton(PlayerInputButton newButton)
    {
        // Set new index
        inputButton = newButton;

        // Update buttons
        characterSelectButton.disabledColor = GameManager.instance.GetPlayerColor(playerIndex);
        characterSelectButton.SetInputPlayer(inputButton == PlayerInputButton.CharSelect ? playerIndex :  -1);
        playerRemoveButton.SetInputPlayer(inputButton == PlayerInputButton.PlayerRemove ? playerIndex : -1);
        playerReadyButton.SetInputPlayer(inputButton == PlayerInputButton.PlayerReady ? playerIndex : -1);
        playerBackButton.SetInputPlayer(inputButton == PlayerInputButton.PlayerBack ? playerIndex : -1);
    }

    // Button clicked
    protected virtual void ButtonClicked(int newPlayerIndex, string buttonID)
    {
        if (playerIndex == newPlayerIndex && gameObject.activeInHierarchy && GameManager.instance.gameState == GameState.PlayerSetup)
        {
            // SUBMIT
            if (string.Equals(buttonID, GameManager.SUBMIT_INPUT_KEY) || string.Equals(buttonID, GameManager.PAUSE_INPUT_KEY))
            {
                switch (inputButton)
                {
                    case PlayerInputButton.CharSelect:
                        SetInputButton(PlayerInputButton.PlayerReady);
                        break;
                    case PlayerInputButton.PlayerRemove:
                        RemoveClicked();
                        break;
                    case PlayerInputButton.PlayerReady:
                        SetReady(true);
                        break;
                    case PlayerInputButton.PlayerBack:
                        SetReady(false);
                        break;
                    case PlayerInputButton.Home:
                    case PlayerInputButton.Start:
                        break;
                }
            }
            // UP
            else if (string.Equals(buttonID, GameManager.VERTICAL_POS_INPUT_KEY))
            {
                switch (inputButton)
                {
                    case PlayerInputButton.CharSelect:
                    case PlayerInputButton.PlayerBack:
                        break;
                    case PlayerInputButton.PlayerRemove:
                    case PlayerInputButton.PlayerReady:
                        SetInputButton(PlayerInputButton.CharSelect);
                        break;
                    case PlayerInputButton.Home:
                    case PlayerInputButton.Start:
                        SetInputButton(_isReady ? PlayerInputButton.PlayerBack : PlayerInputButton.PlayerReady);
                        break;
                }
            }
            // DOWN
            else if (string.Equals(buttonID, GameManager.VERTICAL_NEG_INPUT_KEY))
            {
                switch (inputButton)
                {
                    case PlayerInputButton.CharSelect:
                        SetInputButton(PlayerInputButton.PlayerReady);
                        break;
                    case PlayerInputButton.PlayerRemove:
                    case PlayerInputButton.PlayerReady:
                    case PlayerInputButton.PlayerBack:
                        if (playerIndex == 0)
                        {
                            if (!_isReady)
                            {
                                SetInputButton(PlayerInputButton.Home);
                            }
                            else
                            {
                                SetInputButton(PlayerInputButton.Start);
                            }
                        }
                        break;
                    case PlayerInputButton.Home:
                    case PlayerInputButton.Start:
                        break;
                }
            }
            // LEFT
            else if (string.Equals(buttonID, GameManager.HORIZONTAL_NEG_INPUT_KEY))
            {
                switch (inputButton)
                {
                    case PlayerInputButton.CharSelect:
                        IncrementPlayer(playerIndex, false);
                        break;
                    case PlayerInputButton.PlayerReady:
                        if (playerIndex != 0)
                        {
                            SetInputButton(PlayerInputButton.PlayerRemove);
                        }
                        break;
                    case PlayerInputButton.Start:
                        if (playerIndex == 0)
                        {
                            SetInputButton(PlayerInputButton.Home);
                        }
                        break;
                    case PlayerInputButton.PlayerRemove:
                    case PlayerInputButton.PlayerBack:
                    case PlayerInputButton.Home:
                        break;
                }
            }
            // RIGHT
            else if (string.Equals(buttonID, GameManager.HORIZONTAL_POS_INPUT_KEY))
            {
                switch (inputButton)
                {
                    case PlayerInputButton.CharSelect:
                        IncrementPlayer(playerIndex, true);
                        break;
                    case PlayerInputButton.PlayerRemove:
                        if (playerIndex != 0)
                        {
                            SetInputButton(PlayerInputButton.PlayerReady);
                        }
                        break;
                    case PlayerInputButton.Home:
                        if (playerIndex == 0 && _isReady)
                        {
                            SetInputButton(PlayerInputButton.Start);
                        }
                        break;
                    case PlayerInputButton.PlayerReady:
                    case PlayerInputButton.PlayerBack:
                    case PlayerInputButton.Start:
                        break;
                }
            }
        }
    }
    // Increment player
    private void IncrementPlayer(int pIndex, bool forward)
    {
        GamePlayer player = GameManager.instance.GetPlayer(pIndex);
        player.characterIndex += (forward ? 1 : -1);
        if (player.characterIndex < 0)
        {
            player.characterIndex = GameManager.instance.gameData.characters.Length - 1;
        }
        else if (player.characterIndex >= GameManager.instance.gameData.characters.Length)
        {
            player.characterIndex = 0;
        }
        SetCharacter(player, player.characterIndex);
    }
    #endregion
}
