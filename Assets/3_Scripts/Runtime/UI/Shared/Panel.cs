using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles transitioning ui panels in/out
public class Panel : MonoBehaviour
{
    // Initial wait
    public float delay = 0f;

    // Perform in reverse
    protected bool _reverse = false;

    // Is showing
    public bool isShowing { get; private set; }
    // Is transitioning
    public bool isTransitioning { get; private set; }
    // Progress
    public float transitionProgress { get; private set; }

    // Rect
    public RectTransform rectTransform { get; private set; }
    // Canvas group
    public CanvasGroup canvasGroup { get; private set; }

    // Get canvas & hide
    protected virtual void Awake()
    {
        // Get rect
        rectTransform = gameObject.GetComponent<RectTransform>();
        // Get canvas group
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Get initial offsets
        if (slideFromRight != null)
        {
            _rightX = slideFromRight.anchoredPosition.x;
        }
        if (slideFromLeft != null)
        {
            _leftX = slideFromLeft.anchoredPosition.x;
        }

        // Transition immediately
        Transition(false, true);
    }
    // Placeholder
    protected virtual void OnDestroy()
    {

    }

    // Show
    public void Show(bool immediately)
    {
        Transition(true, immediately);
    }
    // Hide
    public void Hide(bool immediately)
    {
        Transition(false, immediately);
    }
    // Fade screen
    protected virtual void Transition(bool show, bool immediately)
    {
        // Ignore
        if (show == isShowing && !immediately)
        {
            return;
        }

        // Cancel any tweens
        LeanTween.cancel(gameObject, false);

        // Set showing & get to value
        isShowing = show;
        float to = isShowing ? 1f : 0f;

        // Begin transition
        OnTransitionBegin();

        // Do now
        if (immediately)
        {
            SetTransitionProgress(to);
            OnTransitionComplete();
        }
        // Tween
        else
        {
            // Duration
            float duration = 0f;
            canvasGroup.alpha = 1f;

            // Show animation
            if (isShowing)
            {
                // Wait
                duration = delay;

                // Slide from right
                SlideRight(true, ref duration);
                // Slide from left
                SlideLeft(true, ref duration);
                // Fade & bounce
                Bounce(true, ref duration);
                // Fade in
                FinalFade(true, ref duration);
            }
            // Hide animation
            else if (_reverse)
            {
                // Fade out
                FinalFade(false, ref duration);
                // Fade out & bounce
                Bounce(false, ref duration);
                // Slide to left
                SlideLeft(false, ref duration);
                // Slide to right
                SlideRight(false, ref duration);
            }
            // Fade all
            else
            {
                LeanTween.value(gameObject, canvasGroup.alpha, 0f, fadeDuration).setEase(fadeEase).setOnUpdate(delegate(float p)
                {
                    canvasGroup.alpha = p;
                });
                duration += fadeDuration;
            }

            // Tween for on complete callback
            LeanTween.value(gameObject, SetTransitionProgress, transitionProgress, to, duration).setOnComplete(OnTransitionComplete);
        }
    }

    // Transition begin
    protected virtual void OnTransitionBegin()
    {
        // Begin
        isTransitioning = true;

        // Show
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }
    // Set progress
    protected virtual void SetTransitionProgress(float progress)
    {
        transitionProgress = progress;
    }
    // Transition complete
    protected virtual void OnTransitionComplete()
    {
        // Done
        isTransitioning = false;

        // Cancel all
        LeanTween.cancel(gameObject, false);

        // Slide from right
        if (slideFromRight != null)
        {
            SetX(slideFromRight, _rightX + (isShowing ? 0f : rectTransform.rect.width));
        }
        // Slide from left
        if (slideFromLeft != null)
        {
            SetX(slideFromLeft, _leftX - (isShowing ? 0f : rectTransform.rect.width));
        }
        // Set alpha
        if (bouncers != null && bouncers.Length > 0f)
        {
            foreach (CanvasGroup bouncer in bouncers)
            {
                bouncer.alpha = isShowing ? 1f : 0f;
                bouncer.transform.localScale = Vector3.one * bounceScaleEnd;
            }
        }
        // Set showing
        if (fader != null)
        {
            fader.alpha = isShowing ? 1f : 0f;
        }
        // Set canvas
        canvasGroup.alpha = isShowing ? 1f : 0f;

        // Disable if hiding
        if (gameObject.activeSelf && !isShowing)
        {
            gameObject.SetActive(false);
        }
    }

