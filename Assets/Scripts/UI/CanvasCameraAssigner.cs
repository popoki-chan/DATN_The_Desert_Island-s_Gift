using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class CanvasCameraAssigner : MonoBehaviour
{
    private void Start()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null)
        {
            canvas.worldCamera = Camera.main;
            if (canvas.worldCamera == null)
            {
                canvas.worldCamera = FindObjectOfType<Camera>();
            }
        }
    }
}
