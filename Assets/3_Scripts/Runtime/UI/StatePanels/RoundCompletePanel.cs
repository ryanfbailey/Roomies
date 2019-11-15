using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoundCompletePanel : GameStatePanel
{
    #region SETUP
    // Add btn delegates
    private void OnEnable()
    {
        SetInterface(GameManager.instance.lastWinner);
    }
    // Remove btn delegates
    private void OnDisable()
    {
        StopAllCoroutines();
    }
    // Show if results panel
    protected override bool ShouldShow()
    {
        return GameManager.instance.gameState == GameState.RoundComplete;
    }
    #endregion

    #region ANIMATION
    [Header("Snatched")]
    // Snatched ui
    public Image snatchedIcon;
    public CanvasGroup snatchedGroup;
    // Snatched animation
    public float snatchedScaleStart = 0.5f;
    public float snatchedScaleDelay = 0f;
    public float snatchedScaleDuration = 0.5f;
    public LeanTweenType snatchedScaleEase = LeanTweenType.easeOutBack;

    [Header("Score Slide")]
    // Score overlay
    public ScoreOverlayPanel scoreOverlay;
    // Score delay
    public float scoreDelay = 0.5f;

    [Header("Plus One")]
    // Plus One UI
    public Image plusOneIcon;
    public CanvasGroup plusOneGroup;
    // Plus one fade animation
    public Vector2 plusOneOffsetStart = new Vector2(0f, 20f);
    public Vector2 plusOneOffsetEnd = new Vector2(0f, 0f);
    public float plusOneOffsetDelay = 0.5f;
    public float plusOneFadeDuration = 0.2f;
    // X animation
    public float plusOneAnimXDelay = 0.2f;
    public float plusOneAnimXDuration = 0.8f;
    public LeanTweenType plusOneXOffsetEase = LeanTweenType.easeInSine;
    // Y animation
    public float plusOneAnimYOffset = 150f;
    public float plusOneAnimYUpDuration = 0.4f;
    public LeanTweenType plusOneAnimYUpEase = LeanTweenType.linear;
    public float plusOneAnimYDownDuration = 0.6f;
    public LeanTweenType plusOneAnimYDownEase = LeanTweenType.easeInQuad;

    [Header("Score Bounce")]
    public float bounceScale = 1.5f;
    public float bounceDelay = 1f;
    public float bounceUpDuration = 1f;
    public float bounceDownDuration = 0.3f;
    public LeanTweenType bounceUpEase = LeanTweenType.easeOutQuad;
    public LeanTweenType bounceDownEase = LeanTweenType.easeInQuad;

    [Header("End")]
    public float finalWait = 0.5f;

    // Refresh interface
    protected virtual void SetInterface(int playerIndex)
    {
        // Get tint
        Color tint = GameManager.instance.GetPlayerColor(playerIndex);

        // Reset snatched
        LeanTween.cancel(snatchedGroup.gameObject, false);
        snatchedGroup.alpha = 0f;
        snatchedIcon.color = tint;
        snatchedGroup.transform.localScale = Vector3.one * snatchedScaleStart;

        // Reset plus one
        LeanTween.cancel(plusOneGroup.gameObject, false);
        plusOneGroup.alpha = 0f;
        plusOneIcon.color = tint;
        RectTransform plusRect = plusOneGroup.GetComponent<RectTransform>();
        Vector3 worldPos = Vector3.zero;
        foreach (GridProp prop in GridManager.instance.props)
        {
            if (prop != null && prop.data != null && prop.data.isWinProp)
            {
                worldPos = prop.transform.position;
                break;
            }
        }
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        Vector2 plusStart = new Vector2(screenPos.x, screenPos.y);
        plusStart *= 1080f / (float)Screen.height;
        plusStart += plusOneOffsetStart;
        plusRect.anchoredPosition = plusStart;

        // Get plus one end
        RectTransform parent = scoreOverlay.playerScoreContainer;
        PlayerScoreCell scoreCell = scoreOverlay.playerScoreContainer.GetChild(playerIndex).GetComponent<PlayerScoreCell>();
        Vector2 plusEnd = scoreCell.playerPosition;
        plusEnd += new Vector2(scoreCell.transform.localScale.x < 1 ? parent.rect.width - plusOneOffsetEnd.x : plusOneOffsetEnd.x, plusOneOffsetEnd.y + parent.rect.height);

        // Play animation
        StopAllCoroutines();
        StartCoroutine(RoundOver(scoreCell, plusRect, plusStart, plusEnd));
    }
    // Animate
    private IEnumerator RoundOver(PlayerScoreCell scoreCell, RectTransform plusRect, Vector2 plusStart, Vector2 plusEnd)
    {
        // Snatched animation
        yield return new WaitForSeconds(snatchedScaleDelay);
        LeanTween.value(snatchedGroup.gameObject, 0f, 1f, fadeDuration).setEase(fadeEase).setOnUpdate(delegate (float alpha)
        {
            snatchedGroup.alpha = alpha;
        });
        LeanTween.value(snatchedGroup.gameObject, snatchedScaleStart, 1f, snatchedScaleDuration).setEase(snatchedScaleEase).setOnUpdate(delegate (float scale)
        {
            snatchedGroup.transform.localScale = Vector3.one * scale;
        });

        // Score animation
        yield return new WaitForSeconds(scoreDelay);
        scoreOverlay.Animate(false);

        // Plus one animation
        yield return new WaitForSeconds(plusOneOffsetDelay);
        LeanTween.value(plusOneGroup.gameObject, 0f, 1f, plusOneFadeDuration).setEase(fadeEase).setOnUpdate(delegate (float alpha)
        {
            plusOneGroup.alpha = alpha;
        });
        LeanTween.value(plusOneGroup.gameObject, plusStart.x, plusEnd.x, plusOneAnimXDuration).setEase(plusOneXOffsetEase).setDelay(plusOneAnimXDelay).setOnUpdate(delegate (float val)
        {
            Vector2 pos = plusRect.anchoredPosition;
            pos.x = val;
            plusRect.anchoredPosition = pos;
        });
        LeanTween.value(plusOneGroup.gameObject, plusStart.y, plusStart.y + plusOneAnimYOffset, plusOneAnimYUpDuration).setEase(plusOneAnimYUpEase).setOnUpdate(delegate (float val)
        {
            Vector2 pos = plusRect.anchoredPosition;
            pos.y = val;
            plusRect.anchoredPosition = pos;
        });
        yield return new WaitForSeconds(plusOneAnimYUpDuration);
        LeanTween.value(plusOneGroup.gameObject, plusStart.y + plusOneAnimYOffset, plusEnd.y, plusOneAnimYDownDuration).setEase(plusOneAnimYDownEase).setOnUpdate(delegate (float val)
        {
            Vector2 pos = plusRect.anchoredPosition;
            pos.y = val;
            plusRect.anchoredPosition = pos;
        });
        LeanTween.value(plusOneGroup.gameObject, 1f, 0f, plusOneFadeDuration).setEase(fadeEase).setDelay(plusOneAnimYDownDuration - plusOneFadeDuration).setOnUpdate(delegate (float alpha)
        {
            plusOneGroup.alpha = alpha;
        });
        yield return new WaitForSeconds(plusOneAnimYDownDuration);


        // Bounce animation
        yield return new WaitForSeconds(bounceDelay);
        _cell = scoreCell;
        scoreCell.LayoutScore();
        LeanTween.value(gameObject, 1f, bounceScale, bounceUpDuration).setEase(bounceUpEase).setOnUpdate(SetScoreScale);
        yield return new WaitForSeconds(bounceUpDuration);
        LeanTween.value(gameObject, bounceScale, 1f, bounceDownDuration).setEase(bounceDownEase).setOnUpdate(SetScoreScale);
        yield return new WaitForSeconds(bounceDownDuration);
        _cell = null;

        // Final wait
        yield return new WaitForSeconds(finalWait);

        // Next round
        if (!GameManager.instance.isEndOfMatch)
        {
            GameManager.instance.PlayNewRound();
        }
        // Show results
        else
        {
            GameManager.instance.SetState(GameState.MatchComplete);
        }
    }
    // Set score scale
    private PlayerScoreCell _cell;
    private void SetScoreScale(float scale)
    {
        if (_cell != null)
        {
            _cell.scoreCurrentLabel.transform.localScale = Vector3.one * scale;
        }
    }
    #endregion
}