    #region SLIDERS
    [Header("Slide Animation")]
    // Sliders
    public RectTransform slideFromRight;
    public RectTransform slideFromLeft;

    // Animation values
    public const float slideDuration = 0.4f;
    public const LeanTweenType slideInEase = LeanTweenType.easeOutQuad;
    public const LeanTweenType slideOutEase = LeanTweenType.easeInQuad;

    // Slide initials
    private float _leftX;
    private float _rightX;

    // Slide from right
    private void SlideRight(bool toIn, ref float duration)
    {
        if (slideFromRight != null)
        {
            float from = toIn ? _rightX + rectTransform.rect.width : _rightX;
            float to = toIn ? _rightX : _rightX + rectTransform.rect.width;
            LeanTweenType ease = toIn ? slideInEase : slideOutEase;
            SetX(slideFromRight, from);
            LeanTween.value(gameObject, from, to, slideDuration).setEase(ease).setDelay(duration).setOnUpdate(delegate (float p)
            {
                SetX(slideFromRight, p);
            });
            duration += slideDuration;
        }
    }
    // Slide from left
    private void SlideLeft(bool toIn, ref float duration)
    {
        if (slideFromLeft != null)
        {
            float from = toIn ? _leftX - rectTransform.rect.width : _leftX;
            float to = toIn ? _leftX : _leftX - rectTransform.rect.width;
            LeanTweenType ease = toIn ? slideInEase : slideOutEase;
            SetX(slideFromLeft, from);
            LeanTween.value(gameObject, from, to, slideDuration).setEase(slideInEase).setDelay(duration).setOnUpdate(delegate (float p)
            {
                SetX(slideFromLeft, p);
            });
            duration += slideDuration;
        }
    }
    // Set x
    private void SetX(RectTransform rect, float x)
    {
        Vector2 pos = rect.anchoredPosition;
        pos.x = x;
        rect.anchoredPosition = pos;
    }
    #endregion

    #region BOUNCERS
    [Header("Bounce Animation")]
    public CanvasGroup[] bouncers;

    // Values
    public const float bounceScaleStart = 0.7f;
    public const float bounceScaleEnd = 1.0f;
    public const float bounceStaggerDelay = 0.15f;
    public const LeanTweenType fadeBounceEase = LeanTweenType.easeOutBack;

    // Bounce
    private void Bounce(bool toIn, ref float duration)
    {
        if (bouncers != null && bouncers.Length > 0)
        {
            float start = duration;
            foreach (CanvasGroup bouncer in bouncers)
            {
                if (bouncer != null)
                {
                    // Fade
                    float to = toIn ? 1f : 0f;
                    float from = 1f - to;
                    bouncer.alpha = from;
                    LeanTween.value(gameObject, from, to, fadeDuration).setEase(fadeEase).setDelay(start).setOnUpdate(delegate (float p)
                    {
                        bouncer.alpha = p;
                    });

                    // Bounce
                    from = toIn ? bounceScaleStart : bounceScaleEnd;
                    to = toIn ? bounceScaleEnd : bounceScaleStart;
                    bouncer.transform.localScale = Vector3.one * from;
                    LeanTween.value(gameObject, from, to, fadeDuration).setEase(fadeBounceEase).setDelay(start).setOnUpdate(delegate (float p)
                    {
                        bouncer.transform.localScale = Vector3.one * p;
                    });
                }
                start += bounceStaggerDelay;
            }
            duration = start - bounceStaggerDelay + fadeDuration;
        }
    }
    #endregion

    #region FADE
    [Header("Fade Animation")]
    public float fadeDelay = 0f;
    public const float fadeDuration = 0.3f;
    public const LeanTweenType fadeEase = LeanTweenType.linear;
    public CanvasGroup fader;


    // Fade items in
    private void FinalFade(bool toIn, ref float duration)
    {
        if (fader != null)
        {
            float to = toIn ? 1f : 0f;
            float from = 1f - to;
            fader.alpha = from;
            duration += fadeDelay;
            LeanTween.value(gameObject, from, to, fadeDuration).setEase(fadeEase).setDelay(duration).setOnUpdate(delegate (float p)
            {
                fader.alpha = p;
            });
            duration += fadeDuration;
        }
    }
    #endregion
}
