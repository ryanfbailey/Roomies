using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleEffect : MonoBehaviour
{
    #region EFFECT
    [Header("Effect")]
    // Scale animation
    public float scaleDelay = 0.0f;
    public float scaleDuration = 0.6f;
    public float scaleStart = 0.1f;
    public float scaleEnd = 0.4f;
    public LeanTweenType scaleEase = LeanTweenType.easeOutQuad;

    // Fade animation
    public float fadeDelay = 0.6f;
    public float fadeDuration = 0.2f;
    public float fadeStart = 1f;
    public float fadeEnd = 0f;
    public LeanTweenType fadeEase = LeanTweenType.linear;

    // Reset
    private void Awake()
    {
        ResetEffect(false);
    }
    // Reset
    private void OnEnable()
    {
        ResetEffect(true);
    }
    // Reset
    private void ResetEffect(bool play)
    {
        // Stop animations
        LeanTween.cancel(gameObject);

        // Reset alpha & scale
        SetAlpha(fadeStart);
        SetScale(scaleStart);

        // Play
        if (play)
        {
            // Randomize
            RandomText();
            // Scale
            LeanTween.value(gameObject, SetScale, scaleStart, scaleEnd, scaleDuration).setEase(scaleEase).setDelay(scaleDelay);
            // Fade
            LeanTween.value(gameObject, SetAlpha, fadeStart, fadeEnd, fadeDuration).setEase(fadeEase).setDelay(fadeDelay).setOnComplete(Done);
        }
    }
    // Set scale
    private void SetScale(float scale)
    {
        container.localScale = Vector3.one * scale;
    }
    // Set alpha for both
    private void SetAlpha(float alpha)
    {
        Color col = textImage.color;
        col.a = alpha;
        textImage.color = col;
        col = bgImage.color;
        col.a = alpha;
        bgImage.color = col;
    }
    // Done
    private void Done()
    {
        gameObject.SetActive(false);
    }
    #endregion

    #region UI
    [Header("Sprites")]
    // Renderers
    public Sprite[] textSprites;
    public Image textImage;
    public Image bgImage;
    public Transform container;

    // Set bg color
    public void SetColor(Color newColor)
    {
        Color col = newColor;
        col.a = bgImage.color.a;
        bgImage.color = col;
    }
    // Randomize
    private void RandomText()
    {
        textImage.sprite = textSprites[Random.Range(0, textSprites.Length)];
    }
    #endregion
}
