using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomPanner : MonoBehaviour
{
    [Header("Giới hạn biên")]
    public float minX = -10f;
    public float maxX = 10f;

    [Header("Thông số đàn hồi")]
    [Range(0.1f, 1f)]
    public float elasticity = 0.3f; // Càng nhỏ càng nặng khi kéo quá biên
    public float snapSmoothTime = 0.15f; // Thời gian nảy về (càng nhỏ càng nhanh)

    [Header("UI Arrow")]
    public GameObject leftArrow;
    public GameObject rightArrow;

    private Vector3 dragOrigin;
    private Vector3 targetPos;
    private Vector3 currentVelocity;
    private bool isDragging = false;

    void Start()
    {
        targetPos = transform.position;
    }

    void Update()
    {
        HandleInput();

        // Nếu không kéo, và đang ở ngoài biên, targetPos sẽ bị ép về biên
        if (!isDragging)
        {
            float clampedX = Mathf.Clamp(targetPos.x, minX, maxX);
            targetPos.x = clampedX;
        }

        // Di chuyển mượt mà tới vị trí mục tiêu
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, snapSmoothTime);

        UpdateArrows();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
            targetPos = transform.position; // Reset target về vị trí hiện tại khi bắt đầu kéo mới
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 moveDelta = dragOrigin - currentMousePos;

            float newX = targetPos.x - moveDelta.x;

            // TÍNH TOÁN ĐỘ CO GIÃN (Rubber Banding Logic)
            if (newX < minX)
            {
                // Nếu vượt quá biên trái, mỗi pixel kéo đi chỉ đi được 'elasticity' pixel thực tế
                float overshoot = minX - newX;
                newX = minX - (overshoot * elasticity);
            }
            else if (newX > maxX)
            {
                // Nếu vượt quá biên phải
                float overshoot = newX - maxX;
                newX = maxX + (overshoot * elasticity);
            }

            targetPos = new Vector3(newX, transform.position.y, transform.position.z);
            dragOrigin = currentMousePos;
        }
    }

    private void UpdateArrows()
    {
        if (leftArrow != null)
            leftArrow.SetActive(transform.position.x < maxX - 0.1f);

        if (rightArrow != null)
            rightArrow.SetActive(transform.position.x > minX + 0.1f);
    }
}
