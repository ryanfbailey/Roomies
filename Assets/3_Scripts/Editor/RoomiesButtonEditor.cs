using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CustomEditor(typeof(RoomiesButton))]
[CanEditMultipleObjects]
public class RoomiesButtonEditor : Editor
{
    // Roomies button
    private RoomiesButton _button;

    // Set path
    private const string SPRITE_PATH = "Assets/1_UI/v0.2/Button/";
    private const string FONT_PATH = "Assets/1_UI/Fonts/KOMTITA_SDF.asset";

    // Inspector GUI
    public override void OnInspectorGUI()
    {
        // Get target
        if (_button == null)
        {
            _button = (RoomiesButton)target;
        }

        // Type
        RoomiesButtonType oldType = _button.buttonType;

        // Inspector gui
        base.OnInspectorGUI();

        // Load icons
        if (_button.buttonType != oldType || _button.normalSprite == null || _button.pressedSprite == null)
        {
            // Set sprite
            string path = SPRITE_PATH + _button.buttonType.ToString();
            _button.normalSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path + ".png");
            _button.pressedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path + "-Pressed.png");

            // Set icon
            Image img = _button.GetComponent<Image>();
            if (img == null)
            {
                img = _button.gameObject.AddComponent<Image>();
            }
            img.sprite = _button.normalSprite;
        }
        // Get highlight
        if (_button.highlightDefaultSprite == null)
        {
            _button.highlightDefaultSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_PATH + "Highlight.png");
        }
        // Get highlight
        if (_button.highlightPressedSprite == null)
        {
            _button.highlightPressedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_PATH + "Highlight-Pressed.png");
        }
        // Get player sprite
        if (_button.playerBGSprite == null)
        {
            _button.playerBGSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_PATH + "Highlight-Label.png");
        }
        // Get player font
        if (_button.playerFont == null)
        {
            _button.playerFont = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(FONT_PATH);
            Debug.Log("FONT: " + (_button.playerFont == null ? "NULL" : _button.playerFont.name));
        }
    }
}
