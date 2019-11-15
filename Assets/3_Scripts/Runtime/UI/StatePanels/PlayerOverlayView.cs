using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerOverlayView : MonoBehaviour
{
    // Label
    public TextMeshProUGUI label;
    // Image
    public Image background;
    // Container
    public GameObject baseContainer;

    // Effect container
    public RectTransform effectContainer;
    // Hit
    public SimpleEffect hitEffect;
    // Whimper
    public SimpleEffect whimperEffect;

    // Camera manager
    private const float _defaultCameraY = 18.5f;
    private static CameraManager _camManager;
    private void OnEnable()
    {
        // Set camera manager
        if (_camManager == null)
        {
            _camManager = GameObject.FindObjectOfType<CameraManager>();
        }

        // Scale
        float gameY = _camManager.gameY;
        if (gameY > 0f)
        {
            float scale = _defaultCameraY / Mathf.Abs(gameY);
            effectContainer.localScale = Vector3.one * scale;
        }
    }

    // Set player
    public void SetPlayer(int playerIndex)
    {
        // Set label
        label.text = "P" + (playerIndex + 1);

        // Set backgrounds
        Color color = GameManager.instance.GetPlayerColor(playerIndex);
        background.color = color;

        // Setup
        hitEffect.SetColor(color);
        hitEffect.gameObject.SetActive(false);
        whimperEffect.SetColor(color);
        whimperEffect.gameObject.SetActive(false);

        // Refresh
        RefreshContainer();
    }
    // Was hit
    public void WasHit()
    {
        whimperEffect.gameObject.SetActive(true);
        RefreshContainer();
    }
    // Hit
    public void DidHit()
    {
        hitEffect.gameObject.SetActive(true);
        RefreshContainer();
    }

    // Refresh while off
    private void Update()
    {
        if (!baseContainer.activeSelf)
        {
            RefreshContainer();
        }
    }
    // Refresh
    private void RefreshContainer()
    {
        bool shouldShow = !whimperEffect.gameObject.activeSelf && !hitEffect.gameObject.activeSelf;
        if (shouldShow != baseContainer.activeSelf)
        {
            baseContainer.SetActive(shouldShow);
        }
    }
}
