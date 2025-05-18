using UnityEngine;

public class Puzzle : MonoBehaviour
{
    private const float LINE_WIDTH = 0.02f;

    public Vector2 size;
    public int pieceCount;

    private Vector3[,] points;
    private LineRenderer[] x_lineRenderers;
    private LineRenderer[] y_lineRenderers;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Divide the puzzle into pieces. We assume each piece starts with a square shape.
        float pieceArea = size.x * size.y / pieceCount;
        float pieceWidth = Mathf.Sqrt(pieceArea);
        float pieceHeight = pieceWidth;

        // The size dimensions are not always divisible by the piece dimensions. Rather than having the last
        // row or column be a partial piece, we subtract by 1 and split a full piece and the overflow across
        // both the first and last rows/columns.
        int pieces_x = Mathf.FloorToInt(size.x / pieceWidth) + 1;
        int pieces_y = Mathf.FloorToInt(size.y / pieceHeight) + 1;

        float overflow_x = size.x - (pieces_x - 2) * pieceWidth;
        float overflow_y = size.y - (pieces_y - 2) * pieceHeight;

        Debug.Log($"pieces_x: {pieces_x}, pieces_y: {pieces_y}");
        Debug.Log($"pieceWidth: {pieceWidth}, pieceHeight: {pieceHeight}");
        Debug.Log($"area: {pieceArea}");
        Debug.Log($"overflow_x: {overflow_x}, overflow_y: {overflow_y}");

        // Create a LineRenderer for each row and column. There is one extra line for the outer border.
        x_lineRenderers = new LineRenderer[pieces_y + 1];
        y_lineRenderers = new LineRenderer[pieces_x + 1];
        for (int i = 0; i < pieces_x + 1; i++)
        {
            GameObject lineObj = new GameObject($"Y_Line_{i}");
            lineObj.transform.parent = transform;  // Make it a child of the current object
            lineObj.transform.localPosition = new Vector3(0, 0, 0);
            y_lineRenderers[i] = lineObj.AddComponent<LineRenderer>();
            y_lineRenderers[i].positionCount = 0; // Start with no positions
            y_lineRenderers[i].startWidth = LINE_WIDTH; // Set the width of the line
            y_lineRenderers[i].endWidth = LINE_WIDTH;
            y_lineRenderers[i].material = new Material(Shader.Find("Sprites/Default")); // Use a default material
            y_lineRenderers[i].useWorldSpace = false;
        }

        for (int i = 0; i < pieces_y + 1; i++)
        {
            GameObject lineObj = new GameObject($"X_Line_{i}");
            lineObj.transform.parent = transform;  // Make it a child of the current object
            lineObj.transform.localPosition = new Vector3(0, 0, 0);
            x_lineRenderers[i] = lineObj.AddComponent<LineRenderer>();
            x_lineRenderers[i].positionCount = 0; // Start with no positions
            x_lineRenderers[i].startWidth = LINE_WIDTH; // Set the width of the line
            x_lineRenderers[i].endWidth = LINE_WIDTH;
            x_lineRenderers[i].material = new Material(Shader.Find("Sprites/Default")); // Use a default material
            x_lineRenderers[i].useWorldSpace = false;
        }

        points = new Vector3[pieces_x + 1, pieces_y + 1];
        Debug.Log($"points: {points.GetLength(0)} {points.GetLength(1)}");
        for (int x = 0; x < pieces_x + 1; x++)
        {
            for (int y = 0; y < pieces_y + 1; y++)
            {
                float x_pos = 0;
                float y_pos = 0;

                // The first and last row/column are half the overflow amount.
                if (x == 0)
                {
                    x_pos = 0;
                }
                else if (x > 0 && x < pieces_x)
                {
                    x_pos = (x - 1) * pieceWidth + overflow_x / 2;
                }
                else
                {
                    x_pos = size.x;
                }

                if (y == 0)
                {
                    y_pos = 0;
                }
                else if (y > 0 && y < pieces_y)
                {
                    y_pos = (y - 1) * pieceHeight + overflow_y / 2;
                }
                else
                {
                    y_pos = size.y;
                }

                points[x, y] = new Vector3(x_pos, y_pos, 0);

                Debug.Log($"points[{x}, {y}]: {points[x, y]}");

                x_lineRenderers[y].positionCount += 1;
                x_lineRenderers[y].SetPosition(x, points[x, y]);

                y_lineRenderers[x].positionCount += 1;
                y_lineRenderers[x].SetPosition(y, points[x, y]);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
