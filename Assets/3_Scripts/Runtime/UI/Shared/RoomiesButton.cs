using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum RoomiesButtonType
{
    Blue,
    Magenta,
    Orange,
    Slate
}

public enum RoomiesButtonState
{
    Normal,
    Highlighted,
    Pressed,
    Disabled
}

public class RoomiesButton : MonoBehaviour
{
    #region LIFECYCLE
    [Header("Button UI")]
    // Button image
    public RoomiesButtonType buttonType = (RoomiesButtonType)(-1);
    [HideInInspector] [SerializeField]
    public Sprite normalSprite;
    [HideInInspector] [SerializeField]
    public Sprite pressedSprite;
    private Image _backgroundImage;

    [Header("Content UI")]
    // Label
    public RectTransform content;
    private const float _contentPressY = -5f;
    private float _contentDefaultY;

    // Disable state
    [Header("Disable UI")]
    public Color disabledColor = Color.white;
    public Sprite disabledSprite;

    // Highlight image
    [Header("Highlight UI")]
    [HideInInspector] [SerializeField]
    public Sprite highlightDefaultSprite;
    [HideInInspector] [SerializeField]
    public Sprite highlightPressedSprite;
    private Image _highlight;
    private const float _highlightPadding = 3f;

    // Player ui
    [Header("Player UI")]
    [HideInInspector] [SerializeField]
    public Sprite playerBGSprite;
    private Image _playerOverlay;
    [HideInInspector] [SerializeField]
    public TMP_FontAsset playerFont;
    private TextMeshProUGUI _playerLabel;
    private const float _playerLabelFontSize = 20f;
    private static Vector2 _playerLabelSize = new Vector2(49f, 24f);

    // Shadow
    [Header("Shadow UI")]
    public Sprite shadowDefaultSprite;
    public Sprite shadowPressedSprite;
    private Image _shadowOverlay;
    private static Vector2 _shadowOffset = new Vector2(4f, -8f);

    // Allow touch input
    public bool allowTouch = true;
    private ButtonListener _btn;

    // Rect
    public RectTransform rectTransform { get; private set; }

    // On submit delegate
    public Action onSubmit;

