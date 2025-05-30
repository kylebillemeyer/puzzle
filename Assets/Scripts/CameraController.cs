using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Edge Scrolling Settings")]
    [SerializeField, Tooltip("Enable or disable edge scrolling")]
    private bool enableEdgeScrolling = true;
    
    [SerializeField, Tooltip("Pixels from screen edge to trigger horizontal movement")]
    private float screenThresholdX = 50f;
    
    [SerializeField, Tooltip("Pixels from screen edge to trigger vertical movement")]
    private float screenThresholdY = 50f;
    
    [SerializeField, Tooltip("Camera movement speed in units per second")]
    private float moveSpeed = 5f;

    [Header("Drag Scrolling Settings")]
    [SerializeField, Tooltip("Enable or disable drag scrolling")]
    private bool enableDragScrolling = true;
    
    [SerializeField, Tooltip("Speed multiplier for drag scrolling")]
    private float dragSpeed = 1f;

    [Header("Zoom Settings")]
    [SerializeField, Tooltip("Zoom speed multiplier")]
    private float zoomSpeed = 2f;
    
    [SerializeField, Tooltip("Minimum orthographic size (maximum zoom in)")]
    private float minZoom = 2f;
    
    [SerializeField, Tooltip("Maximum orthographic size (maximum zoom out)")]
    private float maxZoom = 20f;

    [Header("Piece Dragging Settings")]
    [SerializeField, Tooltip("Layer mask for puzzle pieces")]
    private LayerMask puzzlePieceLayer;

    private Camera mainCamera;
    private Vector2 lastMousePosition;
    private bool isDragging = false;
    private PuzzlePiece draggedPiece;
    private Vector3 dragOffset;

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
        if (enableEdgeScrolling && !isDragging)
        {
            HandleEdgeScrolling();
        }

        HandleDragScrolling();
        HandleZoom();
        HandlePieceDragging();
        HandleReset();
    }

    private void HandleReset()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            // Find all puzzle pieces in the scene
            PuzzlePiece[] pieces = FindObjectsOfType<PuzzlePiece>();
            foreach (PuzzlePiece piece in pieces)
            {
                piece.ResetToOriginalPosition();
            }
        }
    }

    private void HandlePieceDragging()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        bool leftMousePressed = Mouse.current.leftButton.isPressed;

        if (leftMousePressed && !draggedPiece)
        {
            // Try to find a puzzle piece under the mouse
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, puzzlePieceLayer);

            if (hit.collider != null)
            {
                Debug.Log($"Hit a piece");
                draggedPiece = hit.collider.GetComponent<PuzzlePiece>();
                if (draggedPiece != null)
                {
                    // Calculate offset from piece center to mouse position
                    dragOffset = draggedPiece.transform.position - mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0));
                }
            }
        }
        else if (!leftMousePressed)
        {
            // Release the piece
            draggedPiece = null;
        }

        if (draggedPiece != null)
        {
            // Update piece position to follow mouse
            Vector3 targetPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0)) + dragOffset;
            targetPosition.z = draggedPiece.transform.position.z; // Maintain original z position
            draggedPiece.transform.position = targetPosition;
        }
    }

    private void HandleDragScrolling()
    {
        if (!enableDragScrolling) return;

        Vector2 currentMousePosition = Mouse.current.position.ReadValue();
        bool middleMousePressed = Mouse.current.middleButton.isPressed;

        if (middleMousePressed && !isDragging)
        {
            // Start dragging
            isDragging = true;
            lastMousePosition = currentMousePosition;
        }
        else if (!middleMousePressed && isDragging)
        {
            // Stop dragging
            isDragging = false;
        }

        if (isDragging)
        {
            // Calculate the movement delta
            Vector2 delta = lastMousePosition - currentMousePosition;
            
            // Convert screen delta to world space movement
            Vector3 worldDelta = mainCamera.ScreenToWorldPoint(new Vector3(delta.x, delta.y, 0)) - 
                                mainCamera.ScreenToWorldPoint(Vector3.zero);
            
            // Apply the movement
            transform.position += worldDelta * dragSpeed;
            
            // Update last position
            lastMousePosition = currentMousePosition;
        }
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
