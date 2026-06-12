using UnityEngine;

public class FishBehavior : MonoBehaviour
{
    public bool isToxic = false;
    
    private Vector3 startPoint;
    private Vector3 endPoint;
    private float speed;
    private float amplitude;
    private float frequency;
    
    private float progress = 0f;
    private float totalDuration;
    private float randomPhaseOffset;
    private bool isInitialized = false;

    public void Initialize(Vector3 start, Vector3 end, float swimSpeed, float sineAmp, float sineFreq, bool toxic)
    {
        startPoint = start;
        endPoint = end;
        speed = swimSpeed;
        amplitude = sineAmp;
        frequency = sineFreq;
        isToxic = toxic;

        float distance = Vector3.Distance(startPoint, endPoint);
        totalDuration = distance / speed;
        randomPhaseOffset = Random.Range(0f, 2f * Mathf.PI);
        progress = 0f;
        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized || totalDuration <= 0) return;

        progress += Time.deltaTime;
        float t = progress / totalDuration;

        if (t >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        // Tính toán vị trí tịnh tiến tuyến tính
        Vector3 currentLinearPos = Vector3.Lerp(startPoint, endPoint, t);

        // Cộng thêm dao động hình Sin vào trục Y
        float sineOffset = Mathf.Sin((t * totalDuration * frequency) + randomPhaseOffset) * amplitude;
        
        transform.position = new Vector3(currentLinearPos.x, currentLinearPos.y + sineOffset, currentLinearPos.z);
    }
}
