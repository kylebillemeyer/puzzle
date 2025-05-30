using UnityEngine;
using UnityEngine.EventSystems;

public class PuzzlePiece : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    void Start()
    {
        // Store original transform
        originalPosition = transform.position;

        // Set layer to Pieces
        gameObject.layer = LayerMask.NameToLayer("Pieces");

        // Get or add BoxCollider2D
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        boxCollider.isTrigger = true;
        
        // Set collider to match sprite size
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            boxCollider.size = spriteRenderer.sprite.bounds.size;
        }

        // Add collider visualizer
        // if (GetComponent<ColliderVisualizer>() == null)
        // {
        //     gameObject.AddComponent<ColliderVisualizer>();
        // }
    }

    public void ResetToOriginalPosition()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }
}