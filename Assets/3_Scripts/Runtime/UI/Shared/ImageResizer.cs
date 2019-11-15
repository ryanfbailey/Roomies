using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ResizeType
{
    Stretch,
    AspectFill,
    AspectFit
}

public class ImageResizer : MonoBehaviour
{
    // Resize type
    public ResizeType resizeType = ResizeType.Stretch;
    // Resize on update
    public bool resizeOnUpdate = true;
    // Image
    public Image image;
    // Transform
    public RectTransform rectTransform { get; private set; }
    private Vector2 _canvasSize;

    // Awake
    private void Awake()
    {
        rectTransform = gameObject.GetComponent<RectTransform>();
        ResizeRect();
    }
    // Refresh
    private void Update()
    {
        if (resizeOnUpdate && _canvasSize != rectTransform.rect.size)
        {
            ResizeRect();
        }
    }
    // Resize rect
    public void ResizeRect()
    {
        // Get canvas size & image size
        _canvasSize = rectTransform.rect.size;
        float canvasRatio = _canvasSize.x / _canvasSize.y;
        Vector2 imageSize = image.sprite == null ? _canvasSize : image.sprite.rect.size;
        float imageRatio = imageSize.x / imageSize.y;
        Vector2 size = _canvasSize;
        bool canvasWidthBigger = canvasRatio < imageRatio;

        // Fit inside canvas so all can be seen
        if (resizeType == ResizeType.AspectFit)
        {
            if (canvasWidthBigger)
            {
                size.x = _canvasSize.y * imageRatio;
            }
            else
            {
                size.y = _canvasSize.x / imageRatio;
            }
        }
        // Fill by cropping image
        else if (resizeType == ResizeType.AspectFill)
        {
            if (canvasWidthBigger)
            {
                size.y = _canvasSize.x / imageRatio;
            }
            else
            {
                size.x = _canvasSize.y * imageRatio;
            }
        }

        // Set image size
        image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
    }
}
