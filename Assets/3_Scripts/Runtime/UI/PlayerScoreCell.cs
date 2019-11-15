using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerScoreCell : MonoBehaviour
{
    #region LAYOUT
    [Header("UI")]
    // Name label
    public TextMeshProUGUI characterName;
    // Icon image
    public RawImage characterIcon;
    // Tintables
    public Image[] characterTintables;

    // Current score
    public TextMeshProUGUI scoreCurrentLabel;
    // Max score
    public TextMeshProUGUI scoreMaxLabel;

    // Current player & position
    public int playerIndex { get; private set; }
    public Vector2 playerPosition { get; private set; }

    // On awake
    private void Awake()
    {
        playerIndex = -1;
        _rect = gameObject.GetComponent<RectTransform>();
        _group = gameObject.GetComponent<CanvasGroup>();
        if (_group == null)
        {
            _group = gameObject.AddComponent<CanvasGroup>();
        }
    }
    // Remove texture
    private void OnDestroy()
    {
        characterIcon.texture = null;
    }

    // Layout interface
    public void LayoutInterface(int newPlayer)
    {
        // Set interface
        playerIndex = newPlayer;

        // Get player
        GamePlayer player = null;
        if (playerIndex >= 0 && GameManager.instance.players != null && playerIndex < GameManager.instance.players.Count)
        {
            player = GameManager.instance.players[playerIndex];
        }
        // Get character
        GameCharacter character = null;
        if (player != null && player.characterIndex >= 0 && player.characterIndex < GameManager.instance.gameData.characters.Length)
        {
            character = GameManager.instance.gameData.characters[player.characterIndex];
        }

        // Set character icon & tints
        characterName.text = character == null ? "???" : character.characterName;
        PlayerCell.SetCharacterIcon(characterIcon, character == null ? null : character.characterIconTexture);
        foreach (Image t in characterTintables)
        {
            t.color = GameManager.instance.GetPlayerColor(playerIndex);
        }

        // Set Default Score
        LayoutScore();

        // Set animation items
        playerPosition = _rect.anchoredPosition;
        Animate(true);
    }

    // Layout scores
    public void LayoutScore()
    {
        // Ensure player exists
        if (GameManager.instance.players == null || playerIndex < 0 || playerIndex >= GameManager.instance.players.Count)
        {
            return;
        }

        // Get player
        GamePlayer player = GameManager.instance.players[playerIndex];

        // Set score labels
        scoreCurrentLabel.text = player.score.ToString("0");
        scoreMaxLabel.text = "/" + GameManager.instance.gameData.roundsPerMatch.ToString("0");
    }
    #endregion

    #region ANIMATIONS
    [Header("Animation")]
    // Fade animation
    public float fadeOutAlpha = 0.5f;
    public float fadeDuration = 0.3f;
    public LeanTweenType fadeEase = LeanTweenType.linear;

    // Slide animation
    public float slideOffset = 20f;
    public float slideDuration = 0.3f;
    public LeanTweenType slideEase = LeanTweenType.easeOutQuint;

    // Helpers
    private CanvasGroup _group;
    private RectTransform _rect;

    // Update score
    public void Animate(bool immediately)
    {
        // Cancel previous
        LeanTween.cancel(gameObject);

        // Get values
        float toAlpha = 1f;
        float toX = 0f;

        // Results
        if (GameManager.instance.gameState == GameState.RoundComplete || GameManager.instance.gameState == GameState.MatchComplete)
        {
            if (playerIndex != GameManager.instance.lastWinner)
            {
                toAlpha = fadeOutAlpha;
            }
            else
            {
                toX = transform.localScale.x * slideOffset;
            }
        }

        // Immediate
        if (immediately)
        {
            SetAlpha(toAlpha);
            SetPosition(toX);
        }
        // Tween
        else
        {
            // Fade animation
            float fromAlpha = _group.alpha;
            LeanTween.value(gameObject, fromAlpha, toAlpha, fadeDuration).setEase(fadeEase).setOnUpdate(SetAlpha);
            // Slide animation
            float fromX = _rect.anchoredPosition.x - playerPosition.x;
            LeanTween.value(gameObject, fromX, toX, slideDuration).setEase(slideEase).setOnUpdate(SetPosition);
        }
    }
    // Set alpha
    private void SetAlpha(float newAlpha)
    {
        _group.alpha = newAlpha;
    }
    // Set position
    private void SetPosition(float newX)
    {
        Vector2 newPos = playerPosition;
        newPos.x += newX;
        _rect.anchoredPosition = newPos;
    }
    #endregion
}
