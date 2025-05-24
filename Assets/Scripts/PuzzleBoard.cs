using UnityEngine;

public class Puzzle : MonoBehaviour
{
    private const float LINE_WIDTH = 0.02f;

    public int pieceCount;
    public int pixelsPerUnit;

    private int src_width;
    private int src_height;
    private int overflow_x;
    private int overflow_y;
    private int pieces_x;
    private int pieces_y;
    private int pieceWidth;
    private int pieceHeight;

    private Texture2D originalImage;
    private Vector3[,] points;
    private Texture2D[,] pieceImages;
    private LineRenderer[] x_lineRenderers;
    private LineRenderer[] y_lineRenderers;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        originalImage = sprite.texture;

        src_width = originalImage.width;
        src_height = originalImage.height;

        // Divide the puzzle into pieces. We assume each piece starts with a square shape.
        float pieceArea = src_width * src_height / (float)pieceCount;
        pieceWidth = Mathf.CeilToInt(Mathf.Sqrt(pieceArea));
        pieceHeight = pieceWidth;

        // The size dimensions are not always divisible by the piece dimensions. Rather than having the last
        // row or column be a partial piece, we subtract by 1 and split a full piece and the overflow across
        // both the first and last rows/columns.
        pieces_x = Mathf.FloorToInt(src_width / pieceWidth) + 1;
        pieces_y = Mathf.FloorToInt(src_height / pieceHeight) + 1;

        overflow_x = src_width - (pieces_x - 2) * pieceWidth;
        overflow_y = src_height - (pieces_y - 2) * pieceHeight;

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
                // The first and last row/column are half the overflow amount.
                float x_pos = PiecePosXInPixels(x) / sprite.pixelsPerUnit;
                float y_pos = PiecePosYInPixels(y) / sprite.pixelsPerUnit;

                points[x, y] = new Vector3(x_pos, y_pos, 0);

                Debug.Log($"points[{x}, {y}]: {points[x, y]}");

                x_lineRenderers[y].positionCount += 1;
                x_lineRenderers[y].SetPosition(x, points[x, y]);

                y_lineRenderers[x].positionCount += 1;
                y_lineRenderers[x].SetPosition(y, points[x, y]);

                CreatePuzzlePiece(x, y, points[x, y]);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void CreatePuzzlePiece(int x, int y, Vector3 position)
    {
        int source_pos_x = PiecePosXInPixels(x);
        int source_pos_y = PiecePosYInPixels(y);
        int dst_width_x = PieceWidthXInPixels(x);
        int dst_width_y = PieceWidthYInPixels(y);

        // Create a new texture for the piece
        Texture2D pieceImage = new Texture2D(dst_width_x, dst_width_y);
        pieceImage.filterMode = FilterMode.Bilinear;
        pieceImage.wrapMode = TextureWrapMode.Clamp;

        // Create a temporary RenderTexture to handle the copy
        RenderTexture rt = RenderTexture.GetTemporary(
            dst_width_x,
            dst_width_y,
            0,
            RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear
        );

        for (int i = 0; i < dst_width_x; i++)
        {
            for (int j = 0; j < dst_width_y; j++)
            {
                Color c = originalImage.GetPixel(source_pos_x + i, source_pos_y + j);
                pieceImage.SetPixel(i, j, c);
            }
        }
        pieceImage.Apply();

        // Create the piece GameObject
        GameObject piece = new GameObject($"Piece_{x}_{y}");
        piece.transform.position = position;
        piece.transform.parent = transform;

        // Add a SpriteRenderer component
        SpriteRenderer renderer = piece.AddComponent<SpriteRenderer>();
        renderer.sprite = Sprite.Create(
            pieceImage,
            new Rect(0, 0, dst_width_x, dst_width_y),
            new Vector2(0.5f, 0.5f),
            100f  // Pixels per unit
        );
    }

    private int PiecePosXInPixels(int x)
    {
        if (x == 0)
            return 0;
        else if (x > 0 && x < pieces_x)
            return (x - 1) * pieceWidth + overflow_x / 2;
        else
            return src_width;
    }
    

    private int PiecePosYInPixels(int y)
    {
        if (y == 0)
            return 0;
        else if (y > 0 && y < pieces_y)
            return (y - 1) * pieceHeight + overflow_y / 2;
        else
            return src_height;
    }

    private int PieceWidthXInPixels(int x)
    {
        if (x == 0)
            return overflow_x / 2;
        else if (x > 0 && x < pieces_x)
            return pieceWidth;
        else
            return overflow_x / 2;
    }

    private int PieceWidthYInPixels(int y)
    {
        if (y == 0)
            return overflow_y / 2;
        else if (y > 0 && y < pieces_y)
            return pieceHeight;
        else
            return overflow_y / 2;
    }
}
