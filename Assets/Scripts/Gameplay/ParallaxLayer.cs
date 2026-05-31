using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Tooltip("Hệ số parallax. 0: di chuyển cùng parent, >0: di chuyển chậm hơn parent (tạo độ sâu), <0: di chuyển nhanh hơn parent (tiền cảnh)")]
    public float parallaxFactor = 0f;

    private float startLocalX;
    private float startParentX;
    private Transform parentTransform;
    private bool initialized = false;

    void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (initialized) return;
        parentTransform = transform.parent;
        startLocalX = transform.localPosition.x;
        if (parentTransform != null)
        {
            startParentX = parentTransform.position.x;
            initialized = true;
        }
    }

    void Update()
    {
        if (!initialized)
        {
            Initialize();
            if (!initialized) return;
        }
        
        float parentDiffX = parentTransform.position.x - startParentX;
        Vector3 localPos = transform.localPosition;
        localPos.x = startLocalX - (parentDiffX * parallaxFactor);
        transform.localPosition = localPos;
    }
}
