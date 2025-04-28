using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public PieceType pieceType;
    public PieceColor pieceColor;

    Vector3 offset;
    Vector3 originalPosition;

    
    [HideInInspector] public Board board;

    bool isDraggingAllowed = false; // Flag to control dragging

    [HideInInspector] public Square occupyingSquare; // The square this piece occupies

    void Start()
    {
        board = GameObject.Find("Manager").GetComponent<Board>();
        SetOccupyingSquare(); // Set the square this piece occupies
    }

    void OnMouseDown()
    {
        if(!CanMove())
        {
            isDraggingAllowed = false;
            return;
        }

        isDraggingAllowed = true; // Allow dragging if it's the player's turn

        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z; // Correct distance
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        offset = transform.position - mouseWorldPos;
        originalPosition = transform.position; // Store the original position for canceling moves
    }

    void OnMouseDrag()
    {
        if(!isDraggingAllowed) return; // Prevent dragging if not allowed
        
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        transform.position = mouseWorldPos + offset; // Drag the piece around
    }

    void OnMouseUp()
    {
        if(!isDraggingAllowed) return; // Prevent dropping if not allowed
        
        // Snap to grid
        Vector3 snappedPos = new Vector3(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y),
            0
        );
        transform.position = snappedPos;

        // Temporarily disable own collider for the move check
        Collider2D myCollider = GetComponent<Collider2D>();
        myCollider.enabled = false;

        // Check if another piece occupies the square
        Collider2D hit = Physics2D.OverlapPoint(new Vector2(snappedPos.x, snappedPos.y));

        // Re-enable own collider after the check
        myCollider.enabled = true;

        if (hit != null)
        {
            Piece otherPiece = hit.GetComponent<Piece>();
            if (otherPiece != null)
            {
                if (otherPiece.pieceColor != pieceColor)
                {
                    Destroy(otherPiece.gameObject); // Capture the opponent's piece
                }
                else
                {
                    // Can't capture own piece, revert the move
                    transform.position = originalPosition;
                    return;
                }
            }
        }

        // Prevent out of bounds movement
        if (snappedPos.x < 0 || snappedPos.x > 7 || snappedPos.y < 0 || snappedPos.y > 7)
        {
            transform.position = originalPosition; // Revert if out of bounds
            return;
        }

        // Update turn only after a valid move
        if (transform.position != originalPosition)
        {
            board.turn++; // Change turn after successful move
        }else{
            return;
        }
    }

    bool CanMove(){
        if(pieceColor == PieceColor.White && board.turn % 2 == 0 || pieceColor == PieceColor.Black && board.turn % 2 == 1){
            return true;
        }else{
            return false;
        }
    }

    void SetOccupyingSquare()
    {
        foreach (Square square in board.squares)
        {
            // Compare positions with a small tolerance
            if (square.transform.position == transform.position)
            {
                square.SetOccupyingPiece(this);
                occupyingSquare = square; // Set the square this piece occupies
                return;
            }
        }
    }
}
