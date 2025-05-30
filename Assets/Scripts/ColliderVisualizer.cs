using UnityEngine;

public class ColliderVisualizer : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    private LineRenderer lineRenderer;
    private Color lineColor = Color.green;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            Debug.LogError("No BoxCollider2D found on this GameObject!");
            enabled = false;
            return;
        }

        // Create LineRenderer
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.positionCount = 5; // 4 corners + back to start
        lineRenderer.loop = true;
        
        UpdateLineRenderer();
    }

    void Update()
    {
        UpdateLineRenderer();
    }

    void UpdateLineRenderer()
    {
        if (boxCollider == null || lineRenderer == null) return;

        Vector2 size = boxCollider.size;
        Vector2 offset = boxCollider.offset;
        Vector2 halfSize = size * 0.5f;

        // Calculate the four corners of the box
        Vector2 topLeft = offset + new Vector2(-halfSize.x, halfSize.y);
        Vector2 topRight = offset + new Vector2(halfSize.x, halfSize.y);
        Vector2 bottomRight = offset + new Vector2(halfSize.x, -halfSize.y);
        Vector2 bottomLeft = offset + new Vector2(-halfSize.x, -halfSize.y);

        // Convert to world space
        Vector3[] points = new Vector3[5];
        points[0] = transform.TransformPoint(topLeft);
        points[1] = transform.TransformPoint(topRight);
        points[2] = transform.TransformPoint(bottomRight);
        points[3] = transform.TransformPoint(bottomLeft);
        points[4] = points[0]; // Close the loop

        lineRenderer.SetPositions(points);
    }
} 