    // On awake
    protected virtual void Awake()
    {
        // Get rect
        rectTransform = gameObject.GetComponent<RectTransform>();
        inputPlayer = -1;

        // Set content origin
        if (content != null)
        {
            _contentDefaultY = content.anchoredPosition.y;
        }

        // Setup button background
        _backgroundImage = gameObject.GetComponent<Image>();
        if (_backgroundImage != null)
        {
            Destroy(_backgroundImage);
        }
        _backgroundImage = CreateChild("BG", rectTransform).gameObject.AddComponent<Image>();
        _backgroundImage.color = Color.white;
        _backgroundImage.sprite = normalSprite;
        _backgroundImage.type = Image.Type.Sliced;

        // Setup button highlight
        RectTransform ht = CreateChild("HIGHLIGHT", rectTransform, _highlightPadding);
        ht.SetSiblingIndex(rectTransform.childCount-1);
        _highlight = ht.gameObject.AddComponent<Image>();
        _highlight.type = Image.Type.Sliced;
        _highlight.sprite = highlightDefaultSprite;
        // Setup button player overlay
        RectTransform pt = CreateChild("PLAYER", ht);
        _playerOverlay = pt.gameObject.AddComponent<Image>();
        _playerOverlay.type = Image.Type.Sliced;
        _playerOverlay.sprite = playerBGSprite;
        // Setup button player label
        RectTransform lt = CreateChild("PLAYER_LABEL", pt);
        lt.anchorMin = Vector2.one;
        lt.anchorMax = Vector2.one;
        lt.pivot = Vector2.one;
        lt.anchoredPosition = Vector2.zero;
        _playerLabel = lt.gameObject.AddComponent<TextMeshProUGUI>();
        _playerLabel.font = playerFont;
        _playerLabel.fontSize = _playerLabelFontSize;
        _playerLabel.alignment = TextAlignmentOptions.Center;
        _playerLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0f, _playerLabelSize.x);
        _playerLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0f, _playerLabelSize.y);

        // Shadow
        if (shadowDefaultSprite != null)
        {
            _shadowOverlay = CreateChild("SHADOW", rectTransform).gameObject.AddComponent<Image>();
            _shadowOverlay.rectTransform.anchoredPosition = _shadowOffset;
            _shadowOverlay.color = Color.white;
            _shadowOverlay.sprite = shadowDefaultSprite;
            _shadowOverlay.type = Image.Type.Sliced;
        }

        // Get button
        if (allowTouch)
        {
            _btn = gameObject.GetComponent<ButtonListener>();
            if (_btn == null)
            {
                _btn = gameObject.AddComponent<ButtonListener>();
                _btn.transition = Selectable.Transition.None;
            }
            _btn.onClick.AddListener(Submit);
        }
    }

    // Set state
    protected virtual void OnEnable()
    {
        UpdateState();
    }
	// Remove player
	protected virtual void OnDisable()
	{
		inputPlayer = -1;
	}

	// On destroy
	protected virtual void OnDestroy()
    {
        if (_btn != null)
        {
            _btn.onClick.RemoveAllListeners();
        }
    }

    // Call submit delegate
    public void Submit()
    {
        if (onSubmit != null)
        {
            onSubmit();
        }
    }

    // Instantiate
    private RectTransform CreateChild(string childName, RectTransform parent, float padding = 0f)
    {
        // Generate rect & setup transform
        RectTransform result = new GameObject(childName).AddComponent<RectTransform>();
        result.transform.SetParent(parent);
        result.transform.SetSiblingIndex(0);
        result.transform.localPosition = Vector3.zero;
        result.transform.localRotation = Quaternion.identity;
        result.transform.localScale = Vector3.one;

        // Setup rect transform
        result.anchorMin = Vector2.zero;
        result.anchorMax = Vector2.one;
        result.pivot = Vector2.one * 0.5f;
        result.anchoredPosition = Vector3.one * padding * 0.5f;
        result.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parent.rect.width + padding);
        result.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parent.rect.height + padding);

        // Return
        return result;
    }
    #endregion

    #region STATE
    // Mouse states
    public bool isMouseHover { get; private set; }
    public bool isMousePress { get; private set; }

    // Input player
    public bool checkInputHorizontal = false;
    public int inputPlayer { get; private set; }
    public bool isInputHighlight { get { return inputPlayer >= 0; } }
    public bool isInputPress { get; private set; }

    // Disable state
    public bool isDisabled { get; private set; }

    // Button state
    public RoomiesButtonState state { get; private set; }
    // State change
    public Action<RoomiesButtonState> onStateChange;

    // Update button state
    protected virtual void Update()
    {
        // Get updates
        bool updated = false;

        // Check button states
        if (_btn != null)
        {
            if (_btn.isHovered != isMouseHover)
            {
                isMouseHover = _btn.isHovered;
                updated = true;
            }
            if (_btn.isPressed != isMousePress)
            {
                isMousePress = _btn.isPressed;
                updated = true;
            }
            if (_btn.interactable == isDisabled)
            {
                isDisabled = !_btn.interactable;
                updated = true;
            }
        }
        // Update press
        bool toInputPress = false;
        if (isInputHighlight)
        {
            if (GameManager.instance.GetPlayerButton(inputPlayer, GameManager.SUBMIT_INPUT_KEY))
            {
                toInputPress = true;
            }
            else if (checkInputHorizontal && (GameManager.instance.GetPlayerButton(inputPlayer, GameManager.HORIZONTAL_POS_INPUT_KEY) || GameManager.instance.GetPlayerButton(inputPlayer, GameManager.HORIZONTAL_NEG_INPUT_KEY)))
            {
                toInputPress = true;
            }
        }
        if (isInputPress != toInputPress)
        {
            isInputPress = toInputPress;
            updated = true;
        }

        // Update state
        if (updated)
        {
            UpdateState();
        }
    }

    // Set input highlight
    public void SetInputPlayer(int newPlayer)
    {
        if (inputPlayer != newPlayer)
        {
            inputPlayer = newPlayer;
            if (newPlayer >= 0)
            {
                _playerOverlay.gameObject.SetActive(true);
                _playerOverlay.color = GameManager.instance.GetPlayerColor(newPlayer);
                _playerLabel.text = "P" + (newPlayer + 1);
            }
            else
            {
                _playerOverlay.gameObject.SetActive(false);
            }
            UpdateState();
        }
    }

    // Set disabled
    public void SetDisabled(bool toDisabled)
    {
        if (isDisabled != toDisabled)
        {
            isDisabled = toDisabled;
            if (_btn != null)
            {
                _btn.interactable = !isDisabled;
            }
            UpdateState();
        }
    }

    // Update state
    protected virtual void UpdateState()
    {
        // Disabled
        RoomiesButtonState newState;
        if (isDisabled)
        {
            newState = RoomiesButtonState.Disabled;
        }
        // Pressed
        else if (isMousePress || isInputPress)
        {
            newState = RoomiesButtonState.Pressed;
        }
        // Hovered
        else if (isMouseHover || isInputHighlight)
        {
            newState = RoomiesButtonState.Highlighted;
        }
        // Default
        else
        {
            newState = RoomiesButtonState.Normal;
        }

        // State changed
        SetState(newState);
    }

    // Set state
    protected void SetState(RoomiesButtonState newState)
    {
        // Set state
        state = newState;

        // Set imag
        if (_backgroundImage != null)
        {
            if (state == RoomiesButtonState.Pressed)
            {
                _backgroundImage.color = Color.white;
                _backgroundImage.sprite = pressedSprite;
            }
            else if (state == RoomiesButtonState.Disabled)
            {
                if (disabledSprite == null)
                {
                    _backgroundImage.color = Color.white;
                    _backgroundImage.sprite = pressedSprite;
                }
                else
                {
                    _backgroundImage.color = disabledColor;
                    _backgroundImage.sprite = disabledSprite;
                }
            }
            else
            {
                _backgroundImage.color = Color.white;
                _backgroundImage.sprite = normalSprite;
            }
        }
        // Shadow
        if (_shadowOverlay != null)
        {
            _shadowOverlay.sprite = state == RoomiesButtonState.Normal || state == RoomiesButtonState.Highlighted ? shadowDefaultSprite : shadowPressedSprite;
        }

        // Set highlight
        if (_highlight != null)
        {
            _highlight.gameObject.SetActive(isInputHighlight);
            _highlight.sprite = state == RoomiesButtonState.Highlighted ? highlightDefaultSprite : highlightPressedSprite;
        }
        // Player overlay
        if (_playerOverlay != null)
        {
            Vector2 pos = Vector2.zero;
            pos.y = state == RoomiesButtonState.Pressed || state == RoomiesButtonState.Disabled ? _contentPressY : 0f;
            _playerOverlay.rectTransform.anchoredPosition = pos;
        }
        // Set content
        if (content != null)
        {
            Vector2 pos = content.anchoredPosition;
            pos.y = _contentDefaultY + (state == RoomiesButtonState.Pressed || state == RoomiesButtonState.Disabled ? _contentPressY : 0f);
            content.anchoredPosition = pos;
        }

        // On state change
        if (onStateChange != null)
        {
            onStateChange(state);
        }
    }
    #endregion
}
