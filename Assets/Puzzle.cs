using UnityEngine;

public class Puzzle : MonoBehaviour
{
    public Vector2 size;
    public int pieceCount;

    private LineRenderer lineRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize the LineRenderer
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 0; // Start with no positions
        lineRenderer.startWidth = 0.1f; // Set the width of the line
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Use a default material

        // Divide the puzzle into pieces. We assume each piece starts with a square shape.
        float pieceArea = size.x * size.y / pieceCount;
        float pieceWidth = Mathf.Sqrt(pieceArea);
        float pieceHeight = pieceWidth;
        int widthInPieces = Mathf.RoundToInt(size.x / pieceWidth);
        int heightInPieces = Mathf.RoundToInt(size.y / pieceHeight);

        Debug.Log("position: " + transform.position);
        Debug.Log("pieceWidth: " + pieceWidth);
        Debug.Log("pieceHeight: " + pieceHeight);
        Debug.Log("widthInPieces: " + widthInPieces);
        Debug.Log("heightInPieces: " + heightInPieces);

        // Draw vertical lines
        for (int i = 0; i <= pieceCount; i++)
        {
            float x = transform.position.x + (i * pieceWidth);
            lineRenderer.positionCount += 2; // Add two points for each line
            lineRenderer.SetPosition(lineRenderer.positionCount - 2, new Vector3(x, transform.position.y, 0));
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, new Vector3(x, transform.position.y + size.y, 0));
            Debug.Log("x: " + x);
        }

        // Draw horizontal lines
        for (int i = 0; i <= pieceCount; i++)
        {
            float y = transform.position.y + (i * pieceHeight);
            lineRenderer.positionCount += 2; // Add two points for each line
            lineRenderer.SetPosition(lineRenderer.positionCount - 2, new Vector3(transform.position.x, y, 0));
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, new Vector3(transform.position.x + size.x, y, 0));
            Debug.Log("y: " + y);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
