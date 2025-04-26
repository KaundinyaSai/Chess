using UnityEngine;

public class Piece : MonoBehaviour
{
    public PieceType pieceType;
    public PieceColor pieceColor;

    Vector3 offset;

    Vector3 originalPosition;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z; // Keep correct distance
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        offset = transform.position - mouseWorldPos;
        originalPosition = transform.position; // Store the original position
    }

    void OnMouseDrag()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        transform.position = mouseWorldPos + offset;
    }

    void OnMouseUp()
    {
        // Snap to grid
        Vector3 snappedPos = new Vector3(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y),
            0
        );
        transform.position = snappedPos;

        // Temporarily disable own collider
        Collider2D myCollider = GetComponent<Collider2D>();
        myCollider.enabled = false;

        // Now check if another piece is already here
        Collider2D hit = Physics2D.OverlapPoint(new Vector2(snappedPos.x, snappedPos.y));

        // Re-enable own collider
        myCollider.enabled = true;

        if (hit != null)
        {
            Piece otherPiece = hit.GetComponent<Piece>();
            if (otherPiece != null)
            {
                if (otherPiece.pieceColor != pieceColor)
                {
                    Destroy(otherPiece.gameObject); // Capture
                }
                else
                {
                    // Can't capture own piece, move back
                    transform.position = originalPosition;
                }
            }
        }

        // Check if out of bounds
        if (transform.position.x < 0 || transform.position.x > 7 || transform.position.y < 0 || transform.position.y > 7)
        {
            transform.position = originalPosition;
        }
    }
}