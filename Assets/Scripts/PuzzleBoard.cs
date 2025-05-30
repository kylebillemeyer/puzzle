using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class Puzzle : MonoBehaviour
{
    private const float LINE_WIDTH = 0.02f;

    public int borderPixels;
    public Color borderColor;
    public int pieceCount;
    public int pixelsPerUnit;
    public bool showGrid = true;
    public int shuffleGap = 15; // Gap between pieces when shuffled

    private int src_width;
    private int src_height;
    private int overflow_x;
    private int overflow_y;
    private int pieces_x;
    private int pieces_y;
    private int pieceWidth;
    private int pieceHeight;

    private Sprite originalImage;
    private Vector3[,] points;
    private Texture2D[,] pieceImages;
    private LineRenderer[] x_lineRenderers;
    private LineRenderer[] y_lineRenderers;
    private List<Vector3> shuffledPositions; // Store shuffled positions

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalImage = GetComponent<SpriteRenderer>().sprite;

        src_width = originalImage.texture.width;
        src_height = originalImage.texture.height;

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

        initGridRendering();

        // Generate shuffled positions before creating pieces
        ShufflePiecePositions();

        initPuzzlePieces();

        // Move the puzzle board so the image is centered at (0, 0).
        Vector2 imageExtents = new Vector2(src_width, src_height) * 0.01f; // Convert to world units
        Vector3 offset = new Vector3(-imageExtents.x * 0.5f, -imageExtents.y * 0.5f, 0);
        transform.position += offset;

        DrawInsetRectangle();
        FitCameraToImage();
    }

    // Update is called once per frame
    void Update()
    {
        
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

    private float ImageWidthXUnits()
    {
        return src_width / (float)originalImage.pixelsPerUnit;
    }

    private float ImageWidthYUnits()
    {
        return src_height / (float)originalImage.pixelsPerUnit;
    }

    private void initGridRendering()
    {
        if (!showGrid)
            return;

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
    }

    private List<Vector3> ShufflePiecePositions()
    {
        List<Vector3> positions = new List<Vector3>();
        Vector3 startingLocation = new Vector3(0, 0, 0);
        int ringX = pieces_x + 2;
        int ringY = pieces_y + 2;
        while (positions.Count < pieceCount)
        {
            startingLocation = new Vector3(
                (startingLocation.x - pieceWidth - shuffleGap) / originalImage.pixelsPerUnit,
                (startingLocation.y - pieceHeight - shuffleGap) / originalImage.pixelsPerUnit,
                0
            );
            positions.AddRange(CreateFrame(startingLocation, ringX, ringY));
            ringX += 2;
            ringY += 2;
        }

        return positions;
    }

    private void initPuzzlePieces()
    {
        points = new Vector3[pieces_x + 1, pieces_y + 1];
        Debug.Log($"points: {points.GetLength(0)} {points.GetLength(1)}");
        int pieceIndex = 0;
        for (int x = 0; x < pieces_x + 1; x++)
        {
            for (int y = 0; y < pieces_y + 1; y++)
            {
                // The first and last row/column are half the overflow amount.
                float x_pos = PiecePosXInPixels(x) / originalImage.pixelsPerUnit;
                float y_pos = PiecePosYInPixels(y) / originalImage.pixelsPerUnit;

                points[x, y] = new Vector3(x_pos, y_pos, 0);

                Debug.Log($"points[{x}, {y}]: {points[x, y]}");

                if (showGrid)
                {
                    x_lineRenderers[y].positionCount += 1;
                    x_lineRenderers[y].SetPosition(x, points[x, y]);

                    y_lineRenderers[x].positionCount += 1;
                    y_lineRenderers[x].SetPosition(y, points[x, y]);
                }

                // Use shuffled position if available
                Vector3 piecePos = (pieceIndex < shuffledPositions.Count) ? shuffledPositions[pieceIndex] : points[x, y];
                CreatePuzzlePiece(x, y, piecePos);
                pieceIndex++;
            }
        }
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
                if (i <= borderPixels || i >= dst_width_x - borderPixels || j <= borderPixels || j >= dst_width_y - borderPixels)
                {
                    pieceImage.SetPixel(i, j, borderColor);
                }
                else
                {
                    Color c = originalImage.texture.GetPixel(source_pos_x + i, source_pos_y + j);
                    pieceImage.SetPixel(i, j, c);
                }
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
            originalImage.pixelsPerUnit
        );

        // Add the PuzzlePiece component
        piece.AddComponent<PuzzlePiece>();
    }

    private void DrawInsetRectangle()
    {
        // Outer rectangle (border)
        GameObject outerRectObj = new GameObject("OuterBorder");
        outerRectObj.transform.parent = transform;
        outerRectObj.transform.localPosition = Vector3.zero;
        var outerLine = outerRectObj.AddComponent<LineRenderer>();
        outerLine.positionCount = 5;
        outerLine.startWidth = LINE_WIDTH * 2;
        outerLine.endWidth = LINE_WIDTH * 2;
        outerLine.material = new Material(Shader.Find("Sprites/Default"));
        outerLine.useWorldSpace = false;
        outerLine.loop = true;
        outerLine.startColor = borderColor;
        outerLine.endColor = borderColor;

        float width = src_width / (float)originalImage.pixelsPerUnit;
        float height = src_height / (float)originalImage.pixelsPerUnit;

        Vector3[] outerCorners = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(width, 0, 0),
            new Vector3(width, height, 0),
            new Vector3(0, height, 0),
            new Vector3(0, 0, 0)
        };
        outerLine.SetPositions(outerCorners);

        // Inset rectangle (darker for "inset" effect)
        GameObject innerRectObj = new GameObject("InnerBorder");
        innerRectObj.transform.parent = transform;
        innerRectObj.transform.localPosition = Vector3.zero;
        var innerLine = innerRectObj.AddComponent<LineRenderer>();
        innerLine.positionCount = 5;
        innerLine.startWidth = LINE_WIDTH;
        innerLine.endWidth = LINE_WIDTH;
        innerLine.material = new Material(Shader.Find("Sprites/Default"));
        innerLine.useWorldSpace = false;
        innerLine.loop = true;
        Color insetColor = borderColor * 0.7f; // Slightly darker
        insetColor.a = borderColor.a;
        innerLine.startColor = insetColor;
        innerLine.endColor = insetColor;

        float inset = 0.05f; // Adjust for desired inset effect
        Vector3[] innerCorners = new Vector3[]
        {
            new Vector3(inset, inset, 0),
            new Vector3(width - inset, inset, 0),
            new Vector3(width - inset, height - inset, 0),
            new Vector3(inset, height - inset, 0),
            new Vector3(inset, inset, 0)
        };
        innerLine.SetPositions(innerCorners);
    }

    private void FitCameraToImage()
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic) return;

        float imageWorldWidth = src_width / (float)originalImage.pixelsPerUnit;
        float imageWorldHeight = src_height / (float)originalImage.pixelsPerUnit;
        float aspect = (float)Screen.width / Screen.height;

        // Add a 25% buffer to both width and height
        float bufferedWidth = imageWorldWidth * 1.25f;
        float bufferedHeight = imageWorldHeight * 1.25f;

        // Calculate the required orthographic size to fit the buffered image
        float sizeByHeight = bufferedHeight * 0.5f;
        float sizeByWidth = (bufferedWidth * 0.5f) / aspect;
        cam.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);

        // Center the camera on the puzzle (center of buffered area)
        Vector3 imageCenter = transform.position + new Vector3(imageWorldWidth * 0.5f, imageWorldHeight * 0.5f, -10f);
        cam.transform.position = new Vector3(imageCenter.x, imageCenter.y, cam.transform.position.z);
    }

    private List<Vector3> CreateFrame(Vector3 startingPosition, int ringX, int ringY)
    {
        List<Vector3> positions = new List<Vector3>();
        Vector3 currentPosition = startingPosition;

        // Right
        var rightSection = CreateFrameSection(currentPosition, Vector3.right, ringX - 1);
        positions.AddRange(rightSection.Item1);
        currentPosition = rightSection.Item2;

        // Up
        var upSection = CreateFrameSection(currentPosition, Vector3.up, ringY - 1);
        positions.AddRange(upSection.Item1);
        currentPosition = upSection.Item2;

        // Left
        var leftSection = CreateFrameSection(currentPosition, Vector3.left, ringX - 1);
        positions.AddRange(leftSection.Item1);
        currentPosition = leftSection.Item2;

        // Down
        var downSection = CreateFrameSection(currentPosition, Vector3.down, ringY - 1);
        positions.AddRange(downSection.Item1);
        // currentPosition = downSection.Item2; // Not needed unless you want to use it after

        return positions;
    }

    private Tuple<List<Vector3>, Vector3> CreateFrameSection(Vector3 startingPosition, Vector3 direction, int count)
    {
        List<Vector3> positions = new List<Vector3>();
        int i = 0;
        Vector3 currentPosition = startingPosition;
        while (i < count)
        {
            positions.Add(currentPosition);
            currentPosition = currentPosition + direction * (pieceWidth + shuffleGap);
            i++;
        }

        return new Tuple<List<Vector3>, Vector3>(positions, currentPosition);
    }
}
