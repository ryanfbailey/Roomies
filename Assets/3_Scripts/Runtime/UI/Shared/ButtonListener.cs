using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonListener : Button
{
    // Pressed
    public bool isPressed { get; private set; }
    // Hovered
    public bool isHovered { get; private set; }

    // Remove
    protected override void OnDisable()
    {
        base.OnDisable();
        isHovered = false;
        isPressed = false;
    }

    // Hover enter
    public override void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        base.OnPointerEnter(eventData);
    }
    // Hover exit
    public override void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        base.OnPointerExit(eventData);
    }

    // Press down
    public override void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        base.OnPointerDown(eventData);
    }
    // Press up
    public override void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        base.OnPointerUp(eventData);
    }
}
