using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Edge Scrolling Settings")]
    [SerializeField, Tooltip("Pixels from screen edge to trigger horizontal movement")]
    private float screenThresholdX = 50f;
    
    [SerializeField, Tooltip("Pixels from screen edge to trigger vertical movement")]
    private float screenThresholdY = 50f;
    
    [SerializeField, Tooltip("Camera movement speed in units per second")]
    private float moveSpeed = 5f;

    [Header("Zoom Settings")]
    [SerializeField, Tooltip("Zoom speed multiplier")]
    private float zoomSpeed = 2f;
    
    [SerializeField, Tooltip("Minimum orthographic size (maximum zoom in)")]
    private float minZoom = 2f;
    
    [SerializeField, Tooltip("Maximum orthographic size (maximum zoom out)")]
    private float maxZoom = 20f;

    private Camera mainCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            enabled = false;
            return;
        }
        
        // Ensure the camera is orthographic for 2D viewing
        mainCamera.orthographic = true;
    }

    // Update is called once per frame
    void Update()
    {
        HandleEdgeScrolling();
        HandleZoom();
    }

    private void HandleEdgeScrolling()
    {
        Vector3 moveDirection = Vector3.zero;
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // Check horizontal movement
        if (mousePosition.x < screenThresholdX)
        {
            moveDirection.x = -1f;
        }
        else if (mousePosition.x > Screen.width - screenThresholdX)
        {
            moveDirection.x = 1f;
        }

        // Check vertical movement
        if (mousePosition.y < screenThresholdY)
        {
            moveDirection.y = -1f;
        }
        else if (mousePosition.y > Screen.height - screenThresholdY)
        {
            moveDirection.y = 1f;
        }

        // Apply movement
        if (moveDirection != Vector3.zero)
        {
            moveDirection.Normalize();  // Ensure consistent speed in all directions
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
    }

    private void HandleZoom()
    {
        float scrollValue = Mouse.current.scroll.ReadValue().y;
        if (scrollValue != 0)
        {
            // Get the mouse position before zooming
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPosBefore = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0));

            // Adjust the orthographic size (negative scrollValue means zoom in)
            float newSize = mainCamera.orthographicSize - (scrollValue * zoomSpeed * 0.01f);
            mainCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);

            // Get the mouse position after zooming
            Vector3 mouseWorldPosAfter = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0));

            // Adjust camera position to keep the point under the mouse cursor stable
            transform.position += mouseWorldPosBefore - mouseWorldPosAfter;
        }
    }
}
