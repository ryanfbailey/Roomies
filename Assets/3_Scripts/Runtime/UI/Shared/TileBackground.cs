using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileBackground : MonoBehaviour
{
    // Move speed in pixels per second
    public float moveSpeed = 10f;
    // Move direction
    public float moveDirection = 225f;
    // Background to move
    public RawImage background;

    // Transform
    public RectTransform rectTransform { get; private set; }

    // On awake
    protected virtual void Awake()
    {
        rectTransform = gameObject.GetComponent<RectTransform>();
        ResetTiling();
    }
    // When showing
    protected virtual void OnEnable()
    {
        ResetTiling();
    }
    // Reset tiling
    private void ResetTiling()
    {
        if (background.texture != null)
        {
            background.uvRect = new Rect(background.uvRect.x, background.uvRect.y, rectTransform.rect.width / background.texture.width, rectTransform.rect.height / background.texture.height);
        }
    }
    // Update
    private void Update()
    {
        if (background.texture != null && moveSpeed != 0f)
        {
            Rect rect = background.uvRect;
            rect.x += Mathf.Cos(moveDirection * Mathf.Deg2Rad) * (moveSpeed / background.texture.width) * Time.deltaTime;
            rect.y += Mathf.Sin(moveDirection * Mathf.Deg2Rad) * (moveSpeed / background.texture.height) * Time.deltaTime;
            background.uvRect = rect;
        }
    }
}
