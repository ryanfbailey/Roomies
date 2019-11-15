using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameIntroPanel : MonoBehaviour
{
    // UI Items
    private CanvasGroup _group;
    public CanvasGroup readyGroup;
    public CanvasGroup seekGroup;

    // Fade animations
    public const float fadeDuration = 0.2f;
    public const LeanTweenType showEase = LeanTweenType.easeOutQuad;
    public const LeanTweenType hideEase = LeanTweenType.easeInQuad;

    // Ready animation
    public const float readyShowDelay = 4f;
    public const float readyHideDelay = 1f;

    // Scale animation
    public const float seekShowDelay = 0f;
    public const float seekShowScale = 1.5f;
    public const float seekShowScaleDuration = 0.25f;
    public LeanTweenType seekShowScaleEase = LeanTweenType.easeOutQuad;
    public const float seekHideDelay = 0.25f;

    // Add delegates
    private void Awake()
    {
        _group = gameObject.GetComponent<CanvasGroup>();
        _group.alpha = 1f;
        gameObject.SetActive(false);
        GameManager.onGameStateChange += OnGameStateChanged;
    }
    // Remove delegates
    private void OnDestroy()
    {
        GameManager.onGameStateChange -= OnGameStateChanged;
    }

    // Begin process on intro
    protected virtual void OnGameStateChanged(GameState newState, bool immediately)
    {
        if (newState == GameState.GameIntro)
        {
            ShowReady();
        }
    }

    // Fade ready in
    private void ShowReady()
    {
        // Start
        gameObject.SetActive(true);

        // Cancel animations
        LeanTween.cancel(gameObject, false);

        // Hide seek
        seekGroup.gameObject.SetActive(false);

        // Show ready
        readyGroup.gameObject.SetActive(true);
        readyGroup.alpha = 0f;
        LeanTween.value(gameObject, 0f, 1f, fadeDuration).setEase(showEase).setDelay(readyShowDelay).setOnComplete(HideReady).setOnUpdate(delegate(float p)
        {
            readyGroup.alpha = p;
        });
    }

    // Fade ready out
    private void HideReady()
    {
        // Cancel animations
        LeanTween.cancel(gameObject, false);

        // Hide ready
        readyGroup.alpha = 1f;
        LeanTween.value(gameObject, 1f, 0f, fadeDuration).setEase(hideEase).setDelay(readyHideDelay).setOnComplete(ShowSeek).setOnUpdate(delegate (float p)
        {
            readyGroup.alpha = p;
        });
    }

    // Show seel
    private void ShowSeek()
    {
        // Cancel all animations
        LeanTween.cancel(gameObject, false);

        // Hide ready
        readyGroup.gameObject.SetActive(false);

        // Show seek
        seekGroup.gameObject.SetActive(true);
        seekGroup.alpha = 0f;
        LeanTween.value(gameObject, 0f, 1f, fadeDuration).setEase(showEase).setDelay(seekShowDelay).setOnUpdate(delegate (float p)
        {
            seekGroup.alpha = p;
        });

        // Scale seek
        seekGroup.transform.localScale = Vector3.one * seekShowScale;
        LeanTween.value(gameObject, seekShowScale, 1f, seekShowScaleDuration).setEase(seekShowScaleEase).setDelay(seekShowDelay).setOnComplete(HideSeek).setOnUpdate(delegate (float p)
        {
            seekGroup.transform.localScale = Vector3.one * p;
        });
    }

    //
    private void HideSeek()
    {
        // Cancel all animations
        LeanTween.cancel(gameObject, false);

        // Begin play
        GameManager.instance.SetState(GameState.GamePlay);

        // Hide seek
        seekGroup.alpha = 1f;
        LeanTween.value(gameObject, 1f, 0f, fadeDuration).setEase(hideEase).setOnComplete(Done).setDelay(seekHideDelay).setOnUpdate(delegate (float p)
        {
            seekGroup.alpha = p;
        });
    }

    // Done
    private void Done()
    {
        // Cancel all animations
        LeanTween.cancel(gameObject, false);

        // Done
        gameObject.SetActive(false);
    }
}
